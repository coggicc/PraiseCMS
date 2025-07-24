using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class MessageRequestViewModel
    {
        public MessageRequestViewModel()
        {
            MessageRequestCategories = new List<MessageRequestCategory>();
        }

        public MessageRequest MessageRequest { get; set; }
        public List<MessageRequestCategory> MessageRequestCategories { get; set; }
    }
}