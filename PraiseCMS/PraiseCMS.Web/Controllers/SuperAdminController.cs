using PraiseCMS.API.Helpers;
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
    [RequireRole(Role = Roles.SuperAdmin)]
    public class SuperAdminController : BaseController
    {
        public ActionResult Index()
        {
            var model = new SuperAdminViewModel
            {
                Churches = work.Church.GetAllActive()
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(SuperAdminViewModel model)
        {
            if (ModelState.IsValid)
            {
                return RedirectToAction("CreateMerchantAccountForChurch", new { id = model.SelectedChurchId });
            }

            model.Churches = work.Church.GetAllActive();

            return View(model);
        }

        #region Create Merchant Account For Chuches 
        //We use this to create a merchant account for a church when the user is stuck or unable to complete the onboarding process.
        public ActionResult CreateMerchantAccountForChurch(string id)
        {
            var model = new ChurchMerchantAccountVM
            {
                Account = work.ChurchMerchantAccount.GetByChurchId(id)
            };

            if (model.Account.IsNotNullOrEmpty())
            {
                model.Account.AccountNumber = model.Account.AccountNumber.Decrypt();
                model.Account.RoutingNumber = model.Account.RoutingNumber.Decrypt();
            }
            else
            {
                model.Account = new ChurchMerchantAccount();
            }

            if (!string.IsNullOrEmpty(id))
            {
                model.Church = work.Church.Get(id);
            }
            else
            {
                CreateAlertMessage("Enter a churchId into the query string.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                model.Church = new Church();
            }

            return View(model);
        }

        [HttpPost]
        public async Task<ActionResult> CreateMerchantAccountForChurch(ChurchMerchantAccountVM model)
        {
            try
            {
                var termsAndConditionsResponseData = await nuveiHelper.GetTermsAndConditionsAsync();

                model.CorrelationId = termsAndConditionsResponseData.correlationId;
                model.TermsAndConditionsId = termsAndConditionsResponseData.termsAndConditionsId;
                model.TermsAndConditionsUrl = termsAndConditionsResponseData.termsAndConditionsURL;

                //TODO - save the correlationId, termsAndConditionsId and termsAndConditionsURL to the terms and conditions PDF here.

                bool success = await nuveiHelper.AcceptTermsAndConditionsAsync(model.CorrelationId, model.TermsAndConditionsId);

                var leadModel = work.Giving.MapToLeadRequestModel(model);

                //don't need correlationid in both the model above and in createlead...it should be in one place
                var leadApiResponse = await nuveiHelper.CreateLeadAsync(model.CorrelationId, leadModel);

                var createLeadResponse = Responses.GetApiTransactionResponse(leadApiResponse?.result);

                if (createLeadResponse == APIStatuses.Success)
                {
                    model.Account.MerchantAccountId = leadApiResponse.merchant_key;
                    model.Account.Merchant = MerchantProviders.Nuvei;
                    model.Account.ChurchId = model.Church.Id;
                    model.Account.TaxId = model.Church.TaxIdNumber;
                    model.Account.BusinessWebsite = model.Church.Website;
                    model.Account.IsActive = true;
                    model.Account.CreatedBy = SessionVariables.CurrentUser.User.Id;
                    model.Account.CreatedDate = DateTime.Now;
                    model.Account.AccountNumber = model.Account.AccountNumber.Encrypt();
                    model.Account.RoutingNumber = model.Account.RoutingNumber.Encrypt();
                    model.Account.IsActive = true;
                    model.Account.CardProcessingFee = "DefaultCardProcessingFee".AppSetting(2.99m);
                    model.Account.ACHProcessingFee = "DefaultACHProcessingFee".AppSetting(0.75m);
                    model.Account.ApiUsername = leadApiResponse.api_username.Encrypt();
                    model.Account.ApiPassword = leadApiResponse.api_password.Encrypt();
                    model.Account.Username = leadApiResponse.username.Encrypt();
                    model.Account.Password = leadApiResponse.password.Encrypt();
                    model.Account.CorrelationId = model.CorrelationId;
                    model.Account.TermsAndConditionsId = model.TermsAndConditionsId;
                    model.Account.TermsAndConditionsUrl = model.TermsAndConditionsUrl;

                    if (!string.IsNullOrEmpty(model.Account.RespContactSSN))
                    {
                        model.Account.RespContactSSN = model.Account.RespContactSSN.Encrypt();

                        string lastFourDigits = model.Account.RespContactSSN.GetLastCharacters(4);

                        if (!string.IsNullOrEmpty(lastFourDigits))
                        {
                            model.Account.RespContactSSNLastFour = Convert.ToInt32(lastFourDigits);
                        }
                    }

                    var church = work.Church.Get(model.Church.Id);

                    church.HasMerchantAccount = true;
                    church.TaxIdNumber = model.Church.TaxIdNumber;
                    church.Website = model.Church.Website;
                    church.LegalName = model.Church.LegalName;
                    church.ModifiedDate = DateTime.Now;
                    church.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                    work.Church.Update(church);

                    if (model.Account.Id.IsNotNullOrEmpty())
                    {
                        model.Account.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                        model.Account.ModifiedDate = DateTime.Now;
                        work.ChurchMerchantAccount.Update(model.Account);
                    }
                    else
                    {
                        model.Account.Id = Utilities.GenerateUniqueId();
                        work.ChurchMerchantAccount.Create(model.Account);
                    }

                    CreateAlertMessage($"<strong>Great News!</strong> The church merchant account for {model.Church.Display} has been created.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                    return RedirectToAction("Index", "Churches");
                }

                CreateAlertMessage(leadApiResponse.message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", leadApiResponse.message);
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Error Occurred", SessionVariables.CurrentUser.User.Id, LogStatuses.Error, logObj);

                return View(model);
            }
            catch (Exception ex)
            {
                //ExceptionLogger.LogException(ex);
                CreateAlertMessage($"Something went wrong while creating the church merchant account for {model.Church.Display}, Error: {ex.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", ex.Message);
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Error Occurred", SessionVariables.CurrentUser.User.Id, LogStatuses.Error, logObj);

                return View(model);
            }
        }
        #endregion

        #region Blacklisted IPs
        public ActionResult BlacklistedIPs()
        {
            var model = new IPViewModel()
            {
                IPBlacklists = work.IPBlacklist.GetAll(),
                Users = work.User.GetAll(true)
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult _CreateBlacklistedIP()
        {
            var model = new IPBlacklist()
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            return PartialView("_CreateEditBlacklistedIP", model);
        }

        [HttpPost]
        public ActionResult _CreateBlacklistedIP(IPBlacklist model)
        {
            if (ModelState.IsValid)
            {
                model.IpAddress = model.IpAddress.IsNotNullOrEmpty() ? model.IpAddress.Trim() : model.IpAddress;
                work.IPBlacklist.Create(model);
                return AjaxRedirectTo("/superadmin/blacklistedips");
            }

            return PartialView("_CreateEditBlacklistedIP", model);
        }

        [HttpGet]
        public ActionResult _EditBlacklistedIP(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = work.IPBlacklist.Get(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return PartialView("_CreateEditBlacklistedIP", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditBlacklistedIP(IPBlacklist model)
        {
            if (ModelState.IsValid)
            {
                work.IPBlacklist.Update(model);

                return AjaxRedirectTo("/superadmin/blacklistedips");
            }

            return PartialView("_CreateEditBlacklistedIP", model);
        }

        [HttpGet]
        public ActionResult DeleteBlacklistedIp(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = work.IPBlacklist.Get(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            work.IPBlacklist.Delete(model);

            return RedirectToAction("blacklistedips");
        }
        #endregion

        #region Whitelisted IPs
        public ActionResult WhitelistedIPs()
        {
            var model = new IPViewModel()
            {
                IPBlacklists = work.IPBlacklist.GetAll(),
                Users = work.User.GetAll(true)
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult _CreateWhitelistedIP()
        {
            var model = new IPWhitelist()
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            return PartialView("_CreateEditWhitelistedIP", model);
        }

        [HttpPost]
        public ActionResult _CreateWhitelistedIP(IPWhitelist model)
        {
            if (ModelState.IsValid)
            {
                work.IPWhitelist.Create(model);

                return AjaxRedirectTo("/superadmin/whitelistedips");
            }

            return PartialView("_CreateEditWhitelistedIP", model);
        }

        [HttpGet]
        public ActionResult _EditWhitelistedIP(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = work.IPWhitelist.Get(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            return PartialView("_CreateEditWhitelistedIP", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditWhitelistedIP(IPWhitelist model)
        {
            if (ModelState.IsValid)
            {
                model.ModifiedDate = DateTime.Now;
                model.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                work.IPWhitelist.Update(model);

                return AjaxRedirectTo("/superadmin/whitelistedips");
            }

            return PartialView("_CreateEditWhitelistedIP", model);
        }

        [HttpGet]
        public ActionResult DeleteWhitelistedIp(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = work.IPWhitelist.Get(id);

            if (model == null)
            {
                return HttpNotFound();
            }

            work.IPWhitelist.Delete(model);

            return RedirectToAction("whitelistedips");
        }
        #endregion

        #region Delete Person
        [HttpGet]
        public ActionResult DeletePerson()
        {
            var people = work.Person.GetAllWhereChurchIdNotNull().Select(p => new SelectListItem
            {
                Value = p.Id,
                Text = p.Display
            }).OrderBy(p => p.Text).ToList();

            var viewModel = new DeletePersonViewModel
            {
                People = people
            };

            return View("DeletePerson", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeletePerson(DeletePersonViewModel model)
        {
            if (string.IsNullOrEmpty(model.SelectedPersonId))
            {
                ModelState.AddModelError("SelectedPersonId", "Please select a person to delete.");
            }

            var person = work.Person.Get(model.SelectedPersonId);

            if (person == null)
            {
                ModelState.AddModelError("", "Person not found.");
            }

            // Only continue if model is valid
            if (ModelState.IsValid)
            {
                var churchPerson = work.Person.GetByPersonId(person.Id);
                var user = work.User.GetByPersonId(person.Id);
                var churchUser = work.Church.GetChurchUser(churchPerson?.ChurchId, user?.Id);
                var userSettings = work.UserSetting.GetByUserId(user?.Id);

                if (person != null)
                {
                    work.Person.Delete(person.Id);
                }

                if (churchPerson != null)
                {
                    work.Person.Delete(person.Id);
                }

                if (user != null)
                {
                    work.User.Delete(user.Id);
                }

                if (churchUser != null)
                {
                    work.Church.DeleteUser(churchUser.Id);
                }

                if (userSettings != null)
                {
                    work.UserSetting.Delete(userSettings.Id);
                }

                return RedirectToAction("DeletePerson");
            }

            // If we got this far, something failed — rebuild the dropdown
            model.People = work.Person.GetAllWhereChurchIdNotNull()
                .Select(p => new SelectListItem
                {
                    Value = p.Id,
                    Text = p.Display
                }).OrderBy(p => p.Text).ToList();

            return View("DeletePerson", model);
        }
        #endregion

        #region Generate Data
        public ActionResult GenerateData()
        {
            var churches = work.Church.GetAllActive();
            return View(churches);
        }

        #region Attendance Data
        public ActionResult GenerateAttendanceData(string churchId)
        {
            if (string.IsNullOrEmpty(churchId))
            {
                ModelState.AddModelError("", "Please select a church.");
                var churches = work.Church.GetAllActive();
                return View("GenerateData", churches);
            }

            var model = new AttendanceInputViewModel
            {
                ChurchId = churchId,
                Campuses = work.Campus.GetAll(churchId)
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult GenerateAttendanceData(AttendanceInputViewModel model)
        {
            if (ModelState.IsValid)
            {
                GenerateAttendanceRecords(model);
                return RedirectToAction("GenerateData");
            }

            // Repopulate the campuses in case of validation errors
            model.Campuses = work.Campus.GetAll(model.ChurchId);

            return View(model);
        }

        private void GenerateAttendanceRecords(AttendanceInputViewModel model)
        {
            var churchId = model.ChurchId;
            var campusIds = model.SelectedCampusIds;
            var weeksOfData = model.WeeksOfData;
            var minTotal = model.MinTotal;
            var maxTotal = model.MaxTotal;
            var createdBy = SessionVariables.CurrentUser.User.Id;
            var currentDate = DateTime.Now;

            // Calculate the last Sunday
            var lastSunday = currentDate.AddDays(-(int)currentDate.DayOfWeek);

            // Generate a list of the last N Sundays
            var sundays = new List<DateTime>();
            for (int i = 0; i < weeksOfData; i++)
            {
                sundays.Add(lastSunday.AddDays(-7 * i));
            }

            foreach (var campusId in campusIds)
            {
                foreach (var occurredOnDate in sundays)
                {
                    var total = new Random().Next(minTotal, maxTotal + 1);

                    var attendance = new Attendance
                    {
                        Id = Utilities.GenerateUniqueId(),
                        ChurchId = churchId,
                        CampusId = campusId,
                        Total = total,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdBy,
                        OccurredOnDate = occurredOnDate
                    };

                    work.Attendance.Create(attendance);
                }
            }
        }
        #endregion

        #region Baptism Data
        public ActionResult GenerateBaptismData(string churchId)
        {
            if (string.IsNullOrEmpty(churchId))
            {
                ModelState.AddModelError("", "Please select a church.");
                var churches = work.Church.GetAllActive();
                return View("GenerateData", churches);
            }

            var model = new BaptismInputViewModel
            {
                ChurchId = churchId,
                Campuses = work.Campus.GetAll(churchId)
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult GenerateBaptismData(BaptismInputViewModel model)
        {
            if (ModelState.IsValid)
            {
                GenerateBaptismRecords(model);
                return RedirectToAction("GenerateData");
            }

            // Repopulate the campuses in case of validation errors
            model.Campuses = work.Campus.GetAll(model.ChurchId);

            return View(model);
        }

        private void GenerateBaptismRecords(BaptismInputViewModel model)
        {
            var churchId = model.ChurchId;
            var campusIds = model.SelectedCampusIds;
            var weeksOfData = model.WeeksOfData;
            var minTotal = model.MinTotal;
            var maxTotal = model.MaxTotal;
            var createdBy = SessionVariables.CurrentUser.User.Id;
            var currentDate = DateTime.Now;

            // Calculate the last Sunday
            var lastSunday = currentDate.AddDays(-(int)currentDate.DayOfWeek);

            // Generate a list of the last N Sundays
            var sundays = new List<DateTime>();
            for (int i = 0; i < weeksOfData; i++)
            {
                sundays.Add(lastSunday.AddDays(-7 * i));
            }

            foreach (var campusId in campusIds)
            {
                foreach (var occurredOnDate in sundays)
                {
                    var total = new Random().Next(minTotal, maxTotal + 1);

                    var baptism = new Baptism
                    {
                        Id = Utilities.GenerateUniqueId(),
                        ChurchId = churchId,
                        CampusId = campusId,
                        Total = total,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdBy,
                        OccurredOnDate = occurredOnDate
                    };

                    work.Baptism.Create(baptism);
                }
            }
        }
        #endregion

        #region Salvation Data
        public ActionResult GenerateSalvationData(string churchId)
        {
            if (string.IsNullOrEmpty(churchId))
            {
                ModelState.AddModelError("", "Please select a church.");
                var churches = work.Church.GetAllActive();
                return View("GenerateData", churches);
            }

            var model = new SalvationInputViewModel
            {
                ChurchId = churchId,
                Campuses = work.Campus.GetAll(churchId)
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult GenerateSalvationData(SalvationInputViewModel model)
        {
            if (ModelState.IsValid)
            {
                GenerateSalvationRecords(model);
                return RedirectToAction("GenerateData");
            }

            // Repopulate the campuses in case of validation errors
            model.Campuses = work.Campus.GetAll(model.ChurchId);

            return View(model);
        }

        private void GenerateSalvationRecords(SalvationInputViewModel model)
        {
            var churchId = model.ChurchId;
            var campusIds = model.SelectedCampusIds;
            var weeksOfData = model.WeeksOfData;
            var minTotal = model.MinTotal;
            var maxTotal = model.MaxTotal;
            var createdBy = SessionVariables.CurrentUser.User.Id;
            var currentDate = DateTime.Now;

            // Calculate the last Sunday
            var lastSunday = currentDate.AddDays(-(int)currentDate.DayOfWeek);

            // Generate a list of the last N Sundays
            var sundays = new List<DateTime>();
            for (int i = 0; i < weeksOfData; i++)
            {
                sundays.Add(lastSunday.AddDays(-7 * i));
            }

            foreach (var campusId in campusIds)
            {
                foreach (var occurredOnDate in sundays)
                {
                    var total = new Random().Next(minTotal, maxTotal + 1);

                    var salvation = new Salvation
                    {
                        Id = Utilities.GenerateUniqueId(),
                        ChurchId = churchId,
                        CampusId = campusId,
                        Total = total,
                        CreatedDate = DateTime.Now,
                        CreatedBy = createdBy,
                        OccurredOnDate = occurredOnDate
                    };

                    work.Salvation.Create(salvation);
                }
            }
        }
        #endregion

        #region Offline Giving Data
        public class OfflineGivingFormViewModel
        {
            public string ChurchId { get; set; }
            public int WeeksOfData { get; set; }
            public List<string> SelectedCampusIds { get; set; }
            public List<Campus> Campuses { get; set; }
            public List<string> SelectedFundIds { get; set; }
            public List<Fund> Funds { get; set; }
            public decimal MinAmount { get; set; }
            public decimal MaxAmount { get; set; }
            public List<string> SelectedPaymentMethods { get; set; }
            public List<string> SelectedPaymentTypes { get; set; }
            public bool IsLumpSumGiving { get; set; }
            public decimal? LumpSumGivingMinAmount { get; set; }
            public decimal? LumpSumGivingMaxAmount { get; set; }

            public OfflineGivingFormViewModel()
            {
                Campuses = new List<Campus>();
                Funds = new List<Fund>();
                SelectedCampusIds = new List<string>();
                SelectedFundIds = new List<string>();
                SelectedPaymentMethods = new List<string>();
                SelectedPaymentTypes = new List<string>();
            }
        }

        public ActionResult GenerateOfflineGivingData(string churchId)
        {
            if (string.IsNullOrEmpty(churchId))
            {
                ModelState.AddModelError("", "Please select a church.");
                var churches = work.Church.GetAllActive();
                return View("GenerateData", churches);
            }

            var model = new OfflineGivingFormViewModel
            {
                ChurchId = churchId,
                Campuses = work.Campus.GetAll(churchId),
                Funds = work.Fund.GetAll(churchId)
            };
            return View(model);
        }

        [HttpPost]
        public ActionResult GenerateOfflineGivingData(OfflineGivingFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                GenerateOfflineGivingRecords(model);
                return RedirectToAction("GenerateData");
            }

            // Repopulate the campuses and funds in case of validation errors
            model.Campuses = work.Campus.GetAll(model.ChurchId);
            model.Funds = work.Fund.GetAll(model.ChurchId);

            return View(model);
        }

        private void GenerateOfflineGivingRecords(OfflineGivingFormViewModel model)
        {
            var churchId = model.ChurchId;
            var campusIds = model.SelectedCampusIds;
            var weeksOfData = model.WeeksOfData;
            var fundIds = model.SelectedFundIds;
            var minAmount = model.MinAmount;
            var maxAmount = model.MaxAmount;
            var selectedPaymentMethods = model.SelectedPaymentMethods;
            var selectedPaymentTypes = model.SelectedPaymentTypes;
            var isLumpSumGiving = model.IsLumpSumGiving;
            var lumpSumGivingMinAmount = model.LumpSumGivingMinAmount ?? 0;
            var lumpSumGivingMaxAmount = model.LumpSumGivingMaxAmount ?? 0;
            var createdBy = SessionVariables.CurrentUser.User.Id;
            var currentDate = DateTime.Now;

            // Fetch the list of active person IDs once
            var personIds = work.Person.GetActivePersonIds(churchId);

            // Calculate the last Sunday
            var lastSunday = currentDate.AddDays(-(int)currentDate.DayOfWeek);

            // Generate a list of the last N Sundays
            var sundays = new List<DateTime>();
            for (int i = 0; i < weeksOfData; i++)
            {
                sundays.Add(lastSunday.AddDays(-7 * i));
            }

            var random = new Random();

            foreach (var campusId in campusIds)
            {
                foreach (var dateReceived in sundays)
                {
                    foreach (var fundId in fundIds)
                    {
                        var offlinePaymentMethod = selectedPaymentMethods.Count > 0
                            ? selectedPaymentMethods[random.Next(selectedPaymentMethods.Count)]
                            : (random.NextDouble() > 0.5 ? OfflinePaymentMethods.Cash : OfflinePaymentMethods.Check);

                        // Generate random amount for regular giving
                        decimal amount = Math.Round((decimal)((random.NextDouble() * (double)(maxAmount - minAmount)) + (double)minAmount), 2);

                        // Generate a random amount for lump sum giving if applicable
                        if (isLumpSumGiving)
                        {
                            // Create Lump Sum Giving record
                            var lumpSumAmount = Math.Round((decimal)((random.NextDouble() * (double)(lumpSumGivingMaxAmount - lumpSumGivingMinAmount)) + (double)lumpSumGivingMinAmount), 2);

                            var lumpSumGiving = new OfflineGiving
                            {
                                Id = Utilities.GenerateUniqueId(),
                                OfflinePaymentMethod = OfflinePaymentMethods.Cash, // Lump sum is not tied to a specific payment method
                                Amount = lumpSumAmount,
                                FundId = fundId,
                                PersonId = null,
                                ChurchId = churchId,
                                CampusId = campusId,
                                CreatedDate = DateTime.Now,
                                CreatedBy = createdBy,
                                OfflinePaymentType = selectedPaymentTypes.Count > 0
                                    ? selectedPaymentTypes[random.Next(selectedPaymentTypes.Count)]
                                    : (random.NextDouble() > 0.5 ? OfflinePaymentTypes.OfferingPlate :
                                      random.NextDouble() > 0.25 ? OfflinePaymentTypes.DropOff : OfflinePaymentTypes.Mail),
                                DateReceived = dateReceived,
                                CheckNumber = null // No check number for lump sum giving
                            };

                            work.OfflineGiving.Create(lumpSumGiving);
                        }

                        // Generate a check number if the payment method is "Check"
                        string checkNumber = offlinePaymentMethod == OfflinePaymentMethods.Check
                            ? random.Next(1000, 10000).ToString("D4")
                            : null;

                        var offlineGiving = new OfflineGiving
                        {
                            Id = Utilities.GenerateUniqueId(),
                            OfflinePaymentMethod = offlinePaymentMethod,
                            Amount = amount,
                            FundId = fundId,
                            PersonId = personIds[random.Next(personIds.Count)],
                            ChurchId = churchId,
                            CampusId = campusId,
                            CreatedDate = DateTime.Now,
                            CreatedBy = createdBy,
                            OfflinePaymentType = selectedPaymentTypes.Count > 0
                                ? selectedPaymentTypes[random.Next(selectedPaymentTypes.Count)]
                                : (random.NextDouble() > 0.5 ? OfflinePaymentTypes.OfferingPlate :
                                  random.NextDouble() > 0.25 ? OfflinePaymentTypes.DropOff : OfflinePaymentTypes.Mail),
                            DateReceived = dateReceived,
                            CheckNumber = checkNumber // Assigns the check number if the payment method is "Check"
                        };

                        work.OfflineGiving.Create(offlineGiving);
                    }
                }
            }
        }
        #endregion
        #endregion

        #region Internal Praise Methods Only
        /*******************DO NOT USE BELOW*********************************
         ***This is used ONLY to create the merchant account for PRAISE******
         ***********to get paid by churches for subscriptions.***************
         ********************************************************************/
        public ActionResult CreatePraiseMerchantAccount()
        {
            var model = new ChurchMerchantAccountVM
            {
                Account = new ChurchMerchantAccount
                {
                    Id = Utilities.GenerateUniqueId(),
                    Merchant = MerchantProviders.Nuvei,
                    BankAccountType = "Checking", //NOVO bank account
                    AccountNumber = "98932992", //NOVO bank account
                    RoutingNumber = "211370150", //NOVO bank account
                    RespContactFirstName = "Cale",
                    RespContactLastName = "Coggins",
                    RespContactPhone = "(205) 915-7429",
                    RespContactEmail = "info@praisecms.com",
                    RespContactDOB = Convert.ToDateTime("05/17/1987"),
                    RespContactSSN = "123456789",
                    RespContactDLN = "123456789",
                    RespContactAddress1 = "2637 Montauk Rd.",
                    RespContactCity = "Hoover",
                    RespContactState = "AL",
                    RespContactZip = "35226",
                    CardProcessingFee = 0.0m,
                    ACHProcessingFee = 0.0m,
                    ApiUsername = string.Empty,
                    ApiPassword = string.Empty,
                    Username = string.Empty,
                    Password = string.Empty
                },

                Church = new Church
                {
                    Id = Utilities.GenerateUniqueId(),
                    Name = "Praise CMS",
                    Phone = "(205) 915-7425",
                    Email = "info@praisecms.com",
                    Website = "https://www.praisecms.com",
                    TaxIdNumber = "38-4151018",
                    PhysicalAddress1 = "2637 Montauk Rd.",
                    PhysicalCity = "Hoover",
                    PhysicalState = "AL",
                    PhysicalZip = "35226",
                    TimeZone = TimeZones.Central,
                    Denomination = "Other",
                    IsActive = true,
                    IsMultiSite = false,
                    PrimaryUserId = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now,
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    LegalName = "Praise, LLC",
                    HasMerchantAccount = false,
                    RegistrationCompleted = true,
                    GivingAccountSetupCompleted = false,
                    AllowDonorCoverProcessingFee = false,
                    PaperlessGiving = false,
                    ShowWelcomeMessage = false,
                    IsAutoEmail = false,
                    SubscriptionFee = 0.0m
                }
            };

            try
            {
                model.Account.Merchant = MerchantProviders.Nuvei;
                model.Account.ChurchId = model.Church.Id;
                model.Account.IsActive = true;
                model.Account.CreatedBy = SessionVariables.CurrentUser.User.Id;
                model.Account.CreatedDate = DateTime.Now;
                model.Account.AccountNumber = model.Account.AccountNumber.Encrypt();
                model.Account.RoutingNumber = model.Account.RoutingNumber.Encrypt();
                model.Account.CardProcessingFee = "DefaultCardProcessingFee".AppSetting(2.99m);
                model.Account.ACHProcessingFee = "DefaultACHProcessingFee".AppSetting(0.75m);
                model.Account.ApiUsername = string.Empty;
                model.Account.ApiPassword = string.Empty;
                model.Account.Username = string.Empty;
                model.Account.Password = string.Empty;
                model.Church.HasMerchantAccount = true;

                work.Church.Create(model.Church);
                work.ChurchMerchantAccount.Create(model.Account);
                CreateAlertMessage("<strong>Great News!</strong> The Praise account for Nuvei has been created!", AlertMessageTypes.Success, AlertMessageIcons.Success);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                CreateAlertMessage($"Something went wrong while creating a merchant account for PRAISE CMS. Error: {ex.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                var logObj = logRepository.JsonConverter("Action Name", RouteHelpers.CurrentAction, "Controller Name", RouteHelpers.CurrentController, "Exception Message", ex.Message);
                logRepository.LogData(RouteHelpers.CurrentAction, RouteHelpers.CurrentController, "Error Occurred", SessionVariables.CurrentUser.User.Id, LogStatuses.Error, logObj);

                return RedirectToAction("Index", "Home");
            }
        }
        #endregion
    }
}