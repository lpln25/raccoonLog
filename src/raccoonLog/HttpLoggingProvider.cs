﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using raccoonLog.Handlers;

namespace raccoonLog
{
    public class HttpLoggingProvider : IHttpLoggingProvider
    {
        private readonly IHttpRequestLogHandler _requestHandler;
        private readonly IHttpResponseLogHandler _responseHandler;

        private readonly ILogger<HttpLoggingProvider> _logger;

        private readonly IHttpLoggingStore _store;

        private readonly IStoreQueue _storeQueue;

        private RaccoonLogHttpOptions _options;

        public HttpLoggingProvider(IHttpResponseLogHandler responseHandler,
            IHttpRequestLogHandler requestHandler,
            IOptions<RaccoonLogHttpOptions> options,
            ILogger<HttpLoggingProvider> logger,
            IHttpLoggingStore store, IStoreQueue storeQueue)
        {
            _store = store;
            _options = options.Value;
            _responseHandler = responseHandler;
            _requestHandler = requestHandler;
            _logger = logger;
            _storeQueue = storeQueue;
        }

        public async ValueTask LogAsync(HttpContext context, CancellationToken cancellationToken = default)
        {
            var requestLog = await _requestHandler.Handle(context.Request, cancellationToken);

            var responseLog = await _responseHandler.Handle(context.Response, cancellationToken);

            var logContext = new LogContext(context.TraceIdentifier, requestLog, responseLog, context.Request.Protocol);

            var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();

            if (exceptionFeature?.Error is Exception error)
            {
                logContext.SetError(error);
            }

            _logger.Log(_options.Level, default, logContext, logContext.Error, _options.Formatter);

            _storeQueue.Enqueue(() => _store.StoreAsync(logContext, CancellationToken.None));
        }
    }
}
