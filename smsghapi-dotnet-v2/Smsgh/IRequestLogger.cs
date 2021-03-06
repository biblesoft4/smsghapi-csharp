﻿using System.Net;

namespace smsghapi_dotnet_v2.Smsgh
{
    public interface IRequestLogger
    {
        bool IsLoggingEnabled();
        void Log(string mesg);

        /// <summary>
        ///     Log the HTTP request and content to be sent with the request.
        /// </summary>
        /// <param name="urlConnection">an open HttpWebRequest url connection</param>
        /// <param name="content">Content to log</param>
        void LogRequest(HttpWebRequest urlConnection, object content);

        /// <summary>
        ///     Logs the HTTP response.
        /// </summary>
        void LogResponse(HttpResponse response);
    }
}