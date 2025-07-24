using System.Web.Mvc;

namespace SalesWebsite.Controllers
{
    public class MarketingController : Controller
    {
        [NonAction]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Social()
        {
            return View();
        }
    }
}