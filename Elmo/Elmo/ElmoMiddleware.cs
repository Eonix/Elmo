using System;
using System.Threading.Tasks;
using Elmo.Logging;
using Microsoft.Owin;
using Microsoft.Owin.Logging;
using Owin;

namespace Elmo
{
    public class ElmoMiddleware : OwinMiddleware
    {
        private readonly IErrorLog errorLog;
        private readonly ILogger logger;

        public ElmoMiddleware(OwinMiddleware next, IAppBuilder app, IErrorLog errorLog)
            : base(next)
        {
            this.errorLog = errorLog;
            logger = app.CreateLogger<ElmoMiddleware>();
        }

        public override async Task Invoke(IOwinContext context)
        {
            try
            {
                await Next.Invoke(context);
                var exception = context.Get<Exception>(ElmoConstants.ExceptionKey);
                if (exception != null)
                    throw exception;
            }
            catch (Exception exception)
            {
                try
                {
                    await errorLog.LogAsync(new Error(exception, context));
                }
                catch (Exception e)
                {
                    logger.WriteError("An error occured while logging the error in Elmo.", e);
                }
            }
        }
    }
}
