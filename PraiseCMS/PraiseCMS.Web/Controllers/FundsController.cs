using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.DataAccess.Singletons;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using QRCoder;
using Rotativa;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]

    [RequirePermission(ModuleId = "0865576942209a2d6deb0147c0bcad")]
    public class FundsController : BaseController
    {
        public ActionResult Index()
        {
            var funds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id);

            if (!funds.Any())
            {
                work.Fund.CreateDefaultFunds(SessionVariables.CurrentChurch.Id);
                funds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id);
            }

            if (!SessionVariables.CurrentChurch.HasMerchantAccount)
            {
                var churchId = SessionVariables.CurrentChurch.Id;
                var alertMessage = $"Digital giving has not been enabled for your church. It only takes a few minutes to get started. <a href=\"/onboarding/CreateMerchantAccount/{churchId}\">Enable giving today!</a>";

                CreateAlertMessage(alertMessage, AlertMessageTypes.Warning, AlertMessageIcons.Warning);
            }

            return View(funds);
        }

        [HttpGet]
        public ActionResult _CreateFund()
        {
            var fund = new Fund()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now,
                Closed = false,
                Hidden = false,
                IsTaxDeductible = true,
                IsDigitalAllowed = true
            };

            var model = new OnboardFundViewModel()
            {
                Fund = fund,
                CommonFunds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id).Any() ? new List<string>() : GivingFunds.Items,
                EnableCloseOrHidden = work.Fund.EnableCloseOrHidden(SessionVariables.CurrentChurch.Id)
            };

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        public ActionResult _CreateFund(OnboardFundViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Check if the church has the default funds
                if (!work.Fund.GetAll(SessionVariables.CurrentChurch.Id).Any())
                {
                    work.Fund.CreateDefaultFunds(SessionVariables.CurrentChurch.Id);
                }

                if (!string.IsNullOrEmpty(model.Fund.Name))
                {
                    model.Fund.DesignationId = model.Fund.Id.SubstringIt(20);

                    if (model.GenerateQRCode && SessionVariables.CurrentChurch.HasMerchantAccount)
                    {
                        model.Fund.QRCodeLink = $"/GivingWorkflow/StartGiving?Id={model.Fund.ChurchId}&selectedFundId={model.Fund.Id}";
                    }

                    work.Fund.Create(model.Fund);
                }

                return RedirectToAction(nameof(Index));
            }

            return PartialView("_CreateEdit", model);
        }

        [HttpGet]
        public ActionResult _EditFund(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var fund = work.Fund.Get(id);

            if (fund == null)
            {
                return HttpNotFound();
            }

            var model = new OnboardFundViewModel()
            {
                Fund = fund,
                EnableCloseOrHidden = fund.Closed || fund.Hidden || work.Fund.EnableCloseOrHidden(SessionVariables.CurrentChurch.Id, fund.Id)
            };

            return PartialView("_CreateEdit", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditFund(OnboardFundViewModel model)
        {
            if (!ModelState.IsValid) return PartialView("_CreateEdit", model);

            model.Fund.ModifiedDate = DateTime.Now;
            model.Fund.ModifiedBy = SessionVariables.CurrentUser.User.Id;
            model.Fund.QRCodeLink = model.GenerateQRCode ? $"/GivingWorkflow/StartGiving?Id={model.Fund.ChurchId}&selectedFundId={model.Fund.Id}" : null;

            work.Fund.Update(model.Fund);

            CreateAlertMessage($"Your changes for the {model.Fund.Display} fund have been saved.", AlertMessageTypes.Success, AlertMessageIcons.Success);

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var fund = work.Fund.Get(id);

            if (fund == null)
            {
                return HttpNotFound();
            }

            // Check if giving records are associated with the fund
            var associatedGiving = work.Giving.GetAllGivingByFund(SessionVariables.CurrentChurch.Id, fund.Id);

            if (associatedGiving.DigitalGiving.Any() || associatedGiving.OfflineGiving.Any())
            {
                var alertMessage = "This fund cannot be deleted because it has associated giving records for: ";

                if (associatedGiving.DigitalGiving.Any())
                {
                    alertMessage += "Digital Giving";
                }

                if (associatedGiving.OfflineGiving.Any())
                {
                    if (associatedGiving.DigitalGiving.Any())
                    {
                        alertMessage += " and ";
                    }

                    alertMessage += "Offline Giving";
                }

                CreateAlertMessage(alertMessage, AlertMessageTypes.Warning, AlertMessageIcons.Warning);
                return RedirectToAction(nameof(Index));
            }

            fund.IsDeleted = true;
            fund.ModifiedBy = SessionVariables.CurrentUser.User.Id;
            fund.ModifiedDate = DateTime.Now;

            work.Fund.Update(fund);
            CreateAlertMessage("Your fund has been deleted.", AlertMessageTypes.Success, AlertMessageIcons.Success);

            var scheduledPayments = work.ScheduledPayment.GetAllByFund(fund.Id);

            if (scheduledPayments.Any())
            {
                var defaultFund = work.Fund.GetByName(SessionVariables.CurrentChurch.Id, GivingFunds.General);
                scheduledPayments.ForEach(d => d.FundId = defaultFund.Id);
                work.ScheduledPayment.Update<ScheduledPayment>(scheduledPayments);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public ActionResult Details(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var fund = work.Fund.Get(id);

            if (fund == null)
            {
                return HttpNotFound();
            }

            return View("Details", fund);
        }

        [HttpGet]
        public ActionResult EnableDigitalGiving(string id)
        {
            var fund = work.Fund.Get(id);

            if (fund == null)
            {
                CreateAlertMessage($"No fund could be found matching id: {id}.", AlertMessageTypes.Warning, AlertMessageIcons.Warning);
            }
            else if (SessionVariables.CurrentChurch.HasMerchantAccount)
            {
                if (fund.IsDigitalAllowed)
                {
                    fund.IsDigitalAllowed = false;
                    CreateAlertMessage($"Digital giving for the {fund.Display} fund has been disabled.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                }
                else
                {
                    fund.IsDigitalAllowed = true;
                    CreateAlertMessage($"Digital giving is now available for the {fund.Display} fund.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                }

                work.Fund.Update(fund);
            }
            else
            {
                CreateAlertMessage("Digital giving has not been enabled for your church. Giving must be enabled before adding this fund.", AlertMessageTypes.Warning, AlertMessageIcons.Warning);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public ActionResult GenerateQRCode(string param)
        {
            if (string.IsNullOrEmpty(param))
                return null;

            var qrCodeImage = GenerateQRCodeImage(param);
            ViewBag.param = param;

            return PartialView("_QRCode", BitmapToBytesCode(qrCodeImage));
        }

        [HttpPost]
        public ActionResult GenerateQRCode(string param, string name, string type = "Image")
        {
            if (string.IsNullOrEmpty(param))
                return null;

            var qrCodeImage = GenerateQRCodeImage(param);

            if (type.EqualsIgnoreCase("Image"))
            {
                ViewBag.param = param;
                ViewBag.printable = true;

                return File(BitmapToBytesCode(qrCodeImage), "image/png", $"{name.Replace(" ", "_")}.png");
            }
            else if (type.EqualsIgnoreCase("PDF"))
            {
                return new PartialViewAsPdf("_QRCode", BitmapToBytesCode(qrCodeImage))
                {
                    FileName = $"{name.Replace(" ", "_")}.pdf"
                };
            }
            else
            {
                return Content("Invalid type parameter specified.");
            }
        }

        private Bitmap GenerateQRCodeImage(string param)
        {
            var decryptedParam = WebUtility.UrlDecode(param).Decrypt();
            var fullUrl = $"{ApplicationCache.Instance.SiteConfiguration.Url}{decryptedParam}";

            var qrCodeGenerator = new QRCodeGenerator();
            var qrCodeData = qrCodeGenerator.CreateQrCode(fullUrl, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrCodeData);

            return qrCode.GetGraphic(20);
        }

        [NonAction]
        private static byte[] BitmapToBytesCode(Bitmap image)
        {
            using var stream = new MemoryStream();
            image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);

            return stream.ToArray();
        }
    }
}