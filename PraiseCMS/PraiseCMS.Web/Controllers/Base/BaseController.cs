using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Newtonsoft.Json;
using PraiseCMS.API.Helpers;
using PraiseCMS.API.Models;
using PraiseCMS.API.Services;
using PraiseCMS.BusinessLayer;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Interfaces;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Repository;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using static PraiseCMS.Shared.Methods.ExtensionMethods;

namespace PraiseCMS.Web.Controllers.Base
{
    public class BaseController : Controller
    {
        #region Boilerplate
        protected Work work;
        protected ApplicationDbContext db;
        protected AdoDataAccess adoData;
        protected ILogRepository logRepository;
        protected TokenManager tokenManager;
        protected NuveiHelper nuveiHelper;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private readonly Timer _timer;
        private readonly HttpClient _httpClient;
        protected bool IsDonorOnlyCheckPerformed { get; set; }

        protected ApplicationSignInManager SignInManager
            => _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();

        protected ApplicationUserManager UserManager
            => _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

        public BaseController()
        {
            db = new ApplicationDbContext();
            adoData = new AdoDataAccess();
            logRepository = new LogsRepository();
            tokenManager = new TokenManager();
            work = new Work();
            nuveiHelper = new NuveiHelper(tokenManager);
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            string controller = filterContext.RouteData.Values["controller"]?.ToString();

            // Whenb currentUser is null, we still want to allow access to the Login and other account pages
            if (controller == "Account" || controller == "ScheduleDemo")
            {
                base.OnActionExecuting(filterContext);
                return;
            }

            if (SessionVariables.CurrentUser?.User == null)
            {
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new { controller = "Account", action = "Login" })
                );
                return;
            }

            // Optimize Subscription Check: Only refresh once per session or every 1 hour
            if (Session["LastSubscriptionCheck"] == null ||
                (DateTime.UtcNow - (DateTime)Session["LastSubscriptionCheck"]).TotalHours > 1)
            {
                LayoutHelper.CheckAndSetSubscriptions(HttpContext);
                Session["LastSubscriptionCheck"] = DateTime.UtcNow;
            }

            // Optimize Layout Settings (Cache Once Per Session)
            if (Session["LayoutSettings"] == null)
            {
                Session["LayoutSettings"] = new LayoutViewModel
                {
                    ThemeColor = LayoutHelper.GetThemeColor(),
                    ContainerSize = LayoutHelper.GetContainerSize(),
                    HasSidebarAccess = LayoutHelper.HasSidebarAccess(),
                    SidebarClasses = LayoutHelper.GetSidebarClasses()
                };
            }

            ViewBag.LayoutViewModel = (LayoutViewModel)Session["LayoutSettings"];
            base.OnActionExecuting(filterContext);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // Dispose of UserManager if it's not null
                if (_userManager != null)
                {
                    _userManager.Dispose();
                    _userManager = null;
                }

                // Dispose of SignInManager if it's not null
                if (_signInManager != null)
                {
                    _signInManager.Dispose();
                    _signInManager = null;
                }

                // Dispose of AdoDataAccess if it's not null
                if (adoData != null)
                {
                    adoData.Dispose();
                    adoData = null;
                }

                // Dispose of Work if it's not null
                if (work != null)
                {
                    work.Dispose();
                    work = null;
                }

                _timer?.Stop();
                _timer?.Dispose();

