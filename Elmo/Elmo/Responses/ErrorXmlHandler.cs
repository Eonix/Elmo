using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Elmo.Logging;
using Microsoft.Owin;
using Newtonsoft.Json;

namespace Elmo.Responses
{
    internal class ErrorXmlHandler
    {
        private readonly IOwinContext owinContext;
        private readonly IErrorLog errorLog;

        public ErrorXmlHandler(IOwinContext owinContext, IErrorLog errorLog)
        {
            this.owinContext = owinContext;
            this.errorLog = errorLog;
        }

        public async Task ProcessRequestAsync()
        {
            owinContext.Response.ContentType = "application/xml";

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
            {
                // TODO: Find out how to serialize error without making the type mutable.
                // Maybe create an XML type specifically for this handler so we can convert Error into XmlError?
                // Maybe use the XmlWriter and do manual writing like Elmah does?

                //var xmlSerializer = new XmlSerializer(errorLogEntry.Error.GetType());
                //xmlSerializer.Serialize(streamWriter, errorLogEntry.Error);
            }
        }
    }
}
