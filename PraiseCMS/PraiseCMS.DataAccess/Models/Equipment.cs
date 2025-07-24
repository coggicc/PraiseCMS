using PraiseCMS.DataAccess.Models.Base;
using PraiseCMS.Shared.Shared;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Equipment")]
    public class Equipment : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Equipment Category")]
        [Required(ErrorMessage = "Equipment category is required.")]
        public string EquipmentCategoryId { get; set; }

        [DisplayName("Name")]
        public string Name { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }

        [DisplayName("Barcode")]
        public string Barcode { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        [DisplayName("Image")]
        public string Image { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Number { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }

    public class EquipmentView
    {
        public EquipmentView()
        {
            EquipmentList = new List<Equipment>();
            EquipmentCategories = new List<EquipmentCategory>();
        }

        public Equipment Equipment { get; set; }
        public EquipmentCategory EquipmentCategory { get; set; }
        public List<Equipment> EquipmentList { get; set; }
        public List<EquipmentCategory> EquipmentCategories { get; set; }
    }
}