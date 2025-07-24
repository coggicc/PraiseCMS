using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Reports")]
    public class Report : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Report Category Id")]
        [Required]
        public string ReportCategoryId { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Report Group")]
        public string ReportGroupId { get; set; }

        [DisplayName("Report URL")]
        public string ReportUrl { get; set; }

        [DisplayName("Report Type")]
        public string ReportType { get; set; }

        [DisplayName("Report Type Id")]
        public string ReportTypeId { get; set; }

        [StringLength(50)]
        [DisplayName("Name")]
        [Required]
        public string Name { get; set; }

        [StringLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [StringLength(500)]
        [DisplayName("Instructions")]
        public string Instructions { get; set; }

        [DisplayName("Query")]
        public string Query { get; set; }

        [StringLength(50)]
        [DisplayName("Graph Type")]
        public string GraphType { get; set; }

        [StringLength(500)]
        [DisplayName("X Axis Column")]
        // [Required]
        public string XAxisColumn { get; set; }

        [StringLength(500)]
        [DisplayName("Y Axis Column")]
        // [Required]
        public string YAxisColumn { get; set; }

        [StringLength(500)]
        [DisplayName("X Axis Title")]
        public string XAxisTitle { get; set; }

        [StringLength(500)]
        [DisplayName("Y Axis Title")]
        public string YAxisTitle { get; set; }

        [StringLength(500)]
        [DisplayName("Y Multi Axis Title")]
        public string YMultiAxisTitle { get; set; }

        [DisplayName("Start Date")]
        [Required]
        public DateTime? StartDate { get; set; }

        [DisplayName("End Date")]
        [Required]
        public DateTime? EndDate { get; set; }

        [DisplayName("Default Date Range")]
        public bool IsDefaultDateRange { get; set; }

        [NotMapped]
        public string Class { get; set; }

        [NotMapped]
        public string ModuleId { get; set; }

        [NotMapped]
        public bool Favorite { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }

    public class ReportView
    {
        public ReportView()
        {
            Report = new Report();
            ReportCategories = new List<ReportCategory>();
            ReportSettings = new ReportSettings();
        }

        public Report Report { get; set; }
        public ReportSettings ReportSettings { get; set; }
        public List<ReportCategory> ReportCategories { get; set; }
        public bool IsNew { get; set; }
        public string CategoryName { get; set; }
    }

    public class ReportListView
    {
        public ReportListView()
        {
            FavoriteReports = new List<FavoriteReport>();
            Reports = new List<Report>();
            ReportCategories = new List<ReportCategory>();
            OfflineGiving = new List<OfflineGiving>();
            Payments = new List<Payment>();
            PrayerRequests = new List<PrayerRequest>();
            Salvations = new List<Salvation>();
            SmallGroups = new List<SmallGroup>();
            ReportGroup = new List<ReportGroup>();
            UserReportGroups = new List<UserReportGroups>();
        }

        public string Tab { get; set; }
        public string ReportSearch { get; set; }
        public List<FavoriteReport> FavoriteReports { get; set; }
        public List<Report> Reports { get; set; }
        public List<ReportCategory> ReportCategories { get; set; }
        public List<OfflineGiving> OfflineGiving { get; set; }
        public List<Payment> Payments { get; set; }
        public List<PrayerRequest> PrayerRequests { get; set; }
        public List<Salvation> Salvations { get; set; }
        public List<SmallGroup> SmallGroups { get; set; }
        public List<ReportGroup> ReportGroup { get; set; }
        public List<UserReportGroups> UserReportGroups { get; set; }
        public string SalvationsTotal { get; set; }
        public string SmallGroupsTotal { get; set; }
        public string PrayerRequestsTotal { get; set; }
        public string GivingTotal { get; set; }
    }

    public class ReportDashboard
    {
        public ReportDashboard()
        {
            FavoriteReports = new List<Report>();
            ReportCategory = new List<ReportCategory>();
            AllReports = new List<Report>();
            Report = new Report();
        }

        public string ReportId { get; set; }
        public string StartDate { get; set; }
        public string EndDate { get; set; }
        public string PresetDateRange { get; set; }
        public List<string> Campus { get; set; }
        public string Tab { get; set; }
        public List<ReportCategory> ReportCategory { get; set; }
        public List<Report> GivingReports { get; set; }
        public List<Report> AttendanceReports { get; set; }
        public List<Report> FavoriteReports { get; set; }
        public List<Report> AllReports { get; set; }
        public Report Report { get; set; }
    }

    public class ReportViewModel
    {
        public int Total { get; set; }
        public string Title { get; set; }
        public int Year { get; set; }
    }

    public class ReportModel
    {
        public string ReportId { get; set; }
        public string Report { get; set; }
        public string ReportName { get; set; }
        public string Category { get; set; }
        public string CampusIds { get; set; }
        public List<string> CampusIdList { get; set; }
    }

    public class CustomReportBuilder
    {
        public CustomReportBuilder()
        {
            Record = new List<ChartRecordModel>();
        }

        public List<ChartRecordModel> Record { get; set; }
        public List<string> XAxisColumns { get; set; }
        public List<string> DataSetLabels { get; set; }
        public List<string> YAxisColumns { get; set; }
        public List<Campus> Campuses { get; set; }
        public string XAxisTitle { get; set; }
        public string YAxisTitle { get; set; }
        public string YMultiAxisTitle { get; set; }
        public string Tab { get; set; }
        public string GraphType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class ChartRecordModel
    {
        public DateTime CreatedDate { get; set; }
        public int Total { get; set; }
        public string Title { get; set; }
        public string CampusId { get; set; }
    }

    public class DBSchemaModel
    {
        public DBSchemaModel()
        {
            Value = new string[] { };
        }

        public string Text { get; set; }
        public string[] Value { get; set; }
    }

    public class ChartViewModel
    {
        public string ReportCategoryName { get; set; }
        public string Chart { get; set; }
    }

    public class ReportDataSet
    {
        public ReportDataSet()
        {
            LinearData = new List<int>();
            BackgroundColorList = new List<string>();
            HoverBackgroundColorList = new List<string>();
            BorderColorList = new List<string>();
            HoverBorderColorList = new List<string>();
            GivingCounts = new List<int>();
        }

        public string Label { get; set; }
        public string BackgroundColor { get; set; }
        public string BorderColor { get; set; }
        public string BorderWidth { get; set; }
        public List<int> LinearData { get; set; }
        public List<int> GivingCounts { get; set; }
        public List<string> BackgroundColorList { get; set; }
        public List<string> HoverBackgroundColorList { get; set; }
        public List<string> BorderColorList { get; set; }
        public List<string> HoverBorderColorList { get; set; }
    }

    public class ReportBuilder
    {
        public ReportBuilder()
        {
            XAxisLabels = new List<string>();
            YAxisLabels = new List<string>();
            ReportDataSets = new List<ReportDataSet>();
        }

        public string Title { get; set; }
        public string ReportType { get; set; }
        public List<string> XAxisLabels { get; set; }
        public List<string> YAxisLabels { get; set; }
        public string XAxisTitle { get; set; }
        public string YAxisTitle { get; set; }
        public string YMultiAxisTitle { get; set; }
        public List<ReportDataSet> ReportDataSets { get; set; }
    }

    public class ReportVM
    {
        public dynamic Giving { get; set; }
        public dynamic PrayerRequests { get; set; }
        public dynamic Attendance { get; set; }
        public dynamic Custom { get; set; }
        public dynamic Favorites { get; set; }
    }
}