using System;

namespace InfluxDB.Client.Buffered.Extensions
{
    public class ProcessMetricsOptions
    {
        public TimeSpan PublishInterval { get; set; } = TimeSpan.FromSeconds(15);
    }
}
