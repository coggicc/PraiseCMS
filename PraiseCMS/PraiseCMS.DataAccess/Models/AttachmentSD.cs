using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Attachments")]
    public class AttachmentSD
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }

        [DisplayName("Type Id")]
        public string TypeId { get; set; }

        [DisplayName("Name")]
        public string Name { get; set; }

        [DisplayName("FileName")]
        public string FileName { get; set; }

        [DisplayName("Notes")]
        public string Notes { get; set; }

        [DisplayName("Sort Order")]
        public int SortOrder { get; set; }

        [DisplayName("Category")]
        public string Category { get; set; }

        [DisplayName("Created Date")]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Created By")]
        public string CreatedBy { get; set; }
    }
}