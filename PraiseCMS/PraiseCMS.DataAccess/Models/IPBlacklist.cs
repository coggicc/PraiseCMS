using PraiseCMS.DataAccess.Models.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("IPBlacklist")]
    public class IPBlacklist : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("IP Address")]
        public string IpAddress { get; set; }

        [DisplayName("Latitude")]
        public string Latitude { get; set; }

        [DisplayName("Longitude")]
        public string Longitude { get; set; }

        [DisplayName("Reason")]
        public string Reason { get; set; }
    }

    public class IPBlacklistDashboard
    {
        public IPBlacklistDashboard()
        {
            IPBlacklist = new List<IPBlacklist>();
        }

        public List<IPBlacklist> IPBlacklist { get; set; }
    }
}