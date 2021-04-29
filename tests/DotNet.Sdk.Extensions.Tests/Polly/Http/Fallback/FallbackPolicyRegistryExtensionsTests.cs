//using System;
//using System.Net.Http;
//using System.Threading.Tasks;
//using DotNet.Sdk.Extensions.Polly;
//using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
//using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
//using DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary;
//using Microsoft.Extensions.DependencyInjection;
//using NSubstitute;
//using Polly.CircuitBreaker;
//using Polly.Registry;
//using Polly.Timeout;
//using Polly.Wrap;
//using Shouldly;
//using Xunit;

//namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback
//{
//    /// <summary>
//    /// Tests for the <see cref="FallbackPolicyRegistryExtensions"/> class
//    /// </summary>
//    [Trait("Category", XUnitCategories.Polly)]
//    public class FallbackPolicyRegistryExtensionsTests
//    {
//        /// <summary>
//        /// Tests that the <see cref="FallbackPolicyRegistryExtensions.AddHttpClientFallbackPolicy(IPolicyRegistry{string},string,IServiceProvider)"/>
//        /// overload method adds the fallback policy to the Polly registry
//        /// </summary>
//        [Fact]
//        public void AddHttpClientFallbackPolicy()
//        {
//            var policyKey = "testPolicy";
//            var services = new ServiceCollection();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                policyRegistry.AddHttpClientFallbackPolicy(policyKey, provider);
//            });
//            var serviceProvider = services.BuildServiceProvider();
//            serviceProvider
//                .GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey)
//                .ShouldNotBeNull();
//        }

//        /// <summary>
//        /// Tests that the <see cref="FallbackPolicyRegistryExtensions.AddHttpClientFallbackPolicy{TPolicyConfiguration}(IPolicyRegistry{string},string,IServiceProvider)"/>
//        /// overload method adds the fallback policy to the Polly registry
//        /// </summary>
//        [Fact]
//        public void AddHttpClientFallbackPolicy2()
//        {
//            var policyKey = "testPolicy";
//            var services = new ServiceCollection();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                policyRegistry.AddHttpClientFallbackPolicy<TestFallbackPolicyConfiguration>(policyKey, provider);
//            });
//            var serviceProvider = services.BuildServiceProvider();
//            serviceProvider
//                .GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey)
//                .ShouldNotBeNull();
//        }

//        /// <summary>
//        /// Tests that the <see cref="FallbackPolicyRegistryExtensions.AddHttpClientFallbackPolicy(IPolicyRegistry{string},string,IFallbackPolicyConfiguration)"/>
//        /// overload method adds the fallback policy to the Polly registry
//        /// </summary>
//        [Fact]
//        public void AddHttpClientFallbackPolicy3()
//        {
//            var policyKey = "testPolicy";
//            var services = new ServiceCollection();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                var fallbackPolicyConfiguration = Substitute.For<IFallbackPolicyConfiguration>();
//                policyRegistry.AddHttpClientFallbackPolicy(policyKey, fallbackPolicyConfiguration);
//            }); 
//            var serviceProvider = services.BuildServiceProvider();
//            serviceProvider
//                .GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey)
//                .ShouldNotBeNull();
//        }

//        /// <summary>
//        /// Tests that the IPolicyRegistry.AddHttpClientFallbackPolicy method configures the
//        /// fallback policy to handle <see cref="TimeoutRejectedException"/>.
//        ///
//        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
//        /// to test that the policy's configuration is as expected.
//        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
//        /// seem to be possible.
//        /// </summary>
//        [Fact]
//        public async Task AddHttpClientFallbackPolicyTriggersOnTimeoutRejectedException()
//        {
//            var policyKey = "testPolicy";
//            var services = new ServiceCollection();
//            var fallbackPolicyConfiguration = Substitute.For<IFallbackPolicyConfiguration>();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                policyRegistry.AddHttpClientFallbackPolicy(policyKey, fallbackPolicyConfiguration);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            var fallbackPolicy = serviceProvider.GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);
//                var exception = new TimeoutRejectedException("test message");
//            var policyResult = await fallbackPolicy.ExecuteAndCaptureAsync(
//                () =>
//                {
//                    throw exception;
//                });
//            policyResult.FinalException.ShouldBeNull();
//            policyResult.Result.ShouldBeOfType<TimeoutHttpResponseMessage>();
//            var result = (TimeoutHttpResponseMessage)policyResult.Result;
//            result.Exception.ShouldBe(exception);
//        }

//        /// <summary>
//        /// Tests that the IPolicyRegistry.AddHttpClientFallbackPolicy method configures the
//        /// fallback policy to handle <see cref="BrokenCircuitException"/>.
//        ///
//        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
//        /// to test that the policy's configuration is as expected.
//        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
//        /// seem to be possible.
//        /// </summary>
//        [Fact]
//        public async Task AddHttpClientFallbackPolicyTriggersOnBrokenCircuitException()
//        {
//            var policyKey = "testPolicy";
//            var services = new ServiceCollection();
//            var fallbackPolicyConfiguration = Substitute.For<IFallbackPolicyConfiguration>();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                policyRegistry.AddHttpClientFallbackPolicy(policyKey, fallbackPolicyConfiguration);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            var fallbackPolicy = serviceProvider.GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);
//            var policyResult = await fallbackPolicy.ExecuteAndCaptureAsync(
//                () =>
//                {
//                    throw new BrokenCircuitException();
//                });
//            policyResult.FinalException.ShouldBeNull();
//            policyResult.Result.ShouldBeOfType<CircuitBrokenHttpResponseMessage>();
//        }

