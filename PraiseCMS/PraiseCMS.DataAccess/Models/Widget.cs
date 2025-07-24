using PraiseCMS.Shared.Shared;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Widgets")]
    public class Widget
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [StringLength(100)]
        [DisplayName("Name")]
        [Required]
        public string Name { get; set; }

        [StringLength(50)]
        [DisplayName("File Name")]
        [Required]
        public string FileName { get; set; }

        [StringLength(50)]
        [DisplayName("Location")]
        [Required]
        public string Location { get; set; }

        [StringLength(500)]
        [DisplayName("Description")]
        public string Description { get; set; }

        [StringLength(50)]
        [DisplayName("Layout Size")]
        [Required]
        public string LayoutSize { get; set; }

        [DisplayName("Image URL")]
        public string ImageUrl { get; set; }

        public string Display => !string.IsNullOrEmpty(Name) ? Name : Constants.DisplayDefaultText;
    }

    public class WidgetSortable
    {
        public WidgetSortable()
        {
            Widget = new Widget();
        }

        public int SortOrder { get; set; }
        public Widget Widget { get; set; }
    }

    public class ManageLayoutVM
    {
        public ManageLayoutVM()
        {
            MainWidgetSortable = new List<WidgetSortable>();
            TileWidgetSortable = new List<WidgetSortable>();
        }

        public List<WidgetSortable> MainWidgetSortable { get; set; }
        public List<WidgetSortable> TileWidgetSortable { get; set; }
        public DashboardTemplate DashboardTemplate { get; set; }
    }
}