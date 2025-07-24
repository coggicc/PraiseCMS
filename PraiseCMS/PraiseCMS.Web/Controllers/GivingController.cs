using PraiseCMS.API.Helpers;
using PraiseCMS.API.Models;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using PraiseCMS.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Mvc;
using static PraiseCMS.Shared.Methods.ExtensionMethods;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]

    [RequirePermission(ModuleId = "882922349528680237c3c34f04b1c3")]
    public class GivingController : BaseController
    {
        public ActionResult Index()
        {
            //Only funds are needed to toggle Fund Report visibility
            var model = work.Fund.GetAll(SessionVariables.CurrentChurch.Id);
            return View(model);
        }

        public ActionResult MonthlyPaymentSummary(string startDate, string endDate, string fundId, string campusId)
        {
            var dashboard = work.Giving.GetChurchPaymentsSummary(SessionVariables.CurrentChurch.Id, startDate, endDate, fundId, campusId);
            return View(dashboard);
        }

        public ActionResult DepositSummary(string startDate, string endDate)
        {
            if (startDate.IsNullOrEmpty() || endDate.IsNullOrEmpty())
            {
                startDate = Convert.ToDateTime($"{DateTime.Now.Month}/1/{DateTime.Now.Year}").ToShortDateString();
                endDate = DateTime.Now.ToShortDateString();
            }

            //var returnData = stewardshipHelper.GetDepositSummary(startDate, endDate);

            //if (returnData.Status != null && returnData.Status.ErrorCode != "0")
            //{
            //    CreateAlertMessage(returnData.Status.Description, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            //}

            //if (returnData.Deposits == null)
            //{
            //    return View(new DepositSummaryDashboard { StartDate = startDate, EndDate = endDate });
            //}

            var depositSummaryDashboard = new DepositSummaryDashboard
            {
                StartDate = startDate,
                EndDate = endDate
                //Deposits = returnData.Deposits.Deposits.OrderByDescending(x => Convert.ToDateTime(x.DepositDate)).ToList()
            };

            return View(depositSummaryDashboard);
        }

        public ActionResult DepositDetails(string startDate, string endDate)
        {
            if (startDate.IsNullOrEmpty() || endDate.IsNullOrEmpty())
            {
                startDate = Convert.ToDateTime($"{DateTime.Now.Month}/1/{DateTime.Now.Year}").ToShortDateString();
                endDate = DateTime.Now.ToShortDateString();
            }

            //var returnData = stewardshipHelper.GetDepositDetails(startDate, endDate);

            //if (returnData.Status != null && returnData.Status.ErrorCode != "0")
            //{
            //    CreateAlertMessage(returnData.Status.Description, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            //}

            //if (returnData.DepositDetails == null)
            //{
            //    return View(new DepositDetailsDashboard { StartDate = startDate, EndDate = endDate });
            //}

            var depositDetailsDashboard = new DepositDetailsDashboard
            {
                StartDate = startDate,
                EndDate = endDate
                //DepositDetails = returnData.DepositDetails.DepositDetails.OrderByDescending(x => Convert.ToDateTime(x.DepositDate)).ToList()
            };

            return View(depositDetailsDashboard);
        }

        public ActionResult DepositDesignationDetails(string startDate, string endDate)
        {
            if (startDate.IsNullOrEmpty() || endDate.IsNullOrEmpty())
            {
                startDate = Convert.ToDateTime($"{DateTime.Now.Month}/1/{DateTime.Now.Year}").ToShortDateString();
                endDate = DateTime.Now.ToShortDateString();
            }

            //var returnData = stewardshipHelper.GetDepositDetailDesignationDetailWithContactInfo(startDate, endDate);

            //if (returnData.Status != null && returnData.Status.ErrorCode != "0")
            //{
            //    CreateAlertMessage(returnData.Status.Description, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            //}

            //if (returnData.DepositDetails != null)
            //{
            var depositDetailsDashboard = new DepositDetailsDashboard
            {
                StartDate = startDate,
                EndDate = endDate
                //DepositDetails = returnData.DepositDetails.DepositDetails.OrderByDescending(x => Convert.ToDateTime(x.DepositDate)).ToList()
            };

            //    return View(depositDetailsDashboard);
            //}

            return View(new DepositDetailsDashboard { StartDate = startDate, EndDate = endDate });
        }

        public async Task<ActionResult> RefundTransaction(string transactionId)
        {
            var refundModel = new RefundModel();

            if (!string.IsNullOrEmpty(transactionId))
            {
                refundModel.TransactionId = transactionId;

                //Get the refund reason types
                //var refundReasons = stewardshipHelper.RefundReasonTypes();

                string errorMessage;

                //"There was a problem retrieving the refund reason types from Stewardship."
                //if (refundReasons.Status != null && refundReasons.Status.ErrorCode != "0")
                //{
                //    errorMessage = "There was a problem processing your request. Please try again or contact us for further assistance.";
                //    var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", errorMessage,
                //    "Exception Code", errorMessage, "TransactionId", transactionId);
                //    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, string.Empty, string.Empty, LogStatuses.Error, logObj);

                //    return View(refundModel);
                //}

                //if (refundReasons.RefundReasonTypes != null)
                //{
                //    refundModel.RefundReasonTypes = refundReasons.RefundReasonTypes;
                //}

                //Get the payment so we can get the UserId
                var payment = work.Payment.GetByTransactionId(transactionId);

                if (payment == null || string.IsNullOrEmpty(payment.UserId))
                {
                    errorMessage = "There was a problem retrieving the payment details. Please try again or contact us for further assistance.";
                    CreateAlertMessage(errorMessage, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                    var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", errorMessage,
                    "Exception Code", errorMessage, "TransactionId", transactionId);
                    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Transaction", transactionId, LogStatuses.Error, logObj);

                    return View(refundModel);
                }

                refundModel.Payment = payment;

                //Get the payment method and donor GUID to be used in view and Stewardship call
                var paymentMethodAccount = work.PaymentMethodAccount.GetByAccountGUID(payment.PaymentMethod);

                if (paymentMethodAccount == null || string.IsNullOrEmpty(paymentMethodAccount.PaymentMethodPreview) || string.IsNullOrEmpty(paymentMethodAccount.DonorGUID))
                {
                    CreateAlertMessage("There was a problem retrieving the payment method details. Please contact us for further assistance.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                    return View(refundModel);
                }

                refundModel.PaymentMethod = paymentMethodAccount.PaymentMethodPreview;

                //Get the transaction from Stewardship

                ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(SessionVariables.CurrentMerchant.ApiUsername, SessionVariables.CurrentMerchant.ApiPassword);

                var transactionResponse = new TransactionResponse();

                //var returnData = stewardshipHelper.VerifyTransaction(paymentMethodAccount.DonorGUID, payment.AccountScheduleGUID, SessionVariables.CurrentChurch.Id);

                transactionResponse = await nuveiHelper.VerifyTransaction(refundModel.TransactionId, apiCredentials);

                var paymentStatus = Responses.GetApiTransactionResponse(transactionResponse?.result);

                if (paymentStatus != APIStatuses.Success)
                {
                    string apiErrorMessage = Responses.HandleApiTransactionFailure(transactionResponse);
                    CreateAlertMessage($"There was a problem verifying your previous donation. Please contact us at support@praisecms.com. Thank you. (Error: {apiErrorMessage})", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                    return View(transactionId);
                }

                var paymentReferenceNumber = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : null;

                //if (returnData.Status != null && returnData.Status.ErrorCode != "0")
                //{
                //    var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", returnData.Status.Description,
                //    "Exception Code", returnData.Status.ErrorCode, "TransactionId", transactionId);
                //    logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Transaction", transactionId, LogStatuses.Error, logObj);
                //}

                //if (returnData.Transactions?.Transaction != null)
                //{
                //    refundModel.Transaction = returnData.Transaction;
                //}

                //var fund = work.Fund.Get(payment.FundId);

                //if (fund != null)
                //{
                //    refundModel.Fund = fund.Display;
                //}

                return View(refundModel);
            }

            CreateAlertMessage("Please select a gift to be refunded.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            return View(refundModel);
        }

        [HttpPost]
        public ActionResult RefundTransaction(RefundModel refundModel)
        {
            //if (string.IsNullOrEmpty(refundModel.Transaction.TransactionGUID))
            //{
            //    return RedirectToAction("Index");
            //}

            //var returnData = stewardshipHelper.RefundTransaction(refundModel.Transaction.TransactionGUID, refundModel.RefundReasonId);

            //if (returnData.Status == null || returnData.Status.ErrorCode == "0")
            //{
            //    return RedirectToAction("Index");
            //}

            //var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", returnData.Status.Description,
            //    "Exception Code", returnData.Status.ErrorCode, "TransactionGUID", refundModel.Transaction.TransactionGUID);
            //logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "TransactionGUID", refundModel.Transaction.TransactionGUID, LogStatuses.Error, logObj);

            //Change to wherever we want the user to end up
            return RedirectToAction("Index");
        }

        //TODO: Stubbed only
        public ActionResult RefundPartialTransaction()
        {
            return View();
        }

        #region Offline Giving
        [HttpGet]
        [RequirePermission(ModuleId = "1521435774e4281cb803c9422d98cf")]
        public ActionResult OfflineGiving()
        {
            var vm = new OfflineGivingListView
            {
                OfflineGiving = work.OfflineGiving.GetAll(SessionVariables.CurrentChurch.Id),
                Funds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id),
                Donors = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id)
            };

            return View(vm);
        }

        [HttpGet]
        [RequirePermission(ModuleId = "1521435774e4281cb803c9422d98cf")]
        public ActionResult CreateOfflineGiving(string type)
        {
            var vm = new OfflineGivingView
            {
                OfflineGiving = new OfflineGiving()
                {
                    Id = Utilities.GenerateUniqueId(),
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now
                },
                Funds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id),
                People = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id),
                Mode = PeopleSelectionMode.System,
                OfflineGivingType = !string.IsNullOrEmpty(type) ? type : string.Empty
            };

            return PartialView("_CreateEditOfflineGiving", vm);
        }

        [HttpPost]
        [RequirePermission(ModuleId = "1521435774e4281cb803c9422d98cf")]
        public ActionResult CreateOfflineGivingUpdated(OfflineGivingView model)
        {
            // Clear existing ModelState to avoid issues with stale validation errors
            ModelState.Clear();

            // Validate Donor
            if (string.IsNullOrWhiteSpace(model.OfflineGiving.PersonId))
            {
                ModelState.AddModelError("OfflineGiving.PersonId", "Please select a donor.");
            }

            // Validate PaymentMethod
            if (string.IsNullOrWhiteSpace(model.OfflineGiving.OfflinePaymentMethod))
            {
                ModelState.AddModelError("OfflineGiving.OfflinePaymentMethod", "Please select a payment method.");
            }

            // Validate CampusId
            if (string.IsNullOrWhiteSpace(model.OfflineGiving.CampusId))
            {
                ModelState.AddModelError("OfflineGiving.CampusId", "Please select a campus.");
            }

            // Validate DateReceived
            if (model.OfflineGiving.DateReceived == null || model.OfflineGiving.DateReceived == default(DateTime))
            {
                ModelState.AddModelError("OfflineGiving.DateReceived", "Please enter a valid date received.");
            }

            // Validate Payments list
            if (model.Payments?.Any() != true)
            {
                ModelState.AddModelError(string.Empty, "You must add at least one donation.");
            }
            else
            {
                for (int i = 0; i < model.Payments.Count; i++)
                {
                    var payment = model.Payments[i];

                    if (string.IsNullOrWhiteSpace(payment.Amount) || !decimal.TryParse(payment.Amount, out _))
                    {
                        ModelState.AddModelError($"Payments[{i}].Amount", "Please enter a valid amount.");
                    }

                    if (string.IsNullOrWhiteSpace(payment.FundId))
                    {
                        ModelState.AddModelError($"Payments[{i}].FundId", "Please select a fund.");
                    }
                }
            }

            if (!ModelState.IsValid)
            {
                // If there are validation errors, return the partial view with validation messages
                model.Funds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id);
                model.People = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id);
                return PartialView("_CreateEditOfflineGiving", model);
            }

            // Handle payments only if they exist and are valid
            if (model.Payments.Any())
            {
                // Filter out invalid payments
                var validPayments = model.Payments
                    .Where(q => !string.IsNullOrWhiteSpace(q.Amount) && decimal.TryParse(q.Amount, out _) && !string.IsNullOrWhiteSpace(q.FundId))
                    .ToList();

                if (validPayments.Any())
                {
                    // Handle Donor scenario with Manual mode
                    if (model.OfflineGivingType != OfflineGiftAmountTypes.LumpSum && model.Mode.Equals(PeopleSelectionMode.Manual))
                    {
                        var person = work.Person.GetByEmailAndPhoneAndName(model.Person.Email, model.Person.PhoneNumber, model.Person.FirstName, model.Person.LastName);
                        if (person.IsNullOrEmpty())
                        {
                            person = new Person
                            {
                                Id = Utilities.GenerateUniqueId(),
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
                        model.OfflineGiving.PersonId = person.Id;
                    }

                    // Create OfflineGiving records for the valid payments
                    var offlineGivingList = validPayments.Select(q => new OfflineGiving
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Amount = decimal.Parse(q.Amount),
                        CheckNumber = model.OfflineGiving.CheckNumber,
                        CampusId = model.OfflineGiving.CampusId,
                        PersonId = model.OfflineGiving.PersonId, // Set PersonId even for LumpSum, if applicable
                        FundId = q.FundId,
                        ChurchId = model.OfflineGiving.ChurchId,
                        CreatedBy = model.OfflineGiving.CreatedBy,
                        CreatedDate = model.OfflineGiving.CreatedDate,
                        DateReceived = model.OfflineGiving.DateReceived,
                        OfflinePaymentType = model.OfflineGiving.OfflinePaymentType,
                        OfflinePaymentMethod = model.OfflineGiving.CheckNumber.IsNotNullOrEmpty() ? OfflinePaymentMethods.Check : OfflinePaymentMethods.Cash
                    }).ToList();

                    var result = work.OfflineGiving.Create(offlineGivingList);

                    if (result.ResultType == ResultType.Success)
                    {
                        CreateAlertMessage("Offline giving has been added.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                    }

                    return Json(new { Success = result.ResultType == ResultType.Success, result.Message });
                }
            }

            // If no valid payments are present, return the partial view
            model.Funds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id);
            model.People = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id);

            return PartialView("_CreateEditOfflineGiving", model);
        }

        public ActionResult CreateOfflineGiving(OfflineGivingView model)
        {
            List<OfflineGiving> offlineGivingList = new List<OfflineGiving>();

            // Handle payments only if they exist
            if (model.Payments.IsNotNullOrEmpty() && model.Payments.Any())
            {
                // Handle Donor scenario with Manual mode
                if (model.OfflineGivingType != OfflineGiftAmountTypes.LumpSum && model.Mode.Equals(PeopleSelectionMode.Manual))
                {
                    var person = work.Person.GetByEmailAndPhoneAndName(model.Person.Email, model.Person.PhoneNumber, model.Person.FirstName, model.Person.LastName);
                    if (person.IsNullOrEmpty())
                    {
                        person = new Person
                        {
                            Id = Utilities.GenerateUniqueId(),
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
                    model.OfflineGiving.PersonId = person.Id;
                }

                // Create OfflineGiving records for the payments
                offlineGivingList = model.Payments.Select(q => new OfflineGiving
                {
                    Id = Utilities.GenerateUniqueId(),
                    Amount = decimal.Parse(q.Amount),
                    CheckNumber = model.OfflineGiving.CheckNumber,
                    CampusId = model.OfflineGiving.CampusId,
                    PersonId = model.OfflineGiving.PersonId, // Set PersonId even for LumpSum, if applicable
                    FundId = q.FundId,
                    ChurchId = model.OfflineGiving.ChurchId,
                    CreatedBy = model.OfflineGiving.CreatedBy,
                    CreatedDate = model.OfflineGiving.CreatedDate,
                    DateReceived = model.OfflineGiving.DateReceived,
                    OfflinePaymentType = model.OfflineGiving.OfflinePaymentType,
                    OfflinePaymentMethod = model.OfflineGiving.CheckNumber.IsNotNullOrEmpty() ? OfflinePaymentMethods.Check : OfflinePaymentMethods.Cash
                }).ToList();

                var result = work.OfflineGiving.Create(offlineGivingList);

                if (result.ResultType == ResultType.Success)
                {
                    CreateAlertMessage("Offline giving has been added.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                }

                return Json(new { Success = result.ResultType == ResultType.Success, result.Message });
            }

            // If no payments are present, return the partial view
            model.Funds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id);
            model.People = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id);

            return PartialView("_CreateEditOfflineGiving", model);
        }

        [HttpGet]
        [RequirePermission(ModuleId = "1521435774e4281cb803c9422d98cf")]
        public ActionResult EditOfflineGiving(string id, string type)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var vm = new OfflineGivingView();

            var gift = work.OfflineGiving.Get(id);

            if (gift == null)
            {
                return HttpNotFound();
            }

            vm.OfflineGiving = gift;
            vm.Amount = gift.Amount.ToCurrencyString();
            vm.Funds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id);
            vm.People = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id);
            vm.OfflineGivingType = !string.IsNullOrEmpty(type) ? type : string.Empty;

            return PartialView(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [RequirePermission(ModuleId = "1521435774e4281cb803c9422d98cf")]
        public ActionResult EditOfflineGiving(OfflineGivingView model)
        {
            if (model.OfflineGivingType.Equals(OfflineGiftAmountTypes.LumpSum))
            {
                if (model.Amount.IsNullOrEmpty())
                {
                    model.Amount = "0";
                }

                model.OfflineGiving.Amount = decimal.Parse(model.Amount);

                if (TryValidateModel(model.OfflineGiving) && model.OfflineGiving.Amount > 0)
                {
                    model.OfflineGiving.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                    model.OfflineGiving.ModifiedDate = DateTime.Now;
                    work.OfflineGiving.Update(model.OfflineGiving);

                    return AjaxRedirectTo("/giving/offlinegiving");
                }
            }
            else
            {
                if (model.Amount.IsNullOrEmpty())
                {
                    model.Amount = "0";
                }

                model.OfflineGiving.Amount = decimal.Parse(model.Amount);

                if (TryValidateModel(model.OfflineGiving) && model.OfflineGiving.Amount > 0)
                {
                    if (model.OfflineGiving.PersonId.IsNotNullOrEmpty())
                    {
                        model.OfflineGiving.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                        model.OfflineGiving.ModifiedDate = DateTime.Now;
                        work.OfflineGiving.Update(model.OfflineGiving);

                        return AjaxRedirectTo("/giving/offlinegiving");
                    }

                    CreateAlertMessage("Please select a donor for giving", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                }
            }

            model.Funds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id);
            model.People = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id);

            return PartialView(model);
        }

        [HttpGet]
        [RequirePermission(ModuleId = "1521435774e4281cb803c9422d98cf")]
        public ActionResult CreateMassOfflineGiving(string type)
        {
            var vm = new MassOfflineGivingViewModel
            {
                OfflineGiving = new OfflineGiving()
                {
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now
                },

                Funds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id),
                People = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id),
                OfflineGivingType = !string.IsNullOrEmpty(type) ? type : OfflineGiftAmountTypes.Donor
            };

            return PartialView("_QuickMassOfflineGiving", vm);
        }

        [HttpPost]
        public ActionResult CreateMassOfflineGiving(MassOfflineGivingViewModel model)
        {
            try
            {
                if (model.Payments.IsNotNullOrEmpty() && model.Payments.Any())
                {
                    var offlineGiving = model.Payments.Select(q => new OfflineGiving()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Amount = decimal.Parse(q.Amount),
                        CheckNumber = q.CheckNumber,
                        PersonId = q.Person,
                        CampusId = model.OfflineGiving.CampusId,
                        FundId = model.OfflineGiving.FundId,
                        ChurchId = model.OfflineGiving.ChurchId,
                        CreatedBy = model.OfflineGiving.CreatedBy,
                        CreatedDate = model.OfflineGiving.CreatedDate,
                        DateReceived = model.OfflineGiving.DateReceived,
                        OfflinePaymentType = model.OfflineGiving.OfflinePaymentType,
                        OfflinePaymentMethod = q.CheckNumber.IsNotNullOrEmpty() ? OfflinePaymentMethods.Check : OfflinePaymentMethods.Cash
                    }).ToList();

                    var result = work.OfflineGiving.Create(offlineGiving);

                    if (result.ResultType == ResultType.Success)
                    {
                        CreateAlertMessage("Bulk offline giving has been saved.", AlertMessageTypes.Success, AlertMessageIcons.Success);
                    }

                    return Json(new { Success = result.ResultType == ResultType.Success, result.Message });
                }

                return Json(new { Success = false, Message = "Please add at least one offline giving record." });
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, ex.Message });
            }
        }

        [HttpGet]
        [RequirePermission(ModuleId = "1521435774e4281cb803c9422d98cf")]
        public ActionResult DeleteOfflineGiving(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var gift = work.OfflineGiving.Get(id);

            if (gift == null)
            {
                return HttpNotFound();
            }

            work.OfflineGiving.Delete(gift);

            return RedirectToAction(nameof(OfflineGiving));
        }
        #endregion
    }
}