using PraiseCMS.DataAccess.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("SmallGroupCategoryTypes")]
    public class SmallGroupCategoryType : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Category")]
        [Required]
        public string Name { get; set; }

        [DisplayName("Available to All")]
        public bool IsGlobal { get; set; }  //Default is false as almost all new types will be custom to a given churchId.

        //Optional.  The base categories are not assigned to a church.  Only church created categories will have a churchId.
        //If a church decides to delete a category with groups already assigned, then those groups will be orphaned.  Need to reassign them.
        public string ChurchId { get; set; }

        [DisplayName("Active")]
        public bool IsActive { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : "[No Category Defined]";
    }
}