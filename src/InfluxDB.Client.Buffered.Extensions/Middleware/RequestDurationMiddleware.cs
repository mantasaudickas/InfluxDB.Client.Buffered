using System.Diagnostics;
using System.Threading.Tasks;
using InfluxDB.Client.Buffered.Extensions.Metrics;
using Microsoft.AspNetCore.Http;

namespace InfluxDB.Client.Buffered.Extensions.Middleware
{
    public class RequestDurationMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestDurationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IDefaultMetrics metrics)
        {
            var timer = Stopwatch.StartNew();
            try
            {
                await _next(context);
            }
            finally
            {
                var request = context.Request;
                var response = context.Response;

                var method = request?.Method ?? "N/A";
                var requestPath = request?.Path ?? "N/A";
                int statusCode = response?.StatusCode ?? 0;

                metrics.PushRequestDuration(method, requestPath, statusCode, timer.Elapsed);
                metrics.PushProcessMetrics();
            }
        }
    }
}
