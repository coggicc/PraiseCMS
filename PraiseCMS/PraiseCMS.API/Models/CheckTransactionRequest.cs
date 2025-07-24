namespace PraiseCMS.API.Models
{
    public class CheckTransactionRequest
    {
        public TokenizedCheck tokenized_check { get; set; }
        public NonTokenizedCheck check { get; set; }
    }
}