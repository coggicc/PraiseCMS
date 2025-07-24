namespace PraiseCMS.API.Models
{
    public class CustomerResponse : ResultModel
    {
        public string customer_key { get; set; }
        public string gateway_id { get; set; }
        public string reseller_id { get; set; }
        public string username { get; set; }
    }
}