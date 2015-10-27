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
    internal class ErrorRssHandler
    {
        private readonly IOwinContext owinContext;
        private readonly IErrorLog errorLog;

        public ErrorRssHandler(IOwinContext owinContext, IErrorLog errorLog)
        {
            this.owinContext = owinContext;
            this.errorLog = errorLog;
        }

        public async Task ProcessRequestAsync()
        {
            const int pageSize = 15;
            var errorLogEntries = await errorLog.GetErrorsAsync(0, pageSize);

            var syndicationFeed = new SyndicationFeed();

            var hostName = EnvironmentUtilities.GetMachineNameOrDefault("Unknown Host");
            syndicationFeed.Title = new TextSyndicationContent($"Error log of {errorLog.ApplicationName} on {hostName}.");
            syndicationFeed.Description = new TextSyndicationContent("Log of recent errors");
            syndicationFeed.Language = "en-us";

            var uriAsString = owinContext.Request.Uri.ToString();
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

            owinContext.Response.ContentType = "application/rss+xml";
            owinContext.Response.StatusCode = 200;
            owinContext.Response.ReasonPhrase = "Ok";

            var xmlWriterSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true
            };

            using (var writer = XmlWriter.Create(owinContext.Response.Body, xmlWriterSettings))
            {
                var formatter = new Rss20FeedFormatter(syndicationFeed);
                formatter.WriteTo(writer);
            }
        }
    }
}
