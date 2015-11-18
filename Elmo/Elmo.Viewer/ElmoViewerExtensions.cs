using System;
using Elmo.Logging;
using Elmo.Viewer.Middlewares;
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
            if (!app.Properties.ContainsKey(ElmoConstants.ErrorLogPropertyKey))
                throw new InvalidOperationException("Can't use the Elmo Viewer without registering an Error Logger first.");

            var errorLog = app.Properties[ElmoConstants.ErrorLogPropertyKey] as IErrorLog;
            if (errorLog == null)
                throw new InvalidOperationException("Can't use the Elmo Viewer without registering an Error Logger first.");

            return app.MapWhen(context => context.Request.Path.StartsWithSegments(options.Path), builder => Configuration(builder, options));
        }

        private static void Configuration(IAppBuilder appBuilder, ElmoOptions options)
        {
            var errorLog = appBuilder.Properties[ElmoConstants.ErrorLogPropertyKey] as IErrorLog;
            appBuilder.Use<RemoteAccessErrorMiddleware>(options);
            appBuilder.Use<ErrorJsonMiddleware>(options, errorLog);
            appBuilder.Use<ErrorRssMiddleware>(options, errorLog);
            appBuilder.Use<ElmoViewerMiddleware>(options, errorLog);
        }
    }
}
