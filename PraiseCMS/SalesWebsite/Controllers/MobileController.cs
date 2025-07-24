using System.Web.Mvc;

namespace SalesWebsite.Controllers
{
    public class MobileController : Controller
    {
        [NonAction]
        public ActionResult Index()
        {
            return View();
        }
    }
}