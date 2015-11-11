using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elmo.Logging;
using Elmo.Responses;
using Elmo.Responses.Views;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Microsoft.Owin.Security;
using Owin;

namespace Elmo
{
    public class ElmoViewerMiddleware : OwinMiddleware
    {
        private readonly ElmoOptions options;
        private readonly IErrorLog errorLog;
        private readonly List<IRequestHandler> handlers;
        private readonly ILogger logger;

        public ElmoViewerMiddleware(OwinMiddleware next, IAppBuilder app, ElmoOptions options, IErrorLog errorLog)
            : base(next)
        {
            this.options = options;
            this.errorLog = errorLog;
            logger = app.CreateLogger<ElmoViewerMiddleware>();

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
                if (!options.AllowRemoteAccess && !IsLocalIpAddress(context) && IsAuthenticated(context.Authentication))
                {
                    await new RemoteAccessErrorView().ProcessRequestAsync(context, errorLog);
                    return;
                }

                PathString subPath;
                context.Request.Path.StartsWithSegments(options.Path, out subPath);

                var firstOrDefault = handlers.FirstOrDefault(handler => handler.CanProcess(subPath.Value));

                if (firstOrDefault != null)
                    await firstOrDefault.ProcessRequestAsync(context, errorLog);
                else
                    await new NotFoundErrorView().ProcessRequestAsync(context, errorLog);
            }
            catch (Exception e)
            {
                logger.WriteError("An error occured while processing Elmo Viewer middleware.", e);
            }
        }

        private static bool IsAuthenticated(IAuthenticationManager authenticationManager)
        {
            return authenticationManager?.User?.Identity != null && authenticationManager.User.Identity.IsAuthenticated;
        }

        private static bool IsLocalIpAddress(IOwinContext owinContext)
        {
            return Convert.ToBoolean(owinContext.Environment["server.IsLocal"]);
        }
    }
}
