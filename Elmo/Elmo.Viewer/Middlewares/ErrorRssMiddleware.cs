using System;
using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;
using Elmo.Logging;
using Elmo.Utilities;
using Elmo.Viewer.Utilities;
using Microsoft.Owin;

namespace Elmo.Viewer.Middlewares
{
    internal class ErrorRssMiddleware : OwinMiddleware
    {
        private readonly ElmoOptions options;
        private readonly IErrorLog errorLog;

        public ErrorRssMiddleware(OwinMiddleware next, ElmoOptions options, IErrorLog errorLog) : base(next)
        {
            this.options = options;
            this.errorLog = errorLog;
        }

        public override async Task Invoke(IOwinContext context)
        {
            PathString subPath;
            var isElmoRequest = context.Request.Path.StartsWithSegments(options.Path, out subPath);
            if (!options.Path.HasValue || !(isElmoRequest && subPath.StartsWithSegments(new PathString("/rss"))))
            {
                await Next.Invoke(context);
                return;
            }

            const int pageSize = 15;
            var errorLogEntries = await errorLog.GetErrorsAsync(0, pageSize);

            var syndicationFeed = new SyndicationFeed();

            var hostName = EnvironmentUtilities.GetMachineNameOrDefault("Unknown Host");
            syndicationFeed.Title = new TextSyndicationContent($"Error log of {errorLog.ApplicationName} on {hostName}.");
            syndicationFeed.Description = new TextSyndicationContent("Log of recent errors");
            syndicationFeed.Language = "en-us";

            var uriAsString = context.Request.Uri.ToString();
            var baseUri = new Uri(uriAsString.Remove(uriAsString.LastIndexOf("/rss", StringComparison.InvariantCulture)));
            syndicationFeed.Links.Add(SyndicationLink.CreateAlternateLink(baseUri));

            var items = new List<SyndicationItem>();
            foreach (var errorLogEntry in errorLogEntries)
            {
                var item = new SyndicationItem
                {
                    Title = SyndicationContent.CreatePlaintextContent(errorLogEntry.Error.Message),
                    Content =
                        SyndicationContent.CreatePlaintextContent(
                            $"An error of type {errorLogEntry.Error.TypeName} occurred. {errorLogEntry.Error.Message}"),
                    PublishDate = errorLogEntry.Error.Time
                };
                item.Links.Add(SyndicationLink.CreateAlternateLink(new Uri(baseUri, $"/detail?id={errorLogEntry.Id}")));

                items.Add(item);
            }

            syndicationFeed.Items = items;

            context.Response.ContentType = "application/rss+xml";
            context.Response.StatusCode = 200;

            using (var writer = XmlWriter.Create(context.Response.Body, SettingsUtility.XmlWriterSettings))
            {
                var formatter = new Rss20FeedFormatter(syndicationFeed);
                formatter.WriteTo(writer);
            }
        }
    }
}
