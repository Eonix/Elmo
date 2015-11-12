using System;
using Elmo.Logging;
using Elmo.Logging.Loggers;
using Owin;

namespace Elmo
{
    public static class ElmoExtensions
    {
        public static string ErrorLogPropertyKey => "Elmo.ErrorLog";

        public static IAppBuilder UseElmoMemoryLog(this IAppBuilder app)
        {
            return UseElmo(app, new MemoryErrorLog());
        }

        public static IAppBuilder UseElmo(this IAppBuilder app, IErrorLog errorLog)
        {
            if (errorLog == null)
                throw new ArgumentNullException(nameof(errorLog));

            app.Properties.Add(ErrorLogPropertyKey, errorLog);
            return app.Use<ElmoMiddleware>(app, errorLog);
        }
    }
}
