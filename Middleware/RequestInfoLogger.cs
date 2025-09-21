using System.Diagnostics;


namespace ResearchScrapper.Api.MiddleWare
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            await _next(context);
            stopwatch.Stop();

            var statusCode = context.Response.StatusCode;
            var method = context.Request.Method;
            var path = context.Request.Path;

            _logger.LogInformation(
                $"Request: {method} {path} | Status: {statusCode} | Duration: {stopwatch.ElapsedMilliseconds}ms"
            );
        }
    }
}
