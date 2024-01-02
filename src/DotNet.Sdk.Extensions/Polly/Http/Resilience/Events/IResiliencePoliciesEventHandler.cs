namespace DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;

/// <summary>
/// Defines the events produced by the HttpClient's resilience policies.
/// </summary>
#pragma warning disable RCS1251 // Remove unnecessary braces from record declaration
// Shouldn't have to disable RCS1251 but if I fix it then the StyleCopAnalyzers starts
// crashing. For now I'm ignoring RCS1251 hoping the StyleCopAnalyzers get updated
// soon, if not, consider removing the StyleCopAnalyzers
public interface IResiliencePoliciesEventHandler :
    ITimeoutPolicyEventHandler,
    IRetryPolicyEventHandler,
    ICircuitBreakerPolicyEventHandler,
    IFallbackPolicyEventHandler
{
}
#pragma warning restore RCS1251 // Remove unnecessary braces from record declaration
