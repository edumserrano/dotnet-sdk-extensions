using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    /// <summary>
    /// Delegate to decide when the RunUntil operation should be terminated.
    /// </summary>
    /// <returns>True when the host should be terminated, false otherwise.</returns>
    public delegate Task<bool> RunUntilPredicateAsync();

    /// <summary>
    /// Delegate to decide when the RunUntil operation should be terminated.
    /// </summary>
    /// <returns>True when the host should be terminated, false otherwise.</returns>
    public delegate bool RunUntilPredicate();
}
