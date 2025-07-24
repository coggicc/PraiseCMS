using Newtonsoft.Json;
using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;

namespace PraiseCMS.API.Helpers
{
    public static class DebugHelper
    {
        public static void LogSerializedPayload(dynamic payload)
        {
            string serializedPayload = JsonConvert.SerializeObject(payload);
            Debug.WriteLine("Serialized Payload: " + serializedPayload);
        }

        public static void LogHttpRequestDetails(HttpRequestMessage httpRequest)
        {
            if (httpRequest == null)
            {
                throw new ArgumentNullException(nameof(httpRequest));
            }

            Debug.WriteLine($"Request Method: {httpRequest.Method}");
            Debug.WriteLine($"Request URI: {httpRequest.RequestUri}");

            LogRequestHeaders(httpRequest.Headers);
        }

        private static void LogRequestHeaders(HttpHeaders headers)
        {
            foreach (var header in headers)
            {
                Debug.WriteLine($"{header.Key}: {string.Join(",", header.Value)}");
            }
        }
    }
}