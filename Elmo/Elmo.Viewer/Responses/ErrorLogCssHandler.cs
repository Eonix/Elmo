using System.Threading.Tasks;
using Elmo.Logging;
using Elmo.Viewer.Properties;
using Microsoft.Owin;

namespace Elmo.Viewer.Responses
{
    internal class ErrorLogCssHandler : IRequestHandler
    {
        public Task ProcessRequestAsync(IOwinContext owinContext, IErrorLog errorLog)
        {
            // TODO: Return specific style sheets and javascript files.
            owinContext.Response.StatusCode = 200;
            owinContext.Response.ContentType = "text/css";
            owinContext.Response.Write(Resources.ErrorLogStyle);

            return Task.FromResult<object>(null);
        }

        public bool CanProcess(string path)
        {
            return path.StartsWith("/stylesheet");
        }
    }
}
