using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout.Events
{
    /// <summary>
    /// Defines the events produced by the HttpClient's timeout policy.
    /// </summary>
    public interface ITimeoutPolicyEventHandler
    {
        /// <summary>
        /// Triggered when a timeout occurs.
        /// </summary>
        /// <param name="timeoutEvent">Event data from the Polly's timeout policy.</param>
        /// <returns>A task that represents the completion of handling the event.</returns>
        Task OnTimeoutAsync(TimeoutEvent timeoutEvent);
    }
}
