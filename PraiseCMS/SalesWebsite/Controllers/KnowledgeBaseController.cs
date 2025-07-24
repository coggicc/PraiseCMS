using System.Web.Mvc;

namespace SalesWebsite.Controllers
{
    public class KnowledgeBaseController : Controller
    {
        [NonAction]
        public ActionResult Index()
        {
            return View();
        }
    }
}