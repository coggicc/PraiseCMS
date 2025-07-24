using PraiseCMS.DataAccess.Models;
using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Interfaces
{
    public interface IGivingDashboard
    {
        List<TotalGivingItem> TotalGiving { get; }
        List<Payment> DigitalGiving { get; }
        List<OfflineGiving> OfflineGiving { get; }
    }
}