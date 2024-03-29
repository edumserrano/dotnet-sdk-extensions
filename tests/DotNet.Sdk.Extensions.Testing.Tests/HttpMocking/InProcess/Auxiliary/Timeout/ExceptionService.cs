namespace DotNet.Sdk.Extensions.Testing.Tests.HttpMocking.InProcess.Auxiliary.Timeout;

[SuppressMessage("Performance", "CA1812:Avoid uninstantiated internal classes", Justification = "Used as generic type param.")]
internal sealed class ExceptionService
{
    private readonly List<Exception> _exceptions = [];

    public IReadOnlyCollection<Exception> Exceptions
    {
        get { return _exceptions; }
    }

    public void AddException(Exception exception)
    {
        _exceptions.Add(exception);
    }
}
