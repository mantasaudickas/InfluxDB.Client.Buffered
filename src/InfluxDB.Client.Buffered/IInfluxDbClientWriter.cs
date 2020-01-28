using InfluxDB.Client.Writes;

namespace InfluxDB.Client.Buffered
{
    public interface IInfluxDbClientWriter
    {
        void Write(params PointData[] points);
    }
}