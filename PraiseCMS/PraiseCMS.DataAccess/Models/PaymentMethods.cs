using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models
{
    public class PaymentMethods
    {
        public List<CreditCard> CreditCards { get; set; }
        public List<BankAccount> BankAccounts { get; set; }

        public PaymentMethods()
        {
            CreditCards = new List<CreditCard>();
            BankAccounts = new List<BankAccount>();
        }
    }
}
