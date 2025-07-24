namespace PraiseCMS.API.Models
{
    public class CardDetailsResponse : ResultModel
    {
        public string result_details { get; set; }
        public PaymentSafeCard paymentsafe_card { get; set; }
    }
}