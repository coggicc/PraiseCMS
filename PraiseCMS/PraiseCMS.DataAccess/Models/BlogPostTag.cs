using PraiseCMS.DataAccess.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("BlogPostTags")]
    public class BlogPostTag : BaseModel
    {
        [DisplayName("ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("BlogPostId")]
        public string BlogPostId { get; set; }

        [DisplayName("BlogTagId")]
        public string BlogTagId { get; set; }

        public BlogPost BlogPost { get; set; }
        public BlogTag BlogTag { get; set; }
    }
}