using System;
using System.Net;
using System.Net.Http;
using DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking;
using Shouldly;
using Xunit;

namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    public class HttpResponseMessageMockResultTests
    {
        /// <summary>
        /// Tests that the <seealso cref="HttpResponseMessageMockResult.Skipped"/> produces a result
        /// with the correct data.
        /// </summary>
        //[Fact]
        //public void SkippedResult()
        //{
        //    var httpResponseMessageMockResult = HttpResponseMessageMockResult.Skipped();
        //    httpResponseMessageMockResult.Status.ShouldBe(HttpResponseMessageMockResults.Skipped);
        //    var exception = Should.Throw<InvalidOperationException>(() => httpResponseMessageMockResult.HttpResponseMessage);
        //    exception.Message.ShouldBe("Cannot retrieve HttpResponseMessage unless Status is Executed. Status is Skipped.");
        //}

        ///// <summary>
        ///// Tests that the <seealso cref="HttpResponseMessageMockResult.Executed"/> produces a result
        ///// with the correct data.
        ///// </summary>
        //[Fact]
        //public void ExecutedResult()
        //{
        //    var httpResponseMessageMockResult = HttpResponseMessageMockResult.Executed(new HttpResponseMessage(HttpStatusCode.BadGateway));
        //    httpResponseMessageMockResult.Status.ShouldBe(HttpResponseMessageMockResults.Executed);
        //    httpResponseMessageMockResult.HttpResponseMessage.StatusCode.ShouldBe(HttpStatusCode.BadGateway);
        //}
    }
}
