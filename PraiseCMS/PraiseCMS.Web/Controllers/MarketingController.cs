using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Net;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    [RequirePermission(ModuleId = "1199082268cf3b89d2d58348c091c2")]
    public class MarketingController : BaseController
    {
        public ActionResult Index()
        {
            var compaigns = work.Email.GetAllCampaign(SessionVariables.CurrentChurch.Id);
            return View(compaigns);
        }

        [HttpGet]
        public ActionResult _CreateEmailCampaign()
        {
            var model = new EmailCampaign()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            return PartialView("_CreateEditEmailCampaign", model);
        }

        [HttpPost]
        public ActionResult _CreateEmailCampaign(EmailCampaign model)
        {
            if (ModelState.IsValid)
            {
                work.Email.CreateCampaign(model);

                return AjaxRedirectTo("/marketing");
            }

            return PartialView("_CreateEditEmailCampaign", model);
        }

        [HttpGet]
        public ActionResult _EditEmailCampaign(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = work.Email.GetCampaign(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return PartialView("_CreateEditEmailCampaign", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditEmailCampaign(EmailCampaign model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                model.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                work.Email.UpdateCampaign(model);

                return AjaxRedirectTo("/marketing");
            }

            return PartialView("_CreateEditEmailCampaign", model);
        }

        [HttpGet]
        public ActionResult DeleteEmailCampaign(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = work.Email.GetCampaign(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            work.Email.DeleteCampaign(model);

            return RedirectToAction("index");
        }
    }
}