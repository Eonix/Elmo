using System;
using System.Net.Http;
using System.Web.Http.Filters;

namespace Elmo.Mvc
{
    public class ElmoExceptionFilterAttribute : ExceptionFilterAttribute
    {
        public override void OnException(HttpActionExecutedContext actionExecutedContext)
        {
            var owinContext = actionExecutedContext.Request.GetOwinContext();
            if (owinContext == null)
                throw new InvalidOperationException("Owin Context is not available.");

            owinContext.Set(ElmoConstants.ExceptionKey, actionExecutedContext.Exception);
            base.OnException(actionExecutedContext);
        }
    }
}
