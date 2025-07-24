using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.Shared.Shared;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Campuses")]
    public class Campus : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [StringLength(50)]
        [DisplayName("Name")]
        public string Name { get; set; }

        [StringLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Image Path")]
        public string ImagePath { get; set; }

        [StringLength(50)]
        [DisplayName("Phone")]
        public string Phone { get; set; }

        [StringLength(75)]
        [DisplayName("Email")]
        public string Email { get; set; }

        [StringLength(75)]
        [DisplayName("Website")]
        public string Website { get; set; }

        [StringLength(100)]
        [DisplayName("Address Line 1")]
        public string Address1 { get; set; }

        [StringLength(100)]
        [DisplayName("Address Line 2")]
        public string Address2 { get; set; }

        [StringLength(50)]
        [DisplayName("City")]
        public string City { get; set; }

        [StringLength(50)]
        [DisplayName("State")]
        public string State { get; set; }

        [StringLength(10)]
        [DisplayName("Zip")]
        public string Zip { get; set; }

        public bool IsActive { get; set; }

        [DisplayName("Time Zone")]
        public string TimeZone { get; set; }

        [DisplayName("Geolocation Type")]
        public string GeolocationType { get; set; }

        [DisplayName("X Coordinate")]
        public string XCoordinate { get; set; }

        [DisplayName("Y Coordinate")]
        public string YCoordinate { get; set; }

        [StringLength(250)]
        [DisplayName("Service Times")]
        public string ServiceTimes { get; set; }

        public string Address => ((Address1 + " " + Address2).Trim() + ", " + City + ", " + State + " " + Zip).Trim().Trim(',').Trim();

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;

        // Navigation properties
        public virtual Church Church { get; set; } // Reference to the church
        public virtual ICollection<ChurchEvent> Events { get; set; } // Events associated with this campus
    }

    public class CampusViewModel
    {
        public string CampusId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string ImagePath { get; set; }
        public string Phone { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public bool IsActive { get; set; }
        public string TimeZone { get; set; }
        public string GeolocationType { get; set; }
        public string XCoordinate { get; set; }
        public string YCoordinate { get; set; }
        public string ServiceTimes { get; set; }
        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }

    public class CampusesView
    {
        public CampusesView()
        {
            Giving = new List<MyGivingVM>();
            Attendance = new List<Attendance>();
            Campuses = new List<Campus>();
            CheckIns = new List<CheckIn>();
            Payments = new List<Payment>();
            PrayerRequests = new List<PrayerRequest>();
            Salvations = new List<Salvation>();
            SmallGroups = new List<SmallGroup>();
            Users = new List<ApplicationUser>();
        }

        public List<MyGivingVM> Giving { get; set; }
        public List<Attendance> Attendance { get; set; }
        public List<Campus> Campuses { get; set; }
        public List<CheckIn> CheckIns { get; set; }
        public List<Payment> Payments { get; set; }
        public List<PrayerRequest> PrayerRequests { get; set; }
        public List<Salvation> Salvations { get; set; }
        public List<SmallGroup> SmallGroups { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }

    public class CampusDashboard
    {
        public CampusDashboard()
        {
            Attendance = new List<Attendance>();
            CheckIns = new List<CheckIn>();
            Payments = new List<Payment>();
            PrayerRequests = new List<PrayerRequest>();
            Salvations = new List<Salvation>();
            ServiceAreas = new List<ServiceArea>();
            SmallGroups = new List<SmallGroup>();
            Users = new List<ApplicationUser>();
        }

        public Campus Campus { get; set; }

        public List<Attendance> Attendance { get; set; }
        public List<CheckIn> CheckIns { get; set; }
        public List<Payment> Payments { get; set; }
        public List<PrayerRequest> PrayerRequests { get; set; }
        public List<Salvation> Salvations { get; set; }
        public List<ServiceArea> ServiceAreas { get; set; }
        public List<SmallGroup> SmallGroups { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }

    public class CampusGivingDashboard
    {
        public CampusGivingDashboard()
        {
            Funds = new List<Fund>();
            Giving = new List<Payment>();
        }

        public Campus Campus { get; set; }
        public List<Fund> Funds { get; set; }
        public List<Payment> Giving { get; set; }
    }
}