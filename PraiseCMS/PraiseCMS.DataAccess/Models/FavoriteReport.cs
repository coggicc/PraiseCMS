using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("FavoriteReports")]
    public class FavoriteReport
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [Required]
        [DisplayName("User")]
        public string UserId { get; set; }

        [Required]
        [DisplayName("Report")]
        public string ReportId { get; set; }
    }
}