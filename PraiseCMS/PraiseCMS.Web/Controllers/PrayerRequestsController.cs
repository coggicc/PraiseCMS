using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]

    [RequirePermission(ModuleId = "95936354299c1669b236e5435cb4a9")]
    public class PrayerRequestsController : BaseController
    {
        private const string UncategorizedCategoryName = "Uncategorized";

        public ActionResult Index(string campusId, string request, string categoryId, string sortType = SortOrders.Descending, string filterKeyword = null, int page = 1, int pageSize = 25)
        {
            //set inbox setting default if null
            var user = SessionVariables.CurrentUser.User;

            if (user.InboxDensity.IsNullOrEmptyOrDbNull() || user.InboxType.IsNullOrEmptyOrDbNull())
            {
                user.InboxDensity = user.InboxDensity.IsNullOrEmptyOrDbNull() ? InboxDensity.Default : user.InboxDensity;
                user.InboxType = user.InboxType.IsNullOrEmptyOrDbNull() ? InboxType.Default : user.InboxType;
                work.User.Update(user);
                SessionVariables.CurrentUser.User = user;
            }

            var prayerRequestsView = work.PrayerRequest.GetDashboard(SessionVariables.CurrentChurch.Id, campusId, request, categoryId, user.InboxType, SessionVariables.CurrentUser.AllPermissions, sortType, filterKeyword, page, pageSize);

            // Fetch all categories and their respective counts for the sidebar
            ViewBag.SidebarData = work.PrayerRequest.GetSidebarData(SessionVariables.CurrentChurch.Id);

            ViewBag.NotPrayedOverCount = prayerRequestsView.NotPrayedOverCount;
            ViewBag.PrayerRequestType = prayerRequestsView.PrayerRequestType;
            ViewBag.CampusId = campusId;
            ViewBag.Request = request;
            ViewBag.CategoryId = categoryId;
            ViewBag.SortType = sortType;
            ViewBag.FilterKeyword = filterKeyword;
            ViewBag.Page = page;
            ViewBag.PageSize = pageSize;

            return View(prayerRequestsView);
        }

        #region CRUD
        [HttpGet]
        public ActionResult _AddPrayerRequest()
        {
            var prayerRequest = new PrayerRequest()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                NotifyPrayerTeam = SessionVariables.CurrentChurch.AutoNotifyPrayerTeam
            };
            var model = new PrayerRequestVM()
            {
                PrayerRequest = prayerRequest,
                Categories = work.PrayerRequest.GetAllCategories(),
                People = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id),
                Mode = PeopleSelectionMode.System
            };
            ViewBag.ActionName = "_AddPrayerRequest";

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _AddPrayerRequest(PrayerRequestVM model)
        {
            try
            {
                model.PrayerRequest.CreatedDate = DateTime.Now;
                model.PrayerRequest.CreatedBy = SessionVariables.CurrentUser.User.Id;

                //Set null categories to uncategorized
                if (!string.IsNullOrEmpty(model.PrayerRequest.CategoryId))
                {
                    var uncategorized = work.PrayerRequest.GetCategoryByName(UncategorizedCategoryName);
                    model.PrayerRequest.CategoryId = uncategorized != null ? uncategorized.Id : string.Empty;
                }

                //Set as incomplete when creating as it is new
                if (model.PrayerRequest.FollowUpRequired)
                {
                    model.PrayerRequest.FollowUpStatus = FollowUpStatuses.Incomplete;
                }

                if (model.Mode.Equals(PeopleSelectionMode.Manual) && TryValidateModel(model.Person))
                {
                    var person = work.Person.GetByEmailAndPhoneAndName(model.Person.Email, model.Person.PhoneNumber, model.Person.FirstName, model.Person.LastName);

                    if (person.IsNullOrEmpty())
                    {
                        person = new Person()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            CreatedBy = SessionVariables.CurrentUser.User.Id,
                            CreatedDate = DateTime.Now,
                            FirstName = model.Person.FirstName,
                            LastName = model.Person.LastName,
                            Email = model.Person.Email,
                            PhoneNumber = model.Person.PhoneNumber,
                            IsActive = true
                        };
                        work.Person.Create(person);
                        work.Person.CreateChurchPerson(new ChurchPerson
                        {
                            Id = Utilities.GenerateUniqueId(),
                            PersonId = person.Id,
                            ChurchId = SessionVariables.CurrentChurch.Id,
                            CreatedBy = SessionVariables.CurrentUser.User.Id,
                            CreatedDate = DateTime.Now
                        });
                    }

                    model.PrayerRequest.PersonId = person.Id;
                }

                work.PrayerRequest.Create(model.PrayerRequest);

                var notification = new Notification
                {
                    Id = Utilities.GenerateUniqueId(),
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now,
                    Type = "Prayer Request",
                    TypeId = model.PrayerRequest.Id,
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    Name = "New Prayer Request",
                    Viewed = false
                };
                work.Notification.Create(notification);

                if (model.PrayerRequest.NotifyPrayerTeam)
                {
                    var subject = string.Empty;

                    if (model.PrayerRequest.Confidential)
                    {
                        subject += "Confidential, ";
                    }

                    if (model.PrayerRequest.HighPriority)
                    {
                        subject += "High Priority, ";
                    }

                    subject += "New Prayer Request";
                    var email = new Email()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Message = EmailTemplates.General.Replace("{message}", model.PrayerRequest.Message),
                        To = SessionVariables.CurrentChurch.PrayerRequestEmail.IsNotNullOrEmpty() ? SessionVariables.CurrentChurch.PrayerRequestEmail : SessionVariables.CurrentChurch.Email,
                        Attachments = null,
                        Subject = subject,
                        CreatedBy = SessionVariables.CurrentUser?.User.Id != null ? SessionVariables.CurrentUser.User.Id : string.Empty,
                        CreatedDate = DateTime.Now
                    };

                    Emailer.SendEmail(email, null, null, new Domain() { EmailLogo = SessionVariables.CurrentChurch.Logo, Name = SessionVariables.CurrentChurch.Display, EmailReplyAddress = SessionVariables.CurrentChurch.Email, EmailDisplay = SessionVariables.CurrentChurch.Display }, SessionVariables.CurrentChurch);
                }

                if (model.PrayerRequest.PrayedOver && model.PrayerRequest.PersonId.IsNotNullOrEmpty() && SessionVariables.CurrentChurch.CompletedPrayerRequestAlert)
                {
                    var prayerRequests = new List<PrayerRequest>
                    {
                        model.PrayerRequest
                    };
                    work.PrayerRequest.SendPrayedOverNotification(prayerRequests, SessionVariables.CurrentChurch, SessionVariables.CurrentUser.User);
                }

                CreateAlertMessage("The prayer request has been created.", AlertMessageTypes.Success, AlertMessageIcons.Success);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                CreateAlertMessage($"{Constants.DefaultErrorMessage} {ex.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return RedirectToAction(nameof(Index));
        }

        public ActionResult _EditPrayerRequest(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var prayerRequest = work.PrayerRequest.Get(id);

            if (prayerRequest == null)
            {
                return HttpNotFound();
            }

            var model = new PrayerRequestVM()
            {
                PrayerRequest = prayerRequest,
                Categories = work.PrayerRequest.GetAllCategories(),
                People = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id),
                Mode = PeopleSelectionMode.System
            };
            ViewBag.ActionName = "_EditPrayerRequest";

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditPrayerRequest(PrayerRequestVM model)
        {
            try
            {
                var prayerRequest = work.PrayerRequest.Get(model.PrayerRequest.Id);
                var prayedOverEmail = !prayerRequest.PrayedOver && model.PrayerRequest.PrayedOver;
                model.PrayerRequest.Number = prayerRequest.Number;
                model.PrayerRequest.ModifiedDate = DateTime.Now;
                model.PrayerRequest.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                //Set null categories to uncategorized
                if (!string.IsNullOrEmpty(model.PrayerRequest.CategoryId))
                {
                    var uncategorized = work.PrayerRequest.GetCategoryByName(UncategorizedCategoryName);
                    model.PrayerRequest.CategoryId = uncategorized != null ? uncategorized.Id : string.Empty;
                }

                if (model.Mode.Equals(PeopleSelectionMode.Manual) && TryValidateModel(model.Person))
                {
                    var person = work.Person.GetByEmailAndPhoneAndName(model.Person.Email, model.Person.PhoneNumber, model.Person.FirstName, model.Person.LastName);

                    if (person.IsNullOrEmpty())
                    {
                        person = new Person()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            CreatedBy = SessionVariables.CurrentUser.User.Id,
                            CreatedDate = DateTime.Now,
                            FirstName = model.Person.FirstName,
                            LastName = model.Person.LastName,
                            Email = model.Person.Email,
                            PhoneNumber = model.Person.PhoneNumber,
                            IsActive = true
                        };
                        work.Person.Create(person);
                        work.Person.CreateChurchPerson(new ChurchPerson
                        {
                            Id = Utilities.GenerateUniqueId(),
                            PersonId = person.Id,
                            ChurchId = SessionVariables.CurrentChurch.Id,
                            CreatedBy = SessionVariables.CurrentUser.User.Id,
                            CreatedDate = DateTime.Now
                        });
                    }
                    model.PrayerRequest.PersonId = person.Id;
                }

                prayerRequest.Read = model.PrayerRequest.Read;
                prayerRequest.Message = model.PrayerRequest.Message;
                prayerRequest.PersonId = model.PrayerRequest.PersonId;
                prayerRequest.Responded = model.PrayerRequest.Responded;
                prayerRequest.RespondedDate = model.PrayerRequest.RespondedDate;
                prayerRequest.RespondedVia = model.PrayerRequest.RespondedVia;
                prayerRequest.ShareName = model.PrayerRequest.ShareName;
                prayerRequest.Starred = model.PrayerRequest.Starred;
                prayerRequest.PrayedOver = model.PrayerRequest.PrayedOver;
                prayerRequest.PrayedOverDate = model.PrayerRequest.PrayedOverDate;
                prayerRequest.CallBackPhone = model.PrayerRequest.CallBackPhone;
                prayerRequest.CategoryId = model.PrayerRequest.CategoryId;
                prayerRequest.Confidential = model.PrayerRequest.Confidential;
                prayerRequest.FollowUpDate = model.PrayerRequest.FollowUpDate;
                prayerRequest.FollowUpMethod = model.PrayerRequest.FollowUpMethod;
                prayerRequest.FollowUpRequired = model.PrayerRequest.FollowUpRequired;
                prayerRequest.FollowUpStatus = model.PrayerRequest.FollowUpRequired && !string.IsNullOrEmpty(model.PrayerRequest.FollowUpStatus) ? model.PrayerRequest.FollowUpStatus : FollowUpStatuses.Incomplete;
                prayerRequest.HighPriority = model.PrayerRequest.HighPriority;
                prayerRequest.InternalNote = model.PrayerRequest.InternalNote;
                prayerRequest.PrayedOverBy = model.PrayerRequest.PrayedOverBy;
                prayerRequest.FollowUpBy = model.PrayerRequest.FollowUpBy;
                work.PrayerRequest.Update(prayerRequest, SessionVariables.CurrentUser.User.Id);

                const string _updatePrayerRequestText = "Updated Prayer Request";

                if (!model.PrayerRequest.PrayedOver)
                {
                    var notification = new Notification
                    {
                        Id = Utilities.GenerateUniqueId(),
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now,
                        Type = "Prayer Request",
                        TypeId = model.PrayerRequest.Id,
                        ChurchId = SessionVariables.CurrentChurch.Id,
                        Name = _updatePrayerRequestText,
                        Viewed = false
                    };

                    work.Notification.Create(notification);

                    var subject = string.Empty;

                    if (model.PrayerRequest.Confidential)
                    {
                        subject += PrayerRequestStatuses.Confidential + ", ";
                    }

                    if (model.PrayerRequest.HighPriority)
                    {
                        subject += PrayerRequestStatuses.HighPriority + ", ";
                    }

                    subject += _updatePrayerRequestText;

                    if (model.PrayerRequest.NotifyPrayerTeam)
                    {
                        var email = new Email()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            Message = EmailTemplates.General.Replace("{message}", model.PrayerRequest.Message),
                            To = SessionVariables.CurrentChurch.PrayerRequestEmail.IsNotNullOrEmpty() ? SessionVariables.CurrentChurch.PrayerRequestEmail : SessionVariables.CurrentChurch.Email,
                            Attachments = null,
                            Subject = subject,
                            CreatedBy = SessionVariables.CurrentUser?.User.Id != null ? SessionVariables.CurrentUser.User.Id : string.Empty,
                            CreatedDate = DateTime.Now
                        };

                        Emailer.SendEmail(email, null, null, new Domain() { EmailLogo = SessionVariables.CurrentChurch.Logo, Name = SessionVariables.CurrentChurch.Display, EmailReplyAddress = SessionVariables.CurrentChurch.Email, EmailDisplay = SessionVariables.CurrentChurch.Display }, SessionVariables.CurrentChurch);
                    }
                }

                if (prayedOverEmail && model.PrayerRequest.PrayedOver && model.PrayerRequest.PersonId.IsNotNullOrEmpty() && SessionVariables.CurrentChurch.CompletedPrayerRequestAlert)
                {
                    var prayerRequests = new List<PrayerRequest>
                    {
                        model.PrayerRequest
                    };
                    work.PrayerRequest.SendPrayedOverNotification(prayerRequests, SessionVariables.CurrentChurch, SessionVariables.CurrentUser.User);
                }

                CreateAlertMessage("The prayer request has been updated.", AlertMessageTypes.Success, AlertMessageIcons.Success);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                CreateAlertMessage($"{Constants.DefaultErrorMessage} {ex.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public ActionResult DeletePrayerRequest(string id, string returnUrl)
        {
            work.PrayerRequest.Delete(id);
            return Redirect(returnUrl);
        }
        #endregion

        #region Prayer Request Details
        public ActionResult DetailPrayerRequest(string id, int orderNumber, int totalCount, string previousId, string nextId)
        {
            var result = new PrayerRequestDetailsViewModel();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            result.PrayerRequest = work.PrayerRequest.Get(id);

            if (result.PrayerRequest.PersonId.IsNotNullOrEmpty())
            {
                result.Person = work.Person.Get(result.PrayerRequest.PersonId);
            }

            result.PrayerRequestCategories = work.PrayerRequest.GetAllCategories();

            result.Users = work.User.GetRequester(result.PrayerRequest.CreatedBy, result.PrayerRequest.ModifiedBy);
            result.PrayerRequest.Read = true;

            work.PrayerRequest.Update(result.PrayerRequest, SessionVariables.CurrentUser.User.Id);

            if (result.PrayerRequest.FollowUpBy.IsNotNullOrEmpty())
            {
                result.PrayerRequest.FollowUpUser = work.User.Get(result.PrayerRequest.FollowUpBy);
            }

            result.OrderNumber = orderNumber;
            result.TotalPrayerRequests = totalCount;
            result.PreviousId = previousId;
            result.NextId = nextId;

            return PartialView("_Details", result);
        }

        [HttpPost]
        public ActionResult DetailPrayerRequest(PrayerRequest model)
        {
            var prayerRequest = work.PrayerRequest.Get(model.Id);

            prayerRequest.ModifiedBy = SessionVariables.CurrentUser.User.Id;
            prayerRequest.ModifiedDate = DateTime.Now;

            if (!string.IsNullOrEmpty(model.InternalNote))
            {
                prayerRequest.InternalNote = model.InternalNote;
            }

            prayerRequest.PrayedOver = model.PrayedOver;
            prayerRequest.HighPriority = model.HighPriority;

            work.PrayerRequest.Update(prayerRequest, SessionVariables.CurrentUser.User.Id);

            var result = new PrayerRequestDetailsViewModel
            {
                PrayerRequest = prayerRequest,
                PrayerRequestCategories = work.PrayerRequest.GetAllCategories()
            };
            result.Users = work.User.GetRequester(result.PrayerRequest.CreatedBy, result.PrayerRequest.ModifiedBy);

            var allRequests = work.PrayerRequest.GetAll(SessionVariables.CurrentChurch.Id)
            .OrderByDescending(x => x.CreatedDate)
            .ToList();

            result.PrayerRequests = allRequests;
            result.OrderNumber = allRequests.IndexOf(result.PrayerRequest) + 1;

            return PartialView("_Details", result);
        }

        public ActionResult DashboardPrayerRequestDetails(string id)
        {
            var result = new PrayerRequestDetailsViewModel();

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            result.PrayerRequest = work.PrayerRequest.Get(id);

            if (result.PrayerRequest.PersonId.IsNotNullOrEmpty())
            {
                result.Person = work.Person.Get(result.PrayerRequest.PersonId);
            }

            result.PrayerRequestCategories = work.PrayerRequest.GetAllCategories();

            result.Users = work.User.GetRequester(result.PrayerRequest.CreatedBy, result.PrayerRequest.ModifiedBy);
            result.PrayerRequest.Read = true;

            work.PrayerRequest.Update(result.PrayerRequest, SessionVariables.CurrentUser.User.Id);

            if (result.PrayerRequest.FollowUpBy.IsNotNullOrEmpty())
            {
                result.PrayerRequest.FollowUpUser = work.User.Get(result.PrayerRequest.FollowUpBy);
            }

            return PartialView("_PrayerRequestDetails", result);
        }
        #endregion

        #region Mark Read/Unread
        [HttpPost]
        public ActionResult _MarkRead(List<string> ids, string url, string action)
        {
            try
            {
                var readAction = action.IsNotNullOrEmpty() && action.EqualsIgnoreCase(PrayerRequestStatuses.Read);

                if (ids?.Any() == true)
                {
                    var prayerRequests = work.PrayerRequest.Get(ids);

                    if (prayerRequests.Any())
                    {
                        MarkPrayerRequestsAsRead(prayerRequests, readAction);
                    }
                }

                var model = work.PrayerRequest.GetRequestListPartial(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.InboxType, SessionVariables.CurrentUser.AllPermissions, url);

                return PartialView("_RequestList", model);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, ex.Message });
            }
        }

        [HttpPost]
        public ActionResult _MarkDetailsRead(string id, string action)
        {
            try
            {
                var readAction = action.IsNotNullOrEmpty() && action.EqualsIgnoreCase(PrayerRequestStatuses.Read);

                if (id.IsNotNullOrEmpty())
                {
                    var prayerRequest = work.PrayerRequest.Get(id);

                    if (prayerRequest.IsNotNullOrEmpty())
                    {
                        prayerRequest.Read = readAction;
                        prayerRequest.ModifiedDate = DateTime.Now;
                        prayerRequest.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                        work.PrayerRequest.Update(prayerRequest, SessionVariables.CurrentUser.User.Id);
                    }

                    return Json(new { Success = true, IsRead = prayerRequest.Read });
                }

                return Json(new { Success = false, Message = "Parameter request Id can't be null." });
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {ex.Message}" });
            }
        }

        [HttpPost]
        public ActionResult MarkReadFromDashboard(string id, string action)
        {
            try
            {
                var readAction = action.IsNotNullOrEmpty() && action.EqualsIgnoreCase(PrayerRequestStatuses.Read);

                if (id.IsNotNullOrEmpty())
                {
                    var prayerRequest = work.PrayerRequest.Get(id);

                    if (prayerRequest != null)
                    {
                        MarkPrayerRequestsAsRead(new List<PrayerRequest> { prayerRequest }, readAction);
                    }
                }

                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, ex.Message });
            }
        }

        private void MarkPrayerRequestsAsRead(IEnumerable<PrayerRequest> prayerRequests, bool readAction)
        {
            if (!prayerRequests.Any())
            {
                return;
            }

            foreach (var item in prayerRequests)
            {
                item.Read = readAction;
                item.ModifiedDate = DateTime.Now;
                item.ModifiedBy = SessionVariables.CurrentUser.User.Id;
            }

            work.PrayerRequest.SaveChanges();
        }
        #endregion

        #region Mark Prayed Over
        [HttpPost]
        public ActionResult _MarkPrayedOver(List<string> ids, string url, string action)
        {
            try
            {
                var prayedOverAction = action.IsNotNullOrEmpty() && action.Equals(PrayerRequestStatuses.PrayedOver);

                if (ids?.Any() == true)
                {
                    var prayerRequests = work.PrayerRequest.Get(ids);

                    if (prayerRequests.Any())
                    {
                        MarkPrayerRequestAsPrayedOver(prayerRequests, prayedOverAction);
                    }
                }

                var model = work.PrayerRequest.GetRequestListPartial(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.InboxType, SessionVariables.CurrentUser.AllPermissions, url);

                return PartialView("_RequestList", model);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, ex.Message });
            }
        }

        [HttpPost]
        public ActionResult _MarkDetailsPrayedOver(string id, string action)
        {
            try
            {
                var prayedOverAction = action.IsNotNullOrEmpty() && action.EqualsIgnoreCase(PrayerRequestStatuses.PrayedOver);

                if (id.IsNotNullOrEmpty())
                {
                    var prayerRequest = work.PrayerRequest.Get(id);

                    if (prayerRequest.IsNotNullOrEmpty())
                    {
                        // Update the PrayedOver status
                        prayerRequest.PrayedOver = prayedOverAction;
                        prayerRequest.ModifiedDate = DateTime.Now;
                        prayerRequest.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                        work.PrayerRequest.Update(prayerRequest, SessionVariables.CurrentUser.User.Id);

                        // Return the updated prayed over status
                        return Json(new { Success = true, IsPrayedOver = prayerRequest.PrayedOver });
                    }

                    return Json(new { Success = false, Message = "Prayer request not found." });
                }

                return Json(new { Success = false, Message = "Parameter request Id can't be null." });
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {ex.Message}" });
            }
        }

        [HttpPost]
        public ActionResult MarkPrayedOverFromDashboard(string id, string action)
        {
            try
            {
                var prayedOverAction = action.IsNotNullOrEmpty() && action.Equals(PrayerRequestStatuses.PrayedOver);

                if (id.IsNotNullOrEmpty())
                {
                    var prayerRequest = work.PrayerRequest.Get(id);

                    if (prayerRequest != null)
                    {
                        MarkPrayerRequestAsPrayedOver(new List<PrayerRequest> { prayerRequest }, prayedOverAction);
                    }
                }

                return Json(new { Success = true });
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, ex.Message });
            }
        }

        private void MarkPrayerRequestAsPrayedOver(IEnumerable<PrayerRequest> prayerRequests, bool prayedOverAction)
        {
            if (!prayerRequests.Any())
            {
                return;
            }

            foreach (var item in prayerRequests)
            {
                item.PrayedOver = prayedOverAction;
                item.PrayedOverBy = prayedOverAction ? SessionVariables.CurrentUser.User.Id : null;
                item.PrayedOverDate = prayedOverAction ? DateTime.Now : (DateTime?)null;
                item.ModifiedDate = DateTime.Now;
                item.ModifiedBy = SessionVariables.CurrentUser.User.Id;
            }

            work.PrayerRequest.SendPrayedOverNotification(prayerRequests.Where(x => !x.PrayedOver && x.PersonId.IsNotNullOrEmpty()).ToList(), SessionVariables.CurrentChurch, SessionVariables.CurrentUser.User);

            work.PrayerRequest.SaveChanges();
        }

        //private void MarkPrayerRequestAsPrayedOver(string id, bool prayedOverAction)
        //{
        //    var item = work.PrayerRequest.Get(id);

        //    if (item != null)
        //    {
        //        if (prayedOverAction && SessionVariables.CurrentChurch.CompletedPrayerRequestAlert)
        //        {
        //            var prayerRequests = new List<PrayerRequest> { item };
        //            work.PrayerRequest.SendPrayedOverNotification(prayerRequests.Where(x => !x.PrayedOver && x.PersonId.IsNotNullOrEmpty()).ToList());
        //        }

        //        item.PrayedOver = prayedOverAction;
        //        item.PrayedOverDate = prayedOverAction ? DateTime.Now : (DateTime?)null;
        //        item.PrayedOverBy = SessionVariables.CurrentUser.User.Id;
        //        item.ModifiedDate = DateTime.Now;
        //        item.ModifiedBy = SessionVariables.CurrentUser.User.Id;

        //        work.PrayerRequest.SaveChanges();
        //    }
        //}

        //[HttpPost]
        //public ActionResult PrayedOver(string id, string url)
        //{
        //    try
        //    {
        //        if (id.IsNotNullOrEmpty())
        //        {
        //            MarkPrayerRequestAsPrayedOver(id, prayedOverAction: true);
        //        }

        //        ViewBag.currentUrl = url;

        //        return AjaxRedirectTo(url);
        //    }
        //    catch (Exception ex)
        //    {
        //        ExceptionLogger.LogException(ex);

        //        return View("Error");
        //    }
        //}        
        #endregion

        #region Follow Up
        public ActionResult FollowUp(string id)
        {
            var users = work.PrayerRequest.GetPrayerRequestsPermittedUsers(SessionVariables.CurrentChurch.Id);
            var prayerRequest = work.PrayerRequest.Get(id);

            if (prayerRequest.IsNotNullOrEmpty() && prayerRequest.FollowUpBy.IsNullOrEmpty())
            {
                prayerRequest.FollowUpBy = users.Any(q => q.Id.Equals(SessionVariables.CurrentUser.User.Id)) ? SessionVariables.CurrentUser.User.Id : null;
            }

            TempData["users"] = users;
            prayerRequest.FollowUpTime = prayerRequest.FollowUpDate.IsNotNullOrEmpty() ? Convert.ToDateTime(prayerRequest.FollowUpDate).ToString("hh:mm tt") : string.Empty;

            return PartialView("_FollowUpDetails", prayerRequest);
        }

        [HttpPost]
        public ActionResult FollowUp(PrayerRequest model, string url)
        {
            var prayerRequests = work.PrayerRequest.Get(model.Id);

            if (prayerRequests.IsNotNullOrEmpty())
            {
                var followUpTime = DateTime.Parse(model.FollowUpTime);
                var followUpDate = Convert.ToDateTime(model.FollowUpDate);
                prayerRequests.FollowUpStatus = model.FollowUpStatus;
                prayerRequests.FollowUpBy = model.FollowUpBy;
                prayerRequests.FollowUpDate = new DateTime(followUpDate.Year, followUpDate.Month, followUpDate.Day, followUpTime.Hour, followUpTime.Minute, followUpTime.Second);
                prayerRequests.FollowUpMethod = model.FollowUpMethod;
                prayerRequests.ModifiedDate = DateTime.Now;
                prayerRequests.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                work.PrayerRequest.SaveChanges();
            }

            ViewBag.currentUrl = url;

            return AjaxRedirectTo(url);
        }
        #endregion

        #region Mark Starred
        [HttpPost]
        public ActionResult MarkStarred(string id)
        {
            try
            {
                if (!string.IsNullOrEmpty(id))
                {
                    var prayerRequest = work.PrayerRequest.Get(id);

                    if (prayerRequest != null)
                    {
                        prayerRequest.Starred = !prayerRequest.Starred;
                        prayerRequest.ModifiedDate = DateTime.Now;
                        prayerRequest.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                        work.PrayerRequest.Update(prayerRequest, SessionVariables.CurrentUser.User.Id);

                        return Json(new { Success = true, Starred = prayerRequest.Starred });
                    }

                    return Json(new { Success = false, Message = "Prayer request not found." });
                }

                return Json(new { Success = false, Message = "Parameter request Id can't be null." });
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {ex.Message}" });
            }
        }
        #endregion

        #region Filter Prayer Requests
        [HttpPost]
        public ActionResult _FilterPrayerRequests(string keyword)
        {
            return AjaxRedirectTo("/PrayerRequests");
        }
        #endregion

        #region External Prayer Requests
        [HttpGet]
        [AllowAnonymous]
        [OverrideActionFilters]
        public ActionResult CreatePrayerRequestExternal(string id)
        {
            if (!id.IsNotNullOrEmpty())
            {
                return Json("Invalid Church Id", JsonRequestBehavior.AllowGet);
            }

            var model = work.PrayerRequest.CreateBasicExternalPrayerRequestModel(id);

            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        [OverrideActionFilters]
        public ActionResult CreatePrayerRequestExternal(PrayerRequestVM model)
        {
            var church = work.Church.Get(model.PrayerRequest.ChurchId);

            if (string.IsNullOrEmpty(church.PrayerRequestReceivedText))
            {
                church.PrayerRequestReceivedText = Constants.DefaultPrayerRequestReceivedText;
            }

            if (string.IsNullOrEmpty(church.PrayerRequestReceivedFollowUpText))
            {
                church.PrayerRequestReceivedFollowUpText = Constants.DefaultPrayerRequestReceivedFollowUpText;
            }

            //Set as incomplete when creating as it is new
            if (model.PrayerRequest.FollowUpRequired)
            {
                model.PrayerRequest.FollowUpStatus = FollowUpStatuses.Incomplete;
            }

            var result = new ResponseModel();

            try
            {
                if (model.Mode.Equals(PeopleSelectionMode.Manual))
                {
                    var person = work.Person.GetByEmailAndPhoneAndName(model.Person.Email, model.Person.PhoneNumber, model.Person.FirstName, model.Person.LastName);

                    if (person.IsNullOrEmpty())
                    {
                        person = new Person()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            FirstName = model.Person.FirstName,
                            LastName = model.Person.LastName,
                            Email = model.Person.Email,
                            PhoneNumber = model.Person.PhoneNumber,
                            IsActive = true,
                            CreatedBy = Constants.System,
                            CreatedDate = DateTime.Now,
                        };
                        work.Person.Create(person);
                        work.Person.CreateChurchPerson(new ChurchPerson
                        {
                            Id = Utilities.GenerateUniqueId(),
                            PersonId = person.Id,
                            ChurchId = church.Id,
                            CreatedBy = person.Id,
                            CreatedDate = DateTime.Now
                        });
                    }

                    if (model.PrayerRequest.FollowUpRequired)
                    {
                        model.PrayerRequest.CallBackPhone = model.Person.PhoneNumber;
                    }

                    model.PrayerRequest.PersonId = person.Id;
                }

                work.PrayerRequest.Create(model.PrayerRequest);

                var notification = new Notification
                {
                    Id = Utilities.GenerateUniqueId(),
                    CreatedBy = Constants.System,
                    CreatedDate = DateTime.Now,
                    Type = "External Prayer Request",
                    TypeId = model.PrayerRequest.Id,
                    ChurchId = church.Id,
                    Name = "New Prayer Request",
                    Viewed = false
                };

                work.Notification.Create(notification);

                var subject = string.Empty;

                if (model.PrayerRequest.Confidential)
                {
                    subject += "Confidential - ";
                }

                subject += "New Prayer Request";

                if (church.PrayerRequestEmail.IsNotNullOrEmpty())
                {
                    var email = new Email()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Message = EmailTemplates.General_With_Heading.Replace("{user_display}", string.Empty).Replace("Hi ,", string.Empty).Replace("{message}", model.PrayerRequest.Message),
                        To = church.PrayerRequestEmail,
                        Attachments = null,
                        Subject = subject,
                        CreatedDate = DateTime.Now,
                        CreatedBy = Constants.System
                    };

                    Emailer.SendEmail(email, null, null, new Domain()
                    {
                        EmailLogo = church.Logo,
                        Name = church.Display,
                        EmailReplyAddress = church.Email,
                        EmailDisplay = church.Display
                    }, church);
                }

                result.Success = true;
                result.Message = model.PrayerRequest.FollowUpRequired ? church.PrayerRequestReceivedFollowUpText : church.PrayerRequestReceivedText;

                return View("ThankyouPage", result);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                CreateAlertMessage($"There was a problem submitting your prayer request. Please try again later. {ex.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                var returnModel = work.PrayerRequest.CreateBasicExternalPrayerRequestModel(church.Id);

                return View(returnModel);
            }
        }
        #endregion

        #region Prayer Request Categories
        [RequireRole(Role = Roles.SuperAdmin)]
        public ActionResult Categories()
        {
            var cat = work.PrayerRequest.GetAllCategories();
            return View(cat);
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult CreateCategory()
        {
            var category = new PrayerRequestCategory()
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedDate = DateTime.Now,
                CreatedBy = SessionVariables.CurrentUser.User.Id
            };

            return PartialView("_CreateEditCategory", category);
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpPost]
        public ActionResult CreateCategory(PrayerRequestCategory category)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEditCategory", category);
            }

            work.PrayerRequest.CreateCategory(category);
            return AjaxRedirectTo("/prayerrequests/categories");
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult EditCategory(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = work.PrayerRequest.GetCategory(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return PartialView("_CreateEditCategory", category);
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(PrayerRequestCategory category)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEditCategory", category);
            }

            work.PrayerRequest.UpdateCategory(category);

            return AjaxRedirectTo("/prayerrequests/categories");
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult DeleteCategory(string id)
        {
            work.PrayerRequest.Delete(id);
            return RedirectToAction("categories");
        }
        #endregion

        #region User Inbox Setting
        public ActionResult UserInboxSetting()
        {
            return PartialView("_UserInboxSettings", SessionVariables.CurrentUser.User);
        }

        [HttpPost]
        public ActionResult UserInboxSetting(ApplicationUser model)
        {
            var user = work.User.Get(model.Id);
            user.InboxDensity = model.InboxDensity;
            user.InboxType = model.InboxType;
            work.User.Update(user);

            SessionVariables.CurrentUser.User = user;

            return AjaxRedirectTo("/prayerrequests");
        }
        #endregion
    }
}