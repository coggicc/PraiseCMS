using PraiseCMS.Shared.Methods;
using System;

namespace PraiseCMS.Shared.Models
{
    public class DateRange
    {
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string CombinedDate
        {
            get
            {
                return $"{StartDate.ToShortDateString()}_{EndDate.ToShortDateString()}";
            }
        }
        public string CombinedPlainDate
        {
            get
            {
                return $"{StartDate.ToPlainDate()}_{EndDate.ToPlainDate()}";
            }
        }
    }
}