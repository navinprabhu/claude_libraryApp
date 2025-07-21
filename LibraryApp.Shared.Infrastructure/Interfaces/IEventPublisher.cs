using LibraryApp.Shared.Events;

namespace LibraryApp.Shared.Infrastructure.Interfaces
{
    /// <summary>
    /// Interface for publishing events across services
    /// </summary>
    public interface IEventPublisher
    {
        /// <summary>
        /// Publishes an event asynchronously
        /// </summary>
        /// <typeparam name="T">The event type that inherits from BaseEvent</typeparam>
        /// <param name="eventData">The event data to publish</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task PublishAsync<T>(T eventData, CancellationToken cancellationToken = default) where T : BaseEvent;

        /// <summary>
        /// Publishes multiple events in a batch
        /// </summary>
        /// <param name="events">Collection of events to publish</param>
        /// <param name="cancellationToken">Cancellation token for the operation</param>
        /// <returns>A task representing the asynchronous operation</returns>
        Task PublishBatchAsync(IEnumerable<BaseEvent> events, CancellationToken cancellationToken = default);
    }
}