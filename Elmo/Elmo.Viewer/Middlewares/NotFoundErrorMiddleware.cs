using System.Threading.Tasks;
using System.Xml;
using Elmo.Viewer.Utilities;
using Microsoft.Owin;

namespace Elmo.Viewer.Middlewares
{
    public class NotFoundErrorMiddleware : OwinMiddleware
    {
        public NotFoundErrorMiddleware(OwinMiddleware next)
            : base(next)
        {
        }

        public override async Task Invoke(IOwinContext context)
        {
            var response = context.Response;
            response.ContentType = "text/html";
            response.StatusCode = 404;

            using (var writer = XmlWriter.Create(response.Body, SettingsUtility.XmlWriterSettings))
            {
                await writer.WriteDocTypeAsync("html"); // doctype

                await writer.WriteStartElementAsync(null, "html", "http://www.w3.org/1999/xhtml");
                {
                    await writer.WriteStartElementAsync("head");
                    {
                        await writer.WriteElementStringAsync("title", "404 Not Found");
                    }
                    await writer.WriteEndElementAsync();

                    await writer.WriteStartElementAsync("body");
                    {
                        await writer.WriteElementStringAsync("h1", "Not Found");
                        await writer.WriteElementStringAsync("p", $"The requested URL {context.Request.Path} was not found on this server.");
                    }
                    await writer.WriteEndElementAsync();
                }
            }
        }
    }
}
