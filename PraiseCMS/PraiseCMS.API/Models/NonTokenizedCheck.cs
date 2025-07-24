namespace PraiseCMS.API.Models
{
    public class NonTokenizedCheck
    {
        public string transaction_type { get; set; }
        public string check_number { get; set; }
        public string transit_number { get; set; }
        public string account_number { get; set; }
        public string amount { get; set; }
        public string magnetic_ink_check_reader { get; set; }
        public string name_on_check { get; set; }
        public string driver_license { get; set; }
        public string social_security_number { get; set; }
        public string date_of_birth { get; set; }
        public string state_code { get; set; }
        public string check_type { get; set; }
        public string account_type { get; set; }
        public string alliance_number { get; set; }
        public string authorization_option_form { get; set; }
        public string authorization_option_voice { get; set; }
        public string bill_to_street { get; set; }
        public string bill_to_city { get; set; }
        public string bill_to_state { get; set; }
        public string bill_to_postal_code { get; set; }
        public string bill_to_country { get; set; }
        public string city_of_account { get; set; }
        public string customer_id { get; set; }
        public string email { get; set; }
        public string external_ip { get; set; }
        public string invoice_number { get; set; }
        public string phone { get; set; }
        public string payment_reference_number { get; set; }
        public string raw_magnetic_ink_check_reader { get; set; }
        public string standard_entry_class_codes_type { get; set; }
    }
}