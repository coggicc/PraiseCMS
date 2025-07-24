using System.Web.Mvc;

namespace SalesWebsite.Controllers
{
    public class AboutUsController : Controller
    {
        [NonAction]
        public ActionResult Index()
        {
            return View();
        }
    }
}