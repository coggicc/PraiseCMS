namespace PraiseCMS.API.Models
{
    public class CardRequest
    {
        public string customer_key { get; set; }
        public string card_number { get; set; }
        public string expiration_date { get; set; }
        public string name_on_card { get; set; }
        public string street1 { get; set; }
        public string zip_code { get; set; }
    }
}