namespace PraiseCMS.API.Models
{
    public class CheckDetailsResponse : ResultModel
    {
        public string result_details { get; set; }
        public PaymentSafeCheck paymentsafe_check { get; set; }
    }
}