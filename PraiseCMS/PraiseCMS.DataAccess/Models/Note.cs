using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Notes")]
    public class Note
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

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Category")]
        public string Category { get; set; }

        [DisplayName("Deleted")]
        public bool Deleted { get; set; }

        [DisplayName("Deleted Date")]
        public DateTime? DeletedDate { get; set; }

        [DisplayName("Deleted By")]
        public string DeletedBy { get; set; }

        [DisplayName("Created Date")]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Created By")]
        public string CreatedBy { get; set; }
    }
}