using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
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
    [RequirePermission(ModuleId = "5657282714eb5e195939d7455e9027")]
    public class TasksController : BaseController
    {
        public ActionResult Index(string status)
        {
            return View(work.Task.GetAllByStatus(status));
        }

        public ActionResult MyTasks()
        {
            return View("Index", work.Task.GetAllByStatus(SessionVariables.CurrentUser.User.Id));
        }

        [HttpGet]
        public ActionResult _CreateTask()
        {
            var task = new TaskSD()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id
            };

            return PartialView("_CreateEdit", task);
        }

        [HttpPost]
        public ActionResult _CreateTask(TaskSD task)
        {
            if (ModelState.IsValid)
            {
                task.CreatedDate = DateTime.Now;
                task.CreatedBy = SessionVariables.CurrentUser.User.Id;

                work.Task.Create(task);

                return AjaxRedirectTo("/tasks");
            }

            return PartialView("_CreateEdit", task);
        }

        [HttpGet]
        public ActionResult _EditTask(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            TaskSD task = work.Task.Get(id);

            if (task == null)
            {
                return HttpNotFound();
            }

            return PartialView("_CreateEdit", task);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditTask(TaskSD task)
        {
            if (ModelState.IsValid)
            {
                task.ModifiedDate = DateTime.Now;
                task.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                work.Task.Update(task);

                return AjaxRedirectTo("/tasks");
            }

            return PartialView("_CreateEdit", task);
        }

        [HttpGet]
        public ActionResult _Complete(string id)
        {
            var task = work.Task.Get(id);
            if (task.Completed)
            {
                task.Completed = false;
                task.DateCompleted = null;
                task.CompletedBy = string.Empty;

                work.Task.Update(task);

                return JavaScript("$('.task-text-" + task.Id + "').removeClass('task-text-complete');$('.task-check-" + task.Id + "').removeClass('task-check-complete kt-font-success');$('.task-label-" + task.Id + "').show();$('.task-status-icon-" + task.Id + "').removeClass('fa-check-circle kt-font-success');$('.task-status-icon-" + task.Id + "').addClass('fa-circle');");
            }
            else
            {
                task.Completed = true;
                task.DateCompleted = DateTime.Now;
                task.CompletedBy = SessionVariables.CurrentUser.User.Id;

                work.Task.Update(task);

                return JavaScript("$('.task-text-" + task.Id + "').addClass('task-text-complete');$('.task-check-" + task.Id + "').addClass('task-check-complete');$('.task-label-" + task.Id + "').hide();$('.task-status-icon-" + task.Id + "').addClass('fa-check-circle kt-font-success');$('.task-status-icon-" + task.Id + "').removeClass('fa-circle');");
            }
        }

        [HttpGet]
        public ActionResult DeleteTask(string id)
        {
            var task = work.Task.Get(id);
            work.Task.Delete(task);

            return RedirectToAction("index");
        }

        public ActionResult DetailTasks(string id)
        {
            var result = new DetailTasksViewModel();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            result.Tasks = work.Task.Get(id);
            result.Users = work.User.GetRequester(result.Tasks.CreatedBy, result.Tasks.ModifiedBy);
            result.UserSetting = work.UserSetting.GetByUserId(result.Tasks.CreatedBy);
            return PartialView("_Details", result);
        }

        [HttpPost]
        public ActionResult DetailTasks(TaskSD tasks)
        {
            var result = work.Task.GetDetails(SessionVariables.CurrentUser.User.Id, tasks);
            return PartialView("_Details", result);
        }

        [HttpGet]
        public ActionResult CompleteTask(string id)
        {
            var task = work.Task.Get(id);

            if (task.Completed)
            {
                task.Completed = false;
                task.DateCompleted = null;
                task.CompletedBy = string.Empty;

                work.Task.Update(task);

                return RedirectToAction("index");
            }
            else
            {
                task.Completed = true;
                task.DateCompleted = DateTime.Now;
                task.CompletedBy = SessionVariables.CurrentUser.User.Id;

                work.Task.Update(task);

                return RedirectToAction("index");
            }
        }
    }
}