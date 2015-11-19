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
            return UseElmoViewer(app, new ElmoViewerOptions());
        }

        public static IAppBuilder UseElmoViewer(this IAppBuilder app, string path)
        {
            return UseElmoViewer(app, new ElmoViewerOptions { Path = new PathString(path) });
        }

        public static IAppBuilder UseElmoViewer(this IAppBuilder app, ElmoViewerOptions options)
        {
            if (!app.Properties.ContainsKey(ElmoConstants.ErrorLogPropertyKey) || !(app.Properties[ElmoConstants.ErrorLogPropertyKey] is IErrorLog))
                throw new InvalidOperationException("Can't use the Elmo Viewer without registering an Error Log first.");

            if (options.Path == null || !options.Path.HasValue)
                throw new InvalidOperationException("Can't use the Elmo Viewer without a root path.");

            return app.MapWhen(context => context.Request.Path.StartsWithSegments(options.Path), builder => Configuration(builder, options));
        }

        private static void Configuration(IAppBuilder appBuilder, ElmoViewerOptions options)
        {
            var errorLog = (IErrorLog) appBuilder.Properties[ElmoConstants.ErrorLogPropertyKey];
            appBuilder.Use<RemoteAccessErrorMiddleware>(options);
            appBuilder.Use<ErrorJsonMiddleware>(options, errorLog);
            appBuilder.Use<ErrorRssMiddleware>(options, errorLog);
            appBuilder.Use<ErrorDigestRssMiddleware>(options, errorLog);
            appBuilder.Use<ErrorLogDownloadMiddleware>(options, errorLog);
            appBuilder.Use<ErrorLogCssMiddleware>(options);
            appBuilder.Use<ErrorDetailViewMiddleware>(options, errorLog);
            appBuilder.Use<ErrorLogViewMiddleware>(options, errorLog);
            appBuilder.Use<NotFoundErrorMiddleware>();
        }
    }
}
