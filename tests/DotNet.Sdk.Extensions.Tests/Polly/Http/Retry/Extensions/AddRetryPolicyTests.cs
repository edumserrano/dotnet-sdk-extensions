using System;
using System.Linq;
using System.Net.Http;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Extensions;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Polly.Retry;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Extensions
{
    /// <summary>
    /// Tests for the <see cref="RetryPolicyHttpClientBuilderExtensions"/> class.
    /// Specifically for the RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy overloads.
    ///
    /// Many tests here use reflection to check that the policy is configured as expected.
    /// Although I'd prefer to do it without using reflection I couldn't find an alternative.
    /// At least not one that wouldn't force me to trigger the policy in different scenarios
    /// to check what I need. If I did that then it would almost be like testing that the Polly
    /// policies do what they are supposed to do and my intention is NOT to test the Polly code.
    ///
    /// Because of the reflection usage these tests can break when updating the Polly packages.
    /// </summary>
    [Trait("Category", XUnitCategories.Polly)]
    [Collection(XUnitTestCollections.RetryPolicy)]
    public class AddRetryPolicyTests : IDisposable
    {
        /// <summary>
        /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy(IHttpClientBuilder,Action{RetryOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        ///
        /// This overload accepts only an action to configure the value of the <see cref="RetryOptions"/>.
        /// </summary>
        [Fact]
        public void AddRetryPolicy1()
        {
            AsyncRetryPolicy<HttpResponseMessage>? retryPolicy = null;
            var httpClientName = "GitHub";
            var retryOptions = new RetryOptions
            {
                RetryCount = 2,
                MedianFirstRetryDelayInSecs = 1
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddRetryPolicy(options =>
                {
                    options.RetryCount = retryOptions.RetryCount;
                    options.MedianFirstRetryDelayInSecs = retryOptions.MedianFirstRetryDelayInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    retryPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var retryPolicyAsserter = new RetryPolicyAsserter(
                httpClientName,
                retryOptions,
                retryPolicy);
            retryPolicyAsserter.PolicyShouldBeConfiguredAsExpected();
            retryPolicyAsserter.PolicyShouldTriggerPolicyEventHandler(typeof(DefaultRetryPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        /// 
        /// This overload accepts only the name of the option to use for the value of the <see cref="RetryOptions"/>.
        /// The options must be added and configured on the <see cref="ServiceCollection"/>. This is done via the
        /// <see cref="RetryOptionsExtensions.AddHttpClientRetryOptions"/> extension method.
        /// </summary>
        [Fact]
        public void AddRetryPolicy2()
        {
            AsyncRetryPolicy<HttpResponseMessage>? retryPolicy = null;
            var httpClientName = "GitHub";
            var retryOptions = new RetryOptions
            {
                RetryCount = 2,
                MedianFirstRetryDelayInSecs = 1
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = retryOptions.RetryCount;
                    options.MedianFirstRetryDelayInSecs = retryOptions.MedianFirstRetryDelayInSecs;
                });
            services
                .AddHttpClient(httpClientName)
                .AddRetryPolicy(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    retryPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var retryPolicyAsserter = new RetryPolicyAsserter(
                httpClientName,
                retryOptions,
                retryPolicy);
            retryPolicyAsserter.PolicyShouldBeConfiguredAsExpected();
            retryPolicyAsserter.PolicyShouldTriggerPolicyEventHandler(typeof(DefaultRetryPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy{TPolicyEventHandler}(IHttpClientBuilder,Action{RetryOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        /// 
        /// This overload accepts the name of the option to use for the value of the <see cref="RetryOptions"/>
        /// and a <see cref="IRetryPolicyEventHandler"/> type to handle the retry policy events.
        /// </summary>
        [Fact]
        public void AddRetryPolicy3()
        {
            AsyncRetryPolicy<HttpResponseMessage>? retryPolicy = null;
            var httpClientName = "GitHub";
            var retryOptions = new RetryOptions
            {
                RetryCount = 2,
                MedianFirstRetryDelayInSecs = 1
            };
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddRetryPolicy<TestRetryPolicyEventHandler>(options =>
                {
                    options.RetryCount = retryOptions.RetryCount;
                    options.MedianFirstRetryDelayInSecs = retryOptions.MedianFirstRetryDelayInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    retryPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            var retryPolicyAsserter = new RetryPolicyAsserter(
                httpClientName,
                retryOptions,
                retryPolicy);
            retryPolicyAsserter.PolicyShouldBeConfiguredAsExpected();
            retryPolicyAsserter.PolicyShouldTriggerPolicyEventHandler(typeof(TestRetryPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a retry policy to the <see cref="HttpClient"/>.
        ///
        /// This overload accepts the name of the option to use for the value of the <see cref="RetryOptions"/>.
        /// The options must be added and configured on the <see cref="ServiceCollection"/>. This is done via the
        /// <see cref="RetryOptionsExtensions.AddHttpClientRetryOptions"/> extension method.
        ///
        /// This overload also accepts an <see cref="IRetryPolicyEventHandler"/> type to handle the retry policy events.
        /// </summary>
        [Fact]
        public void AddRetryPolicy4()
        {
            AsyncRetryPolicy<HttpResponseMessage>? retryPolicy = null;
            var httpClientName = "GitHub";
            var retryOptions = new RetryOptions
            {
                RetryCount = 2,
                MedianFirstRetryDelayInSecs = 1
            };
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = retryOptions.RetryCount;
                    options.MedianFirstRetryDelayInSecs = retryOptions.MedianFirstRetryDelayInSecs;
                });
            services
                .AddHttpClient(httpClientName)
                .AddRetryPolicy<TestRetryPolicyEventHandler>(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    retryPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
            
            var retryPolicyAsserter = new RetryPolicyAsserter(
                httpClientName,
                retryOptions,
                retryPolicy);
            retryPolicyAsserter.PolicyShouldBeConfiguredAsExpected();
            retryPolicyAsserter.PolicyShouldTriggerPolicyEventHandler(typeof(TestRetryPolicyEventHandler));
        }
        
        /// <summary>
        /// This tests that the policies added to the <see cref="HttpClient"/> by the
        /// RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy methods are unique.
        ///
        /// Policies should NOT be the same between HttpClients or else when one HttpClient triggers
        /// the policy it would trigger for all.
        /// </summary>
        [Fact]
        public void AddRetryPolicyUniquePolicyPerHttpClient()
        {
            AsyncRetryPolicy<HttpResponseMessage>? retryPolicy1 = null;
            AsyncRetryPolicy<HttpResponseMessage>? retryPolicy2 = null;
            var services = new ServiceCollection();
            services
                .AddHttpClient("GitHub")
                .AddRetryPolicy(options =>
                {
                    options.RetryCount = 2;
                    options.MedianFirstRetryDelayInSecs = 1;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    retryPolicy1 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });
            services
                .AddHttpClient("Microsoft")
                .AddRetryPolicy(options =>
                {
                    options.RetryCount = 3;
                    options.MedianFirstRetryDelayInSecs = 1;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    retryPolicy2 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient("GitHub");
            serviceProvider.InstantiateNamedHttpClient("Microsoft");

            retryPolicy1.ShouldNotBeNull();
            retryPolicy2.ShouldNotBeNull();
            ReferenceEquals(retryPolicy1, retryPolicy2).ShouldBeFalse();
            retryPolicy1.PolicyKey.ShouldNotBe(retryPolicy2.PolicyKey);
        }

        public void Dispose()
        {
            TestRetryPolicyEventHandler.Clear();
        }
    }
}
