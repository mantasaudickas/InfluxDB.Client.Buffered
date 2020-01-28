using System;
using Buffered.Channel;
using InfluxDB.Client.Buffered.Extensions.Metrics;
using InfluxDB.Client.Buffered.Extensions.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace InfluxDB.Client.Buffered.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddInfluxDbBufferedWriter(this IServiceCollection services, string url, string token, string bucket, string org, BufferedChannelOptions options = null)
        {
            var influxDbOptionsBuilder = InfluxDBClientOptions.Builder
                .CreateNew()
                .Url(url)
                .AuthenticateToken(token.ToCharArray())
                .Bucket(bucket)
                .Org(org);

            var clientOptions = influxDbOptionsBuilder.Build();

            return AddInfluxDbBufferedWriter(services, clientOptions, options);
        }

        public static IServiceCollection AddInfluxDbBufferedWriter(this IServiceCollection services, InfluxDBClientOptions clientOptions, BufferedChannelOptions options = null)
        {
            if (clientOptions == null) throw new ArgumentNullException(nameof(clientOptions));

            if (options == null)
            {
                options = new BufferedChannelOptions
                {
                    BufferSize = BufferedChannelOptions.DefaultBufferSize,
                    FlushInterval = BufferedChannelOptions.DefaultFlushInterval
                };
            }

            return services
                .AddTransient<IDefaultMetrics, DefaultMetrics>()
                .AddSingleton<IInfluxDbClientWriter>(p =>
                {
                    var client = InfluxDBClientFactory.Create(clientOptions);
                    return new InfluxDbClientWriter(client, options);
                });
        }

        public static IApplicationBuilder UseHttpMetrics(this IApplicationBuilder app)
        {
            return app.UseMiddleware<RequestDurationMiddleware>(); // should be first in the pipeline.
        }
    }
}
