using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Polly;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
using DotNet.Sdk.Extensions.Polly.Http.Retry.Extensions;
using DotNet.Sdk.Extensions.Polly.Http.Timeout;
using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
using Microsoft.Extensions.Options;

namespace WebApplication1
{
    internal class TestOptionsValidation : IValidateOptions<TimeoutOptions>
    {
        public ValidateOptionsResult Validate(string name, TimeoutOptions options)
        {
            return ValidateOptionsResult.Success;
        }
    }

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

            //services
            //    .AddSingleton<IValidateOptions<TimeoutOptions>, TestOptionsValidation>()
            //    .AddOptions<TimeoutOptions>(name: "GitHubTimeoutOptions")
            //    .Bind(Configuration.GetSection("GitHub"));;

            //services
            //    .AddHttpClientTimeoutOptions(name: "GitHubTimeoutOptions")
            //    .Bind(Configuration.GetSection("GitHub"));
            //services
            //    .AddHttpClientTimeoutOptions(name: "GitHubTimeoutOptions")
            //    .Configure(options => options.TimeoutInSecs = 1);
            //services
            //    .AddHttpClientRetryOptions(name: "GitHubRetryOptions")
            //    .Bind(Configuration.GetSection("HttpClients:Default:RetryPolicy"));
            //services
            //    .AddHttpClientCircuitBreakerOptions(name: "GitHubCircuitBreakerOptions")
            //    .Bind(Configuration.GetSection("GitHub"));

            //services
            //    .AddHttpClientResilienceOptions(name: "GitHubResilienceOptions")
            //    .Configure(options =>
            //    {
            //        options.Timeout.TimeoutInSecs = 1;

            //        options.Retry.MedianFirstRetryDelayInSecs = 1;
            //        options.Retry.RetryCount = 3;

            //        options.CircuitBreaker.DurationOfBreakInSecs = 1;
            //        options.CircuitBreaker.FailureThreshold = 0.5;
            //        options.CircuitBreaker.SamplingDurationInSecs = 60;
            //        options.CircuitBreaker.MinimumThroughput = 4;
            //    })
            //    //.Bind(Configuration.GetSection("GitHub"))
            //    ;

            //services.AddPolicyRegistry((serviceProvider, registry) =>
            //{
            //    //registry.AddHttpClientResiliencePolicies(
            //    //    policyKey: "GitHub", 
            //    //    optionsName: "GitHubResilienceOptions", 
            //    //    serviceProvider);

            //    registry.AddHttpClientTimeoutPolicy<GitHubPoliciesConfiguration>(policyKey: "GitHubTimeout", optionsName: "GitHubTimeoutOptions", serviceProvider);
            //    registry.AddHttpClientRetryPolicy<GitHubPoliciesConfiguration>(policyKey: "GitHubRetry", optionsName: "GitHubRetryOptions", serviceProvider);
            //    registry.AddHttpClientCircuitBreakerPolicy<GitHubPoliciesConfiguration>(policyKey: "GitHubCircuitBreaker", optionsName: "GitHubCircuitBreakerOptions", serviceProvider);
            //    registry.AddHttpClientFallbackPolicy<GitHubPoliciesConfiguration>(policyKey: "GitHubFallback", serviceProvider);

            //    //registry.AddHttpClientTimeoutPolicy(policyKey: "GitHubTimeout", optionsName: "GitHubTimeoutOptions", serviceProvider);
            //    //registry.AddHttpClientRetryPolicy(policyKey: "GitHubRetry", optionsName: "GitHubRetryOptions", serviceProvider);
            //    //registry.AddHttpClientCircuitBreakerPolicy(policyKey: "GitHubCircuitBreaker", optionsName: "GitHubCircuitBreakerOptions", serviceProvider);
            //    //registry.AddHttpClientFallbackPolicy(policyKey: "GitHubFallback", serviceProvider);
            //});
            
            services
                .AddHttpClient<GitHubClient>() //.AddPolicyHandlerFromRegistry(policyKey: "GitHubCircuitBreaker")
                //.AddResiliencePoliciesFromRegistry(policyKey:"GitHub")
                //.AddPolicyHandlerFromRegistry(policyKey: "GitHubFallback")          // fallback response
                //.AddPolicyHandlerFromRegistry(policyKey: "GitHubRetry")             // do retries
                //.AddPolicyHandlerFromRegistry(policyKey: "GitHubCircuitBreaker")    // circuit breaker
                //.AddPolicyHandlerFromRegistry(policyKey: "GitHubTimeout")           // because there is a retry policy first this is a timeout for each call/retry, not a timeout for all retries
                //.AddTimeoutPolicy<GitHubPoliciesConfiguration>(
                //    configureOptions: options =>
                //    {
                //        options.TimeoutInSecs = 3;
                //    })
                
                //.AddFallbackPolicy()
                .AddRetryPolicy(options => { })
                .AddRetryPolicy<GitHubPoliciesEventHandler>(options => { })
                //.AddRetryPolicy(
                //    configureOptions:options => { },
                //    eventHandlerFactory: serviceProvider=> { return new GitHubPoliciesEventHandler() })
                //.AddCircuitBreakerPolicy(options => { })
                //.AddTimeoutPolicy(options => options.TimeoutInSecs = 1)

                .AddResiliencePolicies(options => {  })
                
                //.AddTimeoutPolicy<GitHubPoliciesConfiguration>(optionsName: "GitHubTimeoutOptions")
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
                });
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
    
    public class GitHubPoliciesEventHandler :
        ITimeoutPolicyEventHandler,
        IRetryPolicyEventHandler,
        ICircuitBreakerPolicyEventHandler,
        IFallbackPolicyEventHandler
    {
        public Task OnTimeoutAsync(TimeoutEvent timeoutEvent)
        {
            return Task.CompletedTask;
        }

        public Task OnRetryAsync(RetryEvent retryEvent)
        {
            return Task.CompletedTask;
        }
        
        public Task OnBreakAsync(BreakEvent breakEvent)
        {
            return Task.CompletedTask;
        }

        public Task OnHalfOpenAsync(HalfOpenEvent halfOpenEvent)
        {
            return Task.CompletedTask;
        }

        public Task OnResetAsync(ResetEvent resetEvent)
        {
            return Task.CompletedTask;
        }

        public Task OnHttpRequestExceptionFallbackAsync(FallbackEvent timeoutFallbackEvent)
        {
            return Task.CompletedTask;
        }

        public Task OnTimeoutFallbackAsync(FallbackEvent timeoutFallbackEvent)
        {
            return Task.CompletedTask;
        }

        public Task OnBrokenCircuitFallbackAsync(FallbackEvent brokenCircuitFallbackEvent)
        {
            return Task.CompletedTask;
        }

        public Task OnTaskCancelledFallbackAsync(FallbackEvent taskCancelledFallbackEvent)
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
