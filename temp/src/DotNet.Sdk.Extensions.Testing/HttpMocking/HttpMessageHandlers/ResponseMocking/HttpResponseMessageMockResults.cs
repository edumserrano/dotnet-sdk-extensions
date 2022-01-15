namespace DotNet.Sdk.Extensions.Testing.HttpMocking.HttpMessageHandlers.ResponseMocking
{
    /// <summary>
    /// Represents the possible outcomes from executing an <see cref="HttpResponseMessageMock"/>.
    /// </summary>
    public enum HttpResponseMessageMockResults
    {
        /// <summary>
        /// The <see cref="HttpResponseMessageMock"/> was not executed.
        /// </summary>
        Skipped,

        /// <summary>
        /// The <see cref="HttpResponseMessageMock"/> was executed.
        /// </summary>
        Executed,
    }
}
