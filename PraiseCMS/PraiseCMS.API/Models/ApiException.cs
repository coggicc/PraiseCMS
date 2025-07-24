using System;
using System.Linq;
using System.Net;

namespace PraiseCMS.API.Models
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string ResponseContent { get; }

        public ApiException(string message, HttpStatusCode statusCode = HttpStatusCode.InternalServerError, string responseContent = null)
            : base(message)
        {
            StatusCode = statusCode;
            ResponseContent = responseContent;
        }
    }
}