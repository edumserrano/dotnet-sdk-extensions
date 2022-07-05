using Polly;

namespace DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;

/// <summary>
/// Contains the event data when a timeout is triggered via Polly's timeout policy.
/// </summary>
public class TimeoutEvent
{
    internal TimeoutEvent(
        string httpClientName,
        TimeoutOptions timeoutOptions,
        Context context,
        TimeSpan requestTimeout,
        Task timedOutTask,
        Exception exception)
    {
        HttpClientName = httpClientName;
        TimeoutOptions = timeoutOptions;
        Context = context;
        RequestTimeout = requestTimeout;
        TimedOutTask = timedOutTask;
        Exception = exception;
    }

    /// <summary>
    /// Gets the name of the HttpClient that triggered this event.
    /// </summary>
    public string HttpClientName { get; }

    /// <summary>
    /// Gets the timeout options applied to the HttpClient that triggered this event.
    /// </summary>
    public TimeoutOptions TimeoutOptions { get; }

    /// <summary>
    /// Gets the Polly Context.
    /// </summary>
    public Context Context { get; }

    /// <summary>
    /// Gets the timeout applied.
    /// </summary>
    public TimeSpan RequestTimeout { get; }

    /// <summary>
    /// Gets a <see cref="Task"/> capturing the abandoned, timed-out action.
    /// </summary>
    /// <remarks>
    /// This will be null if the executed action responded cooperatively to cancellation before the policy timed it out.
    /// </remarks>
    public Task? TimedOutTask { get; }

    /// <summary>
    /// Gets the captured exception.
    /// </summary>
    public Exception Exception { get; }
}
