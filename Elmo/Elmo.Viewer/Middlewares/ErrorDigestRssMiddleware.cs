using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Elmo.Logging;
using Elmo.Utilities;
using Elmo.Viewer.Utilities;
using Microsoft.Owin;

namespace Elmo.Viewer.Middlewares
{
    internal class ErrorDigestRssMiddleware : OwinMiddleware
    {
        private readonly ElmoViewerOptions options;
        private readonly IErrorLog errorLog;

        public ErrorDigestRssMiddleware(OwinMiddleware next, ElmoViewerOptions options, IErrorLog errorLog) : base(next)
        {
            this.options = options;
            this.errorLog = errorLog;
        }

        public override async Task Invoke(IOwinContext context)
        {
            PathString subPath;
            context.Request.Path.StartsWithSegments(options.Path, out subPath);
            if (!subPath.StartsWithSegments(new PathString("/digestrss")))
            {
                await Next.Invoke(context);
                return;
            }

            var syndicationFeed = new SyndicationFeed();

            var hostName = EnvironmentUtilities.GetMachineNameOrDefault("Unknown Host");
            syndicationFeed.Title = new TextSyndicationContent($"Daily digest of errors in {errorLog.ApplicationName} on {hostName}.");
            syndicationFeed.Description = new TextSyndicationContent("Daily digest of application errors");
            syndicationFeed.Language = "en-us";

            var uriAsString = context.Request.Uri.ToString();
            var baseUri = new Uri(uriAsString.Remove(uriAsString.LastIndexOf("/digestrss", StringComparison.InvariantCulture)));
            syndicationFeed.Links.Add(SyndicationLink.CreateAlternateLink(baseUri));

            var logEntries = await GetAllEntriesAsync();
            var groupBy = logEntries.GroupBy(
                entry => new DateTime(entry.Error.Time.Year, entry.Error.Time.Month, entry.Error.Time.Day));


            var itemList = new List<SyndicationItem>();
            foreach (var grouping in groupBy)
            {
                var syndicationItem = new SyndicationItem
                {
                    Title =
                        new TextSyndicationContent(
                            $"Digest for {grouping.Key.ToString("yyyy-MM-dd")} ({grouping.Key.ToLongDateString()})"),
                    PublishDate = grouping.Key,
                    Id = grouping.Key.ToString("yyyy-MM-dd")
                };

                var builder = new StringBuilder();

                builder.AppendLine("<ul>");
                foreach (var errorLogEntry in grouping)
                {
                    builder.AppendLine("<li>");
                    builder.AppendLine($"{errorLogEntry.Error.TypeName}: <a href=\"{baseUri}/detail?id={errorLogEntry.Id}\">{errorLogEntry.Error.Message}</a>");
                    builder.AppendLine("</li>");
                }
                builder.AppendLine("</ul>");

                syndicationItem.Content = SyndicationContent.CreateHtmlContent(builder.ToString());

                itemList.Add(syndicationItem);
            }

            syndicationFeed.Items = itemList;

            context.Response.ContentType = "application/rss+xml";
            context.Response.StatusCode = 200;
            
            using (var writer = XmlWriter.Create(context.Response.Body, SettingsUtility.XmlWriterSettings))
            {
                var formatter = new Rss20FeedFormatter(syndicationFeed);
                formatter.WriteTo(writer);
            }
        }

        private async Task<IEnumerable<ErrorLogEntry>> GetAllEntriesAsync()
        {
            const int defaultPageSize = 30;
            const int maxPageLimit = 30;
            var totalErrorCount = await errorLog.GetTotalErrorCountAsync();

            var count = 0;
            var list = new List<ErrorLogEntry>();
            for (var pageIndex = 0; pageIndex < maxPageLimit && count < totalErrorCount; pageIndex++)
            {
                var entries = await errorLog.GetErrorsAsync(pageIndex, defaultPageSize);
                count += entries.Count;
                list.AddRange(entries);
            }

            return list;
        }
    }
}
