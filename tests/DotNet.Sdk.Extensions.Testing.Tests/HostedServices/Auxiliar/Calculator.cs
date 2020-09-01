namespace DotNet.Sdk.Extensions.Testing.Tests.HostedServices.Auxiliar
{
    public interface ICalculator
    {
        int Sum(int left, int right);
    }

    public class Calculator : ICalculator
    {
        public int Sum(int left, int right)
        {
            return left + right;
        }
    }
}
