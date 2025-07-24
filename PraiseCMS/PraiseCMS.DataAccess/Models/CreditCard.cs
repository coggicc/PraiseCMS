namespace PraiseCMS.DataAccess.Models
{
    public class CreditCard
    {
        public string NameOnCard { get; set; }
        public string CardNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string CardType { get; set; }
        public string Nickname { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
        public string StatusName { get; set; }
        public string AccountGUID { get; set; }
        public string MaskedCardNumber { get; set; }
    }
}
