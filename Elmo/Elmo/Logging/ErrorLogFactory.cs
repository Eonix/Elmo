using Elmo.Logging.Loggers;
using Microsoft.Owin;

namespace Elmo.Logging
{
    public static class ErrorLogFactory
    {
        public static IErrorLog GetDefault(IOwinContext context)
        {
            // TODO: Get error log from some source.
            return new MemoryErrorLog();
        }
    }
}