                // Dispose of DbContext
                db.Dispose();
                // Dispose of any other resources if needed
            }

            base.Dispose(disposing);
        }
        #endregion

        public JavaScriptResult AjaxReloadPage => JavaScript("location.reload(true)");

        public JavaScriptResult AjaxRedirectTo(string url)
        {
            return JavaScript($"document.location.href='{url}'");
        }

        public FileResult ExportToCsv(string csv, string fileName)
        {
            return File(new UTF8Encoding().GetBytes(csv), "text/csv", fileName);
        }

        public static bool ValidatePaymentModel(object model)
        {
            var context = new ValidationContext(model, serviceProvider: null, items: null);
            var validationResults = new List<ValidationResult>();

            return Validator.TryValidateObject(model, context, validationResults, true);
        }

        public string RenderPartialToString(string viewName, object model, ControllerContext controllerContext)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                viewName = ControllerContext.RouteData.GetRequiredString("action");
            }

            var viewData = new ViewDataDictionary();
            var tempData = new TempDataDictionary();
            viewData.Model = model;
            using var sw = new StringWriter();
            var viewResult = ViewEngines.Engines.FindPartialView(controllerContext, viewName);
            var viewContext = new ViewContext(controllerContext, viewResult.View, viewData, tempData, sw);
            viewResult.View.Render(viewContext, sw);

            return sw.GetStringBuilder().ToString();
        }

        #region Alert and Error Messages        
        public void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
        }

        public void DisplayErrors()
        {
            var err = string.Join("<br>", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Distinct());
            CreateAlertMessage($"<strong>Please correct the following errors:</strong><br>{err}", AlertMessageTypes.Failure, AlertMessageTypes.Failure);
        }

        public AlertMessage Alert
        {
            get => TempData.ContainsKey(TempDataKeys.AlertMessage) ? (AlertMessage)TempData[TempDataKeys.AlertMessage] : new AlertMessage();
            set => TempData[TempDataKeys.AlertMessage] = value;
        }

        public void CreateAlertMessage(string message, string type, string icon)
        {
            TempData[TempDataKeys.AlertMessage] = new AlertMessage { Message = message, Type = type, Icon = icon };
        }

        public void CreateAlertMessage(string message, ResultType type, ResultIcon icon)
        {
            CreateAlertMessage(message, type.DescriptionAttr(), icon.DescriptionAttr());
        }
        #endregion

        #region Church IP Location
        protected async Task SetChurchLocationInfoAsync(Church church)
        {
            try
            {
                var ipAddress = Utilities.GetIPAsync().Result;
                if (ipAddress != null)
                {
                    church.IPAddress = ipAddress;

                    var ipInfo = await GetFullLocationAsync(ipAddress);
                    if (ipInfo != null)
                    {
                        var locations = GetLocations(ipInfo.Loc);
                        if (locations != null)
                        {
                            church.Latitude = locations.Latitude;
                            church.Longitude = locations.Longitude;
                        }
                    }
                }
                else
                {
                    // Handle the case where ipAddress is null
                    // For example, log the error or display a message to the user
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }
        }

        public async Task<IpInfo> GetFullLocationAsync(string ipAddress)
        {
            try
            {
                if (ipAddress.IsNullOrEmpty())
                {
                    // Handle the case where ipAddress is null
                    // For example, log the error or return null
                    return null;
                }

                string url = $"http://ipinfo.io/{ipAddress}";
                HttpResponseMessage response = await _httpClient.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    IpInfo ipInfo = JsonConvert.DeserializeObject<IpInfo>(json);

                    // Update country name
                    var regionInfo = new RegionInfo(ipInfo.Country);
                    ipInfo.Country = regionInfo.EnglishName;

                    return ipInfo;
                }
                else
                {
                    // Handle unsuccessful response
                    return null;
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return null;
            }
        }

        protected static Locations GetLocations(string value)
        {
            if (value.IsNullOrEmpty())
            {
                return null;
            }

            try
            {
                var location = value.Split(',');

                return new Locations
                {
                    Latitude = Convert.ToDouble(location[0]),
                    Longitude = Convert.ToDouble(location[1])
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return null;
            }
        }
        #endregion

        protected bool IsDonorOnlyWithFunds()
        {
            if (SessionVariables.CurrentUser.IsDonorOnly && !IsDonorOnlyCheckPerformed)
            {
                IsDonorOnlyCheckPerformed = true;

                // Check if there are funds available for donation
                var hasFunds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id).Count != 0;

                if (hasFunds)
                {
                    return true; // User is a donor only and funds are available
                }
                else
                {
                    /***********
                     What is the point of clearing the session variables?
                     **********/

                    SessionVariables.Clear();
                    return false; // User is a donor only, but no funds are available
                }
            }

            return false; // User is not just a donor or check already performed
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> _VerifyRoutingNumber(string routingNumber)
        {
            if (SessionVariables.CurrentMerchant == null)
            {
                return null;
            }

            const string valid = "is-valid";
            const string invalid = "is-invalid";
            const string validFeedback = "valid-feedback";
            const string invalidFeedback = "invalid-feedback";

            if (!string.IsNullOrEmpty(routingNumber))
            {
                ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(SessionVariables.CurrentMerchant.ApiUsername, SessionVariables.CurrentMerchant.ApiPassword);

                var responseModel = await nuveiHelper.VerifyBankRoutingAsync(routingNumber, apiCredentials);
                var errorDescription = Responses.GetApiTransactionResponse(responseModel?.result) != APIStatuses.Success ? responseModel.result_message : string.Empty;
                var routingNum = responseModel.aba.routing_number ?? string.Empty;
                var bankName = responseModel.aba.bank_name ?? string.Empty;

                if (!string.IsNullOrEmpty(routingNum) || !string.IsNullOrEmpty(bankName))
                {
                    return JavaScript(GenerateRoutingNumberScript(valid, validFeedback, bankName));
                }

                if (!string.IsNullOrEmpty(errorDescription))
                {
                    return JavaScript(GenerateRoutingNumberScript(invalid, invalidFeedback, $"Uh-oh: {errorDescription}"));
                }
            }

            return JavaScript(GenerateRoutingNumberScript(invalid, invalidFeedback, "Uh-oh: Please enter a valid routing number."));
        }

        private static string GenerateRoutingNumberScript(string elementClass, string feedbackClass, string feedbackMessage)
        {
            return $"$('#PaymentAccount_RoutingNumber').addClass('{elementClass}').removeClass('{(elementClass == "is-valid" ? "is-invalid" : "is-valid")}'); " +
                   $"$('.routing-feedback').addClass('{feedbackClass}').removeClass('{(feedbackClass == "valid-feedback" ? "invalid-feedback" : "valid-feedback")}'); " +
                   $"$('.routing-feedback').text('{feedbackMessage}');";
        }

        #region Initialization Methods
        private void InitializeUserLastAccessedDate(CurrentUser currentUser)
        {
            if (currentUser?.User?.LastAccessedDate == null || currentUser.User.LastAccessedDate.Value.Date < DateTime.Now.Date)
            {
                var user = work.User.Get(currentUser.User.Id);
                if (user != null)
                {
                    user.LastAccessedDate = DateTime.Now;
                    currentUser.User.LastAccessedDate = user.LastAccessedDate; // Update the cached user
                    work.User.Update(user);
                }
            }
        }

        private void InitializeReportCategories(CurrentUser currentUser, Church currentChurch)
        {
            if (currentChurch != null && currentUser != null && SessionVariables.ReportCategories.IsNullOrEmpty())
            {
                SessionVariables.ReportCategories = work.Report.GetCurrentReports(currentChurch.Id, currentUser.User.Id);
            }
        }
        #endregion

        public async Task<string> DeletePaymentAccountAsync(string donorGUID, string accountGUID, string accountType, ApiCredentials apiCredentials)
        {
            string deleteResponse = string.Empty;

            try
            {
                var deleteResponseData = new PaymentSafeDeletionResponse();

                if (accountType == DigitalPaymentMethods.Card)
                {
                    var cardDeletionRequest = new CardDeletionRequest
                    {
                        card_key = accountGUID,
                        customer_key = donorGUID
                    };
                    deleteResponseData = await nuveiHelper.DeleteCardAsync(cardDeletionRequest, apiCredentials);
                }
                else if (accountType == DigitalPaymentMethods.ACH)
                {
                    var checkDeletionRequest = new CheckDeletionRequest
                    {
                        check_key = accountGUID,
                        customer_key = donorGUID
                    };
                    deleteResponseData = await nuveiHelper.DeleteCheckAsync(checkDeletionRequest, apiCredentials);
                }
                else
                {
                    throw new ArgumentException("Invalid account type.", nameof(accountType));
                }

                deleteResponse = Responses.GetApiTransactionResponse(deleteResponseData?.result);

                if (deleteResponse != APIStatuses.Success)
                {
                    var logObj = logRepository.JsonConverter("Exception Message", deleteResponseData.result_description,
                        "Exception Code", deleteResponseData.result_message, "Account Id", accountGUID);
                    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Delete Card/Bank Account", SessionVariables.CurrentUser.IsNotNullOrEmpty() && SessionVariables.CurrentUser.User.IsNotNullOrEmpty() ? SessionVariables.CurrentUser.User.Id : string.Empty, LogStatuses.Error, logObj);
                }
            }
            catch (HttpRequestException ex)
            {
                ExceptionLogger.LogHttpRequestException(ex);
            }

            return deleteResponse;
        }

        public PartialViewResult MainNavBar()
        {
            var reportCategories = SessionVariables.ReportCategories?.ToList() ?? new List<ReportCategory>();

            var favoriteReports = reportCategories
                .SelectMany(x => x.Reports.Where(r => r.Favorite))
                .OrderBy(q => q.Name)
                .ToList();

            var givingReports = reportCategories
                .FirstOrDefault(x => x.Name.EqualsIgnoreCase("Giving"))?
                .Reports.OrderBy(q => q.Name)
                .ToList() ?? new List<Report>();

            var outreachReports = reportCategories
                .Where(x => x.Name.EqualsIgnoreCase("Prayer Request") || x.Name.EqualsIgnoreCase("Salvation") || x.Name.EqualsIgnoreCase("Attendance"))
                .SelectMany(x => x.Reports)
                .OrderBy(q => q.Name)
                .ToList();

            var otherReports = reportCategories
                .FirstOrDefault(x => x.Name.EqualsIgnoreCase("Other"))?
                .Reports.OrderBy(q => q.Name)
                .ToList() ?? new List<Report>();

            var viewModel = new MainNavBarViewModel
            {
                ReportCategories = reportCategories,
                FavoriteReports = favoriteReports,
                GivingReports = givingReports,
                OutreachReports = outreachReports,
                OtherReports = otherReports
            };

            return PartialView("_MainNavBar", viewModel);
        }
    }
}