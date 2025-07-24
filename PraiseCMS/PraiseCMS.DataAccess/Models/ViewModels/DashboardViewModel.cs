using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class DashboardViewModel
    {
        public DashboardViewModel()
        {
            Attendance = new List<Attendance>();
            Baptisms = new List<Baptism>();
            Campuses = new List<Campus>();
            CheckIns = new List<CheckIn>();
            Churches = new List<Church>();
            Events = new List<EventSD>();
            Giving = new List<MyGivingVM>();
            MyGiving = new GivingDashboard();
            Notifications = new List<Notification>();
            OfflineGiving = new List<OfflineGiving>();
            Payments = new List<Payment>();
            PrayerRequests = new List<PrayerRequest>();
            RecentDeaths = new List<Person>();
            Salvations = new List<Salvation>();
            SmallGroups = new List<SmallGroup>();
            UpcomingBirthdays = new List<UpcomingBirthdaysVM>();
        }

        public int NewDonors { get; set; }
        public GivingDashboard MyGiving { get; set; }
        public List<Attendance> Attendance { get; set; }
        public List<Baptism> Baptisms { get; set; }
        public List<Campus> Campuses { get; set; }
        public List<CheckIn> CheckIns { get; set; }
        public List<Church> Churches { get; set; }
        public List<EventSD> Events { get; set; }
        public List<MyGivingVM> Giving { get; set; }
        public List<Notification> Notifications { get; set; }
        public List<OfflineGiving> OfflineGiving { get; set; }
        public List<Payment> Payments { get; set; }
        public List<Person> RecentDeaths { get; set; }
        public List<PrayerRequest> PrayerRequests { get; set; }
        public List<Salvation> Salvations { get; set; }
        public List<SmallGroup> SmallGroups { get; set; }
        public List<UpcomingBirthdaysVM> UpcomingBirthdays { get; set; }

        public WeeklyGivingComparisonViewModel WeeklyGivingComparison { get; set; } // New property for the weekly comparison
        public string CurrentWeeksGivingAmount { get; set; }
        public WeeklyComparisonViewModel WeeklyAttendanceComparison { get; set; }
        public string CurrentWeeksAttendance { get; set; }
        public WeeklyComparisonViewModel WeeklyBaptismComparison { get; set; }
        public string CurrentWeeksBaptisms { get; set; }

        public WeeklyComparisonViewModel WeeklySalvationComparison { get; set; }
        public string CurrentWeeksSalvations { get; set; }
    }

    public class WidgetsGraphModel
    {
        public WidgetsGraphModel()
        {
            Data = new List<GraphData>();
        }

        public string Key { get; set; }
        public List<GraphData> Data { get; set; }
    }

    public class GraphData
    {
        public string Value { get; set; }
        public string Category { get; set; }
    }

    public class WeeklyGivingComparisonViewModel
    {
        public decimal CurrentWeeksGiving { get; set; }
        public decimal LastWeeksGiving { get; set; }
        public decimal GivingDifference { get; set; }
        public decimal PercentChange { get; set; }
    }

    public class WeeklyComparisonViewModel
    {
        public int CurrentWeekCount { get; set; }
        public int LastWeekCount { get; set; }
        public int Difference { get; set; }
        public decimal PercentChange { get; set; }
    }

    public class WeeklyAttendanceComparisonViewModel
    {
        public int CurrentWeeksAttendance { get; set; }
        public int LastWeeksAttendance { get; set; }
        public int AttendanceDifference { get; set; }
        public decimal PercentChange { get; set; }
    }

    public class WeeklyBaptismComparisonViewModel
    {
        public int CurrentWeeksBaptisms { get; set; }
        public int LastWeeksBaptisms { get; set; }
        public int BaptismDifference { get; set; }
        public decimal PercentChange { get; set; }
    }

    public class WeeklySalvationComparisonViewModel
    {
        public int CurrentWeeksSalvations { get; set; }
        public int LastWeeksSalvations { get; set; }
        public int SalvationDifference { get; set; }
        public decimal PercentChange { get; set; }
    }
}