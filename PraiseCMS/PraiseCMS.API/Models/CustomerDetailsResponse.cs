namespace PraiseCMS.API.Models
{
    public class CustomerDetailsResponse : ResultModel
    {
        public string result_details { get; set; }
        public CustomerDetails customer { get; set; }
    }
}