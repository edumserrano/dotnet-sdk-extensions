using System;
using Xunit;
using DotNet.Sdk.Extensions.Polly;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Shouldly;

namespace DotNet.Sdk.Extensions.Tests.Polly
{
    [Trait("Category", XUnitCategories.Polly)]
    public class PollyServiceCollectionExtensionsTests
    {
        [Fact]
        public void ValidateArguments()
        {
            var exception1 = Should.Throw<ArgumentNullException>(() =>
            {
                Extensions.Polly.PollyServiceCollectionExtensions.AddPolicyRegistry(
                    services: null!,
                    configureRegistry: (provider, pairs) => { });
            });
            exception1.Message.ShouldBe("Value cannot be null. (Parameter 'services')");

            var exception2 = Should.Throw<ArgumentNullException>(() =>
            {
                Extensions.Polly.PollyServiceCollectionExtensions.AddPolicyRegistry(
                    services: new ServiceCollection(),
                    configureRegistry: null!);
            });
            exception2.Message.ShouldBe("Value cannot be null. (Parameter 'configureRegistry')");
        }

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
            ReferenceEquals(expectedPolicy, policy).ShouldBeTrue();
        }
    }
}
