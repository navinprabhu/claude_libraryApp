using LibraryApp.Shared.Events;
using LibraryApp.Shared.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace LibraryApp.Shared.Infrastructure.Services
{
    /// <summary>
    /// In-memory event publisher for local development and testing
    /// In production, this would be replaced with message queue implementation (RabbitMQ, Azure Service Bus, etc.)
    /// </summary>
    public class InMemoryEventPublisher : IEventPublisher
    {
        private readonly ILogger<InMemoryEventPublisher> _logger;
        private readonly List<BaseEvent> _publishedEvents;
        private readonly object _lock = new object();

        public InMemoryEventPublisher(ILogger<InMemoryEventPublisher> logger)
        {
            _logger = logger;
            _publishedEvents = new List<BaseEvent>();
        }

        public Task PublishAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : BaseEvent
        {
            if (eventData == null)
                throw new ArgumentNullException(nameof(eventData));

            try
            {
                // Ensure correlation ID is set
                if (string.IsNullOrEmpty(eventData.CorrelationId))
                {
                    eventData.CorrelationId = Guid.NewGuid().ToString();
                }

                // Serialize event for logging and potential persistence
                var eventJson = JsonSerializer.Serialize(eventData, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = false
                });

                lock (_lock)
                {
                    _publishedEvents.Add(eventData);
                }

                _logger.LogInformation(
                    "Event published: {EventType} with ID {EventId} and CorrelationId {CorrelationId}",
                    typeof(T).Name,
                    eventData.EventId,
                    eventData.CorrelationId);

                _logger.LogDebug("Event data: {EventData}", eventJson);

                // Simulate async operation
                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, 
                    "Failed to publish event {EventType} with ID {EventId}", 
                    typeof(T).Name, 
                    eventData.EventId);
                throw;
            }
        }

        public async Task PublishBatchAsync(IEnumerable<BaseEvent> events, CancellationToken cancellationToken = default)
        {
            if (events == null)
                throw new ArgumentNullException(nameof(events));

            var eventList = events.ToList();
            if (!eventList.Any())
                return;

            _logger.LogInformation("Publishing batch of {EventCount} events", eventList.Count);

            var tasks = eventList.Select(evt => PublishAsync(evt, cancellationToken));
            await Task.WhenAll(tasks);

            _logger.LogInformation("Successfully published batch of {EventCount} events", eventList.Count);
        }

        /// <summary>
        /// Gets all published events for testing purposes
        /// </summary>
        /// <returns>Read-only collection of published events</returns>
        public IReadOnlyList<BaseEvent> GetPublishedEvents()
        {
            lock (_lock)
            {
                return _publishedEvents.ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Gets published events of a specific type for testing purposes
        /// </summary>
        /// <typeparam name="T">The event type to filter by</typeparam>
        /// <returns>Read-only collection of published events of the specified type</returns>
        public IReadOnlyList<T> GetPublishedEvents<T>() where T : BaseEvent
        {
            lock (_lock)
            {
                return _publishedEvents.OfType<T>().ToList().AsReadOnly();
            }
        }

        /// <summary>
        /// Clears all published events for testing purposes
        /// </summary>
        public void ClearEvents()
        {
            lock (_lock)
            {
                _publishedEvents.Clear();
            }
            _logger.LogDebug("Cleared all published events");
        }
    }
}