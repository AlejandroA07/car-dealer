#nullable enable

using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace WestcoastCars.Api.Observability;

public sealed class AppTelemetry : IDisposable
{
    public const string ActivitySourceName = "WestcoastCars.Api";
    public const string MeterName = "WestcoastCars.Api";

    private readonly ActivitySource _activitySource = new(ActivitySourceName);
    private readonly Meter _meter = new(MeterName);
    private readonly Counter<long> _vehicleOperationsCounter;
    private readonly Histogram<double> _vehicleOperationDurationMs;
    private readonly Counter<long> _serviceBookingOperationsCounter;
    private readonly Histogram<double> _serviceBookingDurationMs;
    private readonly Counter<long> _blocketSyncCounter;
    private readonly Histogram<double> _blocketSyncDurationMs;

    public AppTelemetry()
    {
        _vehicleOperationsCounter = _meter.CreateCounter<long>("westcoastcars.vehicle.operations");
        _vehicleOperationDurationMs = _meter.CreateHistogram<double>("westcoastcars.vehicle.operation.duration", unit: "ms");
        _serviceBookingOperationsCounter = _meter.CreateCounter<long>("westcoastcars.service_bookings.operations");
        _serviceBookingDurationMs = _meter.CreateHistogram<double>("westcoastcars.service_bookings.operation.duration", unit: "ms");
        _blocketSyncCounter = _meter.CreateCounter<long>("westcoastcars.blocket_sync.operations");
        _blocketSyncDurationMs = _meter.CreateHistogram<double>("westcoastcars.blocket_sync.duration", unit: "ms");
    }

    public Activity? StartVehicleActivity(string operation, string? registrationNumber = null, int? vehicleId = null)
    {
        var activity = _activitySource.StartActivity($"vehicle.{operation}", ActivityKind.Internal);
        activity?.SetTag("vehicle.operation", operation);

        if (!string.IsNullOrWhiteSpace(registrationNumber))
        {
            activity?.SetTag("vehicle.registration_number", registrationNumber);
        }

        if (vehicleId.HasValue)
        {
            activity?.SetTag("vehicle.id", vehicleId.Value);
        }

        return activity;
    }

    public Activity? StartServiceBookingActivity(string registrationNumber)
    {
        var activity = _activitySource.StartActivity("service-booking.create", ActivityKind.Internal);
        activity?.SetTag("service_booking.vehicle_registration_number", registrationNumber);
        return activity;
    }

    public Activity? StartBlocketSyncActivity(int requestedLimit)
    {
        var activity = _activitySource.StartActivity("vehicle.blocket-sync", ActivityKind.Internal);
        activity?.SetTag("blocket_sync.requested_limit", requestedLimit);
        return activity;
    }

    public void RecordVehicleOperation(string operation, string outcome, TimeSpan duration)
    {
        _vehicleOperationsCounter.Add(1,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("outcome", outcome));

        _vehicleOperationDurationMs.Record(duration.TotalMilliseconds,
            new KeyValuePair<string, object?>("operation", operation),
            new KeyValuePair<string, object?>("outcome", outcome));
    }

    public void RecordServiceBookingOperation(string outcome, TimeSpan duration)
    {
        _serviceBookingOperationsCounter.Add(1,
            new KeyValuePair<string, object?>("operation", "create"),
            new KeyValuePair<string, object?>("outcome", outcome));

        _serviceBookingDurationMs.Record(duration.TotalMilliseconds,
            new KeyValuePair<string, object?>("operation", "create"),
            new KeyValuePair<string, object?>("outcome", outcome));
    }

    public void RecordBlocketSync(string outcome, TimeSpan duration, int requestedLimit)
    {
        _blocketSyncCounter.Add(1,
            new KeyValuePair<string, object?>("outcome", outcome),
            new KeyValuePair<string, object?>("requested_limit", requestedLimit));

        _blocketSyncDurationMs.Record(duration.TotalMilliseconds,
            new KeyValuePair<string, object?>("outcome", outcome),
            new KeyValuePair<string, object?>("requested_limit", requestedLimit));
    }

    public void Dispose()
    {
        _activitySource.Dispose();
        _meter.Dispose();
    }
}
