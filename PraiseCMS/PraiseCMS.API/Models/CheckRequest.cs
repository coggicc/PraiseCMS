namespace PraiseCMS.API.Models
{
    public class CheckRequest
    {
        public string customer_key { get; set; }
        public string check_type { get; set; }
        public string account_type { get; set; }
        public string account_number { get; set; }
        public string transit_number { get; set; }
        public string name_on_check { get; set; }
        public string day_phone { get; set; }
        public string street1 { get; set; }
        public string city { get; set; }
        public string state_id { get; set; }
        public string zip_code { get; set; }
        public string secc_type { get; set; }
        public string auth_option_form { get; set; }
        public string auth_option_voice { get; set; }
    }
}