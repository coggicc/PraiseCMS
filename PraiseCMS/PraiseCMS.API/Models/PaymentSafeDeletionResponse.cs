namespace PraiseCMS.API.Models
{
    public class PaymentSafeDeletionResponse : ResultModel
    {
        public string card_key { get; set; }
        public string check_key { get; set; }
        public string customer_key { get; set; }
        public string gateway_id { get; set; }
        public string reseller_id { get; set; }
        public string username { get; set; }
    }
}