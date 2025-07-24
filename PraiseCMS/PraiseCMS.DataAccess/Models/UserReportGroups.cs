using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("UserReportGroups")]
    public class UserReportGroups
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [DisplayName("Report Id")]
        public string ReportId { get; set; }

        [Required]
        [DisplayName("User Id")]
        public string UserId { get; set; }

        [Required]
        [DisplayName("Church Id")]
        public string ChurchId { get; set; }

        [Required]
        [DisplayName("Group Id")]
        public string GroupId { get; set; }
    }
}