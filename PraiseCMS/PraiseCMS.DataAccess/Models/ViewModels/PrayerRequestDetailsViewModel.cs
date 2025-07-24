using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class PrayerRequestDetailsViewModel
    {
        public PrayerRequestDetailsViewModel()
        {
            PrayerRequest = new PrayerRequest();
            PrayerRequestCategories = new List<PrayerRequestCategory>();
            Users = new List<ApplicationUser>();
            Person = new Person();
            PrayerRequests = new List<PrayerRequest>();
        }

        public PrayerRequest PrayerRequest { get; set; }
        public List<PrayerRequestCategory> PrayerRequestCategories { get; set; }
        public Person Person { get; set; }
        public List<ApplicationUser> Users { get; set; }
        public List<PrayerRequest> PrayerRequests { get; set; }
        public int OrderNumber { get; set; }
        public int TotalPrayerRequests { get; set; }
        public string PreviousId { get; set; }
        public string NextId { get; set; }
    }
}