using Xunit;
using DotNet.Sdk.Extensions.Polly;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly
{
    [Trait("Category", "Polly")]
    public class PollyServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddsRequiredPollyRegistryToContainer()
        {
            var services = new ServiceCollection();
            services.AddPolicyRegistry((provider, policyRegistry) => { });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetService<IPolicyRegistry<string>>();
            var readOnlyRegistry = serviceProvider.GetService<IReadOnlyPolicyRegistry<string>>();
            registry.ShouldNotBeNull();
            readOnlyRegistry.ShouldNotBeNull();
            ReferenceEquals(registry, readOnlyRegistry).ShouldBeTrue();
        }

        [Fact]
        public void ConfigureRegistry()
        {
            var policyKey = "testPolicy";
            var expectedPolicy = Policy.NoOp();
            var services = new ServiceCollection();
            services.AddPolicyRegistry((provider, policyRegistry) =>
            {
                policyRegistry.Add(key: policyKey, expectedPolicy);
            });
            
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetRequiredService<IReadOnlyPolicyRegistry<string>>();
            registry.TryGet<IsPolicy>(policyKey, out var policy).ShouldBeTrue();
            ReferenceEquals(expectedPolicy,policy).ShouldBeTrue();
        }
    }
}
