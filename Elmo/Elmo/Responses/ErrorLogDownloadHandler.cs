using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Elmo.Logging;
using Microsoft.Owin;

namespace Elmo.Responses
{
    internal class ErrorLogDownloadHandler
    {
        private readonly IOwinContext owinContext;
        private readonly IErrorLog errorLog;

        public ErrorLogDownloadHandler(IOwinContext owinContext, IErrorLog errorLog)
        {
            this.owinContext = owinContext;
            this.errorLog = errorLog;
        }

        public async Task ProcessRequestAsync()
        {
            const int defaultPageSize = 100;
            var pageIndex = 0;
            var count = 0;

            var total = await errorLog.GetTotalErrorCountAsync();
            var limit = Convert.ToInt32(owinContext.Request.Query["limit"]);
            var maxDownloadCount = limit > 0 ? Math.Min(total, limit) : total;
            var requestUrl = owinContext.Request.Uri;

            owinContext.Response.ContentType = "text/csv; header=present";
            owinContext.Response.Headers.Append("Content-Disposition", "attachment; filename=errorlog.csv");
            owinContext.Response.StatusCode = 200;
            owinContext.Response.ReasonPhrase = "Ok";

            using (var writer = new StreamWriter(owinContext.Response.Body))
            {
                await writer.WriteLineAsync("Application,Host,Time,Type,Source,User,Status Code,Message,URL,JSONREF");

                do
                {
                    var pageSize = Math.Min(maxDownloadCount - count, defaultPageSize);
                    var errorLogEntries = await errorLog.GetErrorsAsync(pageIndex++, pageSize);
                    count += errorLogEntries.Count;

                    foreach (var errorLogEntry in errorLogEntries)
                    {
                        var error = errorLogEntry.Error;
                        var time = error.Time.ToUniversalTime();
                        var query = $"?id={errorLogEntry.Id}";
                        await writer.WriteLineAsync($"{error.ApplicationName},{error.HostName},{time.ToString("yyyy-MM-dd HH:mm:ss")},{error.TypeName},{error.Source},{error.User},{error.StatusCode},{error.Message},{new Uri(requestUrl, "detail" + query)},{new Uri(requestUrl, "json" + query)}");
                    }
                } while (count < maxDownloadCount);
            }
        }
    }
}
