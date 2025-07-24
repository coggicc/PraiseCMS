namespace PraiseCMS.API.Models
{
    public class TokenizedCard
    {
        public string transaction_type { get; set; }
        public string card_info_key { get; set; }
        public string amount { get; set; }
        public string pos_environment_indicator { get; set; }
    }
}