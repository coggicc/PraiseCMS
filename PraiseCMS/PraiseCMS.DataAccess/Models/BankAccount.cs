namespace PraiseCMS.DataAccess.Models
{
    public class BankAccount
    {
        public string AccountType { get; set; }
        public string RoutingNumber { get; set; }
        public string AccountNumber { get; set; }
        public string Nickname { get; set; }
        public string BankName { get; set; }
        public string AccountGUID { get; set; }
        public string StatusName { get; set; }
        public string MaskedAccountNumber { get; set; }
    }
}
