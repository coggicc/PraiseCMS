namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class TopDonorsVM
    {
        public Person Donor { get; set; }
        public decimal OfflineGivingAmount { get; set; }
        public decimal DigitalGivingAmount { get; set; }
        public decimal TotalAmount { get; set; }
    }
}