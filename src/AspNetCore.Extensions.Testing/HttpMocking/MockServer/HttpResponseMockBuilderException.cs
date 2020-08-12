﻿using System;

namespace AspNetCore.Extensions.Testing.HttpMocking.MockServer
{
    internal class HttpResponseMockBuilderException : Exception
    {
        public HttpResponseMockBuilderException(string message)
            : base(message)
        {
        }

        public HttpResponseMockBuilderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}