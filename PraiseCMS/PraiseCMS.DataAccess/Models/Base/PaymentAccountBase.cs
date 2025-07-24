using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models.Base
{
    public abstract class PaymentAccountBase : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }
        public string DonorGUID { get; set; }
        public string AccountGUID { get; set; }
        public string Merchant { get; set; }
        public string AccountType { get; set; }
        public string AccountProvider { get; set; }
        public string ExpMonth { get; set; }
        public string ExpYear { get; set; }
        public string NickName { get; set; }
        public string AccountSubType { get; set; }
        public string PaymentMethodPreview { get; set; }
        public bool IsActive { get; set; }
        public bool? IsExpired { get; set; }
        public bool IsPrimary { get; set; }

        [DisplayName("Expired Notification Cleared")]
        public bool? ExpiredNotificationCleared { get; set; }
    }
}
