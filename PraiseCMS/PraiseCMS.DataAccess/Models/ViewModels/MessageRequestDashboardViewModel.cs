using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class MessageRequestDashboardViewModel
    {
        public MessageRequestDashboardViewModel()
        {
            MessageRequestCategories = new List<MessageRequestCategory>();
            MessageRequests = new List<MessageRequest>();
        }

        public List<MessageRequest> MessageRequests { get; set; }
        public List<MessageRequestCategory> MessageRequestCategories { get; set; }
    }
}
