namespace PraiseCMS.API.Models
{
    public class PaymentSafeCard
    {
        public string customer_key { get; set; }
        public string card_key { get; set; }
        public string expiration_date { get; set; }
        public string card_number_last_four_digits { get; set; }
        public string card_type { get; set; }
        public string name_on_card { get; set; }
        public string street1 { get; set; }
        public string city { get; set; }
        public string state { get; set; }
        public string zip_code { get; set; }
        public string card_account_updater_enabled { get; set; }
        public string last_update { get; set; }

        // New properties for month and year
        public string ExpirationMonth
        {
            get
            {
                return expiration_date.Substring(0, 2);
                //return DateTime.ParseExact(expiration_date, "MM/yy", CultureInfo.InvariantCulture).Month.ToString("D2");                
            }
        }

        public string ExpirationYear
        {
            get
            {
                return expiration_date.Substring(2);
                //return DateTime.ParseExact(expiration_date, "MM/yy", CultureInfo.InvariantCulture).Year.ToString();
            }
        }
    }
}