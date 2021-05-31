using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace DotNet.Sdk.Extensions.Polly.Http.Fallback.FallbackHttpResponseMessages
{
    public class AbortedHttpResponseMessage : HttpResponseMessage
    {
        public AbortedHttpResponseMessage(TaskCanceledException exception)
        {
            StatusCode = HttpStatusCode.InternalServerError;
            Exception = exception;
        }

        public TaskCanceledException Exception { get; }
    }
}