﻿using System.IO;
using System.Text;
using System.Threading.Tasks;
using Elmo.Logging;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Elmo.Responses
{
    internal class ErrorJsonHandler
    {
        private readonly IOwinContext owinContext;
        private readonly IErrorLog errorLog;

        public ErrorJsonHandler(IOwinContext owinContext, IErrorLog errorLog)
        {
            this.owinContext = owinContext;
            this.errorLog = errorLog;
        }

        public async Task ProcessRequestAsync()
        {
            owinContext.Response.ContentType = "application/json";

            var errorId = owinContext.Request.Query["id"];
            if (string.IsNullOrEmpty(errorId))
            {
                owinContext.Response.StatusCode = 404;
                owinContext.Response.ReasonPhrase = "Not Found";
                return;
            }

            owinContext.Response.StatusCode = 200;
            owinContext.Response.ReasonPhrase = "Ok";

            var errorLogEntry = await errorLog.GetErrorAsync(errorId);
            if (errorLogEntry == null)
            {
                owinContext.Response.StatusCode = 404;
                owinContext.Response.ReasonPhrase = "Not Found";
                return;
            }
            
            using (var streamWriter = new StreamWriter(owinContext.Response.Body, Encoding.UTF8))
            using (var jsonTextWriter = new JsonTextWriter(streamWriter))
            {
                JsonSerializer.Create().Serialize(jsonTextWriter, errorLogEntry.Error);
            }
        }
    }
}
