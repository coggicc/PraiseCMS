using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    [RequirePermission(ModuleId = "8731643575db043bd252a54284bee1")]
    public class AttendanceController : BaseController
    {
        public ActionResult Index(string campusId)
        {
            var attendance = work.Attendance.GetAll(SessionVariables.CurrentChurch.Id);

            if (!string.IsNullOrEmpty(campusId))
            {
                attendance = attendance.Where(x => x.CampusId == campusId).ToList();
            }

            return View(attendance);
        }

        [HttpGet]
        public ActionResult _CreateAttendance()
        {
            var attendance = new Attendance()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            return PartialView("_CreateEdit", attendance);
        }

        [HttpPost]
        public ActionResult _CreateAttendance(Attendance attendance)
        {
            if (!ModelState.IsValid) return PartialView("_CreateEdit", attendance);
            work.Attendance.Create(attendance);

            return AjaxRedirectTo("/attendance");
        }

        [HttpGet]
        public ActionResult _EditAttendance(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var attendance = work.Attendance.Get(id);

            if (attendance == null)
            {
                return HttpNotFound();
            }

            return PartialView("_CreateEdit", attendance);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditAttendance(Attendance attendance)
        {
            if (!ModelState.IsValid) return PartialView("_CreateEdit", attendance);
            attendance.ModifiedDate = DateTime.Now;
            attendance.ModifiedBy = SessionVariables.CurrentUser.User.Id;

            work.Attendance.Update(attendance);

            return AjaxRedirectTo("/attendance");
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var attendance = work.Attendance.Get(id);

            if (attendance == null)
            {
                return HttpNotFound();
            }

            work.Attendance.Delete(attendance.Id);

            return RedirectToAction("index");
        }
    }
}