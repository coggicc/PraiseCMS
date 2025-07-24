using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class BillingActivityVM
    {
        public Subscription Subscription { get; set; }
        public IEnumerable<SubscriptionTransaction> Transactions { get; set; }
    }
}