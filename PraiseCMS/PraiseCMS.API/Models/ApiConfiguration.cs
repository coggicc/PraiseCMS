using PraiseCMS.Shared.Shared;
using System.Configuration;

namespace PraiseCMS.API.Models
{
    public class ApiConfiguration
    {
        public ApiCredentials Credentials { get; set; }
        public string BaseUrl { get; set; }
        public string LeadUrl { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public readonly int ResellerKey = 193;
        public string IdentityEndpoint { get; set; }
        public string TermsAndConditionsEndpoint { get; set; }
        public string AcceptTermsEndpoint { get; set; }
        public string LeadApiEndpoint { get; set; }
        public string CustomerEndpoint { get; set; }
        public string PaymentSafeEndpoint { get; set; }
        public string CardEndpoint { get; set; }
        public string TransactionsEndpoint { get; set; }
        public string CheckEndpoint { get; set; }
        public string VerifyBankRoutingEndpoint { get; set; }

        public ApiConfiguration()
        {
            var isDevEnvironment = AppEnvironment.IsDev;

            Credentials = new ApiCredentials();

            if (isDevEnvironment)
            {
                BaseUrl = "https://stage.paragonsolutions.com/api/v2/";
                LeadUrl = "https://stage.leadapi.paragonsolutions.com/v2/";
                Username = "193leadapi";
                Password = "15H43ByL";
            }
            else
            {
                BaseUrl = "https://paragonsolutions.com/api/v2/";
                LeadUrl = "https://leadapi.paragonsolutions.com/v2/";
                Username = string.Empty;
                Password = string.Empty;
            }
            IdentityEndpoint = "Identity";
            TermsAndConditionsEndpoint = "PDF/correlationId";
            AcceptTermsEndpoint = "AcceptTermsAndConditions";
            LeadApiEndpoint = "lead";
            CustomerEndpoint = "customers";
            PaymentSafeEndpoint = "paymentsafe";
            CardEndpoint = $"{PaymentSafeEndpoint}/cards";
            TransactionsEndpoint = "transactions";
            CheckEndpoint = $"{PaymentSafeEndpoint}/checks";
            VerifyBankRoutingEndpoint = "utility/abalookup";
        }
    }
}