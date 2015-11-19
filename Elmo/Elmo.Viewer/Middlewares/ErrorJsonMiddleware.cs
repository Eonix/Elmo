using System.IO;
using System.Text;
using System.Threading.Tasks;
using Elmo.Logging;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Elmo.Viewer.Middlewares
{
    internal class ErrorJsonMiddleware : OwinMiddleware
    {
        private readonly ElmoViewerOptions options;
        private readonly IErrorLog errorLog;

        public ErrorJsonMiddleware(OwinMiddleware next, ElmoViewerOptions options, IErrorLog errorLog) : base(next)
        {
            this.options = options;
            this.errorLog = errorLog;
        }

        public override async Task Invoke(IOwinContext context)
        {
            PathString subPath;
            context.Request.Path.StartsWithSegments(options.Path, out subPath);
            if (!subPath.StartsWithSegments(new PathString("/json")))
            {
                await Next.Invoke(context);
                return;
            }

            context.Response.ContentType = "application/json";

            var errorId = context.Request.Query["id"];
            if (string.IsNullOrEmpty(errorId))
            {
                context.Response.StatusCode = 404;
                return;
            }

            context.Response.StatusCode = 200;

            var errorLogEntry = await errorLog.GetErrorAsync(errorId);
            if (errorLogEntry == null)
            {
                context.Response.StatusCode = 404;
                return;
            }

            using (var streamWriter = new StreamWriter(context.Response.Body, Encoding.UTF8))
            using (var jsonTextWriter = new JsonTextWriter(streamWriter))
            {
                JsonSerializer.Create(new JsonSerializerSettings {Formatting = Formatting.Indented}).Serialize(jsonTextWriter, errorLogEntry.Error);
            }
        }
    }
}
