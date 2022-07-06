namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.OutOfProcess.ResponseMocking;

[Trait("Category", XUnitCategories.HttpMockingOutOfProcess)]
public class HttpResponseMockBuilderTests
{
    /// <summary>
    /// Validates the arguments for the <seealso cref="HttpResponseMockBuilder.Where(Func{HttpRequest, bool})"/> method.
    /// </summary>
    [Fact]
    public void Where1ValidateArguments()
    {
        var builder = new HttpResponseMockBuilder();
        var exception = Should.Throw<ArgumentNullException>(() => builder.Where((Func<HttpRequest, bool>)null!));
        exception.Message.ShouldBe("Value cannot be null. (Parameter 'predicate')");
    }

    /// <summary>
    /// Validates the arguments for the <seealso cref="HttpResponseMockBuilder.Where(HttpResponseMockPredicateAsyncDelegate)"/> method.
    /// </summary>
    [Fact]
    public void Where2ValidateArguments()
    {
        var builder = new HttpResponseMockBuilder();
        var exception = Should.Throw<ArgumentNullException>(() => builder.Where((HttpResponseMockPredicateAsyncDelegate)null!));
        exception.Message.ShouldBe("Value cannot be null. (Parameter 'predicate')");
    }

    /// <summary>
    /// Validates the arguments for the <seealso cref="HttpResponseMockBuilder.RespondWith(Action{HttpResponse})"/> method.
    /// </summary>
    [Fact]
    public void Respond1WithValidateArguments()
    {
        var builder = new HttpResponseMockBuilder();
        var exception = Should.Throw<ArgumentNullException>(() => builder.RespondWith((Action<HttpResponse>)null!));
        exception.Message.ShouldBe("Value cannot be null. (Parameter 'configureHttpResponse')");
    }

    /// <summary>
    /// Validates the arguments for the <seealso cref="HttpResponseMockBuilder.RespondWith(Action{HttpRequest, HttpResponse})"/> method.
    /// </summary>
    [Fact]
    public void Respond2WithValidateArguments()
    {
        var builder = new HttpResponseMockBuilder();
        var exception = Should.Throw<ArgumentNullException>(() => builder.RespondWith((Action<HttpRequest, HttpResponse>)null!));
        exception.Message.ShouldBe("Value cannot be null. (Parameter 'handler')");
    }

    /// <summary>
    /// Validates the arguments for the <seealso cref="HttpResponseMockBuilder.RespondWith(HttpResponseMockHandlerAsyncDelegate)"/> method.
    /// </summary>
    [Fact]
    public void Respond3WithValidateArguments()
    {
        var builder = new HttpResponseMockBuilder();
        var exception = Should.Throw<ArgumentNullException>(() => builder.RespondWith(null!));
        exception.Message.ShouldBe("Value cannot be null. (Parameter 'handlerAsync')");
    }

    /// <summary>
    /// Validates that the predicate can only be set once.
    /// </summary>
    [Fact]
    public void WhereCanOnlyBeDefinedOnce()
    {
        var builder = new HttpResponseMockBuilder();
        builder.Where(_ => false);
        var exception = Should.Throw<InvalidOperationException>(() => builder.Where(_ => true));
        exception.Message.ShouldBe("HttpResponseMockBuilder.Where condition already configured.");
    }

    /// <summary>
    /// Validates that the <seealso cref="HttpResponse"/> to be returned can only be configured once.
    /// </summary>
    [Fact]
    public void RespondWithCanOnlyBeDefinedOnce()
    {
        var builder = new HttpResponseMockBuilder();
        builder.RespondWith(httpResponse => httpResponse.StatusCode = StatusCodes.Status200OK);
        var exception = Should.Throw<InvalidOperationException>(() => builder.RespondWith(httpResponse => httpResponse.StatusCode = StatusCodes.Status200OK));
        exception.Message.ShouldBe("HttpResponseMockBuilder.RespondWith already configured.");
    }

    /// <summary>
    /// Validates that you must at least specify the <seealso cref="HttpResponse"/> to be returned.
    /// </summary>
    [Fact]
    public void RespondWithIsMandatory()
    {
        var builder = new HttpResponseMockBuilder();
        var exception = Should.Throw<InvalidOperationException>(() => builder.Build());
        exception.Message.ShouldBe("HttpResponse not configured for HttpResponseMock. Use HttpResponseMockBuilder.RespondWith to configure it.");
    }
}
