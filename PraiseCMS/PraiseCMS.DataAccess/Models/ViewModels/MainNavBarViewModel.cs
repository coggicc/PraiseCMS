using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class MainNavBarViewModel
    {
        public List<ReportCategory> ReportCategories { get; set; }
        public List<Report> FavoriteReports { get; set; }
        public List<Report> GivingReports { get; set; }
        public List<Report> OutreachReports { get; set; }
        public List<Report> OtherReports { get; set; }
    }
}
