namespace PraiseCMS.API.Models
{
    public class CardTransactionRequest
    {
        public TokenizedCard tokenized_card { get; set; }
        public NonTokenizedCard credit_card { get; set; }
    }
}