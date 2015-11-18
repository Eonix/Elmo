using System;
using Elmo.Loggers;
using Elmo.Logging;
using Owin;

namespace Elmo
{
    public static class ElmoExtensions
    {
        public static IAppBuilder UseElmoMemoryLog(this IAppBuilder app)
        {
            return UseElmo(app, new MemoryErrorLog());
        }

        public static IAppBuilder UseElmoMemoryLog(this IAppBuilder app, int logSize)
        {
            return UseElmo(app, new MemoryErrorLog(logSize));
        }

        public static IAppBuilder UseElmo(this IAppBuilder app, IErrorLog errorLog)
        {
            if (errorLog == null)
                throw new ArgumentNullException(nameof(errorLog));

            app.Properties.Add(ElmoConstants.ErrorLogPropertyKey, errorLog);
            return app.Use<ElmoMiddleware>(app, errorLog);
        }
    }
}
