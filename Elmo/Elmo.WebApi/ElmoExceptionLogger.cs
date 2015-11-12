using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;
using Elmo.Logging;

namespace Elmo.WebApi
{
    public class ElmoExceptionLogger : ExceptionLogger
    {
        private readonly IErrorLog errorLog;

        public ElmoExceptionLogger(IErrorLog errorLog)
        {
            this.errorLog = errorLog;
        }

        public override async Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            // TODO: Find out if we instead of logging to an instance should save the exception into Owin Environment and fetch it in the ElmoMiddleware.
            // Ref: http://stackoverflow.com/questions/31744146/replace-iexceptionhandler-in-web-api-2-2-with-owin-middleware-exception-handler
            await errorLog.LogAsync(new Error(context.Exception, context.Request.GetOwinContext()));
            await base.LogAsync(context, cancellationToken);
        }
    }
}
