using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
using DotNet.Sdk.Extensions.Polly.Policies;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http;
using Polly;
using Polly.Extensions.Http;
using Polly.Registry;

namespace CircuitBreakerCheckerPolicy
{
    /*
     * This shows how to use the CircuitBreakerCheckerAsyncPolicy Polly policy.
     * The purpose of this policy is to eliminate exceptions thrown by the circuit breaker's policy. See https://github.com/App-vNext/Polly/wiki/Circuit-Breaker#reducing-thrown-exceptions-when-the-circuit-is-broken.
     *
     * As with any Polly policy, the CircuitBreakerCheckerAsyncPolicy policy can be used with any return type. In this demo
     * we are using it with an HttpClient meaning a return type of HttpResponseMessage (CircuitBreakerCheckerAsyncPolicy<HttpResponseMessage>).
     *
     * Most of the code in this example is just to configure the HttpClient with a circuit breaker policy and a circuit breaker checker policy
     * as well as to setup the demo so that it shows the difference between applying or not the circuit breaker checker policy.
     * The parts of the code relevant to the setup of the CircuitBreakerCheckerAsyncPolicy policy and it's usage have comments.
     *
     * This demo provides 2 endpoints:
     * 1) https://localhost:5001/with : use case where the circuit breaker checker policy is used
     * 2) https://localhost:5001/without : use case where the circuit breaker checker policy is NOT used
     *
     * When running the demo the output from the endpoints will provide further information.
     */
    public class Startup
    {
        private DateTime? _withCircuitBreakerCloseTime;
        private DateTime? _withoutCircuitBreakerCloseTime;
        private const int _maxAllowedConsecutiveFailures = 2;
        private const int _durationOfBreakInSecs = 3;
        private int _withConsecutiveFailedRequestsCount;
        private int _withoutConsecutiveFailedRequestsCount;
        private readonly string _withEndpointInfo = $@"
More info
========================
1. This endpoint is configured WITH a circuit breaker checker policy.
2. When the circuit is closed, one in every three requests returns a 200 status code, the other 2 return 503 status code.
3. After {_maxAllowedConsecutiveFailures} consecutive failures the circuit is open.
4. When the circuit is open the requests fail fast and are configured to return a 500 status code.
5. After {_durationOfBreakInSecs} seconds the circuit state transitions from open to closed and normal operation is resumed.

Keep refreshing the /with endpoint to see this example in action.
To check an example WITHOUT the circuit breaker checker policy go to the https://localhost:5001/without endpoint.
";
        private readonly string _withoutEndpointInfo = $@"
More info
========================
1. This endpoint is configured WITHOUT a circuit breaker checker policy.
2. When the circuit is closed, one in every three requests returns a 200 status code, the other 2 return 503 status code.
3. After {_maxAllowedConsecutiveFailures} consecutive failures the circuit is open.
4. When the circuit is open the requests fail fast but they throw an exception.
5. After {_durationOfBreakInSecs} seconds the circuit state transitions from open to closed and normal operation is resumed.

Keep refreshing the /without endpoint to see this example in action.
To check an example WITH the circuit breaker checker policy go to the https://localhost:5001/with endpoint.
";
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            var circuitBreakerPolicy = HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(2, TimeSpan.FromMinutes(1));
            var circuitBreakerCheckerPolicy = CircuitBreakerCheckerAsyncPolicy.Create(
                circuitBreakerPolicy: circuitBreakerPolicy,
                factory: (circuitState, context, cancellationToken) =>
                {
                    var response = new CircuitBrokenHttpResponseMessage(circuitState);
                    return Task.FromResult<HttpResponseMessage>(response);
                });
            var registry = new PolicyRegistry
            {
                {"circuit-breaker", circuitBreakerPolicy},
                {"circuit-breaker-checker", circuitBreakerCheckerPolicy}
            };
            services.AddPolicyRegistry(registry);

