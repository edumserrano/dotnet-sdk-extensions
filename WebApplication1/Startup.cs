using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using DotNet.Sdk.Extensions.Polly;
using DotNet.Sdk.Extensions.Polly.HttpClient.CircuitBreaker;
using DotNet.Sdk.Extensions.Polly.HttpClient.CircuitBreaker.Extensions;
using DotNet.Sdk.Extensions.Polly.HttpClient.Fallback;
using DotNet.Sdk.Extensions.Polly.HttpClient.Fallback.Extensions;
using DotNet.Sdk.Extensions.Polly.HttpClient.Resilience.Extensions;
using DotNet.Sdk.Extensions.Polly.HttpClient.Retry;
using DotNet.Sdk.Extensions.Polly.HttpClient.Retry.Extensions;
using DotNet.Sdk.Extensions.Polly.HttpClient.Timeout;
using DotNet.Sdk.Extensions.Polly.HttpClient.Timeout.Extensions;
using Microsoft.Extensions.Http;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebApplication1", Version = "v1" });
            });

            services
                .AddHttpClientTimeoutOptions(name: "GitHubTimeoutOptions")
                .Configure(options => options.TimeoutInSecs = 1);
            services
                .AddHttpClientRetryOptions(name: "GitHubRetryOptions")
                .Bind(Configuration.GetSection("HttpClients:Default:RetryPolicy"));
            services
                .AddHttpClientCircuitBreakerOptions(name: "GitHubCircuitBreakerOptions")
                .Bind(Configuration.GetSection("GitHub"));

            services
                .AddHttpClientResilienceOptions(name: "GitHubResilienceOptions")
                .Configure(options =>
                {
                    options.Timeout.TimeoutInSecs = 1;

                    options.Retry.MedianFirstRetryDelayInSecs = 1;
                    options.Retry.RetryCount = 3;

                    options.CircuitBreaker.DurationOfBreakInSecs = 1;
                    options.CircuitBreaker.FailureThreshold = 0.5;
                    options.CircuitBreaker.SamplingDurationInSecs = 60;
                    options.CircuitBreaker.MinimumThroughput = 4;
                })
                //.Bind(Configuration.GetSection("GitHub"))
                ;
            
            services.AddPolicyRegistry((serviceProvider, registry) =>
            {
                //registry.AddHttpClientResiliencePolicies(
                //    policyKey: "GitHub", 
                //    optionsName: "GitHubResilienceOptions", 
                //    serviceProvider);

                registry.AddHttpClientTimeoutPolicy<GitHubPoliciesConfiguration>(policyKey: "GitHubTimeout", optionsName: "GitHubTimeoutOptions", serviceProvider);
                registry.AddHttpClientRetryPolicy<GitHubPoliciesConfiguration>(policyKey: "GitHubRetry", optionsName: "GitHubRetryOptions", serviceProvider);
                registry.AddHttpClientCircuitBreakerPolicy<GitHubPoliciesConfiguration>(policyKey: "GitHubCircuitBreaker", optionsName: "GitHubCircuitBreakerOptions", serviceProvider);
                registry.AddHttpClientFallbackPolicy<GitHubPoliciesConfiguration>(policyKey: "GitHubFallback", serviceProvider);

                //registry.AddHttpClientTimeoutPolicy(policyKey: "GitHubTimeout", optionsName: "GitHubTimeoutOptions", serviceProvider);
                //registry.AddHttpClientRetryPolicy(policyKey: "GitHubRetry", optionsName: "GitHubRetryOptions", serviceProvider);
                //registry.AddHttpClientCircuitBreakerPolicy(policyKey: "GitHubCircuitBreaker", optionsName: "GitHubCircuitBreakerOptions", serviceProvider);
                //registry.AddHttpClientFallbackPolicy(policyKey: "GitHubFallback", serviceProvider);
            });

            services
                .AddHttpClient<GitHubClient>() //.AddPolicyHandlerFromRegistry(policyKey: "GitHubCircuitBreaker")
                //.AddResiliencePoliciesFromRegistry(policyKey:"GitHub")
                .AddPolicyHandlerFromRegistry(policyKey: "GitHubFallback")          // fallback response
                .AddPolicyHandlerFromRegistry(policyKey: "GitHubRetry")             // do retries
                .AddPolicyHandlerFromRegistry(policyKey: "GitHubCircuitBreaker")    // circuit breaker
                .AddPolicyHandlerFromRegistry(policyKey: "GitHubTimeout")           // because there is a retry policy first this is a timeout for each call/retry, not a timeout for all retries
                .AddHttpMessageHandler(() =>
                {
                    var testMessageHandler = new TestHttpMessageHandler();
                    testMessageHandler.MockHttpResponse(builder =>
                    {
                        builder

                            .Where(x =>
                            {
                                var s = x.GetPolicyExecutionContext();
                                return true;
                            })
                            .TimesOut(TimeSpan.FromSeconds(60));

                        //builder.RespondWith(message =>
                        //{
                        //    return new HttpResponseMessage(HttpStatusCode.InternalServerError);
                        //});
                    });
                    return testMessageHandler;
                })
                .ConfigureHttpMessageHandlerBuilder(builder =>
                {
                    var a = builder.AdditionalHandlers;
                    var c = a[1];
                    var x = ReflectionExtensions.GetInstanceField(
                        typeof(PolicyHttpMessageHandler),
                        c,
                        "_policy");
                    var z = x as AsyncRetryPolicy<HttpResponseMessage>;
                    var b = 2;
                })
                ;
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebApplication1 v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }

    public static class ReflectionExtensions
    {
        internal static object GetInstanceField(Type type, object instance, string fieldName)
        {
            var bindFlags = BindingFlags.Instance 
                            | BindingFlags.Public
                            | BindingFlags.NonPublic
                            | BindingFlags.Static;
            var field = type.GetField(fieldName, bindFlags);
            return field.GetValue(instance);
        }
    }

    public class GitHubPoliciesConfiguration : 
        IRetryPolicyConfiguration, 
        ITimeoutPolicyConfiguration, 
        ICircuitBreakerPolicyConfiguration, 
        IFallbackPolicyConfiguration
    {
        public Task OnRetryAsync(
            RetryOptions retryOptions,
            DelegateResult<HttpResponseMessage> outcome,
            TimeSpan retryDelay,
            int retryNumber,
            Context pollyContext)
        {

            return Task.CompletedTask;
        }

        public Task OnTimeoutASync(
            TimeoutOptions timeoutOptions,
            Context context,
            TimeSpan requestTimeout,
            Task timedOutTask,
            Exception exception)
        {

            return Task.CompletedTask;
        }

        public Task OnBreakAsync(
            CircuitBreakerOptions circuitBreakerOptions,
            DelegateResult<HttpResponseMessage> lastOutcome,
            CircuitState previousState,
            TimeSpan durationOfBreak,
            Context context)
        {
            return Task.CompletedTask;
        }

        public Task OnHalfOpenAsync(CircuitBreakerOptions circuitBreakerOptions)
        {
            return Task.CompletedTask;
        }

        public Task OnResetAsync(CircuitBreakerOptions circuitBreakerOptions, Context context)
        {
            return Task.CompletedTask;
        }

        public Task OnTimeoutFallbackAsync(DelegateResult<HttpResponseMessage> outcome, Context context)
        {
            return Task.CompletedTask;
        }

        public Task OnBrokenCircuitFallbackAsync(DelegateResult<HttpResponseMessage> outcome, Context context)
        {
            return Task.CompletedTask;
        }

        public Task OnTaskCancelledFallbackAsync(DelegateResult<HttpResponseMessage> outcome, Context context)
        {
            return Task.CompletedTask;
        }
    }

    public class GitHubClient
    {
        private readonly HttpClient _httpClient;

        public GitHubClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> Get()
        {
            var response = await _httpClient.GetAsync("https://github.com");
            return "bla";
        }
    }
}
