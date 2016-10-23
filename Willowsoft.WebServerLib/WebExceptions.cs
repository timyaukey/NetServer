using System;

namespace Willowsoft.WebServerLib
{
    public class WebExceptions
    {
        /// <summary>
        /// Exception to throw when the server must respond to a request
        /// with an HTTP failure status. Is subclassed for individual status values.
        /// </summary>
        abstract public class HttpStatusException : Exception
        {
            public readonly HttpStatus Status;

            public HttpStatusException(HttpStatus status)
            {
                Status = status;
            }

            /// <summary>
            /// Defensive coding to force subclasses to provide a meaningful message.
            /// </summary>
            /// <returns></returns>
            abstract public string GetExceptionText();

            public override string Message
            {
                get { return GetExceptionText(); }
            }
        }

        public class ResourceNotFound : HttpStatusException
        {
            public readonly string RequestURI;

            public ResourceNotFound(string requestURI)
                : base(HttpStatus.NotFound)
            {
                RequestURI = requestURI;
            }

            public override string GetExceptionText()
            {
                return "Resource not found: " + RequestURI;
            }
        }

        public class BadRequest : HttpStatusException
        {
            public readonly string RequestError;

            public BadRequest(string requestError)
                : base(HttpStatus.BadRequest)
            {
                RequestError = requestError;
            }

            public override string GetExceptionText()
            {
                return "Bad Request: " + RequestError;
            }
        }

        public class NotImplemented : HttpStatusException
        {
            public readonly string MissingFeature;

            public NotImplemented(string missingFeature)
                : base(HttpStatus.NotImplemented)
            {
                MissingFeature = missingFeature;
            }

            public override string GetExceptionText()
            {
                return "Not Implemented: " + MissingFeature;
            }
        }
    }
}
