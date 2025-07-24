using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models.Base
{
    public class BaseModel
    {
        [DisplayName("Created Date")]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Created By")]
        public string CreatedBy { get; set; }

        [NotMapped]
        public ApplicationUser CreatedByUser { get; set; }

        [DisplayName("Modified Date")]
        public DateTime? ModifiedDate { get; set; }

        [DisplayName("Modified By")]
        public string ModifiedBy { get; set; }

        [NotMapped]
        public ApplicationUser ModifiedByUser { get; set; }
    }
}