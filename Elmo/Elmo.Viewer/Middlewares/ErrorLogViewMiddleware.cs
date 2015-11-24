using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using Elmo.Logging;
using Elmo.Utilities;
using Elmo.Viewer.Components;
using Elmo.Viewer.Utilities;
using Microsoft.Owin;

namespace Elmo.Viewer.Middlewares
{
    internal class ErrorLogViewMiddleware : ErrorViewBaseMiddleware
    {
        private IList<ErrorLogEntry> errorLogEntries;
        private int pageIndex;
        private int pageSize;
        private int totalCount;

        private const int DefaultPageSize = 15;
        private const int MaximumPageSize = 100;

        public ErrorLogViewMiddleware(OwinMiddleware next, ElmoViewerOptions options, IErrorLog errorLog)
            : base(next, options, errorLog) { }

        protected override async Task RenderContentsAsync(XmlWriter writer)
        {
            await RenderTitleAsync(writer);

            await
                SpeedBarComponent.RenderAsync(writer, SpeedBarComponent.GetRssFeed(BasePageName),
                    SpeedBarComponent.GetRssDigestFeed(BasePageName), SpeedBarComponent.GetDownloadLog(BasePageName));

            if (errorLogEntries.Count < 1)
            {
                await RenderNoErrorsAsync(writer);
            }
            else
            {
                // Write error number range displayed on this page and the
                // total available in the log, followed by stock
                // page sizes.
                await writer.WriteStartElementAsync("p");

                await RenderStatsAsync(writer);
                await RenderStockPageSizesAsync(writer);

                await writer.WriteEndElementAsync(); // </p>

                // Write out the main table to display the errors.
                await RenderErrorsAsync(writer);

                // Write out page navigation links.
                await RenderPageNavigatorsAsync(writer);
            }
        }

        private async Task RenderPageNavigatorsAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync("p");
            {
                var nextPageIndex = pageIndex + 1;
                var moreErrors = nextPageIndex*pageSize < totalCount;

                if (moreErrors)
                {
                    await writer.WriteStartElementAsync("a");
                    await writer.WriteAttributeStringAsync("href", $"{BasePageName}?page={nextPageIndex + 1}&size={pageSize}");
                    await writer.WriteAttributeStringAsync("rel", "next");
                    {
                        await writer.WriteStringAsync("Next errors");
                    }
                    await writer.WriteEndElementAsync();
                }

                // If not on the first page then render a link to the firs page.

                if (pageIndex > 0 && totalCount > 0)
                {
                    if (moreErrors)
                        await writer.WriteRawAsync("; ");

                    await writer.WriteStartElementAsync("a");
                    await writer.WriteAttributeStringAsync("href", $"{BasePageName}?page=1&size={pageSize}");
                    await writer.WriteAttributeStringAsync("rel", "start");
                    {
                        await writer.WriteStringAsync("Back to first page");
                    }
                    await writer.WriteEndElementAsync();
                }
            }
            await writer.WriteEndElementAsync();
        }

        private async Task RenderErrorsAsync(XmlWriter writer)
        {
            //
            // Create a table to display error information in each row.
            //

            await writer.WriteStartElementAsync("table");
            await writer.WriteAttributeStringAsync("id", "ErrorLog");
            await writer.WriteAttributeStringAsync("style", "border-spacing: 0;");
            {

                //
                // Create the table row for headings.
                //

                await writer.WriteStartElementAsync("tr");
                {
                    await RenderCellAsync(writer, "th", "Host", "host-col");
                    await RenderCellAsync(writer, "th", "Code", "code-col");
                    await RenderCellAsync(writer, "th", "Type", "type-col");
                    await RenderCellAsync(writer, "th", "Error", "error-col");
                    await RenderCellAsync(writer, "th", "User", "user-col");
                    await RenderCellAsync(writer, "th", "Date", "date-col");
                    await RenderCellAsync(writer, "th", "Time", "time-col");
                }
                await writer.WriteEndElementAsync();

                //
                // Generate a table body row for each error.
                //

                for (int index = 0; index < errorLogEntries.Count; index++)
                {
                    var errorLogEntry = errorLogEntries[index];

                    await writer.WriteStartElementAsync("tr");
                    await writer.WriteAttributeStringAsync("class", index % 2 == 0 ? "even-row" : "odd-row");
                    {
                        // Format host and status code cells.

                        var error = errorLogEntry.Error;

                        await RenderCellAsync(writer, "td", error.HostName, "host-col");
                        await RenderCellAsync(writer, "td", error.StatusCode.ToString(), "code-col", HttpCodeParser.GetstatusDescription(error.StatusCode));
                        await RenderCellAsync(writer, "td", ErrorShortName(error.TypeName), "host-col", error.TypeName);

                        //
                        // Format the message cell, which contains the message 
                        // text and a details link pointing to the page where
                        // all error details can be viewed.
                        //

                        await writer.WriteStartElementAsync("td");
                        await writer.WriteAttributeStringAsync("class", "error-col");
                        {
                            await writer.WriteStartElementAsync("label");
                            {
                                await writer.WriteStringAsync(error.Message);
                            }
                            await writer.WriteEndElementAsync();

                            await writer.WriteStartElementAsync("a");
                            await writer.WriteAttributeStringAsync("href", $"{BasePageName}/detail?id={errorLogEntry.Id}");
                            {
                                await writer.WriteRawAsync("Details&hellip;");
                            }
                            await writer.WriteEndElementAsync();
                        }
                        await writer.WriteEndElementAsync();

                        // Format the user, date and time cells.
                        await RenderCellAsync(writer, "td", error.User, "user-col");
                        await RenderCellAsync(writer, "td", error.Time.DateTime.ToShortDateString(), "date-col", error.Time.DateTime.ToLongDateString());
                        await RenderCellAsync(writer, "td", error.Time.DateTime.ToShortTimeString(), "time-col", error.Time.DateTime.ToLongTimeString());

                    }
                    await writer.WriteEndElementAsync();
                }
            }
            await writer.WriteEndElementAsync();
        }

