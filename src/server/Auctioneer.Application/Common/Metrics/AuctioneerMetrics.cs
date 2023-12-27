using System.Diagnostics.Metrics;

namespace Auctioneer.Application.Common.Metrics;

public class AuctioneerMetrics
{
    private const string MeterName = "Auctioneer.API";

    private readonly Counter<long> _auctioneerRequestCounter;
    private readonly Histogram<double> _auctioneerRequestDuration;

    public AuctioneerMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        _auctioneerRequestCounter = meter.CreateCounter<long>(
            "auctioneer.api.auctioneer_requests_count");

        _auctioneerRequestDuration = meter.CreateHistogram<double>(
            "auctioneer.api.auctioneer.requests.duration", "ms");
    }

    public void IncreaseAuctioneerRequestCount()
    {
        _auctioneerRequestCounter.Add(1);
    }

    public TrackedRequestDuration MeasureRequestDuration()
    {
        return new TrackedRequestDuration(_auctioneerRequestDuration);
    }
}

public class TrackedRequestDuration(Histogram<double> histogram) : IDisposable
{
    private readonly long _requestStartTime = TimeProvider.System.GetTimestamp();

    public void Dispose()
    {
        var elapsedTime = TimeProvider.System.GetElapsedTime(_requestStartTime);
        histogram.Record(elapsedTime.TotalMilliseconds);
        GC.SuppressFinalize(this);
    }
}