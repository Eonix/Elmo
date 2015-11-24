using System;
using System.Threading.Tasks;
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

            return app.Map(options.Path, builder => Configuration(builder, options));
        }

        private static void Configuration(IAppBuilder appBuilder, ElmoViewerOptions options)
        {
            var errorLog = (IErrorLog) appBuilder.Properties[ElmoConstants.ErrorLogPropertyKey];
            appBuilder.Use<RemoteAccessErrorMiddleware>(options);
            appBuilder.Map("/test", app => app.Run(TestHandler));
            appBuilder.Map("/json", app => app.Use<ErrorJsonMiddleware>(errorLog));
            appBuilder.Map("/rss", app => app.Use<ErrorRssMiddleware>(errorLog));
            appBuilder.Map("/digestrss", app => app.Use<ErrorDigestRssMiddleware>(errorLog));
            appBuilder.Map("/download", app => app.Use<ErrorLogDownloadMiddleware>(errorLog));
            appBuilder.Map("/stylesheet", app => app.Use<ErrorLogCssMiddleware>());
            appBuilder.Map("/detail", app => app.Use<ErrorDetailViewMiddleware>(options, errorLog));
            appBuilder.MapWhen(context => string.IsNullOrWhiteSpace(context.Request.Path.Value.Trim('/')), app => app.Use<ErrorLogViewMiddleware>(options, errorLog));
            appBuilder.Use<NotFoundErrorMiddleware>();
        }

        private static Task TestHandler(IOwinContext owinContext)
        {
            throw new Exception();
        }
    }
}
