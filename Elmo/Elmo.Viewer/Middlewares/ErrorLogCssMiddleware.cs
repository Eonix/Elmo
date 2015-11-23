using System.Threading.Tasks;
using Elmo.Viewer.Properties;
using Microsoft.Owin;

namespace Elmo.Viewer.Middlewares
{
    internal class ErrorLogCssMiddleware : OwinMiddleware
    {
        public ErrorLogCssMiddleware(OwinMiddleware next) : base(next)
        {
        }

        public override Task Invoke(IOwinContext context)
        {
            context.Response.StatusCode = 200;
            context.Response.ContentType = "text/css";
            context.Response.Write(Resources.ErrorLogStyle);

            return Task.FromResult<object>(null);
        }
    }
}
