using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.ExceptionHandling;

namespace Elmo.WebApi
{
    public class ElmoExceptionLogger : ExceptionLogger
    {
        public override async Task LogAsync(ExceptionLoggerContext context, CancellationToken cancellationToken)
        {
            var owinContext = context.Request.GetOwinContext();
            if (owinContext == null)
                throw new InvalidOperationException("Owin Context is not available.");

            owinContext.Set(ElmoConstants.ExceptionKey, context.Exception);
            await base.LogAsync(context, cancellationToken);
        }
    }
}
