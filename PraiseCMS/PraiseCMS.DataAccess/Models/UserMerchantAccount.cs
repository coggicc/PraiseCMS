using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("UserMerchantAccounts")]
    public class UserMerchantAccount
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string UserId { get; set; }
        public string DonorGUID { get; set; }
        public string Merchant { get; set; }
        public bool IsActive { get; set; }

        [DisplayName("Created Date")]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Created By")]
        public string CreatedBy { get; set; }

        public string DonorId { get; set; }

        public string customer_key => DonorGUID;
    }
}