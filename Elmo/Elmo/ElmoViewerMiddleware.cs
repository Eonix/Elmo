using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elmo.Logging;
using Elmo.Properties;
using Elmo.Responses;
using Elmo.Views;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Elmo
{
    public class ElmoViewerMiddleware : OwinMiddleware
    {
        private readonly ElmoOptions options;
        private readonly IErrorLog errorLog;

        public ElmoViewerMiddleware(OwinMiddleware next, ElmoOptions options, IErrorLog errorLog)
            : base(next)
        {
            this.options = options;
            this.errorLog = errorLog;
        }

        public async override Task Invoke(IOwinContext context)
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

                if (PathEquals(context, "xml"))
                {
                    await new ErrorXmlHandler(context, errorLog).ProcessRequestAsync();
                }
                else if (PathEquals(context, "json"))
                {
                    await new ErrorJsonHandler(context, errorLog).ProcessRequestAsync();
                }
                else if (PathEquals(context, "rss"))
                {
                    await new ErrorRssHandler(context, errorLog).ProcessRequestAsync();
                }
                else if (PathEquals(context, "digestrss"))
                {
                    await new ErrorDigestRssHandler(context, errorLog).ProcessRequestAsync();
                }
                else if (PathEquals(context, "download"))
                {
                    await new ErrorLogDownloadHandler(context, errorLog).ProcessRequestAsync();
                }
                else if (PathEquals(context, "stylesheet"))
                {
                    // TODO: Return specific style sheets and javascript files.
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "text/css";
                    context.Response.Write(Resources.ErrorLogStyle);
                }
                else
                {
                    // TODO: Not found if path doesn't exist.
                    // TODO: Correct Encoding.
                    await new ErrorLogView(context, errorLog).RenderAsync();
                }
            }
            catch (Exception e)
            {
                // TODO: Log this error to generic logger.
                Console.WriteLine(e);
            }
        }

        private static bool IsLocalIpAddress(IOwinContext owinContext)
        {
            return Convert.ToBoolean(owinContext.Environment["server.IsLocal"]);
        }

        private bool PathEquals(IOwinContext context, string segment)
        {
            return context.Request.Path.Equals(new PathString(options.Path.Value).Add(new PathString($"/{segment}")));
        }
    }
}
