using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary
{
    internal class NumberOfCallsDelegatingHandler : DelegatingHandler
    {
        public int NumberOfHttpRequests { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            NumberOfHttpRequests++;
            return base.SendAsync(request, cancellationToken);
        }

        public void Reset()
        {
            NumberOfHttpRequests = 0;
        }
    }
}
