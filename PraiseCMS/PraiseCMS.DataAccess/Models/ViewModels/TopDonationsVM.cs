using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class TopDonationsVM
    {
        public TopDonationsVM()
        {
            Donations = new List<TopDonorsVM>();
        }

        public int Year { get; set; }
        public List<TopDonorsVM> Donations { get; set; }
    }

    public class TopDonationVM
    {
        public TopDonationVM()
        {
            Donations = new List<TopDonationModel>();
        }
        public int Year { get; set; }
        public List<TopDonationModel> Donations { get; set; }
    }

    public class TopDonationModel
    {
        public Person Donor { get; set; }
        public string GivingType { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal TotalDigitalAmount { get; set; }
        public decimal TotalOfflineAmount { get; set; }
    }
}