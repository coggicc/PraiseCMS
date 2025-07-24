using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using PraiseCMS.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    public class AttachmentsController : BaseController
    {
        public ActionResult Index()
        {
            var result = work.Attachment.GetAll();
            return View(result);
        }

        [HttpGet]
        public ActionResult _Create(string type, string typeId, string returnUrl)
        {
            SingleAttachmentViewModel model = new SingleAttachmentViewModel
            {
                Attachment = new AttachmentSD { Id = Utilities.GenerateUniqueId(), CreatedDate = DateTime.Now, CreatedBy = SessionVariables.CurrentUser.User.Id, Type = type, TypeId = typeId, SortOrder = 0, ChurchId = SessionVariables.CurrentChurch.Id }
            };
            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        public ActionResult _Create(SingleAttachmentViewModel model, HttpPostedFileBase File, HttpPostedFileBase File1, HttpPostedFileBase File2, HttpPostedFileBase File3, HttpPostedFileBase File4, HttpPostedFileBase File5, HttpPostedFileBase File6, HttpPostedFileBase File7)
        {
            if (ModelState.IsValid)
            {
                if (File?.ContentLength > 0)
                {
                    var newFileName = Utilities.GenerateUniqueFileName(File.FileName);
                    // var success = Utilities.IsImage(File.FileName) ? AwsHelpers.UploadImage(newFileName, File) : AwsHelpers.UploadFile(newFileName, File);
                    var success = Utilities.IsImage(File.FileName) ? Uploader.UploadImage(newFileName, File) : Uploader.UploadFile(newFileName, File);

                    if (success)
                    {
                        model.Attachment.FileName = newFileName;
                        model.Attachment.Name = !string.IsNullOrEmpty(model.Attachment.Name) ? model.Attachment.Name : File.FileName;

                        work.Attachment.Create(model.Attachment);
                    }
                    #region Additional Files
                    var additionalFiles = new List<string>();

                    if (File1?.ContentLength > 0)
                    {
                        newFileName = Utilities.GenerateUniqueFileName(File1.FileName);
                        //success = Utilities.IsImage(File1.FileName) ? AwsHelpers.UploadImage(newFileName, File1) : AwsHelpers.UploadFile(newFileName, File1);
                        success = Utilities.IsImage(File1.FileName) ? Uploader.UploadImage(newFileName, File1) : Uploader.UploadFile(newFileName, File1);

                        if (success)
                        {
                            additionalFiles.Add(newFileName);
                        }
                    }

                    if (File2?.ContentLength > 0)
                    {
                        newFileName = Utilities.GenerateUniqueFileName(File2.FileName);
                        // success = Utilities.IsImage(File2.FileName) ? AwsHelpers.UploadImage(newFileName, File2) : AwsHelpers.UploadFile(newFileName, File2);
                        success = Utilities.IsImage(File2.FileName) ? Uploader.UploadImage(newFileName, File2) : Uploader.UploadFile(newFileName, File2);

                        if (success)
                        {
                            additionalFiles.Add(newFileName);
                        }
                    }

                    if (File3?.ContentLength > 0)
                    {
                        newFileName = Utilities.GenerateUniqueFileName(File3.FileName);
                        // success = Utilities.IsImage(File3.FileName) ? AwsHelpers.UploadImage(newFileName, File3) : AwsHelpers.UploadFile(newFileName, File3);
                        success = Utilities.IsImage(File3.FileName) ? Uploader.UploadImage(newFileName, File3) : Uploader.UploadFile(newFileName, File3);

                        if (success)
                        {
                            additionalFiles.Add(newFileName);
                        }
                    }

                    if (File4?.ContentLength > 0)
                    {
                        newFileName = Utilities.GenerateUniqueFileName(File4.FileName);
                        // success = Utilities.IsImage(File4.FileName) ? AwsHelpers.UploadImage(newFileName, File4) : AwsHelpers.UploadFile(newFileName, File4);
                        success = Utilities.IsImage(File4.FileName) ? Uploader.UploadImage(newFileName, File4) : Uploader.UploadFile(newFileName, File4);

                        if (success)
                        {
                            additionalFiles.Add(newFileName);
                        }
                    }

                    if (File5?.ContentLength > 0)
                    {
                        newFileName = Utilities.GenerateUniqueFileName(File5.FileName);
                        // success = Utilities.IsImage(File5.FileName) ? AwsHelpers.UploadImage(newFileName, File5) : AwsHelpers.UploadFile(newFileName, File5);
                        success = Utilities.IsImage(File5.FileName) ? Uploader.UploadImage(newFileName, File5) : Uploader.UploadFile(newFileName, File5);

                        if (success)
                        {
                            additionalFiles.Add(newFileName);
                        }
                    }

                    if (File6?.ContentLength > 0)
                    {
                        newFileName = Utilities.GenerateUniqueFileName(File6.FileName);
                        // success = Utilities.IsImage(File6.FileName) ? AwsHelpers.UploadImage(newFileName, File6) : AwsHelpers.UploadFile(newFileName, File6);
                        success = Utilities.IsImage(File6.FileName) ? Uploader.UploadImage(newFileName, File6) : Uploader.UploadFile(newFileName, File6);

                        if (success)
                        {
                            additionalFiles.Add(newFileName);
                        }
                    }

                    if (File7?.ContentLength > 0)
                    {
                        newFileName = Utilities.GenerateUniqueFileName(File7.FileName);
                        // success = Utilities.IsImage(File7.FileName) ? AwsHelpers.UploadImage(newFileName, File7) : AwsHelpers.UploadFile(newFileName, File7);
                        success = Utilities.IsImage(File7.FileName) ? Uploader.UploadImage(newFileName, File7) : Uploader.UploadFile(newFileName, File7);

                        if (success)
                        {
                            additionalFiles.Add(newFileName);
                        }
                    }

                    foreach (var additionalFile in additionalFiles)
                    {
                        var data = new AttachmentSD
                        {
                            Category = model.Attachment.Category,
                            ChurchId = model.Attachment.ChurchId,
                            CreatedBy = model.Attachment.CreatedBy,
                            CreatedDate = DateTime.Now,
                            FileName = additionalFile,
                            Id = Utilities.GenerateUniqueId(),
                            Name = !string.IsNullOrEmpty(model.Attachment.Name) ? model.Attachment.Name : additionalFile,
                            Notes = model.Attachment.Notes,
                            Type = model.Attachment.Type,
                            TypeId = model.Attachment.TypeId,
                            SortOrder = model.Attachment.SortOrder + 1
                        };

                        work.Attachment.Create(data);
                    }
                    #endregion
                }
                else
                {
                    CreateAlertMessage("You must specify a file for the attachment.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                }

                // return Redirect(model.ReturnUrl);
                return Redirect("/Attachments#Attachments");
            }

            return PartialView("_CreateEdit", model);
        }

        [HttpGet]
        public ActionResult _Edit(string id, string returnUrl)
        {
            SingleAttachmentViewModel model = new SingleAttachmentViewModel
            {
                Attachment = work.Attachment.Get(id)
            };
            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        public ActionResult _Edit(SingleAttachmentViewModel model, HttpPostedFileBase File)
        {
            if (ModelState.IsValid)
            {
                string newFileName = null;

                if (File?.ContentLength > 0)
                {
                    newFileName = Utilities.GenerateUniqueFileName(File.FileName);
                    // var success = Utilities.IsImage(File.FileName) ? AwsHelpers.UploadImage(newFileName, File) : AwsHelpers.UploadFile(newFileName, File);
                    var success = Utilities.IsImage(File.FileName) ? Uploader.UploadImage(newFileName, File) : Uploader.UploadFile(newFileName, File);

                    if (success)
                    {
                        if (!string.IsNullOrEmpty(model.Attachment.FileName))
                        {
                            if (Utilities.IsImage(File.FileName))
                            {
                                AwsHelpers.DeleteImage(model.Attachment.FileName, string.Empty);
                            }
                            else
                            {
                                AwsHelpers.DeleteFile(model.Attachment.FileName);
                            }
                        }

                        model.Attachment.FileName = newFileName;
                        model.Attachment.Name = !string.IsNullOrEmpty(model.Attachment.Name) ? model.Attachment.Name : model.Attachment.FileName;
                    }
                }

                work.Attachment.Update(model, newFileName);
                return Redirect("/Attachments#Attachments");
            }

            return PartialView("_CreateEdit", model);
        }

        [HttpGet]
        public ActionResult _Widget(string type, string typeId, string returnUrl)
        {
            var model = work.Attachment.GetAttachmentsView(type, typeId, returnUrl);
            return PartialView(model);
        }

        [HttpGet]
        public ActionResult _PhotoWidget(string type, string typeId, string returnUrl)
        {
            var model = work.Attachment.GetAttachmentsView(type, typeId, returnUrl);
            return PartialView(model);
        }

        [HttpGet]
        public ActionResult Delete(string id, string returnUrl)
        {
            var attachment = work.Attachment.Get(id);
            var success = AwsHelpers.DeleteFile(attachment.FileName);

            if (success)
            {
                work.Attachment.Delete(attachment);
            }

            return Redirect(returnUrl);
        }

        [HttpGet]
        public ActionResult _Delete(string id)
        {
            var attachment = work.Attachment.Get(id);
            var success = AwsHelpers.DeleteFile(attachment.FileName);

            if (success)
            {
                work.Attachment.Delete(attachment);
                return JavaScript("$('.attachment-" + id + "').hide();");
            }
            else
            {
                return JavaScript("alert('there was an issue deleting this attachment');");
            }
        }

        [HttpGet]
        public ActionResult _MoveAttachmentUp(string id, string widget, string returnUrl)
        {
            var attachment = work.Attachment.Get(id);
            work.Attachment.MoveUp(attachment);
            return JavaScript("$('#" + widget + "-" + attachment.TypeId + "').load('/attachments/" + widget + "?type=" + Url.Encode(attachment.Type) + "&typeId=" + attachment.TypeId + "&returnUrl=" + Url.Encode(returnUrl) + "');");
        }

        [HttpGet]
        public ActionResult _MoveAttachmentDown(string id, string widget, string returnUrl)
        {
            var attachment = work.Attachment.Get(id);
            work.Attachment.MoveAttachmentDown(attachment);
            return JavaScript("$('#" + widget + "-" + attachment.TypeId + "').load('/attachments/" + widget + "?type=" + Url.Encode(attachment.Type) + "&typeId=" + attachment.TypeId + "&returnUrl=" + Url.Encode(returnUrl) + "');");
        }

        [HttpPost]
        public ActionResult _UploadFiles(HttpPostedFileBase file, string type, string typeId, string returnUrl)
        {
            if (file?.ContentLength > 0)
            {
                var newFileName = Utilities.GenerateUniqueFileName(file.FileName);
                var success = Utilities.IsImage(file.FileName) ? AwsHelpers.UploadImage(newFileName, file, "Uploads/Images") : AwsHelpers.UploadFile(newFileName, file);

                if (success)
                {
                    var lastAttachment = work.Attachment.GetLastAttachmentFor(type, typeId);

                    var attachment = new AttachmentSD
                    {
                        Id = Utilities.GenerateUniqueId(),
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now,
                        Name = file.FileName,
                        FileName = newFileName,
                        Type = type,
                        TypeId = typeId,
                        SortOrder = lastAttachment != null ? lastAttachment.SortOrder + 1 : 1
                    };

                    work.Attachment.Create(attachment);

                    CreateAlertMessage("Successfully added file(s).", AlertMessageTypes.Primary, AlertMessageIcons.Primary);
                }
                else
                {
                    CreateAlertMessage("There was a problem uploading your file(s).", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                }

                return AjaxReloadPage;
            }

            return AjaxReloadPage;
        }

        [HttpPost]
        public ActionResult _UploadPhotos(HttpPostedFileBase file, string type, string typeId, string returnUrl)
        {
            if (file?.ContentLength > 0)
            {
                if (Utilities.IsImage(file.FileName))
                {
                    var newFileName = Utilities.GenerateUniqueFileName(file.FileName);
                    var success = AwsHelpers.UploadImage(newFileName, file, "Uploads/Photos");

                    if (success)
                    {
                        var lastAttachment = work.Attachment.GetLastAttachmentFor(type, typeId);
                        var attachment = new AttachmentSD
                        {
                            Id = Utilities.GenerateUniqueId(),
                            CreatedBy = SessionVariables.CurrentUser.User.Id,
                            CreatedDate = DateTime.Now,
                            Name = newFileName,
                            FileName = newFileName,
                            Type = type,
                            TypeId = typeId,
                            SortOrder = lastAttachment != null ? lastAttachment.SortOrder + 1 : 1
                        };

                        work.Attachment.Create(attachment);

                        CreateAlertMessage("Successfully added file(s).", AlertMessageTypes.Primary, AlertMessageIcons.Primary);
                    }
                    else
                    {
                        CreateAlertMessage("There was a problem uploading your file(s).", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                    }

                    return AjaxReloadPage;
                }
                else
                {
                    CreateAlertMessage("Only images are allowed. Please remove non-image files and try again.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                }
            }

            return AjaxReloadPage;
        }
    }
}