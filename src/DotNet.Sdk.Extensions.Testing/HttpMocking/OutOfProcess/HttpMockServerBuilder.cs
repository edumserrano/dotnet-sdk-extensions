using System;
using System.Collections.Generic;
using DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess.MockServers;
using Microsoft.AspNetCore.Server.Kestrel.Core.Internal.Http;
using Microsoft.Extensions.Hosting;

namespace DotNet.Sdk.Extensions.Testing.HttpMocking.OutOfProcess
{
    /// <summary>
    /// Provides methods to configure and create an <see cref="HttpMockServer"/>.
    /// </summary>
    public class HttpMockServerBuilder
    {
        private string[] _hostArgs = new string[0];
        private readonly List<HttpMockServerUrlDescriptor> _hostUrls = new List<HttpMockServerUrlDescriptor>();

        /// <summary>
        /// Defines an URL for the <see cref="HttpMockServer"/> to be listening on. Multiple URLs can be set.
        /// </summary>
        /// <remarks>
        /// The host part of the URL is always localhost.
        /// </remarks>
        /// <param name="scheme">The scheme part of the URL.</param>
        /// <param name="port">The port part of the URL.</param>
        /// <returns>The <see cref="HttpMockServerBuilder"/> for chaining.</returns>
        public HttpMockServerBuilder UseUrl(HttpScheme scheme, int port)
        {
            var urlDescriptor = new HttpMockServerUrlDescriptor(scheme, port);
            _hostUrls.Add(urlDescriptor);
            return this;
        }

        /// <summary>
        /// Allows passing more configuration into the <see cref="HttpMockServer"/>.
        /// </summary>
        /// <remarks>
        /// The <see cref="HttpMockServer"/> is based on <see cref="IHost"/> so it will accept any arguments that the <see cref="IHost"/> accepts.
        /// </remarks>
        /// <param name="hostArgs">The list of arguments to allow further configuration of the <see cref="HttpMockServer"/>.</param>
        /// <returns>The <see cref="HttpMockServerBuilder"/> for chaining.</returns>
        public HttpMockServerBuilder UseHostArgs(string[] hostArgs)
        {
            _hostArgs = hostArgs ?? throw new ArgumentNullException(nameof(hostArgs));
            return this;
        }

        /// <summary>
        /// Defines that the <see cref="HttpMockServer"/> will be based on request/response mocking.
        /// </summary>
        /// <returns>The <see cref="ResponseBasedBuilder"/> for chaining.</returns>
        public ResponseBasedBuilder UseHttpResponseMocks()
        {
            var args = new HttpMockServerArgs(_hostUrls, _hostArgs);
            return new ResponseBasedBuilder(args);
        }

        /// <summary>
        /// Defines that the <see cref="HttpMockServer"/> will be based on a Startup class.
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> of the Startup class to be used by the <see cref="HttpMockServer"/>.</typeparam>
        /// <returns>The <see cref="StartupBasedBuilder&lt;T&gt;"/> for chaining.</returns>
        public StartupBasedBuilder<T> UseStartup<T>() where T : class
        {
            var args = new HttpMockServerArgs(_hostUrls, _hostArgs);
            return new StartupBasedBuilder<T>(args);
        }
    }
}