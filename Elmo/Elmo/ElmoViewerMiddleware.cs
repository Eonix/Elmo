using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elmo.Logging;
using Elmo.Responses;
using Elmo.Responses.Views;
using Microsoft.Owin;

namespace Elmo
{
    public class ElmoViewerMiddleware : OwinMiddleware
    {
        private readonly ElmoOptions options;
        private readonly IErrorLog errorLog;
        private readonly List<IRequestHandler> handlers;

        public ElmoViewerMiddleware(OwinMiddleware next, ElmoOptions options, IErrorLog errorLog)
            : base(next)
        {
            this.options = options;
            this.errorLog = errorLog;

            handlers = new List<IRequestHandler>
            {
                new ErrorJsonHandler(),
                new ErrorRssHandler(),
                new ErrorDigestRssHandler(),
                new ErrorLogDownloadHandler(),
                new ErrorLogCssHandler(),
                new ErrorLogView(options.Path),
                new ErrorDetailView(options.Path)
            };
        }

        public override async Task Invoke(IOwinContext context)
        {
            if (!options.Path.HasValue || !context.Request.Path.StartsWithSegments(options.Path))
            {
                await Next.Invoke(context);
                return;
            }

            try
            {
                if (!options.AllowRemoteAccess && !IsLocalIpAddress(context))
                {
                    // TODO: Check if authorized.
                    context.Response.StatusCode = 403;
                    context.Response.ReasonPhrase = "Forbidden";
                    return;
                    // TODO: Serve RemoteAccessError.html
                }

                // TODO: Correct Encoding.

                PathString subPath;
                context.Request.Path.StartsWithSegments(options.Path, out subPath);

                var firstOrDefault = handlers.FirstOrDefault(handler => handler.CanProcess(subPath.Value));
                if (firstOrDefault != null)
                {
                    await firstOrDefault.ProcessRequestAsync(context, errorLog);
                }
                else
                {
                    context.Response.StatusCode = 404;
                    context.Response.ReasonPhrase = "Not Found";
                    // TODO: Display proper not found message.
                }
            }
            catch (Exception e)
            {
                // TODO: Log this error to built-in owin logger. Display error if possible.
                Console.WriteLine(e);
            }
        }

        private static bool IsLocalIpAddress(IOwinContext owinContext)
        {
            return Convert.ToBoolean(owinContext.Environment["server.IsLocal"]);
        }
    }
}
