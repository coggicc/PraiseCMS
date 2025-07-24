namespace PraiseCMS.API.Models
{
    public class VerifyBankRoutingResponse : ResultModel
    {
        public string result_details { get; set; }
        public AbaInfo aba { get; set; }
    }
}