//        /// <summary>
//        /// Tests that the IPolicyRegistry.AddHttpClientFallbackPolicy method configures the
//        /// fallback policy to handle <see cref="IsolatedCircuitException"/>.
//        ///
//        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
//        /// to test that the policy's configuration is as expected.
//        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
//        /// seem to be possible.
//        /// </summary>
//        [Fact]
//        public async Task AddHttpClientFallbackPolicyTriggersOnIsolatedCircuitException()
//        {
//            var policyKey = "testPolicy";
//            var services = new ServiceCollection();
//            var fallbackPolicyConfiguration = Substitute.For<IFallbackPolicyConfiguration>();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                policyRegistry.AddHttpClientFallbackPolicy(policyKey, fallbackPolicyConfiguration);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            var fallbackPolicy = serviceProvider.GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);
//            var policyResult = await fallbackPolicy.ExecuteAndCaptureAsync(
//                () =>
//                {
//                    throw new IsolatedCircuitException("test");
//                });
//            policyResult.FinalException.ShouldBeNull();
//            policyResult.Result.ShouldBeOfType<CircuitBrokenHttpResponseMessage>();
//        }

//        /// <summary>
//        /// Tests that the IPolicyRegistry.AddHttpClientFallbackPolicy method configures the
//        /// fallback policy to handle <see cref="TaskCanceledException"/>.
//        ///
//        /// This might seem like I'm testing that polly does what it's supposed to do but I don't know another way
//        /// to test that the policy's configuration is as expected.
//        /// I'd prefer if I could check some kind of property on the policy that is created but that doesn't
//        /// seem to be possible.
//        /// </summary>
//        [Fact]
//        public async Task AddHttpClientFallbackPolicyTriggersOnTaskCanceledException()
//        {
//            var policyKey = "testPolicy";
//            var services = new ServiceCollection();
//            var fallbackPolicyConfiguration = Substitute.For<IFallbackPolicyConfiguration>();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                policyRegistry.AddHttpClientFallbackPolicy(policyKey, fallbackPolicyConfiguration);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            var fallbackPolicy = serviceProvider.GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);
//            var exception = new TaskCanceledException("test message");
//            var policyResult = await fallbackPolicy.ExecuteAndCaptureAsync(
//                () =>
//                {
//                    throw exception;
//                });
//            policyResult.FinalException.ShouldBeNull();
//            policyResult.Result.ShouldBeOfType<AbortedHttpResponseMessage>();
//            var result = (AbortedHttpResponseMessage)policyResult.Result;
//            result.Exception.ShouldBe(exception);
//        }

//        /// <summary>
//        /// Tests that the IPolicyRegistry.AddHttpClientFallbackPolicy method configures the
//        /// fallback policy to handle <see cref="TaskCanceledException"/> and that if
//        /// the inner exception of the TaskCanceledException was of type <see cref="TimeoutException"/>
//        /// than the AbortedHttpResponseMessage.TriggeredByTimeoutException should be true.
//        /// </summary>
//        [Fact]
//        public async Task AddHttpClientFallbackPolicyTriggersOnTaskCanceledException2()
//        {
//            var policyKey = "testPolicy";
//            var services = new ServiceCollection();
//            var fallbackPolicyConfiguration = Substitute.For<IFallbackPolicyConfiguration>();
//            services.AddPolicyRegistry((provider, policyRegistry) =>
//            {
//                policyRegistry.AddHttpClientFallbackPolicy(policyKey, fallbackPolicyConfiguration);
//            });

//            var serviceProvider = services.BuildServiceProvider();
//            var fallbackPolicy = serviceProvider.GetHttpPolicy<AsyncPolicyWrap<HttpResponseMessage>>(policyKey);
//            var taskCancelledException1 = new TaskCanceledException("test message");
//            var policyResult1 = await fallbackPolicy.ExecuteAndCaptureAsync(
//                action: () =>
//                {
//                    throw taskCancelledException1;
//                });
//            var result1 = (AbortedHttpResponseMessage)policyResult1.Result;
//            result1.TriggeredByTimeoutException.ShouldBeFalse();

//            var innerException = new TimeoutException();
//            var taskCancelledException2 = new TaskCanceledException("test message", innerException);
//            var policyResult2 = await fallbackPolicy.ExecuteAndCaptureAsync(
//                action: () =>
//                {
//                    throw taskCancelledException2;
//                });
//            var result2 = (AbortedHttpResponseMessage)policyResult2.Result;
//            result2.TriggeredByTimeoutException.ShouldBeTrue();
//        }
//    }
//}
