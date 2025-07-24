using PraiseCMS.DataAccess.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("IPWhitelist")]
    public class IPWhitelist : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("IP Address")]
        public string IpAddress { get; set; }

        [DisplayName("Name")]
        public string Name { get; set; }
    }
}