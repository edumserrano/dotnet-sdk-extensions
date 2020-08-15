using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Testing.HostedServices
{
    public delegate Task<bool> RunUntilPredicateAsync();

    public delegate bool RunUntilPredicate();
}
