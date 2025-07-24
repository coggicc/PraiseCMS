using System;

namespace PraiseCMS.DataAccess.Interfaces
{
    public interface IGivingItem
    {
        decimal Amount { get; set; }
        DateTime CreatedDate { get; set; }
        string CampusId { get; set; }
        string FundId { get; set; }
        string PaymentType { get; }  // Note: Only getter, no setter
    }
}