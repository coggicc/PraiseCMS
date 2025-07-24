using PraiseCMS.DataAccess.Models.Base;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("ReportSettings")]
    public class ReportSettings : BaseModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string UserId { get; set; }

        [DisplayName("Date From")]
        public DateTime DateFrom { get; set; }

        [DisplayName("Date End")]
        public DateTime DateEnd { get; set; }
    }
}