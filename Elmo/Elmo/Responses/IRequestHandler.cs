using System.Threading.Tasks;
using Elmo.Logging;
using Microsoft.Owin;

namespace Elmo.Responses
{
    internal interface IRequestHandler
    {
        Task ProcessRequestAsync(IOwinContext owinContext, IErrorLog errorLog);

        bool CanProcess(string path);
    }
}
