﻿using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using System;
using System.Threading.Tasks;

namespace raccoonLog.Http
{
    public class DefaultHttpRequestLogHandler : IHttpRequestLogHandler
    {
        private readonly IHttpRequestLogFormHandler _formContentHandler;

        private readonly IHttpLogMessageFactory _logMessageFactory;

        private readonly IHttpRequestLogBodyHandler _bodyHandler;

        private readonly IHttpRequestLogAgentHandler _logAgentHandler;

        public DefaultHttpRequestLogHandler(IHttpLogMessageFactory logMessageFactory,
            IHttpRequestLogFormHandler formContentHandler,
            IHttpRequestLogBodyHandler bodyHandler,
            IHttpRequestLogAgentHandler logAgentHandler)
        {
            _formContentHandler = formContentHandler;
            _logMessageFactory = logMessageFactory;
            _logAgentHandler = logAgentHandler;
            _bodyHandler = bodyHandler;
        }

        public async Task<HttpRequestLog> Handle(HttpRequest request)
        {
            if (request == null)
            {
                throw new NullReferenceException(nameof(request));
            }

            request.EnableBuffering();

            var logMessage = await CreateLogMessage();

            if (logMessage == null)
            {
                throw new NullReferenceException(nameof(logMessage));
            }

            logMessage.Method = request.Method;

            logMessage.SetParameters(request.Query);

            logMessage.SetCookies(request.Cookies);

            logMessage.SetUrl(request.GetEncodedUrl(), request.Protocol);

            await _logAgentHandler.Handle(request, logMessage);

            if (request.HasFormContentType)
            {
                await _formContentHandler.Handle(request, logMessage);
            }
            else
            {
                await _bodyHandler.Handle(request.Body, logMessage);
            }

            return logMessage;
        }


        private Task<HttpRequestLog> CreateLogMessage()
        {
            return _logMessageFactory.Create<HttpRequestLog>();
        }
    }
}
