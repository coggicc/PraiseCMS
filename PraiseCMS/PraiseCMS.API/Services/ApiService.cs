using Newtonsoft.Json;
using PraiseCMS.API.Helpers;
using PraiseCMS.API.Models;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PraiseCMS.API.Services
{
    public class ApiService
    {
        #region Boilerplate
        private readonly HttpClient httpClient;
        private readonly ApiConfiguration apiConfig;
        private readonly TokenManager tokenManager;

        public ApiService(TokenManager tokenManager)
        {
            // Initialize HttpClient and ApiConfiguration within the ApiService
            httpClient = new HttpClient();
            apiConfig = new ApiConfiguration();
            this.tokenManager = tokenManager;
        }

        private string GetFullApiUrl(string endpoint, bool useLeadUrl = false)
        {
            // Use either the BaseUrl or LeadUrl based on the provided flag
            string baseUrl = useLeadUrl ? apiConfig.LeadUrl : apiConfig.BaseUrl;

            // Construct the full URL based on the environment
            return $"{baseUrl}{endpoint}";
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, string endpoint, HttpContent content, string token = null)
        {
            var httpRequest = new HttpRequestMessage(method, endpoint)
            {
                Content = content
            };

            if (!string.IsNullOrEmpty(token))
            {
                httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

#if DEBUG
            DebugHelper.LogHttpRequestDetails(httpRequest);

            if (httpRequest.Content != null)
            {
                var requestContent = await httpRequest.Content.ReadAsStringAsync();

                DebugHelper.LogSerializedPayload(requestContent);
            }
#endif

            // Send the request and return the response
            return await httpClient.SendAsync(httpRequest);
        }

        private async Task<HttpResponseMessage> SendPostRequestAsync(string endpoint, object data, string token = null, string routeParameters = null)
        {
            var jsonContent = new StringContent(JsonConvert.SerializeObject(data));
            jsonContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            string requestUrl = string.IsNullOrEmpty(routeParameters) ? endpoint : $"{endpoint}/{routeParameters}";

            return await httpClient.PostAsync(requestUrl, jsonContent);
        }

        private async Task<HttpResponseMessage> SendGetRequestAsync(string endpoint, string token)
        {
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await httpClient.GetAsync(endpoint);
        }

        private async Task<HttpResponseMessage> SendDeleteRequestAsync(string endpoint, CardDeletionRequest request, string token = null)
        {
            var jsonContent = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            return await SendRequestAsync(HttpMethod.Delete, endpoint, content, token);
        }

        private async Task<HttpResponseMessage> SendDeleteRequestAsync(string endpoint, CheckDeletionRequest request, string token = null)
        {
            var jsonContent = JsonConvert.SerializeObject(request);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            return await SendRequestAsync(HttpMethod.Delete, endpoint, content, token);
        }
        #endregion

        #region Bearer Token
        public async Task<string> GetBearerTokenAsync(ApiCredentials apiCredentials)
        {
            // Check if the token is already obtained and not expired
            var accessToken = tokenManager.GetAccessToken();

            if (!string.IsNullOrEmpty(accessToken))
            {
                return RemoveBearerPrefix(accessToken);
            }

            // If the token is expired or not obtained, retrieve a new one
            var identityEndpoint = GetFullApiUrl(apiConfig.IdentityEndpoint);

            var identityRequest = new
            {
                apiCredentials.Username,
                apiCredentials.Password
            };

            var response = await SendPostRequestAsync(identityEndpoint, identityRequest);

            var accessTokenResponse = JsonConvert.DeserializeObject<AccessTokenResponse>(await response.Content.ReadAsStringAsync());

            accessToken = RemoveBearerPrefix(accessTokenResponse?.access_token);

            tokenManager.StoreAccessToken(accessTokenResponse);

            return accessToken;
        }

        private string RemoveBearerPrefix(string accessToken)
        {
            if (!string.IsNullOrEmpty(accessToken))
            {
                int index = accessToken.IndexOf("Bearer ", StringComparison.OrdinalIgnoreCase);

                if (index == 0)
                {
                    return accessToken.Substring("Bearer ".Length).Trim();
                }
            }

            return accessToken;
        }
        #endregion

        #region TermsAndConditions
        public async Task<TermsAndConditionsResponse> GetTermsAndConditionsAsync()
        {
            var apiCredentials = new ApiCredentials { Username = apiConfig.Username, Password = apiConfig.Password };

            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var termsAndConditionsEndpoint = GetFullApiUrl(apiConfig.TermsAndConditionsEndpoint);

            var response = await SendGetRequestAsync(termsAndConditionsEndpoint, bearerToken);

            return await response.Content.ReadAsAsync<TermsAndConditionsResponse>();
        }

        public async Task<bool> AcceptTermsAndConditionsAsync(string correlationId, string termsAndConditionsId)
        {
            var acceptTermsEndpoint = GetFullApiUrl(apiConfig.AcceptTermsEndpoint);
            var acceptTermsRequest = new
            {
                termsAndConditionsAccepted = true,
                correlationId,
                termsAndConditionsId,
                resellerKey = 193
            };

            var response = await SendPostRequestAsync(acceptTermsEndpoint, acceptTermsRequest);

            return response.IsSuccessStatusCode;
        }
        #endregion

        #region Leads
        public async Task<LeadApiResponse> SubmitLeadAsync(string correlationId, LeadApiRequest leadApiRequest)
        {
            var identityRequest = new
            {
                apiConfig.Username,
                apiConfig.Password,
                apiConfig.ResellerKey
            };

            leadApiRequest.reseller_username = identityRequest.Username;
            leadApiRequest.reseller_password = identityRequest.Password;
            leadApiRequest.reseller_key = identityRequest.ResellerKey;

            var apiCredentials = new ApiCredentials { Username = apiConfig.Username, Password = apiConfig.Password };

            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var leadApiEndpoint = GetFullApiUrl(apiConfig.LeadApiEndpoint, useLeadUrl: true);

            leadApiRequest.correlation_id = correlationId;

            var response = await SendPostRequestAsync(leadApiEndpoint, leadApiRequest, bearerToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<LeadApiResponse>();
            }

            // Capture error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException($"API call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {errorContent}");
        }

        #endregion

        #region Customers
        public async Task<CustomerResponse> CreateCustomerAsync(CustomerRequest customerRequest, ApiCredentials apiCredentials)
        {
            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var customerEndpoint = GetFullApiUrl(apiConfig.CustomerEndpoint);

            var response = await SendPostRequestAsync(customerEndpoint, customerRequest, bearerToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<CustomerResponse>();
            }

            // Capture error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException($"API call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {errorContent}");
        }

        public async Task<CustomerDetailsResponse> GetCustomerDetailsAsync(string customerKey, ApiCredentials apiCredentials)
        {
            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var customerDetailsEndpoint = $"{GetFullApiUrl(apiConfig.CustomerEndpoint)}/{customerKey}";

            var response = await SendGetRequestAsync(customerDetailsEndpoint, bearerToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<CustomerDetailsResponse>();
            }

            // Capture error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException($"API call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {errorContent}");
        }

        public async Task<CustomerPaymentMethodsResponse> GetCustomerPaymentMethods(string customerKey, ApiCredentials apiCredentials)
        {
            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var customerPaymentMethodsEndpoint = $"{GetFullApiUrl(apiConfig.CustomerEndpoint)}/{customerKey}/{apiConfig.PaymentSafeEndpoint}";

            var response = await SendGetRequestAsync(customerPaymentMethodsEndpoint, bearerToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<CustomerPaymentMethodsResponse>();
            }

            // Capture error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException($"API call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {errorContent}");
        }
        #endregion

        #region Cards
        public async Task<CreateCardResponse> CreateCardAsync(CardRequest cardRequest, ApiCredentials apiCredentials)
        {
            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var cardEndpoint = GetFullApiUrl(apiConfig.CardEndpoint);

#if DEBUG
            DebugHelper.LogSerializedPayload(cardRequest);
#endif
            var response = await SendPostRequestAsync(cardEndpoint, cardRequest, bearerToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<CreateCardResponse>();
            }

            // Capture error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException($"API call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {errorContent}");
        }

        public async Task<CardDetailsResponse> GetCardDetailsAsync(string cardKey, ApiCredentials apiCredentials)
        {
            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var cardDetailsEndpoint = GetFullApiUrl($"{apiConfig.CardEndpoint}/{cardKey}");

            var response = await SendGetRequestAsync(cardDetailsEndpoint, bearerToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<CardDetailsResponse>();
            }

            // Capture error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException($"API call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {errorContent}");
        }

        public async Task<TransactionResponse> ProcessCreditCardTransactionAsync(CardTransactionRequest cardTransactionRequest, ApiCredentials apiCredentials)
        {
            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var transactionEndpoint = GetFullApiUrl(apiConfig.TransactionsEndpoint);

#if DEBUG
            DebugHelper.LogSerializedPayload(cardTransactionRequest);
#endif

            var response = await SendPostRequestAsync(transactionEndpoint, cardTransactionRequest, bearerToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<TransactionResponse>();
            }

            // Capture error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException($"API call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {errorContent}");
        }

        public async Task<PaymentSafeDeletionResponse> DeleteCardAsync(CardDeletionRequest request, ApiCredentials apiCredentials)
        {
            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var cardEndpoint = $"{GetFullApiUrl(apiConfig.CardEndpoint)}/{request.card_key}";

            var response = await SendDeleteRequestAsync(cardEndpoint, request, bearerToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<PaymentSafeDeletionResponse>();
            }

            // Capture error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException($"API call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {errorContent}");
        }
        #endregion

        #region Checks
        public async Task<CreateCheckResponse> CreateCheckAsync(CheckRequest checkRequest, ApiCredentials apiCredentials)
        {
            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var checkEndpoint = GetFullApiUrl(apiConfig.CheckEndpoint);

#if DEBUG
            DebugHelper.LogSerializedPayload(checkRequest);
#endif

            var response = await SendPostRequestAsync(checkEndpoint, checkRequest, bearerToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<CreateCheckResponse>();
            }

            // Capture error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException($"API call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {errorContent}");
        }

        public async Task<CheckDetailsResponse> GetCheckDetailsAsync(string checkKey, ApiCredentials apiCredentials)
        {
            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var checkDetailsEndpoint = $"{GetFullApiUrl(apiConfig.CheckEndpoint)}/{checkKey}";

            var response = await SendGetRequestAsync(checkDetailsEndpoint, bearerToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<CheckDetailsResponse>();
            }

            // Capture error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException($"API call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {errorContent}");
        }

        public async Task<TransactionResponse> ProcessCheckTransactionAsync(CheckTransactionRequest checkTransactionRequest, ApiCredentials apiCredentials)
        {
            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var transactionEndpoint = GetFullApiUrl(apiConfig.TransactionsEndpoint);

#if DEBUG
            DebugHelper.LogSerializedPayload(checkTransactionRequest);
#endif

            var response = await SendPostRequestAsync(transactionEndpoint, checkTransactionRequest, bearerToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<TransactionResponse>();
            }

            // Capture error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException($"API call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {errorContent}");
        }

        public async Task<PaymentSafeDeletionResponse> DeleteCheckAsync(CheckDeletionRequest request, ApiCredentials apiCredentials)
        {
            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var checkEndpoint = $"{GetFullApiUrl(apiConfig.CheckEndpoint)}/{request.check_key}";

            var response = await SendDeleteRequestAsync(checkEndpoint, request, bearerToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<PaymentSafeDeletionResponse>();
            }

            // Capture error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException($"API call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {errorContent}");
        }

        public async Task<VerifyBankRoutingResponse> VerifyBankRoutingAsync(string routingNumber, ApiCredentials apiCredentials)
        {
            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var verifyBankRoutingEndpoint = GetFullApiUrl(apiConfig.VerifyBankRoutingEndpoint);

            var response = await SendPostRequestAsync(verifyBankRoutingEndpoint, null, bearerToken, routingNumber);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<VerifyBankRoutingResponse>();
            }

            // Capture error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException($"API call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {errorContent}");
        }
        #endregion

        #region VerifyTransaction
        public async Task<TransactionResponse> VerifyTransaction(string paymenetReferenceNumber, ApiCredentials apiCredentials)
        {
            var bearerToken = await GetBearerTokenAsync(apiCredentials);

            var transactionsEndpoint = $"{GetFullApiUrl(apiConfig.TransactionsEndpoint)}/{paymenetReferenceNumber}";

            var response = await SendGetRequestAsync(transactionsEndpoint, bearerToken);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadAsAsync<TransactionResponse>();
            }

            // Capture error details from response
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new ApiException($"API call failed: {(int)response.StatusCode} {response.ReasonPhrase}. Response: {errorContent}");
        }
        #endregion
    }
}