            // setup an HttpClient WITH the circuit breaker checker policy
            services
                .AddHttpClient("http-client-with")
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.BaseAddress = new Uri("https://github.com");
                })
                .AddPolicyHandlerFromRegistry("circuit-breaker-checker")
                .AddPolicyHandlerFromRegistry("circuit-breaker")
                
                //.AddHttpMessageHandler(() =>
                //{
                //    // create the circuit breaker policy
                //    var circuitBreakerPolicy = HttpPolicyExtensions
                //        .HandleTransientHttpError()
                //        .CircuitBreakerAsync(handledEventsAllowedBeforeBreaking: _maxAllowedConsecutiveFailures, durationOfBreak: TimeSpan.FromSeconds(_durationOfBreakInSecs));
                //    // create the circuit breaker checker policy   
                //    var circuitBreakerCheckerPolicy = CircuitBreakerCheckerAsyncPolicy.Create(
                //         circuitBreakerPolicy: circuitBreakerPolicy,
                //         factory: (circuitState, context, cancellationToken) =>
                //         {
                //             // this function needs to return a type of HttpResponseMessage
                //             // in this demo I'm using the CircuitBrokenHttpResponseMessage that is part of the DotNet-Sdk-Extensions nuget
                //             // however you could create your own type that derives from HttpResponseMessage or simply an HttpResponseMessage
                //             // with the desired return status code. This acts as a fallback return when the circuit breaker state is open/isolated.
                //             var response = new CircuitBrokenHttpResponseMessage(circuitState);
                //             var r = new HttpResponseMessage(HttpStatusCode.InternalServerError);
                //             return Task.FromResult<HttpResponseMessage>(response);
                //         });
                //    // create a policy that applies both the circuit breaker checker and the circuit breaker policy
                //    var finalPolicy = Policy.WrapAsync(circuitBreakerCheckerPolicy, circuitBreakerPolicy);
                //    return new PolicyHttpMessageHandler(finalPolicy);
                //})
                .AddHttpMessageHandler(() =>
                {
                    var count = 0;
                    var testMessageHandler = new TestHttpMessageHandler();
                    return testMessageHandler.MockHttpResponse(builder =>
                    {
                        builder.RespondWith(message => count++ % 3 == 0
                            ? new HttpResponseMessage(HttpStatusCode.OK)
                            : new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                    });
                });

            // setup an HttpClient WITHOUT the circuit breaker checker policy
            services
                .AddHttpClient("http-client-without")
                .ConfigureHttpClient(httpClient =>
                {
                    httpClient.BaseAddress = new Uri("https://github.com");
                })
                .AddHttpMessageHandler(() =>
                {
                    var circuitBreakerPolicy = HttpPolicyExtensions
                        .HandleTransientHttpError()
                        .CircuitBreakerAsync(_maxAllowedConsecutiveFailures, TimeSpan.FromSeconds(_durationOfBreakInSecs));
                    return new PolicyHttpMessageHandler(circuitBreakerPolicy);
                })
                .AddHttpMessageHandler(() =>
                {
                    var count = 0;
                    var testMessageHandler = new TestHttpMessageHandler();
                    return testMessageHandler.MockHttpResponse(builder =>
                    {
                        builder.RespondWith(message => count++ % 3 == 0
                            ? new HttpResponseMessage(HttpStatusCode.OK)
                            : new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));
                    });
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app
                .UseWhen(x => env.IsDevelopment(), appBuilder => appBuilder.UseDeveloperExceptionPage())
                .UseRouting()
                .UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/with", async context =>
                    {
                        var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var httpClient = httpClientFactory.CreateClient("http-client-with");
                        var response = await httpClient.GetAsync("/");
                        if (response.IsSuccessStatusCode)
                        {
                            _withConsecutiveFailedRequestsCount = 0;
                            _withCircuitBreakerCloseTime = null;
                        }
                        else
                        {
                            _withConsecutiveFailedRequestsCount++;
                        }

                        var message = response switch
                        {
                            CircuitBrokenHttpResponseMessage circuitBrokenHttpResponseMessage => CreateCircuitOpenMessageForWithEndpoint(circuitBrokenHttpResponseMessage),
                            _ => CreateCircuitClosedMessage(response, _withConsecutiveFailedRequestsCount, _withEndpointInfo)
                        };
                        await context.Response.WriteAsync(message);
                    });
                    endpoints.MapGet("/without", async context =>
                    {
                        var httpClientFactory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                        var httpClient = httpClientFactory.CreateClient("http-client-without");
                        string message;

                        try
                        {
                            var response = await httpClient.GetAsync("/");
                            _withoutConsecutiveFailedRequestsCount = response.IsSuccessStatusCode
                                ? 0
                                : _withoutConsecutiveFailedRequestsCount + 1;
                            message = CreateCircuitClosedMessage(response, _withoutConsecutiveFailedRequestsCount, _withoutEndpointInfo);
                        }
                        catch (Exception e)
                        {
                            message = CreateCircuitOpenMessageForWithoutEndpoint(e);
                        }

                        await context.Response.WriteAsync(message);
                    });
                });
        }

        private string CreateCircuitOpenMessageForWithEndpoint(CircuitBrokenHttpResponseMessage circuitBrokenHttpResponseMessage)
        {
            _withCircuitBreakerCloseTime ??= DateTime.Now.AddSeconds(_durationOfBreakInSecs);
            return @$"
FAILING FAST
========================
Circuit state: {circuitBrokenHttpResponseMessage.CircuitBreakerState}
Response status code: {(int)circuitBrokenHttpResponseMessage.StatusCode} {circuitBrokenHttpResponseMessage.StatusCode}
Circuit state will transition to close after {_durationOfBreakInSecs} seconds at {_withCircuitBreakerCloseTime}

{_withEndpointInfo}
";
        }

        private string CreateCircuitOpenMessageForWithoutEndpoint(Exception exception)
        {
            _withoutCircuitBreakerCloseTime ??= DateTime.Now.AddSeconds(_durationOfBreakInSecs);
            return @$"
FAILING FAST
========================
Circuit state: open
Circuit state will transition to close after {_durationOfBreakInSecs} seconds at {_withoutCircuitBreakerCloseTime}

Exception thrown: {exception.GetType()}
{exception}

{_withoutEndpointInfo}
";
        }

        private static string CreateCircuitClosedMessage(HttpResponseMessage response, int consecutiveFailedRequestsCount, string endpointMessage)
        {
            return @$"
NORMAL OPERATION
========================
Circuit state: closed or half open
Response status code: {(int)response.StatusCode} {response.StatusCode}
Consecutive failed requests: {consecutiveFailedRequestsCount}/{_maxAllowedConsecutiveFailures}

{endpointMessage}
";
        }
    }
}
