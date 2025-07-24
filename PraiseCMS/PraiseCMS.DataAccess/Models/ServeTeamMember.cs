using PraiseCMS.DataAccess.Models.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("ServeTeamMembers")]
    public class ServeTeamMember : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Campus")]
        public string CampusId { get; set; }

        [DisplayName("Service Area")]
        public string ServiceAreaId { get; set; }

        [Required]
        [DisplayName("User")]
        public string UserId { get; set; }

        [DisplayName("Is Active")]
        public bool IsActive { get; set; }
    }

    public class ServeTeamDashboard
    {
        public ServeTeamDashboard()
        {
            Campuses = new List<Campus>();
            ServiceAreas = new List<ServiceArea>();
            TeamMembers = new List<ServeTeamMember>();
            Users = new List<ApplicationUser>();
        }

        public List<Campus> Campuses { get; set; }
        public List<ServiceArea> ServiceAreas { get; set; }
        public List<ServeTeamMember> TeamMembers { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }

    public class ServeTeamMemberView
    {
        public ServeTeamMemberView()
        {
            ServiceAreas = new List<ServiceArea>();
            Users = new List<ApplicationUser>();
        }

        public ServeTeamMember TeamMember { get; set; }
        public List<ServiceArea> ServiceAreas { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }
}