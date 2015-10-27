﻿using static System.String;

namespace Elmo.Utilities
{
    internal static class HttpCodeParser
    {
        public static string GetstatusDescription(int code)
        {
            if (code < 100 || code >= 600)
                return Empty;

            var i = code/100;
            var j = code%100;

            return j < HttpStatusDescriptions[i].Length ? HttpStatusDescriptions[i][j] : Empty;
        }

        private static readonly string[][] HttpStatusDescriptions = {
            null,

            new[]
            {
                /* 100 */"Continue",
                /* 101 */ "Switching Protocols",
                /* 102 */ "Processing"
            },

            new[]
            {
                /* 200 */"OK", 
                /* 201 */ "Created",
                /* 202 */ "Accepted",
                /* 203 */ "Non-Authoritative Information",
                /* 204 */ "No Content", 
                /* 205 */ "Reset Content",
                /* 206 */ "Partial Content", 
                /* 207 */ "Multi-Status"
            },

            new[]
            {
                /* 300 */"Multiple Choices",
                /* 301 */ "Moved Permanently", 
                /* 302 */ "Found",
                /* 303 */ "See Other", 
                /* 304 */ "Not Modified", 
                /* 305 */ "Use Proxy",
                /* 306 */ Empty, 
                /* 307 */ "Temporary Redirect"
            },

            new[]
            {
                /* 400 */"Bad Request", 
                /* 401 */ "Unauthorized", 
                /* 402 */ "Payment Required",
                /* 403 */ "Forbidden", 
                /* 404 */ "Not Found",
                /* 405 */ "Method Not Allowed",
                /* 406 */ "Not Acceptable",
                /* 407 */ "Proxy Authentication Required", 
                /* 408 */ "Request Timeout",
                /* 409 */ "Conflict", 
                /* 410 */ "Gone", 
                /* 411 */ "Length Required",
                /* 412 */ "Precondition Failed", 
                /* 413 */ "Request Entity Too Large",
                /* 414 */ "Request-Uri Too Long",
                /* 415 */ "Unsupported Media Type",
                /* 416 */ "Requested Range Not Satisfiable", 
                /* 417 */ "Expectation Failed",
                /* 418 */ Empty, 
                /* 419 */ Empty, 
                /* 420 */ Empty,
                /* 421 */ Empty, 
                /* 422 */ "Unprocessable Entity",
                /* 423 */ "Locked",
                /* 424 */ "Failed Dependency"
            },

            new[]
            { 
                /* 500 */"Internal Server Error",
                /* 501 */ "Not Implemented", 
                /* 502 */ "Bad Gateway",
                /* 503 */ "Service Unavailable",
                /* 504 */ "Gateway Timeout",
                /* 505 */ "Http Version Not Supported", 
                /* 506 */ Empty,
                /* 507 */ "Insufficient Storage"
            }
        };
    }
}