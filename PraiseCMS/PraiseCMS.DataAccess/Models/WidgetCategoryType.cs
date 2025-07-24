using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models
{
    [Table("WidgetCategoryTypes")]
    public class WidgetCategoryType : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [StringLength(100)]
        [DisplayName("Name")]
        [Required]
        public string Name { get; set; }

        [StringLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }
        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }

    public class CreateEditWidgetCategoryVM
    {
        public CreateEditWidgetCategoryVM()
        {
            WigetCategoryType = new WidgetCategoryType();
            WidgetCategoryRoles = new List<WidgetCategoryRole>();
            Roles = new List<ApplicationRoles>();
            SelectedRoles = new List<string>();
        }

        public WidgetCategoryType WigetCategoryType { get; set; }
        public List<WidgetCategoryRole> WidgetCategoryRoles { get; set; }
        public List<ApplicationRoles> Roles { get; set; }

        [Required]
        public List<string> SelectedRoles { get; set; }
    }

    public class CreateEditWidgetVM
    {
        public CreateEditWidgetVM()
        {
            Widget = new Widget();
            WidgetCategoryTypes = new List<WidgetCategoryType>();
            Roles = new List<ApplicationRoles>();
            WidgetPermissions = new List<WidgetPermission>();
            WidgetCategories = new List<WidgetCategory>();
            SelectedCategories = new List<string>();
            SelectedRoles = new List<string>();
            Files = new List<SelectListItem>();
        }

        public Widget Widget { get; set; }
        public List<WidgetCategoryType> WidgetCategoryTypes { get; set; }
        public List<ApplicationRoles> Roles { get; set; }
        public List<WidgetPermission> WidgetPermissions { get; set; }
        public List<WidgetCategory> WidgetCategories { get; set; }

        [Required]
        public List<string> SelectedCategories { get; set; }

        [Required]
        public List<string> SelectedRoles { get; set; }

        [Required]
        public List<SelectListItem> Files { get; set; }
    }
}