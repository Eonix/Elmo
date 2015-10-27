using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Elmo.Logging;
using Elmo.Utilities;
using Microsoft.Owin;

namespace Elmo.Responses
{
    internal class ErrorDigestRssHandler
    {
        private readonly IOwinContext owinContext;
        private readonly IErrorLog errorLog;

        public ErrorDigestRssHandler(IOwinContext owinContext, IErrorLog errorLog)
        {
            this.owinContext = owinContext;
            this.errorLog = errorLog;
        }

        public async Task ProcessRequestAsync()
        {
            owinContext.Response.ContentType = "application/xml";

            var syndicationFeed = new SyndicationFeed();

            var hostName = EnvironmentUtilities.GetMachineNameOrDefault("Unknown Host");
            syndicationFeed.Title = new TextSyndicationContent($"Daily digest of errors in {errorLog.ApplicationName} on {hostName}.");
            syndicationFeed.Description = new TextSyndicationContent("Daily digest of application errors");
            syndicationFeed.Language = "en";

            var baseUri = owinContext.Request.Uri;
            syndicationFeed.Links.Add(new SyndicationLink(baseUri));

            // 0. Find out the maximum errors to display.
            // 1. Iterate through the errors.
            // 2. Group errors by day.
            // 3. Add an RSS item per day.
            // 4. Add all errors for that day to the RSS item.
            // 5. Max days (pages) to display is 30.

            const int maxPageLimit = 30;
            const int defaultPageSize = 30;



            var itemList = new List<SyndicationItem>();
            var pageIndex = 0;

            var runningDay = DateTime.MaxValue;
            var runningErrorCount = 0;
            var errorEntriesCount = 0;

            do
            {
                // Get a logical page of recent errors and loop through them.

                var errorLogEntries = await errorLog.GetErrorsAsync(pageIndex++, defaultPageSize);
                errorEntriesCount = errorLogEntries.Count;

                foreach (var entry in errorLogEntries)
                {
                    var error = entry.Error;
                    var errorDay = new DateTime(error.Time.Year, error.Time.Month, error.Time.Day);

                    var syndicationItem = new SyndicationItem();
                    syndicationItem.Title = new TextSyndicationContent($"Digest for {errorDay.ToString("yyyy-MM-dd")} ({errorDay.ToLongDateString()})");
                    syndicationItem.PublishDate = error.Time;
                    syndicationItem.Content = new TextSyndicationContent(error.Message, TextSyndicationContentKind.Html);

                    itemList.Add(syndicationItem);
                }

            } while (pageIndex < maxPageLimit && itemList.Count < defaultPageSize && errorEntriesCount > 0);

            syndicationFeed.Items = itemList;

            owinContext.Response.StatusCode = 200;
            owinContext.Response.ReasonPhrase = "Ok";
            using (var writer = XmlWriter.Create(owinContext.Response.Body))
            {
                new Rss20FeedFormatter(syndicationFeed).WriteTo(writer);
            }
        }
    }
}
