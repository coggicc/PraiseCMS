using System;

namespace PraiseCMS.DataAccess.Models.Base
{
    public class ServiceTimeBase : BaseModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? ShowEventAt { get; set; } //one hour before
        public DateTime? HideEventAt { get; set; } //one hour after
    }
}