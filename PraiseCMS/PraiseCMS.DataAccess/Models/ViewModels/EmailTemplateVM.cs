using PraiseCMS.DataAccess.Models.ViewModels.Base;
using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class EmailTemplateVM : BaseViewModel
    {
        public EmailTemplateVM()
        {
            EmailTemplates = new List<EmailTemplate>();
            Church = new Church();
        }

        public List<EmailTemplate> EmailTemplates { get; set; }
    }
}