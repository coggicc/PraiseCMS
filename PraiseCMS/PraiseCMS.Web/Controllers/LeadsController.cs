using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    [RequirePermission(ModuleId = "0886839649fa1152c9abb149978848")]
    [RequireRole(Role = Roles.SuperAdmin)]
    public class LeadsController : BaseController
    {
        public ActionResult Index()
        {
            return View(work.Leads.GetAll());
        }

        public ActionResult Create()
        {
            var model = new Lead()
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedDate = DateTime.Now,
                CreatedBy = SessionVariables.CurrentUser.User.Id
            };

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(Lead model)
        {
            if (ModelState.IsValid)
            {
                var result = work.Leads.Create(model);

                if (result.ResultType == ResultType.Success)
                {
                    CreateAlertMessage("The lead has been created.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                    return AjaxRedirectTo("/Leads");
                }

                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return PartialView("_CreateEdit", model);
        }

        public ActionResult Edit(string id)
        {
            var lead = work.Leads.Get(id);
            return PartialView("_CreateEdit", lead);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(Lead model)
        {
            if (ModelState.IsValid)
            {
                var result = work.Leads.Update(model);

                if (result.ResultType == ResultType.Success)
                {
                    CreateAlertMessage("The lead has been updated.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                    return AjaxRedirectTo("/Leads");
                }

                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return PartialView("_CreateEdit", model);
        }

        public ActionResult Delete(string id)
        {
            var result = work.Leads.Delete(id);

            if (result.ResultType == ResultType.Success)
            {
                CreateAlertMessage("The lead has been deleted.", AlertMessageTypes.Success, AlertMessageIcons.Success);
            }
            else
            {
                CreateAlertMessage(result.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return RedirectToAction(nameof(Index));
        }
    }
}