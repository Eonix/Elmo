using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Elmo.Logging;
using Elmo.Utilities;
using Microsoft.Owin;

namespace Elmo.Responses.Views
{
    internal class NotFoundErrorView : IRequestHandler
    {
        public async Task ProcessRequestAsync(IOwinContext owinContext, IErrorLog errorLog)
        {
            var settings = new XmlWriterSettings
            {
                Async = true,
                Indent = true,
                IndentChars = "  ",
                Encoding = Encoding.UTF8,
                OmitXmlDeclaration = true
            };

            var response = owinContext.Response;
            response.ContentType = "text/html";
            response.StatusCode = 404;
            response.ReasonPhrase = "Not Found";

            using (var writer = XmlWriter.Create(response.Body, settings))
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
                        await writer.WriteElementStringAsync("p", $"The requested URL {owinContext.Request.Path} was not found on this server.");
                    }
                    await writer.WriteEndElementAsync();
                }
            }
        }

        public bool CanProcess(string path)
        {
            throw new NotSupportedException();
        }
    }
}
