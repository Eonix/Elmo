using System;
using Elmo.Logging;
using Microsoft.Owin;
using Owin;

namespace Elmo.Viewer
{
    public static class ElmoViewerExtensions
    {
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
            if (!app.Properties.ContainsKey(ElmoExtensions.ErrorLogPropertyKey))
                throw new InvalidOperationException("Can't use the Elmo Viewer without registering an Error Logger first.");

            var errorLog = app.Properties[ElmoExtensions.ErrorLogPropertyKey] as IErrorLog;
            if (errorLog == null)
                throw new InvalidOperationException("Can't use the Elmo Viewer without registering an Error Logger first.");

            return app.Use<ElmoViewerMiddleware>(app, options, errorLog);
        }
    }
}
