using Elmo.Logging.Loggers;
using Microsoft.Owin;
using Owin;

namespace Elmo
{
    public static class ElmoExtensions
    {
        public static IAppBuilder UseElmo(this IAppBuilder app)
        {
            return app.Use<ElmoMiddleware>(app, new MemoryErrorLog());
        }

        public static IAppBuilder UseElmoViewer(this IAppBuilder app)
        {
            return UseElmoViewer(app, new ElmoOptions());
        }

        public static IAppBuilder UseElmoViewer(this IAppBuilder app, string path)
        {
            return UseElmoViewer(app, new ElmoOptions { Path = new PathString(path) });
        }

        public static IAppBuilder UseElmoViewer(this IAppBuilder app, ElmoOptions options)
        {
            return app.Use<ElmoViewerMiddleware>(options, new MemoryErrorLog());
        }
    }
}
