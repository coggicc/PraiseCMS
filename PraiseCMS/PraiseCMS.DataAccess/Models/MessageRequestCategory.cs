using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("MessageRequestCategories")]
    public class MessageRequestCategory : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Name")]
        public string Name { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }
}