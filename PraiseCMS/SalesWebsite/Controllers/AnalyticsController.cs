using System.Web.Mvc;

namespace SalesWebsite.Controllers
{
    public class AnalyticsController : Controller
    {
        [NonAction]
        public ActionResult Index()
        {
            return View();
        }
    }
}