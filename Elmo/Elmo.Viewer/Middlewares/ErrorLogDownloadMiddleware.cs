using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Elmo.Logging;
using Microsoft.Owin;

namespace Elmo.Viewer.Middlewares
{
    internal class ErrorLogDownloadMiddleware : OwinMiddleware
    {
        private readonly IErrorLog errorLog;

        public ErrorLogDownloadMiddleware(OwinMiddleware next, IErrorLog errorLog) : base(next)
        {
            this.errorLog = errorLog;
        }

        public override async Task Invoke(IOwinContext context)
        {
            const int defaultPageSize = 100;
            var pageIndex = 0;
            var count = 0;

            var total = await errorLog.GetTotalErrorCountAsync();
            var limit = Convert.ToInt32(context.Request.Query["limit"]);
            var maxDownloadCount = limit > 0 ? Math.Min(total, limit) : total;
            var requestUrl = context.Request.Uri;

            context.Response.ContentType = "text/csv; header=present";
            context.Response.Headers.Set("Content-Disposition", "attachment; filename=errorlog.csv");
            context.Response.StatusCode = 200;

            using (var writer = new StreamWriter(context.Response.Body, Encoding.UTF8))
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
