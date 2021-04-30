using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using Polly;

namespace DotNet.Sdk.Extensions.Tests.Polly.Http.Auxiliary.Polly
{
    public static class PolicyReflectionExtensions
    {
        public static ExceptionPredicates GetExceptionPredicates(this PolicyBase policy)
        {
            return policy.GetInstanceProperty<ExceptionPredicates>("ExceptionPredicates");
        }

        public static int GetExceptionPredicatesCount(this ExceptionPredicates exceptionPredicates)
        {
            var exceptionPredicatesField = exceptionPredicates.GetInstanceField<IList<ExceptionPredicate>>("_predicates");
            if (exceptionPredicatesField is null) // even though there's a warning about this never being null it can actually be null
            {
                return 0;
            }
            return exceptionPredicatesField.Count;
        }

        public static bool HandlesException<T>(this ExceptionPredicates exceptionPredicates) where T : Exception
        {
            var exception = Activator.CreateInstance<T>() as Exception;
            return exceptionPredicates.HandlesException(exception);
        }

        public static bool HandlesException(this ExceptionPredicates exceptionPredicates, Exception exception)
        {
            var exceptionPredicate = exceptionPredicates.FirstMatchOrDefault(exception);
            return exceptionPredicate != null;
        }

        public static ResultPredicates<T> GetResultPredicates<T>(this PolicyBase<T> policy)
        {
            return policy.GetInstanceProperty<ResultPredicates<T>>("ResultPredicates");
        }

        public static int GetResultPredicatesCount<T>(this ResultPredicates<T> resultPredicates)
        {
            var resultPredicatesField = resultPredicates.GetInstanceField<IList<ResultPredicate<T>>>("_predicates");
            if (resultPredicatesField is null) // even though there's a warning about this never being null it can actually be null
            {
                return 0;
            }
            return resultPredicatesField.Count;
        }

        public static bool HandlesResult<T>(this ResultPredicates<T> resultPredicates, T result)
        {
            return resultPredicates.AnyMatch(result);
        }

        public static bool HandlesTransientHttpStatusCode(this ResultPredicates<HttpResponseMessage> resultPredicates)
        {
            var httpStatusCodes = Enum.GetValues(typeof(HttpStatusCode)).Cast<HttpStatusCode>();
            foreach (var httpStatusCode in httpStatusCodes)
            {
                var resultMatch = resultPredicates.AnyMatch(new HttpResponseMessage(httpStatusCode));
                var isTransientHttpStatusCode = httpStatusCode is >= HttpStatusCode.InternalServerError or HttpStatusCode.RequestTimeout;

                // if it's a transient http response then should be match, abort if it doesn't match
                if (isTransientHttpStatusCode && !resultMatch)
                {
                    return false;
                }

                // if it is NOT a transient http response then should NOT be match, abort if it matches
                if (!isTransientHttpStatusCode && resultMatch)
                {
                    return false;
                }
            }

            return true;
        }
    }
}
