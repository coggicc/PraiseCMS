using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class ErrorViewModel
    {
        public bool IsDevEnvironment { get; set; }
        public HandleErrorInfo Exception { get; set; }
        public string AccessDeniedMessage { get; set; }
        public bool ShowErrorDetails { get; set; }
    }
}