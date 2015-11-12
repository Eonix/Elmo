using System.Threading.Tasks;
using Elmo.Logging;
using Microsoft.Owin;

namespace Elmo.Viewer.Responses
{
    internal interface IRequestHandler
    {
        Task ProcessRequestAsync(IOwinContext owinContext, IErrorLog errorLog);

        bool CanProcess(string path);
    }
}
