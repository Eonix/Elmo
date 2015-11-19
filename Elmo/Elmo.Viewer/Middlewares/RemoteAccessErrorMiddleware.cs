using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Elmo.Viewer.Utilities;
using Microsoft.Owin;
using Microsoft.Owin.Security;

namespace Elmo.Viewer.Middlewares
{
    internal class RemoteAccessErrorMiddleware : OwinMiddleware
    {
        private readonly ElmoViewerOptions options;

        public RemoteAccessErrorMiddleware(OwinMiddleware next, ElmoViewerOptions options)
            : base(next)
        {
            this.options = options;
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (IsLocalIpAddress(context) || (options.AllowRemoteAccess && IsAuthenticated(context.Authentication)))
            {
                await Next.Invoke(context);
            }
            else
            {
                var response = context.Response;
                response.ContentType = "text/html";
                response.StatusCode = 403;

                await WriteToResponseBody(context);
            }
        }

        private static async Task WriteToResponseBody(IOwinContext context)
        {
            using (var writer = XmlWriter.Create(context.Response.Body, SettingsUtility.XmlWriterSettings))
            {
                await writer.WriteDocTypeAsync("html"); // doctype

                await writer.WriteStartElementAsync(null, "html", "http://www.w3.org/1999/xhtml");
                {
                    await writer.WriteStartElementAsync("head");
                    {
                        await writer.WriteElementStringAsync("title", "403 Forbidden");
                    }
                    await writer.WriteEndElementAsync();

                    await writer.WriteStartElementAsync("body");
                    {
                        await writer.WriteElementStringAsync("h1", "Forbidden");
                        await writer.WriteElementStringAsync("p", $"You don't have permission to access {context.Request.Path} on this server.");
                    }
                    await writer.WriteEndElementAsync();
                }
            }

        }

        private static bool IsAuthenticated(IAuthenticationManager authenticationManager)
        {
            return authenticationManager?.User?.Identity != null && authenticationManager.User.Identity.IsAuthenticated;
        }

        private static bool IsLocalIpAddress(IOwinContext owinContext)
        {
            return owinContext.Get<bool>("server.IsLocal");
        }
    }
}