        private static string ErrorShortName(string fullName)
        {
            if (fullName.LastIndexOf('.') > 0)
                return fullName.Substring(fullName.LastIndexOf('.') + 1);

            return fullName;
        }

        private static async Task RenderCellAsync(XmlWriter writer, string cellType, string contents, string cssClassName, string tooltip = "")
        {
            await writer.WriteStartElementAsync(cellType);
            await writer.WriteAttributeStringAsync("class", cssClassName);

            if (string.IsNullOrEmpty(contents?.Trim()))
            {
                await writer.WriteRawAsync("&nbsp;");
            }
            else
            {
                if (string.IsNullOrEmpty(tooltip?.Trim()))
                {
                    await writer.WriteStringAsync(contents);
                }
                else
                {
                    await writer.WriteStartElementAsync("label");
                    await writer.WriteAttributeStringAsync("title", tooltip);
                    await writer.WriteStringAsync(contents);
                    await writer.WriteEndElementAsync();
                }
            }

            await writer.WriteEndElementAsync();
        }

        private async Task RenderStatsAsync(XmlWriter writer)
        {
            var firstErrorNumber = pageIndex * pageSize + 1;
            var lastErrorNumber = firstErrorNumber + errorLogEntries.Count - 1;
            var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);

            await writer.WriteStringAsync($"Errors {firstErrorNumber} to {lastErrorNumber} of total {totalCount} (page {(pageIndex + 1)} of {totalPages}). ");
        }

        private async Task RenderStockPageSizesAsync(XmlWriter writer)
        {
            //
            // Write out a set of stock page size choices. Note that
            // selecting a stock page size re-starts the log 
            // display from the first page to get the right paging.
            //

            await writer.WriteStringAsync("Start with ");

            var stockSizes = new[] { 10, 15, 20, 25, 30, 50, 100 };

            for (var stockSizeIndex = 0; stockSizeIndex < stockSizes.Length; stockSizeIndex++)
            {
                var stockSize = stockSizes[stockSizeIndex];

                if (stockSizeIndex > 0)
                    await writer.WriteStringAsync(stockSizeIndex + 1 < stockSizes.Length ? ", " : " or ");

                var href = $"{BasePageName}?page=1&size={stockSize}";

                await writer.WriteStartElementAsync("a");
                await writer.WriteAttributeStringAsync("href", href);
                await writer.WriteAttributeStringAsync("rel", "start");

                await writer.WriteStringAsync(stockSize.ToString());
                await writer.WriteEndElementAsync();
            }

            await writer.WriteStringAsync(" errors per page.");
        }

        private async Task RenderNoErrorsAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync("p");

            await writer.WriteStringAsync("No errors found. ");

            //
            // It is possible that there are no error at the requested 
            // page in the log (especially if it is not the first page).
            // However, if there are error in the log
            //

            if (pageIndex > 0 && totalCount > 0)
            {
                var href = $"{BasePageName}?page=1&size={pageSize}";

                await writer.WriteStartElementAsync("a");
                await writer.WriteAttributeStringAsync("href", href);
                await writer.WriteAttributeStringAsync("rel", "start");

                await writer.WriteStringAsync("Go to first page");
                await writer.WriteEndElementAsync();

                await writer.WriteStringAsync(". ");
            }

            await writer.WriteEndElementAsync();
        }

        private async Task RenderTitleAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync("h1");
            await writer.WriteAttributeStringAsync("id", "PageTitle");
            await writer.WriteStringAsync("Error Log for ");

            await writer.WriteStartElementAsync("span");
            await writer.WriteAttributeStringAsync("id", "ApplicationName");
            await writer.WriteAttributeStringAsync("title", ApplicationName);
            await writer.WriteStringAsync(ApplicationName);

            var machineName = EnvironmentUtilities.GetMachineNameOrDefault();
            if (!string.IsNullOrEmpty(machineName?.Trim()))
            {
                await writer.WriteStringAsync(" on ");
                await writer.WriteStringAsync(machineName);
            }

            await writer.WriteEndElementAsync(); // </span>

            await writer.WriteEndElementAsync(); // </h1>
        }

        protected override async Task LoadContentsAsync(IOwinContext context)
        {
            // Get the page index and size parameters within their bounds.
            pageSize = Convert.ToInt32(context.Request.Query["size"], CultureInfo.InvariantCulture);
            pageSize = Math.Min(MaximumPageSize, Math.Max(0, pageSize));

            if (pageSize == 0)
                pageSize = DefaultPageSize;

            pageIndex = Convert.ToInt32(context.Request.Query["page"], CultureInfo.InvariantCulture);
            pageIndex = Math.Max(1, pageIndex) - 1;

            // Read the error records.
            errorLogEntries = await ErrorLog.GetErrorsAsync(pageIndex, pageSize);
            totalCount = ErrorLog.GetTotalErrorCount();

            // Set the title of the page.
            var hostName = EnvironmentUtilities.GetMachineNameOrDefault();
            PageTitle = hostName.Length > 0
                ? $"Error log for {ApplicationName} on {hostName} (Page #{(pageIndex + 1).ToString("N0")})"
                : $"Error log for {ApplicationName} (Page #{(pageIndex + 1).ToString("N0")})";
        }
        
    }
}
