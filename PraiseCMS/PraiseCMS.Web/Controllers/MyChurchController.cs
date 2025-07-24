using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    [RequirePermission(ModuleId = "711965845719f2d5baeaa345919ec4")]
    public class MyChurchController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }
    }
}