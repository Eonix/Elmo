using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Xml;
using Elmo.Logging;
using Elmo.Viewer.Utilities;
using Microsoft.Owin;

namespace Elmo.Viewer.Middlewares
{
    internal abstract class ErrorViewBaseMiddleware : OwinMiddleware
    {
        public ElmoViewerOptions Options { get; }
        protected string PageTitle { get; set; }
        protected IErrorLog ErrorLog { get; }
        protected string BasePageName => Options.Path.Value;
        protected string ApplicationName => ErrorLog.ApplicationName;

        protected ErrorViewBaseMiddleware(OwinMiddleware next, ElmoViewerOptions options, IErrorLog errorLog) : base(next)
        {
            Options = options;
            ErrorLog = errorLog;
        }

        public override async Task Invoke(IOwinContext context)
        {
            await RenderAsync(context);
        }

        private async Task RenderDocumentStartAsync(XmlWriter writer)
        {
            await writer.WriteDocTypeAsync("html"); // doctype

            await writer.WriteStartElementAsync(null, "html", "http://www.w3.org/1999/xhtml"); // html start

            await writer.WriteStartElementAsync("head"); // head start
            await RenderHeadAsync(writer);
            await writer.WriteEndElementAsync(); // head end

            await writer.WriteStartElementAsync("body"); // body start
        }

        private async Task RenderHeadAsync(XmlWriter writer)
        {
            await writer.WriteStartElementAsync("meta");
            await writer.WriteAttributeStringAsync("http-equiv", "X-UA-Compatible");
            await writer.WriteAttributeStringAsync("content", "IE=EmulateIE7");
            await writer.WriteEndElementAsync();

            await writer.WriteElementStringAsync("title", PageTitle);

            await writer.WriteStartElementAsync("link");
            await writer.WriteAttributeStringAsync("rel", "stylesheet");
            await writer.WriteAttributeStringAsync("type", "text/css");
            await writer.WriteAttributeStringAsync("href", BasePageName + "/stylesheet");
            await writer.WriteEndElementAsync();
        }
        
        private async Task RenderDocumentEndAsync(XmlWriter writer)
        {
            //
            // Write the powered-by signature, that includes version information.
            //

            await writer.WriteStartElementAsync("p"); // <p>
            await writer.WriteAttributeStringAsync("id", "Footer");


            //PoweredBy poweredBy = new PoweredBy();
            //poweredBy.RenderControl(writer);

            //
            // Write out server date, time and time zone details.
            //

            var now = DateTime.Now;

            await writer.WriteStringAsync("Server date is ");
            await writer.WriteStringAsync(now.ToString("D", CultureInfo.InvariantCulture));

            await writer.WriteStringAsync(". Server time is ");
            await writer.WriteStringAsync(now.ToString("T", CultureInfo.InvariantCulture));

            await writer.WriteStringAsync(". All dates and times displayed are in the ");
            await writer.WriteStringAsync(TimeZone.CurrentTimeZone.IsDaylightSavingTime(now) ?
                TimeZone.CurrentTimeZone.DaylightName : TimeZone.CurrentTimeZone.StandardName);
            await writer.WriteStringAsync(" zone. ");

            //
            // Write out the source of the log.
            //

            await writer.WriteStringAsync("This log is provided by the ");
            await writer.WriteStringAsync(ErrorLog.Name);
            await writer.WriteStringAsync(".");

            await writer.WriteEndElementAsync();// </p>

            await writer.WriteEndElementAsync(); // </body>
            await writer.WriteEndElementAsync(); // </html>
        }

        private async Task RenderAsync(IOwinContext context)
        {
            await LoadContentsAsync(context);

            var response = context.Response;
            response.ContentType = "text/html";
            response.StatusCode = 200;

            using (var writer = XmlWriter.Create(response.Body, SettingsUtility.XmlWriterSettings))
            {
                await RenderDocumentStartAsync(writer);
                await RenderContentsAsync(writer);
                await RenderDocumentEndAsync(writer);
            }
        }

        protected abstract Task RenderContentsAsync(XmlWriter writer);

        protected abstract Task LoadContentsAsync(IOwinContext context);
    }
}
