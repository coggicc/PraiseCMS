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
    
    [RequirePermission(ModuleId = "0854401246ac1cc881165641169cd4")]
    public class ServeTeamController : BaseController
    {
        #region Boilerplate
                #endregion

        public ActionResult Index()
        {
            var model = new ServeTeamDashboard
            {
                ServiceAreas = work.ServiceArea.GetAll(SessionVariables.CurrentChurch.Id),
                TeamMembers = work.ServeTeam.GetAll(SessionVariables.CurrentChurch.Id),
                Users = work.User.GetAllByChurchId(SessionVariables.CurrentChurch.Id),
                Campuses = work.Campus.GetAll(SessionVariables.CurrentChurch.Id)
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult _CreateServeTeamMember()
        {
            var serveTeamMember = new ServeTeamMember()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            var model = new ServeTeamMemberView
            {
                TeamMember = serveTeamMember,
                ServiceAreas = work.ServiceArea.GetAll(SessionVariables.CurrentChurch.Id),
                Users = work.User.GetAllByChurchId(SessionVariables.CurrentChurch.Id)
            };

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        public ActionResult _CreateServeTeamMember(ServeTeamMemberView model)
        {
            if (ModelState.IsValid)
            {
                work.ServeTeam.Create(model.TeamMember);

                return AjaxRedirectTo("/serveteam");
            }

            return PartialView("_CreateEdit", model);
        }

        [HttpGet]
        public ActionResult _EditServeTeamMember(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var serveTeamMember = work.ServeTeam.Get(id);

            if (serveTeamMember == null)
            {
                return HttpNotFound();
            }

            var model = new ServeTeamMemberView
            {
                TeamMember = serveTeamMember,
                ServiceAreas = work.ServiceArea.GetAll(SessionVariables.CurrentChurch.Id),
                Users = work.User.GetAllByChurchId(SessionVariables.CurrentChurch.Id)
            };

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditServeTeamMember(ServeTeamMemberView model)
        {
            if (ModelState.IsValid)
            {
                model.TeamMember.ModifiedDate = DateTime.Now;
                model.TeamMember.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                work.ServeTeam.Update(model);

                return AjaxRedirectTo("/serveteam");
            }

            return PartialView("_CreateEdit", model);
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var serveTeamMember = work.ServeTeam.Get(id);

            if (serveTeamMember == null)
            {
                return HttpNotFound();
            }

            work.ServeTeam.Delete(serveTeamMember);

            return RedirectToAction("index");
        }
    }
}