using Newtonsoft.Json.Linq;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    public class LogsController : BaseController
    {
        [HttpPost]
        public void GetLogs(LogsDetail model)
        {
            Session["LogData"] = model;

            string[] url = model.Url.Split('/');
            string controller = string.Empty;
            string Action = string.Empty;

            switch (url.Length)
            {
                case 4:
                    if (url[3]?.Length == 0)
                    {
                        controller = "Home";
                        Action = "Index";
                    }
                    else
                    {
                        controller = url[3].ToTitleCase();
                        Action = "Index";
                    }
                    break;
                case 5:
                    controller = url[3].ToTitleCase();
                    string[] ActionUrl = url[4].Split('?');
                    Action = ActionUrl[0];
                    break;
                default:
                    break;
            }

            Log log = new Log
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                Controller = controller,
                Action = Action,
                Status = string.Empty,
                Parameter = string.Empty,
                Type = string.Empty,
                TypeId = string.Empty,
                Autosave = false,
                UserId = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now,
                CreatedBy = SessionVariables.CurrentUser.User.Id
            };

            work.Log.Create(log);
        }

        [RequirePermission(ModuleId = "862887373010e02732b7574b0190f2")]
        [HttpGet]
        public ActionResult LogHistory(string logsType = null, string sortType = SortOrders.Descending, string church = null, string controllerName = null, string type = null, int page = 1, int pageSize = 50)
        {
            ViewBag.LogType = logsType;
            var logs = work.Log.GetAll(logsType, sortType, church, controllerName, type, page, pageSize);
            return View(logs);
        }

        [RequirePermission(ModuleId = "862887373010e02732b7574b0190f2")]
        [HttpGet]
        public ActionResult LogDetails(string id)
        {
            var logView = new LogViewModel
            {
                Log = work.Log.Get(id)
            };
            logView.Person = work.User.Get(logView.Log.UserId);

            try
            {
                JObject json = JObject.Parse(logView.Log.Parameter);
                var fieldsCollector = new JsonFieldsCollector(json);
                logView.LogFields = fieldsCollector.GetAllFields();
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                logView.DataType = LogStatuses.Exception;
            }

            return PartialView("LogDetails", logView);
        }

        [HttpGet]
        public void SaveCurrentLocation(string latitude, string longitude, string currentUrl)
        {
            //TODO
            //Add check for if CurrentChurch.Latitude and longitude are null. If they are then do below because that means we never got it.
            //It should be added to the footer.cshtml code.

            if (SessionVariables.CurrentUser != null
                && !string.IsNullOrWhiteSpace(latitude)
                && !string.IsNullOrWhiteSpace(longitude)
                && latitude != "0"
                && longitude != "0")
            {
                if (!SessionVariables.CurrentUser.User.Email.ContainsIgnoreCase("@praisecms")
                    && !SessionVariables.CurrentUser.User.Email.ContainsIgnoreCase("@novadevelopment")
                    && !SessionVariables.CurrentUser.User.Email.ContainsIgnoreCase("coggicc"))
                {
                    var latLong = new LatLong
                    {
                        Id = Utilities.GenerateUniqueId(),
                        UserId = SessionVariables.CurrentUser.User.Id,
                        Type = "Church",
                        TypeId = !string.IsNullOrEmpty(SessionVariables.CurrentChurch?.Id) ? SessionVariables.CurrentChurch.Id : null,
                        Latitude = latitude,
                        Longitude = longitude,
                        CurrentUrl = currentUrl,
                        IpAddress = Request != null ? Request.UserHostAddress : string.Empty,
                        CreatedDate = DateTime.Now
                    };
                    work.LatLong.Create(latLong);
                }
            }
        }
    }
}