using PraiseCMS.DataAccess.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("BlogCategories")]
    public class BlogCategory : BaseModel
    {
        [DisplayName("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Title")]
        public string Title { get; set; }

        [DisplayName("Status")]
        public int Status { get; set; }

        public string Display => !string.IsNullOrEmpty(Title) ? Title : "[No Title Defined]";
    }
}