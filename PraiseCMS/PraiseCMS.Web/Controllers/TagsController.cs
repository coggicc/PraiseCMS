using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    [RequirePermission(ModuleId = "295783966513529ae01de048d7aaa8")]
    public class TagsController : BaseController
    {
        public ActionResult Index()
        {
            var folders = work.Tags.GetAllFolders(SessionVariables.CurrentChurch.Id);

            if (!folders.Any())
            {
                folders = work.Tags.CreateDefaultFolders(SessionVariables.CurrentChurch.Id);
            }

            var vm = work.Tags.GetChildFoldersAndTagsByParentId(folders.Find(x => x.ParentId.IsNullOrEmptyOrDbNull()).Id);
            ViewBag.AllTags = work.Tags.GetAll(SessionVariables.CurrentChurch.Id);

            return View(vm);
        }

        public ActionResult GetFoldersAndTags(string id)
        {
            var vm = work.Tags.GetChildFoldersAndTagsByParentId(folderId: id);
            return PartialView("_TagsAndFolders", vm);
        }

        public ActionResult GetFolders()
        {
            var folders = work.Tags.GetAllFolders(SessionVariables.CurrentChurch.Id);
            var json = work.Tags.BindFoldersHierarchy(folders);

            return Json(json, JsonRequestBehavior.AllowGet);
        }

        public ActionResult TagDetails(string id)
        {
            var model = new TagDetailsViewModel
            {
                Tag = work.Tags.Get(id),
                People = work.Tags.GetPeopleByTag(id),
                CommunicationHistory = new CommunicationHistoryModel { CommunicationHistory = work.TagCommunication.GetAllByTag(id), TagId = id }
            };

            return View("Details", model);
        }

        public ActionResult UpdateParent(string childId, string parentId)
        {
            try
            {
                var folder = work.Tags.GetFolder(childId);

                if (folder.IsNotNullOrEmpty())
                {
                    folder.ParentId = parentId;
                    var result = work.Tags.Update(folder);

                    if (result.ResultType == ResultType.Success)
                    {
                        return Json(true, JsonRequestBehavior.AllowGet);
                    }
                }

                return Json(false, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(false, JsonRequestBehavior.AllowGet);
            }
        }

        #region Tags
        public ActionResult CreateTag(string folderId)
        {
            var tag = new Tag
            {
                Id = Utilities.GenerateUniqueId(),
                FolderId = folderId,
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            return PartialView("_CreateEdit", tag);
        }

        [HttpPost]
        public ActionResult CreateTag(Tag tag)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = work.Tags.Create(tag);

                    if (result.ResultType == ResultType.Success)
                    {
                        return Json(new { Success = true, Message = "Tag added" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {result.Message}" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Success = false, Message = "Please fill out all required fields" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EditTag(string id, bool isDetailPage = false)
        {
            ViewBag.IsDetailPage = isDetailPage;
            var tag = work.Tags.Get(id);

            return PartialView("_CreateEdit", tag);
        }

        [HttpPost]
        public ActionResult EditTag(Tag tag)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = work.Tags.Update(tag);

                    if (result.ResultType == ResultType.Success)
                    {
                        return Json(new { Success = true, Message = "Tag updated" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {result.Message}" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Success = false, Message = "Please fill out all required fields" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteTag(string id)
        {
            try
            {
                var tag = work.Tags.Get(id);
                var result = work.Tags.Delete(tag);

                if (result.ResultType == ResultType.Success)
                {
                    return Json(new { Success = true, Message = "Tag deleted" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {result.Message}" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        #region Folder      
        public ActionResult CreateFolder(string folderId)
        {
            var folder = new Folder
            {
                Id = Utilities.GenerateUniqueId(),
                ParentId = folderId,
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now,
                Type = FolderTypes.Tag
            };

            return PartialView("_CreateEditFolder", folder);
        }

        [HttpPost]
        public ActionResult CreateFolder(Folder folder)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = work.Tags.Create(folder);

                    if (result.ResultType == ResultType.Success)
                    {
                        return Json(new { Success = true, Message = "Folder added" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {result.Message}" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Success = false, Message = "Please fill out all required fields" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult EditFolder(string id)
        {
            var folder = work.Tags.GetFolder(id);
            return PartialView("_CreateEditFolder", folder);
        }

        [HttpPost]
        public ActionResult EditFolder(Folder folder)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    var result = work.Tags.Update(folder);

                    if (result.ResultType == ResultType.Success)
                    {
                        return Json(new { Success = true, Message = "Folder updated" }, JsonRequestBehavior.AllowGet);
                    }

                    return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {result.Message}" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Success = false, Message = "Please fill out all required fields" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }

        public ActionResult DeleteFolder(string id)
        {
            try
            {
                var folder = work.Tags.GetFolder(id);
                var result = work.Tags.Delete(folder);

                if (result.ResultType == ResultType.Success)
                {
                    return Json(new { Success = true, Message = "Folder deleted" }, JsonRequestBehavior.AllowGet);
                }

                return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {result.Message}" }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {ex.Message}" }, JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

        public ActionResult AddPeopleToTag(string tagId)
        {
            var tagPeople = work.Tags.GetTagPeople(tagId);
            var allPeoples = work.Person.GetAllByPersonIds(churchId: SessionVariables.CurrentChurch.Id).Where(q => !tagPeople.Select(x => x.PersonId).Contains(q.Id)).ToList();
            var model = new TagPeopleViewModel
            {
                People = allPeoples,
                TagId = tagId
            };

            return PartialView("_AddPeopleToTag", model);
        }
        [HttpPost]
        public ActionResult AddPeopleToTag(IEnumerable<string> persons, string tagId)
        {
            var result = work.Tags.AddPeopleToTag(persons, tagId);

            if (result.ResultType == ResultType.Success)
            {
                var model = new TagDetailsViewModel
                {
                    Tag = work.Tags.Get(tagId),
                    People = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id, result.List.Select(q => q.PersonId))
                };

                return PartialView("_TagPeople", model);
            }

            return Json(new { Success = false, result.Message });
        }

        public ActionResult RemovePersonFromTag(string personId, string tagId)
        {
            var result = work.Tags.RemovePeopleFromTag(personId, tagId);

            if (result.ResultType == ResultType.Success)
            {
                var model = new TagDetailsViewModel
                {
                    Tag = work.Tags.Get(tagId),
                    People = work.Tags.GetPeopleByTag(tagId)
                };

                return PartialView("_TagPeople", model);
            }

            return Json(new { Success = false, result.Message });
        }
        public ActionResult RemoveAllPeopleFromTag(string tagId)
        {
            var result = work.Tags.RemoveAllPeopleFromTag(tagId);

            if (result.ResultType == ResultType.Success)
            {
                var model = new TagDetailsViewModel
                {
                    Tag = work.Tags.Get(tagId),
                    People = new List<Person>()
                };

                return PartialView("_TagPeople", model);
            }

            return Json(new { Success = false, result.Message });
        }

        #region Communication        
        public ActionResult Email(string tagId)
        {
            var allPeople = work.Tags.GetPeopleByTag(tagId).Where(q => q.Email.IsNotNullOrEmpty()).ToList();
            var model = new EmailModel
            {
                TagId = tagId,
                BCC = allPeople.Select(x => x.Email).ToList(),
                People = allPeople
            };

            return PartialView("_Email", model);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Email(EmailModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { Success = false, Message = "Invalid input. Please correct the errors and try again." });
            }

            try
            {
                var to = model.To.IsNotNullOrEmpty() && model.To.Any() ? model.To.Select(q => new MailAddress(q)).ToList() : null;
                var cc = model.CC.IsNotNullOrEmpty() && model.CC.Any() ? model.CC.Select(q => new MailAddress(q)).ToList() : null;
                var bcc = model.BCC.IsNotNullOrEmpty() && model.BCC.Any() ? model.BCC.Select(q => new MailAddress(q)).ToList() : null;
                var response = Emailer.SendPlainEmail(model.Subject, model.Message, new MailAddress(SessionVariables.CurrentUser.User.Email), null, multipleTo: to, cc, bcc, "Tag", model.TagId);

                var communicationModel = new CommunicationHistory
                {
                    CommunicationMethod = (int)ContactMethod.Email,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now,
                    Id = Utilities.GenerateUniqueId(),
                    Message = model.Message,
                    Subject = model.Subject,
                    TagId = model.TagId,
                    IsSuccess = response.Success
                };
                var result = work.TagCommunication.Create(communicationModel);

                if (result.ResultType == ResultType.Success)
                {
                    var returnModel = new CommunicationHistoryModel
                    {
                        CommunicationHistory = work.TagCommunication.GetAllByTag(model.TagId),
                        TagId = model.TagId
                    };

                    return PartialView("_CommunicationHistory", returnModel);
                }

                return Json(new { Success = false, Message = $"Something went wrong, Error: {result.Message}" });
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, Message = $"Something went wrong, Error: {ex.Message}" });
            }
        }

        public ActionResult Text(string tagId)
        {
            var allPeople = work.Tags.GetPeopleByTag(tagId).Where(q => q.PhoneNumber.IsNotNullOrEmpty()).ToList();
            var model = new TextModel
            {
                TagId = tagId,
                People = allPeople
            };

            return PartialView("_TextMessage", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Text(TextModel model)
        {
            if (!ModelState.IsValid)
            {
                var allPeople = work.Tags.GetPeopleByTag(model.TagId).Where(q => q.PhoneNumber.IsNotNullOrEmpty()).ToList();
                var newModel = new TextModel
                {
                    TagId = model.TagId,
                    People = allPeople
                };

                return PartialView("_TextMessage", newModel);
                //return Json(new { Success = false, Message = "Invalid input. Please correct the errors and try again." });
            }

            try
            {
                var validNumbers = model.To
                    .Select(number => number.PhoneFriendly())
                    .Where(plainNumber => plainNumber.Length == 10)
                    .ToList();

                foreach (var number in validNumbers)
                {
                    var smsMessage = new SmsMessage
                    {
                        Id = Utilities.GenerateUniqueId(),
                        To = number,
                        Message = model.Message,
                        CreatedDate = DateTime.Now,
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        Type = "Tag",
                        TypeId = model.TagId
                    };
                    Utilities.SendMessage(smsMessage);
                }

                var communicationModel = new CommunicationHistory
                {
                    CommunicationMethod = (int)ContactMethod.Text,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now,
                    Id = Utilities.GenerateUniqueId(),
                    Message = model.Message,
                    TagId = model.TagId,
                    IsSuccess = true
                };
                var result = work.TagCommunication.Create(communicationModel);

                if (result.ResultType == ResultType.Success)
                {
                    var returnModel = new CommunicationHistoryModel
                    {
                        CommunicationHistory = work.TagCommunication.GetAllByTag(model.TagId),
                        TagId = model.TagId
                    };

                    return PartialView("_CommunicationHistory", returnModel);
                }

                return Json(new { Success = false, Message = $"Something went wrong, Error: {result.Message}" });
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, Message = $"Something went wrong, Error: {ex.Message}" });
            }
        }
        #endregion

        #region AssignToTag
        public ActionResult AssignToTag(string tagId)
        {
            var disable = new List<string> { tagId };
            var folders = work.Tags.GetAllFolders(SessionVariables.CurrentChurch.Id);
            var tags = work.Tags.GetAll(SessionVariables.CurrentChurch.Id).Where(q => !q.Id.Equals(tagId)).ToList();
            var json = work.Tags.BindFoldersHierarchy(folders, tags, null, null, disable);
            var model = new AssignToTagModel
            {
                FolderJson = json,
                People = work.Tags.GetPeopleByTag(tagId),
                Tag = work.Tags.Get(tagId)
            };

            return PartialView("_AssignToTags", model);
        }

        [HttpPost]
        public ActionResult AssignToTag(List<string> tags, List<string> people)
        {
            var result = work.Tags.AddPeopleToTag(people, tags);

            return result.ResultType == ResultType.Success ? Json(new { Success = true }) : Json(new { Success = false, result.Message });
        }
        #endregion
    }
}