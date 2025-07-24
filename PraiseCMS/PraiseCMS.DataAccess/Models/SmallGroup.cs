using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("SmallGroup")]
    public class SmallGroup : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        [Required]
        public string ChurchId { get; set; }

        [DisplayName("Campus")]
        public string CampusId { get; set; }

        [StringLength(100)]
        [DisplayName("Name")]
        [Required]
        public string Name { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [StringLength(50)]
        [DisplayName("Leader One Name")]
        public string LeaderOneName { get; set; }

        [StringLength(50)]
        [DisplayName("Leader One Phone")]
        public string LeaderOnePhone { get; set; }

        [StringLength(75)]
        [DisplayName("Leader One Email")]
        public string LeaderOneEmail { get; set; }

        [StringLength(50)]
        [DisplayName("Leader Two Name")]
        public string LeaderTwoName { get; set; }

        [StringLength(50)]
        [DisplayName("Leader Two Phone")]
        public string LeaderTwoPhone { get; set; }

        [StringLength(75)]
        [DisplayName("Leader Two Email")]
        public string LeaderTwoEmail { get; set; }

        [StringLength(500)]
        [DisplayName("Age Range")]
        public string AgeRange { get; set; }
        [DisplayName("Child Care Provided")]
        public bool ChildCareProvided { get; set; }
        [DisplayName("Handicap Accessible")]
        public bool HandicapAccessible { get; set; }

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

        [DisplayName("Active")]
        public bool IsActive { get; set; }

        [DisplayName("Category")]
        public string CategoryId { get; set; }

        public string Address => ((Address1 + " " + Address2).Trim() + ", " + City + " " + State + " " + Zip).Trim().Trim(',').Trim();

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }
}