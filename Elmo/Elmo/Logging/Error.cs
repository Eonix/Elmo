using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Owin;

namespace Elmo.Logging
{
    public sealed class Error
    {
        public string ApplicationName { get; }
        public Exception Exception { get; }
        public string HostName { get; }
        public string TypeName { get; }
        public string Message { get; }
        public string Source { get; }
        public string Detail { get; }
        public string User { get; }
        public DateTimeOffset Time { get; }
        public int StatusCode { get; }
        public Dictionary<string, string> ServerEnvironment { get; }
        public Dictionary<string, string[]> RequestHeaders { get; }
        public Dictionary<string, string[]> Query { get; }
        public Dictionary<string, string> Cookies { get; }
        
        public Error(Exception exception, IOwinContext owinContext)
        {
            this.Exception = exception;
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            var baseException = exception.GetBaseException();

            HostName = GetMachineName();
            TypeName = baseException.GetType().FullName;
            Message = baseException.Message;
            Source = baseException.Source;
            Detail = exception.ToString();
            User = Thread.CurrentPrincipal.Identity.Name ?? string.Empty;
            Time = DateTimeOffset.Now;

            StatusCode = owinContext.Response.StatusCode;
            // TODO: Generate an HTML Error Message if necessary (YSOD).
            // webHostHtmlMessage = TryGetHtmlErrorMessage(httpException);

            var webUser = owinContext.Authentication.User;
            if (!string.IsNullOrEmpty(webUser?.Identity?.Name))
            {
                User = webUser.Identity.Name;
            }

            ServerEnvironment = owinContext.Environment.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString() ?? string.Empty);
            RequestHeaders = owinContext.Request.Headers.ToDictionary(pair => pair.Key, pair => pair.Value);

            // TODO: Figure out if AUTH_PASSWORD is included and mask it
            //if (_serverVariables != null)
            //{
            //    // Hack for issue #140:
            //    // http://code.google.com/p/elmah/issues/detail?id=140

            //    const string authPasswordKey = "AUTH_PASSWORD";
            //    string authPassword = _serverVariables[authPasswordKey];
            //    if (authPassword != null) // yes, mask empty too!
            //        _serverVariables[authPasswordKey] = "*****";
            //}

            Query = owinContext.Request.Query.ToDictionary(pair => pair.Key, pair => pair.Value);

            // TODO: Figure out how to get form data from request.
            // owinContext.Request.ReadFormAsync()

            Cookies = owinContext.Request.Cookies.ToDictionary(pair => pair.Key, pair => pair.Value);

            ApplicationName = AppDomain.CurrentDomain.FriendlyName;
        }
        
        private static string GetMachineName()
        {
            try
            {
                return Environment.MachineName;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}
