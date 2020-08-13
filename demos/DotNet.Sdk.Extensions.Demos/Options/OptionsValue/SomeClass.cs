namespace DotNet.Sdk.Extensions.Demos.Options.OptionsValue
{
    public interface ISomeClass
    {
        string GetMessage();
    }

    public class SomeClass : ISomeClass
    {
        private readonly MyOptions _myOptions;

        public SomeClass(MyOptions myOptions)
        {
            _myOptions = myOptions;
        }

        public string GetMessage()
        {
            return $"appsettings says: {_myOptions.SomeOption}";
        }
    }
}
