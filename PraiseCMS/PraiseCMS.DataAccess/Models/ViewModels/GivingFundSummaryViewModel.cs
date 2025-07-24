using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class GivingFundSummaryViewModel
    {
        public GivingFundSummaryViewModel()
        {
            DigitalGiving = new List<Payment>();
            OfflineGiving = new List<OfflineGiving>();
        }

        public List<Payment> DigitalGiving { get; set; }
        public List<OfflineGiving> OfflineGiving { get; set; }
    }
}
