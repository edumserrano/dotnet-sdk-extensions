using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Retry.Events
{
    /// <summary>
    /// Defines the events produced by the HttpClient's retry policy
    /// </summary>
    public interface IRetryPolicyEventHandler
    {
        /// <summary>
        /// Triggered when a retry occurs.
        /// </summary>
        /// <param name="retryEvent">Event data from the Polly's retry policy.</param>
        /// <returns>A task that represents the completion of handling the event.</returns>
        Task OnRetryAsync(RetryEvent retryEvent);
    }
}
