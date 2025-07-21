using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace LibraryApp.Shared.Infrastructure.Telemetry
{
    public class ServiceCallMetrics : IDisposable
    {
        private readonly Meter _meter;
        private readonly Counter<int> _serviceCallCounter;
        private readonly Histogram<double> _serviceCallDuration;
        private readonly Counter<int> _serviceCallErrors;
        private readonly Histogram<double> _serviceCallSize;

        public ServiceCallMetrics()
        {
            var version = typeof(ServiceCallMetrics).Assembly.GetName().Version?.ToString() ?? "1.0.0";
            _meter = new Meter("LibraryApp.ServiceCalls", version);

            _serviceCallCounter = _meter.CreateCounter<int>(
                name: "service_calls_total",
                description: "Total number of service calls");

            _serviceCallDuration = _meter.CreateHistogram<double>(
                name: "service_call_duration_ms",
                description: "Duration of service calls in milliseconds");

            _serviceCallErrors = _meter.CreateCounter<int>(
                name: "service_call_errors_total",
                description: "Total number of service call errors");

            _serviceCallSize = _meter.CreateHistogram<double>(
                name: "service_call_response_size_bytes",
                description: "Size of service call responses in bytes");
        }

        public void RecordServiceCall(string serviceName, string operation, string correlationId)
        {
            var tags = new TagList
            {
                new("service_name", serviceName),
                new("operation", operation),
                new("correlation_id", correlationId)
            };

            _serviceCallCounter.Add(1, tags);
        }

        public void RecordServiceCallDuration(string serviceName, string operation, double durationMs, bool success, string correlationId)
        {
            var tags = new TagList
            {
                new("service_name", serviceName),
                new("operation", operation),
                new("success", success.ToString().ToLower()),
                new("correlation_id", correlationId)
            };

            _serviceCallDuration.Record(durationMs, tags);

            if (!success)
            {
                _serviceCallErrors.Add(1, tags);
            }
        }

        public void RecordResponseSize(string serviceName, string operation, long sizeBytes, string correlationId)
        {
            var tags = new TagList
            {
                new("service_name", serviceName),
                new("operation", operation),
                new("correlation_id", correlationId)
            };

            _serviceCallSize.Record(sizeBytes, tags);
        }

        public void Dispose()
        {
            _meter?.Dispose();
        }
    }

    public interface IServiceCallMetrics
    {
        void RecordServiceCall(string serviceName, string operation, string correlationId);
        void RecordServiceCallDuration(string serviceName, string operation, double durationMs, bool success, string correlationId);
        void RecordResponseSize(string serviceName, string operation, long sizeBytes, string correlationId);
    }

    public class ServiceCallMetricsService : IServiceCallMetrics
    {
        private readonly ServiceCallMetrics _metrics;

        public ServiceCallMetricsService(ServiceCallMetrics metrics)
        {
            _metrics = metrics;
        }

        public void RecordServiceCall(string serviceName, string operation, string correlationId)
        {
            _metrics.RecordServiceCall(serviceName, operation, correlationId);
        }

        public void RecordServiceCallDuration(string serviceName, string operation, double durationMs, bool success, string correlationId)
        {
            _metrics.RecordServiceCallDuration(serviceName, operation, durationMs, success, correlationId);
        }

        public void RecordResponseSize(string serviceName, string operation, long sizeBytes, string correlationId)
        {
            _metrics.RecordResponseSize(serviceName, operation, sizeBytes, correlationId);
        }
    }
}