using PraiseCMS.DataAccess.Session;
using PraiseCMS.Shared.Methods;
using System.Web.Mvc;

namespace SalesWebsite.Controllers
{
    public class ErrorController : Controller
    {
        public ActionResult GenericError(HandleErrorInfo exception)
        {
            Response.ContentType = "text/html";
            ViewBag.Stylesheet = "error-6.css";

            if (!SessionVariables.CurrentUser.IsNotNull()) return View(exception);

            //var logObj = logRepository.JsonConverter("Action Name", exception.ActionName, "Controller Name", exception.ControllerName, "Exception Message", exception.Exception.Message);
            //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Error Occurred", SessionVariables.CurrentUser.IsNotNullOrEmpty() && SessionVariables.CurrentUser.User.IsNotNullOrEmpty() ? SessionVariables.CurrentUser.User.Id : "", "Error", logObj);

            return View(exception);
        }

        public ActionResult NotFound(HandleErrorInfo exception)
        {
            Response.ContentType = "text/html";
            ViewBag.Stylesheet = "error-3.css";
            //var logObj = logRepository.JsonConverter("Action Name", exception.ActionName, "Controller Name", exception.ControllerName, "Exception Message", exception.Exception.Message);
            //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Not Found", SessionVariables.CurrentUser.IsNotNullOrEmpty() && SessionVariables.CurrentUser.User.IsNotNullOrEmpty() ? SessionVariables.CurrentUser.User.Id : "", "Error", logObj);

            return View(exception);
        }

        public ActionResult NoAccess(string msg = null)
        {
            ViewBag.Message = msg.IsNotNullOrEmpty() ? msg : string.Empty;
            //var logObj = logRepository.JsonConverter();
            //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "No Access", SessionVariables.CurrentUser.IsNotNullOrEmpty() && SessionVariables.CurrentUser.User.IsNotNullOrEmpty() ? SessionVariables.CurrentUser.User.Id : "", "Error", logObj);

            return View();
        }

        public ActionResult _NoAccess(string msg)
        {
            ViewBag.Message = msg;
            return PartialView();
        }
    }
}