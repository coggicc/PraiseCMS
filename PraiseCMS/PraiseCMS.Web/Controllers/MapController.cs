using Newtonsoft.Json;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using PraiseCMS.Web.Helpers;
using System.Linq;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    public class MapController : BaseController
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult GeMapLocations()
        {
            return Json(work.Church.GetLocation(), JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetData()
        {
            var filePath = Server.MapPath("~/Content/assets/plugins/custom/jqvmap/usa_states.json");
            var text = System.IO.File.ReadAllText(filePath);

            foreach (var state in work.Church.GetStateCounts())
            {
                var stateName = Constants.AbbrevToState.FirstOrDefault(x => x.Key.Equals(state.Name)).Value;
                text = text.Replace(stateName, $"{stateName} ({state.Count})");
            }

            var model = JsonConvert.DeserializeObject<UsMapRoot>(text);

            return Json(model, JsonRequestBehavior.AllowGet);
        }
    }
}