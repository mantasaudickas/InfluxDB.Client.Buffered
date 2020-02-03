using System;
using System.Diagnostics;
using System.Net;
using System.Threading;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;

namespace InfluxDB.Client.Buffered.Extensions.Metrics
{
    public interface IDefaultMetrics
    {
        void PushRequestDuration(string method, string endpoint, int statusCode, TimeSpan duration);
        void PushProcessMetrics();
    }

    public class DefaultMetrics: IDefaultMetrics
    {
        private static readonly string MachineName = Dns.GetHostName();
        private static long _cpuTotal;

        private readonly IInfluxDbClientWriter _writer;
        private readonly Process _process;

        public DefaultMetrics(IInfluxDbClientWriter writer)
        {
            _writer = writer;
            _process = Process.GetCurrentProcess();
        }

        public void PushRequestDuration(string method, string endpoint, int statusCode, TimeSpan duration)
        {
            var now = DateTime.UtcNow;

            var point = PointData
                .Measurement("request")
                .Tag("host", MachineName)
                .Tag("method", method)
                .Tag("requestPath", endpoint)
                .Tag("statusCode", statusCode.ToString())
                .Field("duration", duration.TotalMilliseconds)
                .Timestamp(now, WritePrecision.Ns);

            _writer.Write(point);
        }

        public void PushProcessMetrics()
        {
            var now = DateTime.UtcNow;

            try
            {
                _process.Refresh();

                var totalProcessorTime = (long) _process.TotalProcessorTime.TotalSeconds;
                Interlocked.Add(ref _cpuTotal, Math.Max(0, totalProcessorTime - _cpuTotal));

                var points = new[]
                {
                    PointData
                        .Measurement("process").Tag("host", MachineName)
                        .Field("totalMemory", GC.GetTotalMemory(false))
                        .Timestamp(now, WritePrecision.Ns),
                    PointData
                        .Measurement("process").Tag("host", MachineName)
                        .Field("virtualMemorySize", _process.VirtualMemorySize64)
                        .Timestamp(now, WritePrecision.Ns),
                    PointData
                        .Measurement("process").Tag("host", MachineName)
                        .Field("workingSet", _process.WorkingSet64)
                        .Timestamp(now, WritePrecision.Ns),
                    PointData
                        .Measurement("process").Tag("host", MachineName)
                        .Field("privateMemorySize", _process.PrivateMemorySize64)
                        .Timestamp(now, WritePrecision.Ns),
                    PointData
                        .Measurement("process").Tag("host", MachineName)
                        .Field("totalProcessorTime", totalProcessorTime)
                        .Timestamp(now, WritePrecision.Ns),
                    PointData
                        .Measurement("process").Tag("host", MachineName)
                        .Field("cpuTotal", totalProcessorTime)
                        .Timestamp(now, WritePrecision.Ns),
                    PointData
                        .Measurement("process").Tag("host", MachineName)
                        .Field("handleCount", _process.HandleCount)
                        .Timestamp(now, WritePrecision.Ns),
                    PointData
                        .Measurement("process").Tag("host", MachineName)
                        .Field("threadCount", _process.Threads.Count)
                        .Timestamp(now, WritePrecision.Ns),
                };

                _writer.Write(points);
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}
