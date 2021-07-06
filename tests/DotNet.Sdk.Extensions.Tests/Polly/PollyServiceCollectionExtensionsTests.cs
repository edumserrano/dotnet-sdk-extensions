using System;
using DotNet.Sdk.Extensions.Polly;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Registry;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly
{
    /// <summary>
    /// Tests for the <see cref="Extensions.Polly.PollyServiceCollectionExtensions"/> class
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    public class PollyServiceCollectionExtensionsTests
    {
        /// <summary>
        /// Validates the arguments for the <seealso cref="Extensions.Polly.PollyServiceCollectionExtensions.AddPolicyRegistry"/>
        /// extension method.
        /// </summary>
        [Fact]
        public void ValidateArguments()
        {
            var exception1 = Should.Throw<ArgumentNullException>(() =>
            {
                Extensions.Polly.PollyServiceCollectionExtensions.AddPolicyRegistry(
                    services: null!,
                    configureRegistry: (_, pairs) => { });
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

        /// <summary>
        /// Tests that the <seealso cref="Extensions.Polly.PollyServiceCollectionExtensions.AddPolicyRegistry"/>
        /// extension method adds the required Polly interfaces to the <see cref="ServiceCollection"/>.
        /// </summary>
        [Fact]
        public void AddsRequiredPollyRegistryToContainer()
        {
            var services = new ServiceCollection();
            services.AddPolicyRegistry((_, _) => { });
            var serviceProvider = services.BuildServiceProvider();
            var registry = serviceProvider.GetService<IPolicyRegistry<string>>();
            var readOnlyRegistry = serviceProvider.GetService<IReadOnlyPolicyRegistry<string>>();
            registry.ShouldNotBeNull();
            readOnlyRegistry.ShouldNotBeNull();
            ReferenceEquals(registry, readOnlyRegistry).ShouldBeTrue();
        }

        /// <summary>
        /// Tests that the <seealso cref="Extensions.Polly.PollyServiceCollectionExtensions.AddPolicyRegistry"/>
        /// extension method configures the Polly's policies registry as expected.
        /// </summary>
        [Fact]
        public void ConfigureRegistry()
        {
            const string policyKey = "testPolicy";
            var expectedPolicy = Policy.NoOp();
            var services = new ServiceCollection();
            services.AddPolicyRegistry((_, policyRegistry) =>
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
