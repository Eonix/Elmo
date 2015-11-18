using Microsoft.Owin;

namespace Elmo.Logging
{
    public static class ErrorLogFactory
    {
        public static IErrorLog GetDefault(IOwinContext context)
        {
            return context.Get<IErrorLog>(ElmoConstants.ErrorLogPropertyKey);
        }
    }
}
