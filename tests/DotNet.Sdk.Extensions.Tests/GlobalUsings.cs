global using System.ComponentModel.DataAnnotations;
global using System.Diagnostics.CodeAnalysis;
global using System.Net;
global using System.Reflection;
global using DotNet.Sdk.Extensions.Options;
global using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker;
global using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Events;
global using DotNet.Sdk.Extensions.Polly.Http.CircuitBreaker.Extensions;
global using DotNet.Sdk.Extensions.Polly.Http.Fallback.Events;
global using DotNet.Sdk.Extensions.Polly.Http.Fallback.Extensions;
global using DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages;
global using DotNet.Sdk.Extensions.Polly.Http.Resilience;
global using DotNet.Sdk.Extensions.Polly.Http.Resilience.Events;
global using DotNet.Sdk.Extensions.Polly.Http.Resilience.Extensions;
global using DotNet.Sdk.Extensions.Polly.Http.Retry;
global using DotNet.Sdk.Extensions.Polly.Http.Retry.Events;
global using DotNet.Sdk.Extensions.Polly.Http.Retry.Extensions;
global using DotNet.Sdk.Extensions.Polly.Http.Timeout;
global using DotNet.Sdk.Extensions.Polly.Http.Timeout.Events;
global using DotNet.Sdk.Extensions.Polly.Http.Timeout.Extensions;
global using DotNet.Sdk.Extensions.Polly.Policies;
global using DotNet.Sdk.Extensions.Testing.Configuration;
global using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers;
global using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;
global using DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.Auxiliary;
global using DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.Auxiliary.DataAnnotations;
global using DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.Auxiliary.IValidateOptions;
global using DotNet.Sdk.Extensions.Tests.Options.ValidateEagerly.Auxiliary.StartupValidation;
global using DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary;
global using DotNet.Sdk.Extensions.Tests.Polly.Http.CircuitBreaker.Auxiliary;
global using DotNet.Sdk.Extensions.Tests.Polly.Http.Fallback.Auxiliary;
global using DotNet.Sdk.Extensions.Tests.Polly.Http.Resilience.Auxiliary;
global using DotNet.Sdk.Extensions.Tests.Polly.Http.Retry.Auxiliary;
global using DotNet.Sdk.Extensions.Tests.Polly.Http.Timeout.Auxiliary;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.AspNetCore.Hosting;
global using Microsoft.AspNetCore.Http;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.Configuration.Memory;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Http;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using NSubstitute;
global using Polly;
global using Polly.CircuitBreaker;
global using Polly.Retry;
global using Polly.Timeout;
global using Polly.Wrap;
global using Shouldly;
global using Xunit;
global using OptionsBuilderExtensions = DotNet.Sdk.Extensions.Options.OptionsBuilderExtensions;