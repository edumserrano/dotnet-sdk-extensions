using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary
{
    public static class HttpStatusCodesExtensions
    {
        public static IEnumerable<HttpStatusCode> GetTransientHttpStatusCodes()
        {
            return Enum
                .GetValues(typeof(HttpStatusCode))
                .Cast<HttpStatusCode>()
                .Where(x => x is >= HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout);
        }
    }
}
