using System.Web.Mvc;

namespace SalesWebsite.Controllers
{
    public class MobileAppController : Controller
    {
        [NonAction]
        public ActionResult Index()
        {
            return View();
        }
    }
}