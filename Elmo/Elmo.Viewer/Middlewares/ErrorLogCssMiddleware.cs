using System.Threading.Tasks;
using Elmo.Viewer.Properties;
using Microsoft.Owin;

namespace Elmo.Viewer.Middlewares
{
    internal class ErrorLogCssMiddleware : OwinMiddleware
    {
        private readonly ElmoViewerOptions options;

        public ErrorLogCssMiddleware(OwinMiddleware next, ElmoViewerOptions options) : base(next)
        {
            this.options = options;
        }

        public override Task Invoke(IOwinContext context)
        {
            PathString subPath;
            context.Request.Path.StartsWithSegments(options.Path, out subPath);
            if (!subPath.StartsWithSegments(new PathString("/stylesheet")))
            {
                return Next.Invoke(context);
            }

            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/css";
            context.Response.Write(Resources.ErrorLogStyle);

            return Task.FromResult<object>(null);
        }
    }
}
