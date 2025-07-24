using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    public class HomeController : BaseController
    {
        public ActionResult Index()
        {
            // Check if the user is a donor only and there are funds for donation
            if (IsDonorOnlyWithFunds())
            {
                return RedirectToAction(nameof(MyGivingController.Index), "MyGiving");
            }

            if (SessionVariables.CurrentUser.IsAdmin && SessionVariables.CurrentChurch.ShowWelcomeMessage)
            {
                HandleWelcomeMessage();
                return RedirectToWelcome();
            }

            var model = work.DashboardTemplate.ConstructDashboardViewModel(SessionVariables.CurrentChurch, SessionVariables.CurrentUser.User.Id);
            SessionVariables.Widgets ??= work.DashboardTemplate.GetActiveWidgetSortable(SessionVariables.CurrentUser.User.Id);

            return View(model);
        }

        private void HandleWelcomeMessage()
        {
            var church = work.Church.Get(SessionVariables.CurrentChurch.Id);
            church.ShowWelcomeMessage = SessionVariables.CurrentChurch.ShowWelcomeMessage = false;
            church.ModifiedDate = DateTime.Now;
            church.ModifiedBy = SessionVariables.CurrentUser.User.Id;
            work.Church.Update(church);
        }

        private ActionResult RedirectToWelcome()
        {
            var welcomeVM = new WelcomeViewModel()
            {
                Header = "Welcome To Praise CMS!",
                BodyText = "We recommend you enable giving so you can start receiving digital tithes &amp; offerings.",
                ButtonLink = $"/onboarding/CreateMerchantAccount/{SessionVariables.CurrentChurch.Id}",
                ButtonText = "Enable Giving",
                ImagePath = "/Content/assets/image/welcome-screen.svg",
                HelperText = "<strong>Note:</strong> Have your tax ID, routing number, and account number handy. You will need them for the next step.",
                CardFooterButtonLink = "/home",
                CardFooterButtonText = "Proceed to Dashboard"
            };

            return View("Welcome", welcomeVM);
        }

        public ActionResult WidgetsGraphData()
        {
            string cacheKey = $"WidgetsGraphData_{SessionVariables.CurrentChurch.Id}";
            List<WidgetsGraphModel> model = HttpRuntime.Cache[cacheKey] as List<WidgetsGraphModel>;

            if (model == null)
            {
                model = new List<WidgetsGraphModel>
                {
                    work.Attendance.GetGraphData(SessionVariables.CurrentChurch),
                    work.Baptism.GetGraphData(SessionVariables.CurrentChurch),
                    work.Salvation.GetGraphData(SessionVariables.CurrentChurch),
                    work.Giving.GetGraphData(SessionVariables.CurrentChurch)
                };

                // Cache the data for 1 hour
                HttpRuntime.Cache.Insert(
                    cacheKey,
                    model,
                    null,
                    DateTime.Now.AddHours(1), // Cache expiration
                    System.Web.Caching.Cache.NoSlidingExpiration
                );
            }

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public ActionResult Welcome()
        {
            if (!SessionVariables.CurrentUser.IsAdmin || !SessionVariables.CurrentUser.User.ShowWelcomeMessage)
            {
                return Redirect("/");
            }

            var user = work.User.Get(SessionVariables.CurrentUser.User.Id);
            var funds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id);
            user.ShowWelcomeMessage = SessionVariables.CurrentUser.User.ShowWelcomeMessage = false;
            work.User.Update(user);

            var model = new WelcomeViewModel()
            {
                Header = funds.Any() ? "Almost there..." : "Almost done...",
                SubHeader = string.Empty,
                HeaderLink = string.Empty,
                BodyText = funds.Any() ? "Your existing funds are currently for offline giving only. You can update your funds to accept digital donations, or you can create a new digital fund." : "Now it's time to create your first fund – a designated space for organizing and assigning donations.",
                ButtonLink = "/funds",
                ButtonText = funds.Any() ? "Manage Funds" : "Add a Fund",
                ImagePath = "/Content/assets/image/piggy-bank.png"
            };

            return View(model);
        }
    }
}