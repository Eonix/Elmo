using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Elmo.Logging;
using Elmo.Viewer.Responses;
using Elmo.Viewer.Responses.Views;
using Microsoft.Owin;

namespace Elmo.Viewer
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

            PathString subPath;
            context.Request.Path.StartsWithSegments(options.Path, out subPath);

            // TODO: Ensure that the same handlers are not invoked by multiple threads.
            var firstOrDefault = handlers.FirstOrDefault(handler => handler.CanProcess(subPath.Value));

            if (firstOrDefault != null)
                await firstOrDefault.ProcessRequestAsync(context, errorLog);
            else
                await new NotFoundErrorView().ProcessRequestAsync(context, errorLog);
        }
    }
}
