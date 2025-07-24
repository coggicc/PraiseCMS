using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class ReportGroupPartialVM
    {
        public ReportGroupPartialVM()
        {
            ReportGroup = new List<ReportGroup>();
            UserReportGroups = new List<UserReportGroups>();
        }
        public List<ReportGroup> ReportGroup { get; set; }
        public List<UserReportGroups> UserReportGroups { get; set; }
        public string ReportId { get; set; }
    }
}