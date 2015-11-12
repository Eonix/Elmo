using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Elmo.Utilities;
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
        public Dictionary<string, string[]> Headers { get; }
        public Dictionary<string, string[]> Query { get; }
        public Dictionary<string, string> Cookies { get; }
        
        public Error(Exception exception, IOwinContext owinContext)
        {
            if (exception == null)
                throw new ArgumentNullException(nameof(exception));

            Exception = exception;
            var baseException = exception.GetBaseException();

            HostName = EnvironmentUtilities.GetMachineNameOrDefault();
            TypeName = baseException.GetType().FullName;
            Message = baseException.Message;
            Source = baseException.Source;
            Detail = exception.ToString();
            User = Thread.CurrentPrincipal.Identity.Name ?? string.Empty;
            Time = DateTimeOffset.Now;

            StatusCode = owinContext.Response.StatusCode;

            var webUser = owinContext.Authentication.User;
            if (!string.IsNullOrEmpty(webUser?.Identity?.Name))
            {
                User = webUser.Identity.Name;
            }

            ServerEnvironment = owinContext.Environment.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString() ?? string.Empty);
            Headers = owinContext.Request.Headers.ToDictionary(pair => pair.Key, pair => pair.Value);
            Query = owinContext.Request.Query.ToDictionary(pair => pair.Key, pair => pair.Value);
            Cookies = owinContext.Request.Cookies.ToDictionary(pair => pair.Key, pair => pair.Value);
            ApplicationName = AppDomain.CurrentDomain.FriendlyName;
        }
    }
}
