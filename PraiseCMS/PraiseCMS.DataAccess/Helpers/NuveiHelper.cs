using PraiseCMS.API.Models;
using PraiseCMS.API.Services;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace PraiseCMS.DataAccess.Helpers
{
    public class NuveiHelper
    {
        private readonly ApiService apiService;

        public NuveiHelper(TokenManager tokenManager)
        {
            apiService = new ApiService(tokenManager);
        }

        #region TermsAndConditions
        public async Task<TermsAndConditionsResponse> GetTermsAndConditionsAsync()
        {
            return await ExecuteApiCallAsync(() => apiService.GetTermsAndConditionsAsync());
        }

        public async Task<bool> AcceptTermsAndConditionsAsync(string correlationId, string termsAndConditionsId)
        {
            return await ExecuteApiCallAsync(() => apiService.AcceptTermsAndConditionsAsync(correlationId, termsAndConditionsId));
        }
        #endregion

        #region Leads
        public async Task<LeadApiResponse> CreateLeadAsync(string correlationId, LeadApiRequest leadApiRequest)
        {
            return await ExecuteApiCallAsync(() => apiService.SubmitLeadAsync(correlationId, leadApiRequest));
        }
        #endregion

        #region Customers
        public async Task<CustomerResponse> CreateCustomerAsync(CustomerRequest customerRequest, ApiCredentials apiCredentials)
        {
            return await ExecuteApiCallAsync(() => apiService.CreateCustomerAsync(customerRequest, apiCredentials));
        }

        public async Task<CustomerDetailsResponse> GetCustomerDetailsAsync(string customerKey, ApiCredentials apiCredentials)
        {
            return await ExecuteApiCallAsync(() => apiService.GetCustomerDetailsAsync(customerKey, apiCredentials));
        }

        public async Task<CustomerPaymentMethodsResponse> GetCustomerPaymentMethods(string customerKey, ApiCredentials apiCredentials)
        {
            return await ExecuteApiCallAsync(() => apiService.GetCustomerPaymentMethods(customerKey, apiCredentials));
        }

        #endregion        

        #region Cards
        public async Task<CreateCardResponse> CreateCardAsync(CardRequest cardRequest, ApiCredentials apiCredentials)
        {
            return await ExecuteApiCallAsync(() => apiService.CreateCardAsync(cardRequest, apiCredentials));
        }

        public async Task<CardDetailsResponse> GetCardDetailsAsync(string cardKey, ApiCredentials apiCredentials)
        {
            return await ExecuteApiCallAsync(() => apiService.GetCardDetailsAsync(cardKey, apiCredentials));
        }

        public async Task<TransactionResponse> ProcessCreditCardTransactionAsync(CardTransactionRequest cardTransactionRequest, ApiCredentials apiCredentials)
        {
            return await ExecuteApiCallAsync(() => apiService.ProcessCreditCardTransactionAsync(cardTransactionRequest, apiCredentials));
        }

        public async Task<PaymentSafeDeletionResponse> DeleteCardAsync(CardDeletionRequest cardDeletionRequest, ApiCredentials apiCredentials)
        {
            return await ExecuteApiCallAsync(() => apiService.DeleteCardAsync(cardDeletionRequest, apiCredentials));
        }
        #endregion

        #region Checks
        public async Task<VerifyBankRoutingResponse> VerifyBankRoutingAsync(string routingNumber, ApiCredentials apiCredentials)
        {
            return await ExecuteApiCallAsync(() => apiService.VerifyBankRoutingAsync(routingNumber, apiCredentials));
        }

        public async Task<CreateCheckResponse> CreateCheckAsync(CheckRequest checkRequest, ApiCredentials apiCredentials)
        {
            return await ExecuteApiCallAsync(() => apiService.CreateCheckAsync(checkRequest, apiCredentials));
        }

        public async Task<CheckDetailsResponse> GetCheckDetailsAsync(string checkKey, ApiCredentials apiCredentials)
        {
            return await ExecuteApiCallAsync(() => apiService.GetCheckDetailsAsync(checkKey, apiCredentials));
        }

        public async Task<TransactionResponse> ProcessCheckTransactionAsync(CheckTransactionRequest checkTransactionRequest, ApiCredentials apiCredentials)
        {
            return await ExecuteApiCallAsync(() => apiService.ProcessCheckTransactionAsync(checkTransactionRequest, apiCredentials));
        }

        public async Task<PaymentSafeDeletionResponse> DeleteCheckAsync(CheckDeletionRequest checkDeletionRequest, ApiCredentials apiCredentials)
        {
            return await ExecuteApiCallAsync(() => apiService.DeleteCheckAsync(checkDeletionRequest, apiCredentials));
        }
        #endregion

        #region Transactions
        public async Task<TransactionResponse> CreateTransactionAsync(string paymentMethod, object transactionRequest, ApiCredentials apiCredentials)
        {
            switch (paymentMethod)
            {
                case DigitalPaymentMethods.Card:
                    if (transactionRequest is CardTransactionRequest cardTransactionRequest)
                    {
                        return await ExecuteApiCallAsync(() => apiService.ProcessCreditCardTransactionAsync(cardTransactionRequest, apiCredentials));
                    }
                    break;

                case DigitalPaymentMethods.ACH:
                    if (transactionRequest is CheckTransactionRequest checkTransactionRequest)
                    {
                        return await ExecuteApiCallAsync(() => apiService.ProcessCheckTransactionAsync(checkTransactionRequest, apiCredentials));
                    }
                    break;

                default:
                    // Handle unsupported payment methods
                    break;
            }

            // Return an appropriate response or throw an exception for unsupported cases
            return null;
        }

        public bool TransactionSucceed(string responseCode)
        {
            const string approvalCode = "00";
            return responseCode == approvalCode;
        }

        public async Task<TransactionResponse> VerifyTransaction(string paymenetReferenceNumber, ApiCredentials apiCredentials)
        {
            return await ExecuteApiCallAsync(() => apiService.VerifyTransaction(paymenetReferenceNumber, apiCredentials));
        }

        #endregion

        public ApiCredentials GetApiCredentials(string userName, string password)
        {
            return new ApiCredentials { Username = userName.Decrypt(), Password = password.Decrypt() };
        }

        private async Task<T> ExecuteApiCallAsync<T>(Func<Task<T>> apiCall) where T : class, new()
        {
            try
            {
                return await apiCall();
            }
            catch (ApiException ex)
            {
                Debug.WriteLine($"API Error: {ex.Message}");

                // If T is a response model with "result" field, return a default failed response
                if (typeof(T).GetProperty("result") != null)
                {
                    var failedResponse = new T();
                    typeof(T).GetProperty("result")?.SetValue(failedResponse, "-1");
                    typeof(T).GetProperty("errorMessage")?.SetValue(failedResponse, ex.Message);
                    return failedResponse;
                }

                return null; // Return null if the model doesn't support result/errorMessage
            }
        }

        private async Task<bool> ExecuteApiCallAsync(Func<Task<bool>> apiCall)
        {
            try
            {
                return await apiCall();
            }
            catch (ApiException ex)
            {
                Debug.WriteLine($"API Error: {ex.Message}");
                return false; // Return false to indicate failure
            }
        }
    }
}