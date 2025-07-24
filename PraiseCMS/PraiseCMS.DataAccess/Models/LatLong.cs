using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("LatLongs")]
    public class LatLong
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("User")]
        public string UserId { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }

        [DisplayName("Type ID")]
        public string TypeId { get; set; }

        [DisplayName("Latitude")]
        public string Latitude { get; set; }

        [DisplayName("Longitude")]
        public string Longitude { get; set; }

        [DisplayName("Current URL")]
        public string CurrentUrl { get; set; }

        [DisplayName("IP Address")]
        public string IpAddress { get; set; }

        [DisplayName("Created Date")]
        public DateTime CreatedDate { get; set; }
    }
}