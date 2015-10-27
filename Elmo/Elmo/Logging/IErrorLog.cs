using System.Collections.Generic;
using System.Threading.Tasks;

namespace Elmo.Logging
{
    public interface IErrorLog
    {
        string Log(Error error);

        Task<string> LogAsync(Error error);

        ErrorLogEntry GetError(string id);

        Task<ErrorLogEntry> GetErrorAsync(string id);

        IList<ErrorLogEntry> GetErrors(int pageIndex, int pageSize);

        Task<IList<ErrorLogEntry>> GetErrorsAsync(int pageIndex, int pageSize);

        int GetTotalErrorCount();

        Task<int> GetTotalErrorCountAsync();

        string Name { get; }

        string ApplicationName { get; }
    }
}
