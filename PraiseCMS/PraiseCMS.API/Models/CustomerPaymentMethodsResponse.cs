using System.Collections.Generic;

namespace PraiseCMS.API.Models
{
    public class CustomerPaymentMethodsResponse : ResultModel
    {
        public string result_details { get; set; }

        public List<PaymentSafeCard> paymentsafe_cards { get; set; }
        public List<PaymentSafeCard> CreditCards => paymentsafe_cards;

        public List<PaymentSafeCheck> paymentsafe_checks { get; set; }
        public List<PaymentSafeCheck> BankAccounts => paymentsafe_checks;
    }
}