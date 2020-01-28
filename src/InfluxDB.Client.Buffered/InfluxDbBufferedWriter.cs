using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buffered.Channel;
using InfluxDB.Client.Writes;

namespace InfluxDB.Client.Buffered
{
    public class InfluxDbClientWriter: IInfluxDbClientWriter, IDisposable
    {
        private readonly WriteApi _api;
        private readonly IBufferedChannel<PointData> _channel;

        public InfluxDbClientWriter(InfluxDBClient client, BufferedChannelOptions options)
        {
            _api = client.GetWriteApi();
            _channel = new BufferedChannel<PointData>(options);
            _channel.RegisterConsumer(SendDataPoints);
        }

        public void Write(params PointData[] points)
        {
            if (points?.Length > 0)
            {
                foreach (var point in points)
                {
                    _channel.TryWrite(point);
                }
            }
        }

        private Task SendDataPoints(IList<PointData> points)
        {
            _api.WritePoints(points.ToArray());
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            _channel?.Dispose();
            _api?.Dispose();
        }
    }
}
