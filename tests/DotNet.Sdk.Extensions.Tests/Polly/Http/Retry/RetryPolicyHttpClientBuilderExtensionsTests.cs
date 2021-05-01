using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Retry;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Extensions;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary.Polly;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary;
using DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Polly.Retry;
using Polly.Timeout;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Retry
{
    /// <summary>
    /// Tests for the <see cref="RetryPolicyHttpClientBuilderExtensions"/> class.
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
    public class RetryPolicyHttpClientBuilderExtensionsTests
    {
        /// <summary>
        /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddRetryPolicy(IHttpClientBuilder,Action{RetryOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        ///
        /// This overload accepts only an action to configure the value of the <see cref="RetryOptions"/>.
        /// </summary>
        [Fact]
        public void AddRetryPolicy1()
        {
            AsyncRetryPolicy<HttpResponseMessage>? retryPolicy = null;
            var httpClientName = "GitHub";
            var retryCount = 2;
            var medianFirstRetryDelayInSecs = 1;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddRetryPolicy(options =>
                {
                    options.RetryCount = retryCount;
                    options.MedianFirstRetryDelayInSecs = medianFirstRetryDelayInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    retryPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncRetryPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            retryPolicy.ShouldNotBeNull();
            retryPolicy.ShouldBeConfiguredAsExpected(retryCount, medianFirstRetryDelayInSecs);
            retryPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                retryCount: retryCount,
                medianFirstRetryDelayInSecs: medianFirstRetryDelayInSecs,
                policyConfigurationType: typeof(DefaultRetryPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddTimeoutPolicy(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        /// 
        /// This overload accepts only the name of the option to use for the value of the <see cref="RetryOptions"/>.
        /// The options must be added and configured on the <see cref="ServiceCollection"/>. This is done via the
        /// <see cref="RetryOptionsExtensions.AddHttpClientRetryOptions"/> extension method.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicy2()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 1;
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = 2;
                    options.MedianFirstRetryDelayInSecs = 1;
                });
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);
            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldBeConfiguredAsExpected(timeoutInSecs);
            timeoutPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyConfigurationType: typeof(DefaultTimeoutPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddTimeoutPolicy{TPolicyEventHandler}(IHttpClientBuilder,Action{RetryOptions})"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        /// 
        /// This overload accepts the name of the option to use for the value of the <see cref="RetryOptions"/>
        /// and a <see cref="ITimeoutPolicyEventHandler"/> type to handle the timeout policy events.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicy3()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 1;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy<TestTimeoutPolicyEventHandler>(options =>
                {
                    options.TimeoutInSecs = timeoutInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldBeConfiguredAsExpected(timeoutInSecs);
            timeoutPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyConfigurationType: typeof(TestTimeoutPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the <see cref="RetryPolicyHttpClientBuilderExtensions.AddTimeoutPolicy{TPolicyEventHandler}(IHttpClientBuilder,string)"/>
        /// overload method adds a <see cref="DelegatingHandler"/> with a timeout policy to the <see cref="HttpClient"/>.
        ///
        /// This overload accepts the name of the option to use for the value of the <see cref="RetryOptions"/>.
        /// The options must be added and configured on the <see cref="ServiceCollection"/>. This is done via the
        /// <see cref="RetryOptionsExtensions.AddHttpClientRetryOptions"/> extension method.
        ///
        /// This overload also accepts an <see cref="ITimeoutPolicyEventHandler"/> type to handle the timeout policy events.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicy4()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 1;
            var optionsName = "GitHubOptions";

            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                .Configure(options =>
                {
                    options.RetryCount = 2;
                    options.MedianFirstRetryDelayInSecs = 1;
                });
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy<TestTimeoutPolicyEventHandler>(optionsName)
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldBeConfiguredAsExpected(timeoutInSecs);
            timeoutPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyConfigurationType: typeof(TestTimeoutPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the overloads of RetryPolicyHttpClientBuilderExtensions.AddTimeoutPolicy that
        /// do not take in a <see cref="ITimeoutPolicyEventHandler"/> type should have their events
        /// handled by the default handler type <see cref="DefaultTimeoutPolicyEventHandler"/>.
        ///
        /// This test does not guarantee that there isn't any issue in the triggering of the 
        /// <see cref="ITimeoutPolicyEventHandler.OnTimeoutAsync"/> but it does assert that
        /// the onTimeoutAsync event from the policy is linked to the <see cref="DefaultTimeoutPolicyEventHandler"/>.
        ///
        /// I couldn't find a way to test that if I triggered the timeout policy that indeed
        /// the  <see cref="DefaultTimeoutPolicyEventHandler"/> was being invoked as expected but I don't
        /// think I should be doing that test anyway. That's too much detail of the current implementation.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicyShouldTriggerDefaultConfiguration()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 2;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = timeoutInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyConfigurationType: typeof(DefaultTimeoutPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the overloads of RetryPolicyHttpClientBuilderExtensions.AddTimeoutPolicy that
        /// take in a <see cref="ITimeoutPolicyEventHandler"/> type should have their events handled by that type.
        ///
        /// This test does not guarantee that there isn't any issue in the triggering of the 
        /// <see cref="ITimeoutPolicyEventHandler.OnTimeoutAsync"/> but it does assert that
        /// the onTimeoutAsync event from the policy is linked to expected type.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicyShouldTriggerCustomConfiguration()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy = null;
            var httpClientName = "GitHub";
            var timeoutInSecs = 2;
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy<TestTimeoutPolicyEventHandler>(options =>
                {
                    options.TimeoutInSecs = timeoutInSecs;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient(httpClientName);

            timeoutPolicy.ShouldNotBeNull();
            timeoutPolicy.ShouldTriggerPolicyEventHandler(
                httpClientName: httpClientName,
                timeoutInSecs: timeoutInSecs,
                policyConfigurationType: typeof(TestTimeoutPolicyEventHandler));
        }

        /// <summary>
        /// Tests that the overloads of RetryPolicyHttpClientBuilderExtensions.AddTimeoutPolicy that
        /// take in a <see cref="ITimeoutPolicyEventHandler"/> type should have their events handled by that type.
        ///
        /// This test triggers the timeout policy to make sure the <see cref="TimeoutEvent"/> is triggered
        /// as expected.
        /// </summary>
        [Fact]
        public async Task AddTimeoutPolicyTriggersCustomConfiguration()
        {
            var httpClientName = "GitHub";
            var timeoutInSecs = 0.05; //50ms
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy<TestTimeoutPolicyEventHandler>(options =>
                {
                    options.TimeoutInSecs = timeoutInSecs;
                })
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    return new TestHttpMessageHandler()
                        .MockHttpResponse(builder =>
                        {
                            // this timeout is a max timeout before aborting
                            // but the polly timeout policy will timeout before this happens
                            builder.TimesOut(TimeSpan.FromSeconds(1));
                        });
                });

            var serviceProvider = services.BuildServiceProvider();
            var httpClient = serviceProvider.InstantiateNamedHttpClient(httpClientName);
            await Should.ThrowAsync<TimeoutRejectedException>(() => httpClient.GetAsync("https://github.com"));

            TestTimeoutPolicyEventHandler.OnTimeoutAsyncCalls.Count.ShouldBe(1);
            var timeoutEvent = TestTimeoutPolicyEventHandler.OnTimeoutAsyncCalls.First();
            timeoutEvent.HttpClientName.ShouldBe(httpClientName);
            //timeoutEvent.RetryOptions.TimeoutInSecs.ShouldBe(timeoutInSecs);
        }

        /// <summary>
        /// Tests that the RetryPolicyHttpClientBuilderExtensions.AddTimeoutPolicy methods
        /// validate the <see cref="RetryOptions"/> with the built in data annotations.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicyOptionsValidation1()
        {
            var httpClientName = "GitHub";
            var services = new ServiceCollection();
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = -1;
                });

            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"DataAnnotation validation failed for members: 'TimeoutInSecs' with the error: 'The field TimeoutInSecs must be between {double.Epsilon} and {double.MaxValue}.'.");
        }

        /// <summary>
        /// Tests that you can add any configuration/validation you want to the <see cref="OptionsBuilder{T}"/>
        /// after using <see cref="RetryOptionsExtensions.AddHttpClientRetryOptions"/> and those option configurations
        /// will be honored.
        ///
        /// In this test we configure the <see cref="RetryOptions.TimeoutInSecs"/> to 1 and force a validation
        /// that this value must be > 3.
        /// Although the default data annotation validations only enforces that the value must be positive, with the
        /// extra validation the options validation will fail.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicyOptionsValidation2()
        {
            var httpClientName = "GitHub";
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            //services
            //    .AddHttpClientRetryOptions(optionsName)
            //    .Configure(options => options.TimeoutInSecs = 1)
            //    .Validate(options =>
            //    {
            //        return options.TimeoutInSecs > 3;
            //    });
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(optionsName);

            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe("A validation error has occurred.");
        }

        /// <summary>
        /// Tests that you can add any configuration/validation you want to the <see cref="OptionsBuilder{T}"/>
        /// after using <see cref="RetryOptionsExtensions.AddHttpClientRetryOptions"/> and those option configurations
        /// will be honored.
        ///
        /// In this test we configure the <see cref="RetryOptions.TimeoutInSecs"/> to -1 and force a validation
        /// that this value must be > 3.
        /// With this setup both the default data annotation validation and the custom one will be triggered.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicyOptionsValidation3()
        {
            var httpClientName = "GitHub";
            var optionsName = "GitHubOptions";
            var services = new ServiceCollection();
            services
                .AddHttpClientRetryOptions(optionsName)
                //.Configure(options => options.TimeoutInSecs = -1)
                //.Validate(options =>
                //{
                //    return options.TimeoutInSecs > 3;
                //})
                ;
            services
                .AddHttpClient(httpClientName)
                .AddTimeoutPolicy(optionsName);

            var serviceProvider = services.BuildServiceProvider();
            var exception = Should.Throw<OptionsValidationException>(() =>
            {
                serviceProvider.InstantiateNamedHttpClient(httpClientName);
            });
            exception.Message.ShouldBe($"A validation error has occurred.; DataAnnotation validation failed for members: 'TimeoutInSecs' with the error: 'The field TimeoutInSecs must be between {double.Epsilon} and {double.MaxValue}.'.");
        }

        /// <summary>
        /// This tests that the policies added to the <see cref="HttpClient"/> by the
        /// RetryPolicyHttpClientBuilderExtensions.AddTimeoutPolicy methods are unique.
        ///
        /// Policies should NOT be the same between HttpClients or else when one HttpClient triggers
        /// the policy it would trigger for all.
        /// </summary>
        [Fact]
        public void AddTimeoutPolicyUniquePolicyPerHttpClient()
        {
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy1 = null;
            AsyncTimeoutPolicy<HttpResponseMessage>? timeoutPolicy2 = null;
            var services = new ServiceCollection();
            services
                .AddHttpClient("GitHub")
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = 1;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy1 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });
            services
                .AddHttpClient("Microsoft")
                .AddTimeoutPolicy(options =>
                {
                    options.TimeoutInSecs = 2;
                })
                .ConfigureHttpMessageHandlerBuilder(httpMessageHandlerBuilder =>
                {
                    timeoutPolicy2 = httpMessageHandlerBuilder.AdditionalHandlers
                        .GetPolicies<AsyncTimeoutPolicy<HttpResponseMessage>>()
                        .FirstOrDefault();
                });

            var serviceProvider = services.BuildServiceProvider();
            serviceProvider.InstantiateNamedHttpClient("GitHub");
            serviceProvider.InstantiateNamedHttpClient("Microsoft");

            timeoutPolicy1.ShouldNotBeNull();
            timeoutPolicy2.ShouldNotBeNull();
            ReferenceEquals(timeoutPolicy1, timeoutPolicy2).ShouldBeFalse();
            timeoutPolicy1.PolicyKey.ShouldNotBe(timeoutPolicy2.PolicyKey);
        }
    }
}
