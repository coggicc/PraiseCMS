namespace PraiseCMS.API.Models
{
    public class LeadApiResponse
    {
        public string result { get; set; }
        public string message { get; set; }
        public string homebase_merchant_id { get; set; }
        public string company_legal_name { get; set; }
        public string merchant_key { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public string api_username { get; set; }
        public string api_password { get; set; }
        public string user_activation_url { get; set; }
    }
}