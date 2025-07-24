using System.Web.Mvc;

namespace SalesWebsite.Controllers
{
    public class DocumentationController : Controller
    {
        [NonAction]
        public ActionResult Index()
        {
            return View();
        }
    }
}