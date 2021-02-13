namespace OptionsValue
{
    public interface ISomeClass
    {
        string GetMessage();
    }

    public class SomeClass : ISomeClass
    {
        private readonly MyOptions1 _myOptions1;
        private readonly MyOptions2 _myOptions2;

        public SomeClass(MyOptions1 myOptions1, MyOptions2 myOptions2)
        {
            _myOptions1 = myOptions1;
            _myOptions2 = myOptions2;
        }

        public string GetMessage()
        {
            return $"appsettings says: {_myOptions1.SomeOption} and {_myOptions2.SomeOption}";
        }
    }
}
