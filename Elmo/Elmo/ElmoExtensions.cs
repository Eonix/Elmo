using System;
using Elmo.Logging;
using Elmo.Logging.Loggers;
using Microsoft.Owin;
using Owin;

namespace Elmo
{
    public static class ElmoExtensions
    {
        public static string ErrorLogPropertyKey => "Elmo.ErrorLog";

        public static IAppBuilder UseElmoMemoryLog(this IAppBuilder app)
        {
            var memoryErrorLog = new MemoryErrorLog();
            app.Properties.Add(ErrorLogPropertyKey, memoryErrorLog);
            return UseElmo(app, memoryErrorLog);
        }

        public static IAppBuilder UseElmo(this IAppBuilder app, IErrorLog errorLog)
        {
            return app.Use<ElmoMiddleware>(app, errorLog);
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
            if (!app.Properties.ContainsKey(ErrorLogPropertyKey))
                throw new InvalidOperationException("Can't use the Elmo Viewer without registering an Error Logger first.");

            var errorLog = app.Properties[ErrorLogPropertyKey] as IErrorLog;
            if (errorLog == null)
                throw new InvalidOperationException("Can't use the Elmo Viewer without registering an Error Logger first.");

            return app.Use<ElmoViewerMiddleware>(app, options, errorLog);
        }
    }
}
