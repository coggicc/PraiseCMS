using PraiseCMS.DataAccess.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("PaymentMethodAccounts")]
    public class PaymentMethodAccount : PaymentAccountBase
    {
        public string Type { get; set; }
        public string TypeId { get; set; }
    }
}