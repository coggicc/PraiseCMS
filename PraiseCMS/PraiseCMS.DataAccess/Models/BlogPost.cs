using PraiseCMS.DataAccess.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models
{
    [Table("BlogPosts")]
    public class BlogPost : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Title")]
        public string Title { get; set; }

        [DisplayName("Lead")]
        public string Lead { get; set; }

        [Required]
        [AllowHtml]
        [DisplayName("Message")]
        public string Message { get; set; }

        [DisplayName("Category Id")]
        public string CategoryId { get; set; }

        [DisplayName("Status")]
        public int Status { get; set; }

        [DisplayName("Viewed Count")]
        public int ViewedCount { get; set; }

        [DisplayName("Author Id")]
        public string AuthorId { get; set; }

        public string Display => !string.IsNullOrEmpty(Title) ? Title : "[No Title Defined]";
    }
}