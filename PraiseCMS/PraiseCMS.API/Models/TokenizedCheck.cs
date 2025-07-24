namespace PraiseCMS.API.Models
{
    public class TokenizedCheck
    {
        public string check_info_key { get; set; }
        public string hp_plus_key { get; set; }
        public string amount { get; set; }
        public string invoice_number { get; set; }
        public bool force { get; set; }
        public string billing_frequency { get; set; }
        public string number_of_payments { get; set; }
        public string recurring_id { get; set; }
        public string effective_date { get; set; }
        public string transaction_type { get; set; }
        public string standard_entry_class_codes_type { get; set; }
        public string checking_account_type { get; set; }
        public string auth_option_form { get; set; }
        public string auth_option_voice { get; set; }
        public string convenience_amount { get; set; }
        public string surcharge_amount { get; set; }
    }
}