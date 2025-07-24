using System;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class UpcomingBirthdaysVM
    {
        public string Id { get; set; }
        public string Display { get; set; }
        public int RemainingDays { get; set; }
        public DateTime DOB { get; set; }
        public string ProfileImageURL { get; set; }
    }
}