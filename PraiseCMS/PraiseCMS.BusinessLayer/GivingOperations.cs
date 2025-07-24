using PraiseCMS.API.Helpers;
using PraiseCMS.API.Models;
using PraiseCMS.API.Services;
using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Mapper;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.DataAccess.Singletons;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace PraiseCMS.BusinessLayer
{
    public class GivingOperations : GenericRepository
    {
        public TokenManager tokenManager;
        public NuveiHelper nuveiHelper;

        public GivingOperations(ApplicationDbContext Db, Work work) : base(Db, work)
        {
            tokenManager = new TokenManager();
            nuveiHelper = new NuveiHelper(tokenManager);
        }

        public List<Payment> GetAll(string churchId, DateRange dateRange = null)
        {
            var query = Read<Payment>().Where(x => x.ChurchId == churchId);

            if (dateRange != null)
            {
                query = query.Where(x => x.CreatedDate >= dateRange.StartDate && x.CreatedDate <= dateRange.EndDate);
            }

            return query.ToList();
        }

        public async Task<Result<GivingDashboard>> GetGivingDashboard(ChurchMerchantAccount churchMerchantAccount, string churchId, string userId, int listSize)
        {
            var dashboard = new GivingDashboard();
            string donorGuid = null;

            //Check if donor has a merchant account in OUR SYSTEM, otherwise create one            
            var userMerchantAccount = Work.UserMerchantAccount.GetByUserId(userId, MerchantProviders.Nuvei);

            if (userMerchantAccount?.DonorGUID.IsNullOrEmpty() != false)
            {
                var result = await CreateDonorAccount(SessionVariables.CurrentUser.User, churchMerchantAccount);

                if (result.ResultType == ResultType.Failure || result.Data.IsNullOrEmpty() || result.Data.DonorGUID.IsNullOrEmpty())
                {
                    return new Result<GivingDashboard>
                    {
                        Data = dashboard,
                        ResultType = ResultType.Failure,
                        ResultIcon = ResultIcon.Failure,
                        Message = result.Message
                    };
                }
                userMerchantAccount = result.Data;
                SessionVariables.CurrentUser.UserMerchantAccounts.Add(userMerchantAccount);
            }

            donorGuid = userMerchantAccount.DonorGUID;

            //Get Payment Methods
            var accounts = GetPaymentMethodsList(donorGuid);
            dashboard.BankAccounts = accounts.BankAccounts;

            var accountGuids = accounts.CreditCards.Select(s => s.AccountGUID).ToList();
            var paymentMethodAccounts = Work.PaymentMethodAccount.GetAllByPaymentMethod(accountGuids).Where(x => accountGuids.Contains(x.AccountGUID)).ToList();

            foreach (var card in accounts.CreditCards)
            {
                // Checking card expiration
                var days = Utilities.CardExpirationCalculateInDays(card.ExpMonth, card.ExpYear);

                if (days > 1 && days <= 30)
                {
                    dashboard.CardExpiryNotification += $"Your card ending in {card.MaskedCardNumber} will expire in {days} days. ";
                }
                else if (days <= 1)
                {
                    var paymentMethodAccount = paymentMethodAccounts.FirstOrDefault(x => x.AccountGUID == card.AccountGUID);

                    if (paymentMethodAccount?.ExpiredNotificationCleared.IsNullOrEmpty() == true)
                    {
                        dashboard.CardExpiredNotification += $"Your card ending in {card.MaskedCardNumber} has expired.";
                        dashboard.ExpiredCardAccountGUID = paymentMethodAccount.AccountGUID;
                    }

                    if (paymentMethodAccount?.IsPrimary == true)
                    {
                        dashboard.CardExpiredNotification = $"Your primary payment method ending in {card.MaskedCardNumber} has expired. ";
                        dashboard.IsPrimary = true;
                    }

                    if (paymentMethodAccount?.IsExpired.IsNullOrEmpty() == true && !Convert.ToBoolean(paymentMethodAccount.IsExpired))
                    {
                        paymentMethodAccount.IsExpired = true;
                        Db.SaveChanges();
                    }
                }

                dashboard.CreditCards.Add(card);
            }

            //Scheduled Giving
            dashboard.ScheduledPayments = Work.ScheduledPayment.GetList(userId, churchId);

            //Giving History
            dashboard.Payments = Work.Payment.GetAllByUserId(churchId, userId, null, listSize);

            // Offline Giving History
            var offlineGiving = Work.OfflineGiving.GetAllByUserId(churchId, userId, listSize);

            // Fetching combined payment methods
            var paymentMethods = dashboard.Payments.Select(q => q.PaymentMethod).ToList();
            paymentMethods.AddRange(offlineGiving.Select(q => q.OfflinePaymentMethod).ToList());
            dashboard.PaymentMethods = Work.PaymentMethodAccount.GetAllByPaymentMethod(paymentMethods.Distinct().ToList());

            // My Giving
            dashboard.MyGiving = Mapper.Map(dashboard.Payments);
            dashboard.MyGiving.AddRange(Mapper.Map(offlineGiving));

            dashboard.MyGiving = dashboard.MyGiving.Count >= listSize ? dashboard.MyGiving.OrderByDescending(x => x.CreatedDate).Take(listSize).ToList() : dashboard.MyGiving.OrderByDescending(x => x.CreatedDate).ToList();

            dashboard.LastGift = dashboard.Payments.Where(x => x.UserId == userId).OrderByDescending(x => x.CreatedDate).FirstOrDefault();
            dashboard.PrimaryAccountGUID = Work.PaymentMethodAccount.GetPrimaryAccountGUIDByDonorGUID(donorGuid);
            dashboard.Funds = Work.Fund.GetAll(churchId);
            dashboard.User = Work.User.Get(userId);

            return new Result<GivingDashboard>
            {
                Data = dashboard,
                ResultType = ResultType.Success,
                ResultIcon = ResultIcon.Failure
            };
        }

        public Result<GivingDashboard> GetHistory(string churchId, string userId = null, string personId = null, string startDate = null, string endDate = null, string fundId = null, string campusId = null)
        {
            List<Payment> payments = new List<Payment>();
            List<OfflineGiving> offlineGiving = new List<OfflineGiving>();

            // First, check if both userId and personId are provided
            if (!string.IsNullOrEmpty(userId) && !string.IsNullOrEmpty(personId))
            {
                // If both are provided, ensure consistency by preferring the userId-based retrieval
                payments = Work.Payment.GetAllByUserId(churchId, userId);

                // Retrieve offline giving history using personId
                offlineGiving = Work.OfflineGiving.GetAllByPersonId(churchId, personId);
            }
            else if (!string.IsNullOrEmpty(userId))
            {
                // If only userId is provided, retrieve both digital and offline giving history by userId
                payments = Work.Payment.GetAllByUserId(churchId, userId);

                var person = Work.Person.GetByUserId(userId);
                if (person != null)
                {
                    offlineGiving = Work.OfflineGiving.GetAllByPersonId(churchId, person.Id);
                }
            }
            else if (!string.IsNullOrEmpty(personId))
            {
                // If only personId is provided, retrieve offline giving history by personId
                offlineGiving = Work.OfflineGiving.GetAllByPersonId(churchId, personId);

                var user = Work.User.GetByPersonId(personId);
                if (user != null)
                {
                    payments = Work.Payment.GetAllByUserId(churchId, user.Id);
                }
            }

            return new Result<GivingDashboard>
            {
                Data = GetGivingHistoryDashboard(payments, offlineGiving, churchId, startDate, endDate, fundId, campusId),
                ResultType = ResultType.Success
            };
        }

        public WidgetsGraphModel GetGraphData(Church church)
        {
            var model = new WidgetsGraphModel();
            var workWeekStartDay = ExtensionMethods.GetWorkWeekStartDay(SessionVariables.CurrentChurch.WorkWeek);

            // Get the list of weeks based on the church's WorkWeek setting
            var weeks = ExtensionMethods.GetDatesOfLastNumberOfWeeks(DateTime.Now, 7, true, workWeekStartDay);

            // Get all giving data within the range of the last 7 weeks
            var giving = GetGiving(church.Id, new DateRange
            {
                StartDate = weeks.First().First(),
                EndDate = weeks.Last().Last()
            });

            model.Key = "giving";

            foreach (var week in weeks)
            {
                var subModel = new GraphData
                {
                    Value = giving
                        .Where(x => x.CreatedDate.Date <= week.Last().Date && x.CreatedDate.Date >= week.First().Date)
                        .Select(s => s.Amount)
                        .Sum()
                        .ToString(),

                    Category = $"{week.First():MMM dd} - {week.Last():MMM dd}"
                };
                model.Data.Add(subModel);
            }

            return model;
        }

        public List<MyGivingVM> GetGiving(string churchId, DateRange dateRange)
        {
            // Digital Giving History
            var payments = Work.Payment.GetAll(churchId, null, dateRange);
            // Offline Giving History
            var offlineGiving = Work.OfflineGiving.GetAll(churchId, null, dateRange);
            var giving = Mapper.Map(payments);
            giving.AddRange(Mapper.Map(offlineGiving));

            return giving.OrderByDescending(x => x.CreatedDate).ToList();
        }

        public GivingFundSummaryViewModel GetAllGivingByFund(string churchId, string fundId)
        {
            return new GivingFundSummaryViewModel
            {
                DigitalGiving = Work.Payment.GetAll(churchId, fundId),
                OfflineGiving = Work.OfflineGiving.GetAll(churchId, fundId)
            };
        }

        public int NewDonors(string churchId)
        {
            // Digital Giving History
            var payments = Work.Payment.GetAll(churchId);
            // Offline Giving History
            var offlineGiving = Work.OfflineGiving.GetAll(churchId);

            // Combine payments and offline giving
            var giving = Mapper.Map(payments);
            giving.AddRange(Mapper.Map(offlineGiving));

            var workWeekStartDay = ExtensionMethods.GetWorkWeekStartDay(SessionVariables.CurrentChurch.WorkWeek);

            // Get the current week dates
            var currentWeek = ExtensionMethods.GetDatesOfWeeks(DateTime.Now.Date, 1, true, workWeekStartDay);

            // Flatten the list of weeks to a single list of dates
            var currentWeekDates = currentWeek.SelectMany(week => week).ToList();

            // Filter donors for the current week and previous weeks
            var currentWeekDonors = giving
                .Where(x => x.PersonId.IsNotNullOrEmpty() && currentWeekDates.Contains(x.CreatedDate.Date))
                .Select(x => x.PersonId)
                .Distinct()
                .ToList();

            var oldDonors = giving
                .Where(x => x.PersonId.IsNotNullOrEmpty() && !currentWeekDates.Contains(x.CreatedDate.Date))
                .Select(x => x.PersonId)
                .Distinct()
                .ToList();

            // Count new donors by excluding old donors from current week donors
            return currentWeekDonors.Except(oldDonors).Count();
        }

        internal GivingDashboard GetGivingHistoryDashboard(List<Payment> payments, List<OfflineGiving> offlineGiving, string churchId, string startDate, string endDate, string fundId, string campusId)
        {
            var dashboard = new GivingDashboard();
            // Fetching combined payment methods
            var paymentMethods = payments.Select(q => q.PaymentMethod).ToList();
            paymentMethods.AddRange(offlineGiving.Select(q => q.OfflinePaymentMethod).ToList());
            dashboard.PaymentMethods = Work.PaymentMethodAccount.GetAllByPaymentMethod(paymentMethods.Distinct().ToList());

            // My Giving
            dashboard.MyGiving = Mapper.Map(payments);
            dashboard.MyGiving.AddRange(Mapper.Map(offlineGiving));
            dashboard.MyGiving = dashboard.MyGiving.OrderByDescending(x => x.CreatedDate.ToLocalTime()).ToList();

            //select only the funds and campuses that the current user has actually given to
            dashboard.Funds = Work.Fund.GetAll(churchId);
            dashboard.Campuses = Work.Campus.GetAll(churchId);

            if (!string.IsNullOrEmpty(startDate))
            {
                dashboard.MyGiving = dashboard.MyGiving.Where(x => x.CreatedDate.Date >= DateTime.Parse(startDate).Date).OrderByDescending(x => x.CreatedDate.ToLocalTime()).ToList();
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                dashboard.MyGiving = dashboard.MyGiving.Where(x => x.CreatedDate.Date <= DateTime.Parse(endDate).Date).OrderByDescending(x => x.CreatedDate.ToLocalTime()).ToList();
            }

            if (!string.IsNullOrEmpty(fundId))
            {
                dashboard.MyGiving = dashboard.MyGiving.Where(x => x.FundId.IsNotNullOrEmpty() && x.FundId == fundId).OrderByDescending(x => x.CreatedDate.ToLocalTime()).ToList();
            }

            if (!string.IsNullOrEmpty(campusId))
            {
                dashboard.MyGiving = dashboard.MyGiving.Where(x => x.CampusId.IsNotNullOrEmpty() && x.CampusId.Equals(campusId)).OrderByDescending(x => x.CreatedDate.ToLocalTime()).ToList();
            }

            return dashboard;
        }

        public ChurchPaymentsDashboard GetChurchPaymentsSummary(string churchId, string startDate, string endDate, string fundId, string campusId)
        {
            // Digital Giving History
            var paymentsList = Work.Payment.GetAll(churchId);

            // Offline Giving History
            //var offlineGiving = Work.OfflineGiving.GetSomeForUser(churchId, userId);

            if (!string.IsNullOrEmpty(startDate))
            {
                paymentsList = paymentsList.Where(x => x.CreatedDate.Date >= DateTime.Parse(startDate).Date).OrderByDescending(x => x.CreatedDate.ToLocalTime()).ToList();
            }

            if (!string.IsNullOrEmpty(endDate))
            {
                paymentsList = paymentsList.Where(x => x.CreatedDate.Date <= DateTime.Parse(endDate).Date).OrderByDescending(x => x.CreatedDate.ToLocalTime()).ToList();
            }

            if (!string.IsNullOrEmpty(fundId))
            {
                paymentsList = paymentsList.Where(x => x.FundId.IsNotNullOrEmpty() && x.FundId == fundId).OrderByDescending(x => x.CreatedDate.ToLocalTime()).ToList();
            }

            if (!string.IsNullOrEmpty(campusId))
            {
                paymentsList = paymentsList.Where(x => x.CampusId.IsNotNullOrEmpty() && x.CampusId.Equals(campusId)).OrderByDescending(x => x.CreatedDate.ToLocalTime()).ToList();
            }

            var funds = Work.Fund.GetAll(churchId);
            var campuses = Work.Campus.GetAll(churchId);
            paymentsList.Select(d =>
            {
                d.FundId = funds.Any(q => q.Id.Equals(d.FundId)) ? funds.First(q => q.Id.Equals(d.FundId)).Display : string.Empty;
                d.CampusId = campuses.Any(q => q.Id.Equals(d.CampusId)) ? campuses.First(q => q.Id.Equals(d.CampusId)).Display : string.Empty;
                return d;
            }).ToList();

            return new ChurchPaymentsDashboard()
            {
                Payments = Mapper.MapPaymentsSummary(paymentsList).OrderByDescending(x => x.CreatedDate).ToList(),
                Funds = funds,
                Campuses = campuses,
                CampusId = campusId,
                FundId = fundId
            };
        }

        public PaymentMethodDashboard GetPaymentMethodsList(string donorGuid)
        {
            var accounts = new List<PaymentMethodAccount>();
            var result = new PaymentMethodDashboard()
            {
                BankAccounts = new List<BankAccount>(),
                CreditCards = new List<CreditCard>()
            };

            if (!string.IsNullOrEmpty(donorGuid))
            {
                accounts = Work.PaymentMethodAccount.GetAll(donorGuid).Where(q => q.IsActive).ToList();
            }

            if (accounts.IsNotNullOrEmpty() && accounts.Any(q => q.AccountType == DigitalPaymentMethods.ACH))
            {
                result.BankAccounts.AddRange(accounts.Where(q => q.AccountType == DigitalPaymentMethods.ACH).Select(q => new BankAccount()
                {
                    Nickname = q.NickName,
                    AccountType = q.AccountSubType,
                    BankName = q.AccountProvider,
                    AccountGUID = q.AccountGUID,
                    MaskedAccountNumber = $"****{q.PaymentMethodPreview.GetLastCharacters(4)}",
                    StatusName = PaymentMethodStatuses.Active
                }).ToList());
            }

            if (accounts.IsNotNullOrEmpty() && accounts.Any(q => q.AccountType == DigitalPaymentMethods.Card))
            {
                result.CreditCards.AddRange(accounts.Where(q => q.AccountType == DigitalPaymentMethods.Card).Select(q => new CreditCard()
                {
                    Nickname = q.NickName,
                    CardType = q.AccountSubType,
                    ExpMonth = q.ExpMonth,
                    ExpYear = q.ExpYear,
                    StatusName = Utilities.CardExpirationCalculateInDays(q.ExpMonth, q.ExpYear) <= 1 ? PaymentMethodStatuses.Expired : PaymentMethodStatuses.Active,
                    AccountGUID = q.AccountGUID,
                    MaskedCardNumber = $"****{q.PaymentMethodPreview.GetLastCharacters(4)}"
                }).ToList());
            }

            return result;
        }

        public PaymentMethodDashboard GetChurchPaymentMethodsList(string donorGuid)
        {
            var accounts = new List<PaymentMethodAccount>();
            var result = new PaymentMethodDashboard()
            {
                BankAccounts = new List<BankAccount>(),
                CreditCards = new List<CreditCard>()
            };

            if (!string.IsNullOrEmpty(donorGuid))
            {
                accounts = Work.PaymentMethodAccount.GetAll(donorGuid).Where(q => q.IsActive).ToList();
            }

            if (accounts.IsNotNullOrEmpty() && accounts.Any(q => q.AccountType == DigitalPaymentMethods.ACH))
            {
                result.BankAccounts.AddRange(accounts.Where(q => q.AccountType == DigitalPaymentMethods.ACH).Select(q => new BankAccount()
                {
                    Nickname = q.NickName,
                    AccountType = GetNickNameByType(q.AccountSubType),
                    BankName = q.AccountProvider,
                    AccountGUID = q.AccountGUID,
                    MaskedAccountNumber = $"****{q.PaymentMethodPreview.GetLastCharacters(4)}",
                    StatusName = PaymentMethodStatuses.Active
                }).ToList());
            }

            if (accounts.IsNotNullOrEmpty() && accounts.Any(q => q.AccountType == DigitalPaymentMethods.Card))
            {
                result.CreditCards.AddRange(accounts.Where(q => q.AccountType == DigitalPaymentMethods.Card).Select(q => new CreditCard()
                {
                    Nickname = q.NickName,
                    CardType = q.AccountSubType,
                    ExpMonth = q.ExpMonth,
                    ExpYear = q.ExpYear,
                    StatusName = Utilities.CardExpirationCalculateInDays(q.ExpMonth, q.ExpYear) <= 1 ? PaymentMethodStatuses.Expired : PaymentMethodStatuses.Active,
                    AccountGUID = q.AccountGUID,
                    MaskedCardNumber = $"****{q.PaymentMethodPreview.GetLastCharacters(4)}"
                }).ToList());
            }

            return result;
        }

        public bool ClearCardExpiredNotification(string accountGUID)
        {
            if (string.IsNullOrEmpty(accountGUID)) return false;

            var paymentMethodAccount = Db.PaymentMethodAccounts.FirstOrDefault(x => x.AccountGUID == accountGUID.Trim());

            if (paymentMethodAccount == null) return true;

            paymentMethodAccount.ExpiredNotificationCleared = true;
            Db.SaveChanges();

            return true;
        }

        public decimal CalculateProcessingFee(string churchId, string amount, string accountType)
        {
            var charges = 0m;
            var hasAmount = decimal.TryParse(amount, NumberStyles.Currency,
              CultureInfo.CurrentCulture.NumberFormat, out var amountDecimal);

            if (hasAmount)
            {
                var churchAccount = Work.ChurchMerchantAccount.GetByChurchId(churchId);

                if (accountType != null && churchAccount != null)
                {
                    if (accountType == DigitalPaymentMethods.ACH)
                    {
                        charges = churchAccount.ACHProcessingFee;
                    }
                    else if (accountType == DigitalPaymentMethods.Card && churchAccount.CardProcessingFee > 0)
                    {
                        var appFeeMultiplier = churchAccount.CardProcessingFee / 100;
                        charges = Math.Round(appFeeMultiplier * amountDecimal, 2);
                    }
                }
            }

            return charges;
        }

        public async Task<Result<UserMerchantAccount>> CreateDonorAccount(ApplicationUser user, ChurchMerchantAccount churchMerchantAccount)
        {
            var isNewAccount = false;
            var userMerchantAccount = new UserMerchantAccount();

            if (churchMerchantAccount.IsNotNull())
            {
                userMerchantAccount = Work.UserMerchantAccount.GetByUserId(user.Id, churchMerchantAccount.Merchant);
            }

            if (userMerchantAccount.IsNullOrEmpty())
            {
                isNewAccount = true;
                userMerchantAccount = new UserMerchantAccount
                {
                    Id = Utilities.GenerateUniqueId(),
                    UserId = user.Id,
                    Merchant = MerchantProviders.Nuvei,
                    DonorId = user.Id.SubstringIt(20),
                    IsActive = true,
                    CreatedDate = DateTime.Now,
                    CreatedBy = user.Id
                };
            }

            if (userMerchantAccount?.DonorGUID.IsNullOrEmpty() == true)
            {
                var customer = CreateCustomerRequestFromUser(user);

                ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(churchMerchantAccount.ApiUsername, churchMerchantAccount.ApiPassword);

                var customerResponse = await nuveiHelper.CreateCustomerAsync(customer, apiCredentials);

                if (customerResponse != null && customerResponse.result != "0")
                {
                    var obj = logsRepository.JsonConverter("Action Name", "CreateDonorAccount", "Exception Message", customerResponse.result_message,
                                "Exception Code", customerResponse.result_description, "User Id", user.Id);
                    logsRepository.LogData("CreateDonorAccount", "Giving Operations", "Create Donor", user.Id, LogStatuses.Error, obj);

                    return new Result<UserMerchantAccount>
                    {
                        Data = new UserMerchantAccount(),
                        ResultType = ResultType.Failure,
                        Message = customerResponse.result_message
                    };
                }

                userMerchantAccount.DonorGUID = customerResponse.customer_key;

                if (isNewAccount)
                {
                    Db.UserMerchantAccounts.Add(userMerchantAccount);
                }

                Db.SaveChanges();
            }

            return new Result<UserMerchantAccount>
            {
                Data = userMerchantAccount,
                ResultType = ResultType.Success,
            };
        }

        public async Task<Church> CreateChurchDonorAccount(Church church)
        {
            //Treat the church as a customer for subscription payments. The church needs a customer/donor account to pay Praise.
            var customerRequestModel = CreateCustomerRequestFromChurch(church);

            try
            {
                //Use Praise's merchant account and to onboard new churches under.                
                var praiseMerchantAccount = Work.ChurchMerchantAccount.GetPraiseChurchAccount();

                //The Praise API credentials tie the church's donor account to Praise
                ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(praiseMerchantAccount.ApiUsername, praiseMerchantAccount.ApiPassword);

                //Create Donor/Customer for the church
                var customerResponse = await nuveiHelper.CreateCustomerAsync(customerRequestModel, apiCredentials);

                var customerKey = !string.IsNullOrEmpty(customerResponse.customer_key) ? customerResponse.customer_key : string.Empty;//This ties the customer to Nuvei - save to database

                var customerFound = await nuveiHelper.GetCustomerDetailsAsync(customerKey, apiCredentials);

                if (customerResponse == null || customerResponse.result != "0")
                {
                    var obj = logsRepository.JsonConverter("Action Name", "CreateChurchDonorAccount", "Exception Message", customerResponse.result_message,
                        "Exception Code", customerResponse.result_description, "Church Id", church.Id);
                    logsRepository.LogData("CreateChurchDonorAccount", "Giving Operations", "Create Donor", church.Id, LogStatuses.Error, obj);

                    //Notify Praise
                    var smsTestPhoneNumber = "SMSTestPhoneNumber".AppSetting("(205) 915-7429");
                    var smsMessage = new SmsMessage
                    {
                        Id = Utilities.GenerateUniqueId(),
                        To = smsTestPhoneNumber,
                        Message = $"Error creating church donor account for {church.Display} ({church.Id})\n\n{customerResponse.result_message}\n\nOn: {DateTime.Now}",
                        CreatedDate = DateTime.Now,
                        CreatedBy = string.IsNullOrEmpty(church.Id) ? Constants.System : church.Id,
                        Type = SmsMessageType.SignIn
                    };
                    Utilities.SendMessage(smsMessage);
                }

                if (customerResponse != null)
                {
                    church.DonorID = customerKey;
                    church.DonorGUID = customerKey;
                    church.ModifiedDate = DateTime.Now;
                    church.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                    Work.Church.Update(church);
                }

                return church;
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return church;
            }
        }

        private CustomerRequest CreateCustomerRequestFromUser(ApplicationUser user)
        {
            return new CustomerRequest(
                customer_type: CustomerTypes.CK,
                customer_id: user.Id.SubstringIt(20),
                customer_name: user.Display,
                first_name: user.FirstName,
                last_name: user.LastName,
                street1: user.Address1.IsNotNullOrEmpty() ? user.Address1 : string.Empty,
                street2: user.Address2.IsNotNullOrEmpty() ? user.Address2 : string.Empty,
                city: user.City.IsNotNullOrEmpty() ? user.City : string.Empty,
                state_id: user.State.IsNotNullOrEmpty() ? user.State : string.Empty,
                zip_code: user.Zip.IsNotNullOrEmpty() ? user.Zip : string.Empty,
                email: user.Email.IsNotNullOrEmpty() ? user.Email : $"{user.FirstName.Replace(" ", string.Empty)}{Utilities.GenerateVerificationCode().GetLastCharacters(4)}@noemailprovided.com",
                status: CustomerStatuses.Active
            );
        }

        private CustomerRequest CreateCustomerRequestFromChurch(Church church)
        {
            return new CustomerRequest(
                customer_type: CustomerTypes.CK,
                customer_id: Utilities.GenerateUniqueId().SubstringIt(20),
                customer_name: church.Name,
                first_name: church.Name,
                last_name: church.Name,
                street1: church.PhysicalAddress1.IsNotNullOrEmpty() ? church.PhysicalAddress1 : string.Empty,
                street2: church.PhysicalAddress2.IsNotNullOrEmpty() ? church.PhysicalAddress2 : string.Empty,
                city: church.PhysicalCity.IsNotNullOrEmpty() ? church.PhysicalCity : string.Empty,
                state_id: church.PhysicalState.IsNotNullOrEmpty() ? church.PhysicalState : string.Empty,
                zip_code: church.PhysicalZip.IsNotNullOrEmpty() ? church.PhysicalZip : string.Empty,
                email: church.Email.IsNotNullOrEmpty() ? church.Email : $"{church.Name.Replace(" ", string.Empty)}{Utilities.GenerateVerificationCode().GetLastCharacters(4)}@noemailprovided.com",
                status: CustomerStatuses.Active
            );
        }

        public Result<GivingSignUpViewModel> StartGiving(string Id, bool isRecuring = false, string selectedFundId = null)
        {
            var church = new Church();

            if (!string.IsNullOrEmpty(Id))
            {
                church = Work.Church.Get(Id);

                if (church.IsNullOrEmpty())
                {
                    return new Result<GivingSignUpViewModel>
                    {
                        Data = new GivingSignUpViewModel(),
                        ResultType = ResultType.Failure,
                        Message = "We did not find the church you are looking for."
                    };
                }

                var funds = Work.Fund.GetAll(Id);

                if (!funds.Any())
                {
                    return new Result<GivingSignUpViewModel>
                    {
                        Data = new GivingSignUpViewModel(),
                        ResultType = ResultType.Failure,
                        Message = "There are no funds associated with this church."
                    };
                }
            }

            var model = new GivingSignUpViewModel
            {
                Church = church
            };

            SessionVariables.CurrentChurch = church;
            var churchMerchant = Db.ChurchMerchantAccounts.FirstOrDefault(x => x.ChurchId.Equals(SessionVariables.CurrentChurch.Id) && x.IsActive);

            if (churchMerchant.IsNullOrEmpty())
            {
                return new Result<GivingSignUpViewModel>
                {
                    Data = new GivingSignUpViewModel(),
                    ResultType = ResultType.Failure,
                    Message = "No merchant account has been set up for the church."
                };
            }

            model.HasMerchantAccount = true;

            model.Payment = new Payment()
            {
                Id = Utilities.GenerateUniqueId(),
                Merchant = churchMerchant.IsNotNull() ? churchMerchant.Merchant : "Merchant Name Not Defined",
                MerchantId = churchMerchant.IsNotNull() ? churchMerchant.MerchantAccountId : string.Empty,
                ChurchId = SessionVariables.CurrentChurch.Id,
                UserId = SessionVariables.CurrentUser.IsNotNull() ? SessionVariables.CurrentUser.User.Id : string.Empty,
                TransactionType = TransactionType.Payment,
                CreatedDate = DateTime.Now,
                CreatedBy = SessionVariables.CurrentUser.IsNotNull() ? SessionVariables.CurrentUser.User.Id : string.Empty,
            };

            model.ScheduledPayment = new ScheduledPayment()
            {
                Id = Utilities.GenerateUniqueId(),
                Merchant = churchMerchant.IsNotNull() ? churchMerchant.Merchant : "Merchant Name Not Defined",
                MerchantId = churchMerchant.IsNotNull() ? churchMerchant.MerchantAccountId : string.Empty,
                UserId = SessionVariables.CurrentUser.IsNotNull() ? SessionVariables.CurrentUser.User.Id : string.Empty,
                ChurchId = SessionVariables.CurrentChurch.Id,
                TransactionType = TransactionType.Payment,
                RecurringFrequency = PaymentFrequency.Biweekly,
                RecurringStartDate = DateTime.Now,
                GiftEndingReason = GiftEndingReasons.WhenICancelIt,
                CreatedBy = SessionVariables.CurrentUser.IsNotNull() ? SessionVariables.CurrentUser.User.Id : string.Empty,
                CreatedDate = DateTime.Now
            };

            model.Funds = Work.Fund.GetDigitalFundsByChurch(model.Church.Id);

            if (selectedFundId.IsNotNullOrEmpty())
            {
                model.Payment.FundId = selectedFundId;
            }

            model.Campuses = Db.Campuses.Where(x => x.ChurchId.Equals(model.Church.Id) && x.IsActive).OrderBy(x => x.Name).Select(s => new SelectListItem()
            {
                Text = s.Name,
                Value = s.Id
            }).ToList();

            if (model.Campuses.IsNotNullOrEmpty() && model.Campuses.Any() && model.Campuses.Count == 1)
            {
                model.Campuses.Select(d => { d.Selected = true; return d; }).ToList();
            }

            model.Payment.Frequency = PaymentOccurrence.OneTime;

            if (isRecuring)
            {
                var userMerchAccount = Db.UserMerchantAccounts.FirstOrDefault(x => x.Merchant == MerchantProviders.Nuvei && x.UserId == SessionVariables.CurrentUser.User.Id);

                if (userMerchAccount?.DonorGUID.IsNotNullOrEmpty() == true)
                {
                    model.Payment.Frequency = PaymentOccurrence.Recurring;
                    model.DonorGUID = userMerchAccount.DonorGUID;
                    model.Accounts = Work.Payment.GetPaymentMethodsDropdownList(userMerchAccount.DonorGUID);
                    model.Phone = SessionVariables.CurrentUser.User.PhoneNumber;
                    model.ScheduledPayment.RecurringFrequency = PaymentFrequency.Biweekly;
                    model.ScheduledPayment.RecurringStartDate = DateTime.Now.AddDays(14);
                }
            }

            if (model.Accounts.Any())
            {
                model.AccountFound = true;
            }

            return new Result<GivingSignUpViewModel> { Data = model, ResultType = ResultType.Success };
        }

        public GuestPaymentModel GuestGiving(string Id)
        {
            var church = new Church();

            if (!string.IsNullOrEmpty(Id))
            {
                church = Work.Church.Get(Id);
            }

            var model = new GuestPaymentModel
            {
                Church = church
            };
            SessionVariables.CurrentChurch = church;
            var churchMerchant = Db.ChurchMerchantAccounts.FirstOrDefault(x => x.ChurchId.Equals(SessionVariables.CurrentChurch.Id));

            if (churchMerchant.IsNull())
            {
                return null;
            }

            model.HasMerchantAccount = true;
            model.Funds = Work.Fund.GetDigitalFundsByChurch(model.Church.Id);
            model.Campuses = Db.Campuses.Where(x => x.ChurchId.Equals(model.Church.Id) && x.IsActive).OrderBy(x => x.Name).Select(s => new SelectListItem()
            {
                Text = s.Name,
                Value = s.Id
            }).ToList();

            if (model.Campuses.IsNotNullOrEmpty() && model.Campuses.Any() && model.Campuses.Count == 1)
            {
                model.Campuses.Select(d => { d.Selected = true; return d; }).ToList();
            }

            return model;
        }

        public string GetNickNameByType(string type)
        {
            switch (type)
            {
                case CreditCardTypes.AmericanExpress:
                    return PaymentMethodNickNames.AmericanExpress;
                case CreditCardTypes.Mastercard:
                    return PaymentMethodNickNames.Mastercard;
                case CreditCardTypes.Visa:
                    return PaymentMethodNickNames.Visa;
                case CreditCardTypes.Discover:
                    return PaymentMethodNickNames.Discover;
                case BankAccountTypes.Checking:
                    return PaymentMethodNickNames.Checking;
                case BankAccountTypes.Savings:
                    return PaymentMethodNickNames.Savings;
            }

            return string.Empty;
        }

        public async Task<bool> CardExistsForDonor(PaymentMethodViewModel model, ApiCredentials apiCredentials)
        {
            var paymentMethodAccounts = Work.PaymentMethodAccount.GetAll(model.DonorGUID);

            var accounts = await nuveiHelper.GetCustomerPaymentMethods(model.DonorGUID, apiCredentials);

            if (accounts.IsNullOrEmpty() || accounts.CreditCards.IsNullOrEmpty())
            {
                return false;
            }

            var matchingCard = accounts.CreditCards.FirstOrDefault(x =>
                x.ExpirationMonth.Equals(model.PaymentCard.CcExpMonth) &&
                x.ExpirationYear.Equals(model.PaymentCard.CcExpYear) &&
                x.card_number_last_four_digits.Equals(model.PaymentCard.CcNumber.Replace(" ", string.Empty).GetLastCharacters(4))
            );

            if (matchingCard == null)
            {
                return false;
            }

            return paymentMethodAccounts.IsNotNullOrEmpty() &&
                   paymentMethodAccounts.Any(x =>
                       x.IsActive &&
                       x.ExpMonth.IsNotNullOrEmpty() &&
                       x.ExpMonth.Equals(model.PaymentCard.CcExpMonth) &&
                       x.ExpYear.IsNotNullOrEmpty() &&
                       x.ExpYear.Equals(model.PaymentCard.CcExpYear.GetLastCharacters(2)) &&
                       x.AccountGUID == matchingCard.card_key &&
                       x.PaymentMethodPreview.GetLastCharacters(4).Equals(model.PaymentCard.CcNumber.Replace(" ", string.Empty).GetLastCharacters(4)));
        }

        public async Task<bool> CardExistsForChurch(PaymentMethodViewModel model, ApiCredentials apiCredentials)
        {
            //Here we treat the church as the donor to Praise and checking for the church's payment methods.
            var paymentMethodAccounts = Db.PaymentMethodAccounts.Where(q => q.DonorGUID.Equals(model.DonorGUID)).ToList();

            var accounts = await nuveiHelper.GetCustomerPaymentMethods(model.DonorGUID, apiCredentials);

            if (accounts.IsNotNullOrEmpty() && accounts.CreditCards.IsNotNullOrEmpty()
                && accounts.CreditCards.IsNotNullOrEmpty() && accounts.CreditCards.Any(x => x.ExpirationMonth.Equals(model.PaymentCard.CcExpMonth)
                && x.ExpirationYear.Equals(model.PaymentCard.CcExpYear) && x.card_number_last_four_digits.Equals(model.PaymentCard.CcNumber.GetLastCharacters(4))))
            {
                var card = accounts.CreditCards.FirstOrDefault(x => x.ExpirationMonth.Equals(model.PaymentCard.CcExpMonth)
                   && x.ExpirationYear.Equals(model.PaymentCard.CcExpYear) && x.card_number_last_four_digits.Equals(model.PaymentCard.CcNumber.GetLastCharacters(4)));

                if (paymentMethodAccounts.IsNotNullOrEmpty() && paymentMethodAccounts.Any(x => x.IsActive && x.ExpMonth.IsNotNullOrEmpty() && x.ExpMonth.Equals(model.PaymentCard.CcExpMonth)
                   && x.ExpYear.IsNotNullOrEmpty() && x.ExpYear.Equals(model.PaymentCard.CcExpYear.GetLastCharacters(2)) && x.AccountGUID == card.card_key
                   && x.PaymentMethodPreview.GetLastCharacters(4).Equals(model.PaymentCard.CcNumber.GetLastCharacters(4))))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Used to Change encryption from old version to new. Should not be needed anymore as of 3/12/2024
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        //public void UpdateChurchMerchantAccountEncryption(ChurchMerchantAccount account)
        //{
        //    var newAccountNumber = !string.IsNullOrEmpty(account.AccountNumber) ? Utilities.MigrateAndEncrypt(account.AccountNumber) : String.Empty;
        //    var newRoutingNumber = !string.IsNullOrEmpty(account.RoutingNumber) ? Utilities.MigrateAndEncrypt(account.RoutingNumber) : String.Empty;
        //    var newUsername = !string.IsNullOrEmpty(account.Username) ? Utilities.MigrateAndEncrypt(account.Username) : String.Empty;
        //    var newPassword = !string.IsNullOrEmpty(account.Password) ? Utilities.MigrateAndEncrypt(account.Password) : String.Empty;
        //    var newApiUsername = !string.IsNullOrEmpty(account.ApiUsername) ? Utilities.MigrateAndEncrypt(account.ApiUsername) : String.Empty;
        //    var newApiPassword = !string.IsNullOrEmpty(account.ApiPassword) ? Utilities.MigrateAndEncrypt(account.ApiPassword) : String.Empty;
        //    var newRespContactSSN = !string.IsNullOrEmpty(account.RespContactSSN) ? Utilities.MigrateAndEncrypt(account.RespContactSSN) : String.Empty;

        //    account.AccountNumber = newAccountNumber;
        //    account.RoutingNumber = newRoutingNumber;
        //    account.Username = newUsername;
        //    account.Password = newPassword;
        //    account.ApiUsername = newApiUsername;
        //    account.ApiPassword = newApiPassword;
        //    account.RespContactSSN = newRespContactSSN;

        //    Work.ChurchMerchantAccount.Update(account);
        //}

        public async Task<bool> CheckAccountExists(PaymentMethodViewModel model, ApiCredentials apiCredentials)
        {
            var paymentMethodAccounts = Work.PaymentMethodAccount.GetAll(model.DonorGUID);

            var accounts = await nuveiHelper.GetCustomerPaymentMethods(model.DonorGUID, apiCredentials);

            if (accounts.IsNotNullOrEmpty() && accounts.BankAccounts.IsNotNullOrEmpty()
                && accounts.BankAccounts.IsNotNullOrEmpty() && accounts.BankAccounts.Any(x =>
                x.account_number_last_four_digits.Equals(model.PaymentAccount.AccountNumber.GetLastCharacters(4)) && x.account_type.Equals(model.PaymentAccount.AccountType)))
            {
                var bank = accounts.BankAccounts.FirstOrDefault(x => x.account_number_last_four_digits.Equals(model.PaymentAccount.AccountNumber.GetLastCharacters(4))
                && x.account_type.Equals(model.PaymentAccount.AccountType));

                if (paymentMethodAccounts.IsNotNullOrEmpty() && paymentMethodAccounts.Any(x => x.IsActive && x.AccountGUID == bank.check_key
                   && x.PaymentMethodPreview.GetLastCharacters(4).Equals(model.PaymentAccount.AccountNumber.GetLastCharacters(4))
                   && x.AccountSubType.Equals(model.PaymentAccount.AccountType)))
                {
                    return true;
                }
            }

            return false;
        }

        public async Task<bool> CheckAccountExistsForChurch(PaymentMethodViewModel model, ApiCredentials apiCredentials)
        {
            //Here we treat the church as the donor to Praise and checking for the church's payment methods.
            var paymentMethodAccounts = Db.PaymentMethodAccounts.Where(q => q.DonorGUID.Equals(model.DonorGUID)).ToList();

            var accounts = await nuveiHelper.GetCustomerPaymentMethods(model.DonorGUID, apiCredentials);

            if (accounts.IsNotNullOrEmpty() && accounts.BankAccounts.IsNotNullOrEmpty()
                && accounts.BankAccounts.IsNotNullOrEmpty() && accounts.BankAccounts.Any(x =>
                x.account_number_last_four_digits.GetLastCharacters(4).Equals(model.PaymentAccount.AccountNumber.GetLastCharacters(4)) && x.account_type.Equals(model.PaymentAccount.AccountType)))
            {
                var bank = accounts.BankAccounts.FirstOrDefault(x => x.account_number_last_four_digits.GetLastCharacters(4).Equals(model.PaymentAccount.AccountNumber.GetLastCharacters(4))
                && x.account_type.SubstringIt(1).Equals(model.PaymentAccount.AccountType));

                if (paymentMethodAccounts.IsNotNullOrEmpty() && paymentMethodAccounts.Any(x => x.IsActive && x.AccountGUID == bank.check_key
                   && x.PaymentMethodPreview.GetLastCharacters(4).Equals(model.PaymentAccount.AccountNumber.GetLastCharacters(4))
                   && x.AccountSubType.Equals(model.PaymentAccount.AccountType)))
                {
                    return true;
                }
            }

            return false;
        }

        public GivingViewModel GetSummary()
        {
            var model = new GivingSignUpViewModel();
            var givingObj = Utilities.ReadCookies<GivingAmountModel>("giving_amount");

            if (SessionVariables.CurrentChurch.IsNull())
            {
                SessionVariables.CurrentChurch = Work.Church.Get(givingObj.ChurchId);
            }

            if (SessionVariables.Campuses.IsNull() || SessionVariables.Campuses.Count == 0)
            {
                SessionVariables.Campuses = Work.Campus.GetAll(givingObj.ChurchId);
            }

            var amount = givingObj.Amount;
            var fund = givingObj.Fund;
            model.Church = new Church { Id = givingObj.ChurchId };

            model.Church = SessionVariables.CurrentChurch;
            model.Payment = new Payment
            {
                Amount = Convert.ToDecimal(amount),
                FundId = fund
            };

            //user
            var givingModel = new GivingViewModel
            {
                Payment =
                {
                    Amount = Convert.ToDecimal(amount),
                    FundId = fund,
                    CampusId = SessionVariables.Campuses.FirstOrDefault(x => x.ChurchId.Equals(SessionVariables.CurrentChurch.Id))?.Id
                },
                ChurchId = givingObj.ChurchId,
                Campuses = Db.Campuses.Where(x => x.ChurchId.Equals(model.Church.Id)).OrderBy(x => x.Name).Select(s => new SelectListItem()
                {
                    Text = s.Name,
                    Value = s.Id
                }).ToList()
            };

            if (givingModel.Campuses.IsNotNullOrEmpty() && givingModel.Campuses.Any() && givingModel.Campuses.Count == 1)
            {
                givingModel.Campuses.Select(d => { d.Selected = true; return d; }).ToList();
            }

            givingModel.Funds = Db.Funds.Where(x => x.ChurchId.Equals(SessionVariables.CurrentChurch.Id) && !x.Hidden && !x.Closed).OrderBy(x => x.Name).Select(s => new SelectListItem()
            {
                Text = s.Name,
                Value = s.Id
            }).ToList();

            if (SessionVariables.CurrentUser.UserMerchantAccounts.Count > 0)
            {
                var donorGuid = SessionVariables.CurrentUser.UserMerchantAccounts.Find(x => x.Merchant.Equals(MerchantProviders.Nuvei)).DonorGUID;
                givingModel.Accounts = Work.Payment.GetPaymentMethodsDropdownList(donorGuid);
            }

            return givingModel;
        }

        public void MakePrimary(string id, string donorGuid, bool updateScheduledPayments = false)
        {
            var paymentMethodAccounts = Db.PaymentMethodAccounts.Where(x => x.DonorGUID.Equals(donorGuid)).ToList();

            // Update IsPrimary flag for payment method accounts
            paymentMethodAccounts.ForEach(d => d.IsPrimary = d.AccountGUID == id);

            // Optionally update scheduled payments
            if (updateScheduledPayments)
            {
                var accountGUIDs = paymentMethodAccounts.Select(x => x.AccountGUID).ToList();

                // Update PaymentMethod for scheduled payments
                foreach (var item in Db.ScheduledPayments.Where(x => accountGUIDs.Contains(x.PaymentMethod)).ToList())
                {
                    item.PaymentMethod = id;
                }
            }

            Db.SaveChanges();
        }

        public GivingViewModel GetViewModel(string donorGuid)
        {
            var model = new GivingViewModel
            {
                Payment = new Payment()
                {
                    Id = Utilities.GenerateUniqueId(),
                    Merchant = MerchantProviders.Nuvei,
                    MerchantId = !string.IsNullOrEmpty(SessionVariables.CurrentMerchant.MerchantAccountId) ? SessionVariables.CurrentMerchant.MerchantAccountId : string.Empty,
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    UserId = SessionVariables.CurrentUser.User.Id,
                    TransactionType = TransactionType.Payment,
                    CreatedDate = DateTime.Now,
                    CreatedBy = SessionVariables.CurrentUser.User.Id
                },
                Church = SessionVariables.CurrentChurch,
                LastGift = Db.Payments.Where(x => x.UserId.Equals(SessionVariables.CurrentUser.User.Id)).OrderByDescending(x => x.CreatedDate).FirstOrDefault(),
                Accounts = Work.Payment.GetPaymentMethodsDropdownList(donorGuid),
                Campuses = SessionVariables.Campuses.Select(campus => new SelectListItem { Text = campus.Name, Value = campus.Id }).OrderBy(x => x.Text).ToList()
            };

            if (model.Campuses.IsNotNullOrEmpty() && model.Campuses.Any() && model.Campuses.Count == 1)
            {
                model.Campuses.Select(d => { d.Selected = true; return d; }).ToList();
            }

            model.Funds = Work.Fund.GetDigitalFundsByChurch(model.Church.Id);

            return model;
        }

        public void SendScheduledGiftEmail(ScheduledGiftViewModel model)
        {
            var user = Db.Users.FirstOrDefault(q => q.Id == model.ScheduledPayment.UserId);
            var church = Db.Churches.FirstOrDefault(q => q.Id == model.ScheduledPayment.ChurchId);
            var fund = Db.Funds.FirstOrDefault(q => q.Id == model.ScheduledPayment.FundId && !q.IsDeleted);
            var paymentMethodAccount = Work.PaymentMethodAccount.GetByAccountGUID(model.ScheduledPayment.PaymentMethod);
            var paymentMethod = paymentMethodAccount.IsNotNullOrEmpty() ? paymentMethodAccount.PaymentMethodPreview : string.Empty;

            var content = EmailTemplates.ScheduledGiving_body.Replace("{amount}", Convert.ToDecimal(model.ScheduledPayment.Amount).ToCurrencyString())
              .Replace("{start_date}", model.ScheduledPayment.RecurringStartDate != null ? Convert.ToDateTime(model.ScheduledPayment.RecurringStartDate).ToShortDateString() : DateTime.Now.ToShortDateString())
              .Replace("{user_display}", user?.Display)
              .Replace("{church_display}", church?.Display)
              .Replace("{paymentmethod}", paymentMethod)
              .Replace("{fund_display}", fund.IsNotNullOrEmpty() ? fund.Display : "Fund not specified")
              .Replace("{frequency}", model.ScheduledPayment.RecurringFrequency);

            var email = new Email()
            {
                Id = Utilities.GenerateUniqueId(),
                Message = content,
                To = user?.Email,
                Attachments = null,
                Subject = $"Scheduled Giving Created for {church?.Display}",
                CreatedBy = SessionVariables.CurrentUser?.User.Id != null ? SessionVariables.CurrentUser.User.Id : string.Empty,
                CreatedDate = DateTime.Now
            };

            Emailer.SendEmail(email, null, new Domain() { EmailLogo = church.Logo, Name = church.Display, EmailReplyAddress = church.Email, EmailDisplay = church.Display }, church, true);
        }

        public ScheduledGiftViewModel GetGivingViewModelById(string id)
        {
            var model = new ScheduledGiftViewModel();

            if (id.IsNotNullOrEmpty())
            {
                model.ScheduledPayment = Work.ScheduledPayment.Get(id);
                model.Amount = model.ScheduledPayment.Amount.ToString();
            }
            else
            {
                var churchMerchant = Work.ChurchMerchantAccount.GetByChurchId(SessionVariables.CurrentChurch.Id);
                model.ScheduledPayment.Merchant = churchMerchant != null ? churchMerchant.Merchant : "Merchant Name Not Defined";
                model.ScheduledPayment.MerchantId = churchMerchant != null ? churchMerchant.Id : "Merchant Id Not Defined";
                model.ScheduledPayment.UserId = SessionVariables.CurrentUser.User.Id;
                model.ScheduledPayment.ChurchId = SessionVariables.CurrentChurch.Id;
                model.ScheduledPayment.Frequency = PaymentOccurrence.Recurring;
                model.ScheduledPayment.TransactionType = TransactionType.Payment;
                model.ScheduledPayment.CreatedBy = SessionVariables.CurrentUser.User.Id;
                model.ScheduledPayment.CreatedDate = DateTime.Now;
                model.ScheduledPayment.RecurringFrequency = PaymentFrequency.Monthly;
                model.ScheduledPayment.RecurringStartDate = DateTime.Now;
                model.ScheduledPayment.GiftEndingReason = GiftEndingReasons.WhenICancelIt;
                model.ScheduledPayment.Amount = 0m;
                model.ScheduledPayment.IsActive = true;
            }

            model.AllowDonorCoverProcessingFee = SessionVariables.CurrentChurch.AllowDonorCoverProcessingFee;
            model.ChurchId = SessionVariables.CurrentChurch.Id;

            var donorGuid = SessionVariables.CurrentUser.UserMerchantAccounts.Find(x => x.Merchant.Equals(MerchantProviders.Nuvei)).DonorGUID;
            model.DonorGUID = donorGuid;
            model.Accounts = Work.Payment.GetPaymentMethodsDropdownList(donorGuid);
            model.Campuses = Work.Campus.GetCampusSelectList(SessionVariables.Campuses);
            model.Funds = Work.Fund.GetDigitalFundsByChurch(model.ChurchId);

            return model;
        }

        public DonorStatus GetDonorStatus(string personId)
        {
            var user = Work.User.GetByPersonId(personId);
            var payments = new List<Payment>();

            if (user.IsNotNullOrEmpty())
            {
                payments = Work.Payment.GetAllByUserId(SessionVariables.CurrentChurch.Id, user.Id);
            }

            if (payments.Any(q => q.Frequency?.Equals(PaymentOccurrence.Recurring) == true && q.ScheduledPaymentId.IsNotNullOrEmpty()))
            {
                var activeGifts = Work.ScheduledPayment.GetAllUnprocessed(user.Id, SessionVariables.CurrentChurch.Id);
                var scheduledGiftIds = payments.Where(q => q.Frequency.Equals(PaymentOccurrence.Recurring) && q.ScheduledPaymentId.IsNotNullOrEmpty()).Select(q => q.ScheduledPaymentId).Distinct().ToList();

                if (scheduledGiftIds.Any(x => activeGifts.Select(q => q.Id).Contains(x)))
                {
                    return DonorStatus.RecurringDonor;
                }
            }

            var offlineGiving = Read<OfflineGiving>().Where(q => q.PersonId.Equals(personId)).ToList();
            var donationCount = offlineGiving.Count + payments.Count;

            switch (donationCount)
            {
                case 1:
                    return DonorStatus.FirstTimeDonor;
                case 2:
                    return DonorStatus.SecondTimeDonor;
                default:
                    if (offlineGiving.Count(x => x.CreatedDate.Date > DateTime.Now.AddYears(-1).Date) + payments.Count(x => x.CreatedDate.Date > DateTime.Now.AddYears(-1).Date) > 11)
                    {
                        return DonorStatus.RegularDonor;
                    }

                    if (donationCount > 2)
                    {
                        return DonorStatus.OccasionalDonor;
                    }

                    break;
            }

            return DonorStatus.NonDonor;
        }

        #region Scheduled Payments        
        public async Task ScheduledPaymentsJob()
        {
            var scheduledPayments = Work.ScheduledPayment.GetAllUnprocessed();
            try
            {
                foreach (var payment in scheduledPayments)
                {
                    if (payment.RecurringEndDate == null && payment.MaxGifts == null)
                    {
                        await TransactionLogic(payment);
                    }
                    else if (payment.RecurringEndDate != null && payment.MaxGifts == null)
                    {
                        if (Convert.ToDateTime(payment.RecurringEndDate).Date >= DateTime.Today)
                        {
                            await TransactionLogic(payment);
                        }
                        else
                        {
                            payment.IsProcessed = true;
                        }
                    }
                    else if (payment.RecurringEndDate == null && payment.MaxGifts != null && payment.MaxGifts > 0)
                    {
                        if (payment.PaymentsMade == null || payment.PaymentsMade < payment.MaxGifts)
                        {
                            await TransactionLogic(payment);
                        }
                        else if (payment.PaymentsMade != null && payment.PaymentsMade == payment.MaxGifts)
                        {
                            payment.IsProcessed = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                Db.SaveChanges();
                Db.Dispose();
            }
        }

        public async Task TransactionLogic(ScheduledPayment payment)
        {
            if (Convert.ToDateTime(payment.RecurringStartDate).Date <= DateTime.Now.Date && payment.NextChargeDate == null)
            {
                var isPayment = await ProcessScheduledPayment(payment);

                if (isPayment)
                {
                    payment.LastChargeDate = DateTime.Now;
                    payment.NextChargeDate = GetNextChargeDate(Convert.ToDateTime(payment.RecurringStartDate), payment.RecurringFrequency);
                    //increase count after each payment
                    payment.PaymentsMade = payment.PaymentsMade == null ? 1 : payment.PaymentsMade + 1;
                }
            }
            else if (payment.NextChargeDate != null && Convert.ToDateTime(payment.NextChargeDate).Date <= DateTime.Now.Date)
            {
                var isPayment = await ProcessScheduledPayment(payment);

                if (isPayment)
                {
                    payment.LastChargeDate = DateTime.Now;
                    payment.NextChargeDate = GetNextChargeDate(Convert.ToDateTime(payment.NextChargeDate), payment.RecurringFrequency);
                    //increase count after each payment
                    payment.PaymentsMade = payment.PaymentsMade == null ? 1 : payment.PaymentsMade + 1;
                }
            }
        }

        public async Task<bool> ProcessScheduledPayment(ScheduledPayment schedulePayment)
        {
            var scheduledPayments = (from SP in Db.ScheduledPayments
                                     join PMA in Db.PaymentMethodAccounts
                                     on SP.PaymentMethod equals PMA.AccountGUID
                                     where SP.Id == schedulePayment.Id
                                     select new { SP, PMA }).FirstOrDefault();

            var model = new GivingViewModel();

            if (!string.IsNullOrEmpty(schedulePayment.FundId))
            {
                var fund = Work.Fund.Get(schedulePayment.FundId);

                if (fund.IsNullOrEmpty() || fund.Closed)
                {
                    fund = Work.Fund.GetByName(model.Church.Id, GivingFunds.General);
                    schedulePayment.FundId = fund.Id;
                }
            }

            if (scheduledPayments != null)
            {
                var paymentMethodAccount = Work.PaymentMethodAccount.GetByAccountGUID(scheduledPayments.SP.PaymentMethod);
                var processingFees = Work.Giving.CalculateProcessingFee(scheduledPayments.SP.ChurchId, scheduledPayments.SP.Amount.ToString(), paymentMethodAccount.AccountType);

                model.ChurchId = schedulePayment.ChurchId;
                model.DonorGUID = scheduledPayments.PMA.DonorGUID;
                model.AccountGUID = schedulePayment.PaymentMethod;
                model.AccountType = paymentMethodAccount.AccountType;
                model.IncludeProcessingFee = scheduledPayments.SP.IncludeProcessingFee;

                model.Payment = new Payment()
                {
                    Id = Utilities.GenerateUniqueId(),
                    Merchant = schedulePayment.Merchant,
                    MerchantId = schedulePayment.MerchantId,
                    ChurchId = schedulePayment.ChurchId,
                    CampusId = schedulePayment.CampusId,
                    UserId = schedulePayment.UserId,
                    ProcessingFee = processingFees,
                    DonorPaidMerchantFee = scheduledPayments.SP.IncludeProcessingFee,
                    Amount = Convert.ToDecimal(scheduledPayments.SP.Amount) + (model.IncludeProcessingFee ? processingFees : 0),
                    FundId = schedulePayment.FundId,
                    Frequency = schedulePayment.Frequency,
                    RecurringFrequency = schedulePayment.RecurringFrequency,
                    PaymentMethod = schedulePayment.PaymentMethod,
                    TransactionType = schedulePayment.TransactionType,
                    CreatedDate = DateTime.Now,
                    CreatedBy = schedulePayment.UserId,
                    ScheduledPaymentId = scheduledPayments.SP.Id
                };

                var churchMerchantAccount = Work.ChurchMerchantAccount.GetByChurchId(schedulePayment.ChurchId);

                ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(churchMerchantAccount.ApiUsername, churchMerchantAccount.ApiPassword);

                if (!string.IsNullOrEmpty(model.DonorGUID) && !string.IsNullOrEmpty(model.AccountGUID) && !string.IsNullOrEmpty(schedulePayment.FundId))
                {
                    var transactionResponse = new TransactionResponse();

                    if (model.AccountType == DigitalPaymentMethods.Card)
                    {
                        var cardTransactionRequest = new CardTransactionRequest
                        {
                            tokenized_card = new TokenizedCard
                            {
                                card_info_key = model.AccountGUID,
                                amount = model.Amount,
                                transaction_type = TransactionTypeShortCode.Auth
                            }
                        };

                        transactionResponse = await nuveiHelper.ProcessCreditCardTransactionAsync(cardTransactionRequest, apiCredentials);

                        model.Payment.PaymentStatus = Responses.GetApiTransactionResponse(transactionResponse?.result);
                        model.Payment.AccountScheduleGUID = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : string.Empty;
                        var result = Work.Payment.Create(model.Payment);
                        model.Payment = result.Data;

                        SendPaymentStatusEmail(model, transactionResponse);

                        if (result.ResultType == ResultType.Success)
                        {
                            return true;
                        }
                    }
                    else
                    {
                        var checkTransactionRequest = new CheckTransactionRequest
                        {
                            tokenized_check = new TokenizedCheck
                            {
                                check_info_key = model.AccountGUID,
                                amount = model.Amount,
                                transaction_type = TransactionTypeShortCode.Auth
                            }
                        };

                        transactionResponse = await nuveiHelper.ProcessCheckTransactionAsync(checkTransactionRequest, apiCredentials);

                        model.Payment.PaymentStatus = Responses.GetApiTransactionResponse(transactionResponse?.result);
                        model.Payment.AccountScheduleGUID = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : string.Empty;
                        var result = Work.Payment.Create(model.Payment);
                        model.Payment = result.Data;

                        SendPaymentStatusEmail(model, transactionResponse);

                        if (result.ResultType == ResultType.Success)
                        {
                            return true;
                        }
                    }
                    var obj = logsRepository.JsonConverter("Action Name", "ProcessScheduledPayment", "Controller Name", "ScheduledPaymentJob", "Exception Message", transactionResponse.result_message,
                       "Exception Code", transactionResponse.result_description, "Fund Id", schedulePayment.FundId, "Amount", model.Payment.Amount.ToString(), "AccountGUID", model.AccountGUID, "DonorGUID", model.DonorGUID);
                    logsRepository.LogData("ProcessScheduledPayment", "ScheduledPaymentJob", "Create Transaction", SessionVariables.CurrentUser?.User?.Id ?? string.Empty, LogStatuses.Error, obj, hasSession: false);
                }
            }

            return false;
        }

        static DateTime GetNextChargeDate(DateTime date, string frequency)
        {
            var nextDueDate = new DateTime();

            switch (frequency.Trim())
            {
                case PaymentFrequency.Weekly:
                    nextDueDate = date.AddDays(7);
                    break;
                case PaymentFrequency.Biweekly:
                    nextDueDate = date.AddDays(14);
                    break;
                case PaymentFrequency.Monthly:
                    nextDueDate = date.AddMonths(1);
                    break;
                case PaymentFrequency.FirstAndFifteenthMonthly:
                    if (date.Day < 15 && date.Day >= 1)
                    {
                        nextDueDate = new DateTime(date.Year, date.Month, 15);
                    }
                    else if (date.Day >= 15)
                    {
                        nextDueDate = string.Equals(date.Month.ToFullMonthName(), "DECEMBER", StringComparison.CurrentCultureIgnoreCase) ? new DateTime(date.Year + 1, 1, 1) : new DateTime(date.Year, date.Month + 1, 1);
                    }
                    break;
            }
            return nextDueDate;
        }
        #endregion

        #region Send Annual Giving Statement Email
        public void SendAnnualGivingStatements()
        {
            var churches = Db.Churches.Where(q => q.IsAutoEmail).ToList();
            churches = churches.Where(q => q.StatementGeneratedDate != null && new DateTime(DateTime.Now.Year, Convert.ToDateTime(q.StatementGeneratedDate).Month, Convert.ToDateTime(q.StatementGeneratedDate).Day) == DateTime.Now.Date).ToList();

            if (churches.IsNotNullOrEmpty() && churches.Any())
            {
                var domain = ApplicationCache.Instance.EnvironmentConfiguration.Url;
                var churchIds = churches.Select(c => c.Id).ToList();
                var churchUsers = Db.ChurchUsers.Where(q => churchIds.Contains(q.ChurchId)).ToList();
                var userIds = churchUsers.Select(s => s.UserId).ToList();
                var users = Db.Users.Where(q => userIds.Contains(q.Id)).ToList();
                var payments = Db.Payments.Where(x => x.CreatedDate.Year == DateTime.Now.Year
                     && churchIds.Contains(x.ChurchId)
                     && !string.IsNullOrEmpty(x.PaymentStatus) && x.PaymentStatus.Equals(PaymentStatus.Success)
                     && !string.IsNullOrEmpty(x.TransactionType) && x.TransactionType.Equals(TransactionType.Payment)).ToList();

                foreach (var user in churchUsers)
                {
                    //Skip if user does not have any payments
                    if (payments.All(q => q.UserId != user.UserId))
                    {
                        continue;
                    }

                    var email = users.FirstOrDefault(x => x.Id == user.UserId)?.Email;

                    //check if user has an email
                    if (email.IsNullOrEmpty())
                    {
                        continue;
                    }

                    var currentChurch = churches.FirstOrDefault(x => x.Id.Equals(user.ChurchId));
                    var statementUrl = $"{domain}/MyGiving/Preview?token={Constants.GenerateStatementPreviewToken(user.UserId, user.ChurchId, DateTime.Now.Year)}";

                    var body = currentChurch?.AnnualStatementEmailBody.IsNotNullOrEmpty() == true ? currentChurch.AnnualStatementEmailBody : "Your {current-year} annual giving statement for {church_display} is now available";
                    var content = EmailTemplates.AnnualGivingStatement_body.Replace("{body}", body).Replace("{current-year}", DateTime.Now.Year.ToString()).Replace("{church_display}", currentChurch?.Display).Replace("{view_statment_url}", statementUrl);
                    var emailObj = new Email()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Message = content,
                        To = email,
                        Attachments = null,
                        Subject = $"{DateTime.Now.Year} Annual Giving Statement",
                        CreatedBy = user.UserId,
                        CreatedDate = DateTime.Now,
                        Status = EmailStatus.Queued,
                        Type = "AnnualGivingStatement",
                        TypeId = user.Id
                    };

                    Emailer.SendEmail(emailObj, null, new Domain() { EmailLogo = currentChurch.Logo, Name = currentChurch.Display, EmailReplyAddress = currentChurch.Email, EmailDisplay = currentChurch.Display }, currentChurch, false, currentChurch.Email, currentChurch.Display);
                }
            }
        }
        #endregion

        public void SendPaymentStatusEmail(GivingViewModel model, TransactionResponse responseModel, string donorEmail = null)
        {
            var user = Db.Users.FirstOrDefault(q => q.Id == model.Payment.UserId);
            var church = Db.Churches.FirstOrDefault(q => q.Id == model.Payment.ChurchId);
            var fund = Db.Funds.FirstOrDefault(q => q.Id == model.Payment.FundId && !q.IsDeleted);
            var campus = Db.Campuses.FirstOrDefault(x => x.Id == model.Payment.CampusId);
            var paymentMethodAccount = Work.PaymentMethodAccount.GetByAccountGUID(model.AccountGUID);
            var paymentMethod = paymentMethodAccount.IsNotNullOrEmpty() ? paymentMethodAccount.PaymentMethodPreview : string.Empty;
            string content = string.Empty;
            string subject = string.Empty;
            string churchDisplay = church.IsNotNullOrEmpty() ? $" to {church.Display}" : string.Empty;

            //Send payment success email
            if (responseModel.result == Constants.ApiTransactionSuccessCode)
            {
                var transactionId = model.Payment.TransactionId;
                subject = "Receipt for your gift" + churchDisplay + (transactionId.IsNotNullOrEmpty() ? " (" + transactionId + ")" : string.Empty);
                content = EmailTemplates.PaymentProcessed_body.Replace("{amount}", model.Payment.Amount.ToCurrencyString())
                    .Replace("{gift_datetime}", DateTime.Now.ToShortDateString())
                    .Replace("{user_display}", user != null ? user.FirstName : string.Empty)
                    .Replace("{church_display}", church != null ? church.Display : string.Empty)
                    .Replace("{paymentmethod}", !string.IsNullOrEmpty(paymentMethod) ? paymentMethod : string.Empty)
                    .Replace("{fund_display}", fund != null ? fund.Display : string.Empty)
                    .Replace("{transactionid}", transactionId.IsNotNullOrEmpty() ? transactionId : string.Empty)
                    .Replace("{campus_display}", campus != null ? $"({campus.Display})" : string.Empty)
                    .Replace("{church_thanks_note}", fund.IsNotNullOrEmpty() && fund.GivingThankYouText.IsNotNullOrEmpty() ? fund.GivingThankYouText : church != null ? church.GivingThankYouText : string.Empty);
            }
            //Send payment error email
            else
            {
                var errorMsg = responseModel.result_description;
                subject = "Giving Error - There was a problem processing your gift" + churchDisplay;
                content = EmailTemplates.Payment_Error_body.Replace("{amount}", model.Payment.Amount.ToString())
                    .Replace("{user_display}", user != null ? user.FirstName : string.Empty)
                    .Replace("{church_display}", church != null ? church.Display : string.Empty)
                    .Replace("{paymentmethod}", !string.IsNullOrEmpty(paymentMethod) ? paymentMethod : string.Empty)
                    .Replace("{fund_display}", fund != null ? fund.Display : string.Empty)
                    .Replace("{campus_display}", campus != null ? $"({campus.Display})" : string.Empty)
                    .Replace("{error_message}", !string.IsNullOrEmpty(errorMsg) ? errorMsg : "Unknown error occurred!");
            }

            if ((user != null && !string.IsNullOrEmpty(user.Email)) || !string.IsNullOrEmpty(donorEmail))
            {
                var email = new Email()
                {
                    Id = Utilities.GenerateUniqueId(),
                    Message = content,
                    To = user != null && !string.IsNullOrEmpty(user.Email) ? user.Email : donorEmail,
                    Attachments = null,
                    Subject = subject,
                    CreatedBy = Constants.System,
                    CreatedDate = DateTime.Now
                };

                Emailer.SendEmail(email, null, null, new Domain() { EmailLogo = church.Logo, Name = church.Display, EmailReplyAddress = church.Email, EmailDisplay = church.Display }, church);
            }
        }

        public CardRequest MapToCardRequestModel(PaymentMethodViewModel paymentMethodViewModel)
        {
            return new CardRequest
            {
                customer_key = paymentMethodViewModel.DonorGUID,
                card_number = paymentMethodViewModel.PaymentCard.CcNumber.Replace(" ", string.Empty),
                expiration_date = $"{paymentMethodViewModel.PaymentCard.CcExpMonth}{paymentMethodViewModel.PaymentCard.CcExpYear}",
                name_on_card = paymentMethodViewModel.PaymentCard.CcName,
                street1 = string.Empty, // Add the appropriate property from your PaymentCard model
                zip_code = string.Empty
            };
        }

        public CheckRequest MapToCheckRequestModel(PaymentMethodViewModel paymentMethodViewModel)
        {
            return new CheckRequest
            {
                customer_key = paymentMethodViewModel.DonorGUID,
                account_type = paymentMethodViewModel.PaymentAccount.AccountType,
                account_number = paymentMethodViewModel.PaymentAccount.AccountNumber,
                transit_number = paymentMethodViewModel.PaymentAccount.RoutingNumber,
                secc_type = "WEB",
                check_type = "Personal"
                //auth_option_form = "SinglePaymentSeries",
                //auth_option_voice = "ConsumerInitiatedCall"
            };
        }

        public async Task<CompleteViewModel> CreatePayment(GivingSignUpViewModel model, string accountType, decimal processingFees, string campusName)
        {
            string message = string.Empty;
            var returnObj = new CompleteViewModel();
            var user = model.Phone.IsNotNullOrEmpty() ? Db.Users.FirstOrDefault(x => x.PhoneNumber == model.Phone) : Db.Users.FirstOrDefault(x => x.Email == model.Email);
            var church = Work.Church.Get(model.Church.Id);
            var fund = GetFund(model);
            message = !string.IsNullOrEmpty(fund?.GivingThankYouText) ? fund.GivingThankYouText : church?.GivingThankYouText ?? string.Empty;

            model.Payment.Id = Utilities.GenerateUniqueId();
            model.Payment.ProcessingFee = processingFees;
            model.Payment.DonorPaidMerchantFee = model.IncludeProcessingFee;
            model.Payment.TransactionType = TransactionType.Payment;
            model.Payment.Frequency = PaymentOccurrence.OneTime;
            model.Payment.UserId = user?.Id;
            model.Payment.CreatedBy = user?.Id;
            model.Payment.DigitalPaymentMethod = accountType;
            model.Payment.DigitalPaymentType = DigitalPaymentTypes.Online;

            var givingModel = new GivingViewModel()
            {
                Payment = model.Payment,
                ChurchId = model.Payment.ChurchId,
                DonorGUID = model.DonorGUID,
                AccountGUID = model.Payment.PaymentMethod,
                AccountType = accountType,
                IncludeProcessingFee = model.IncludeProcessingFee,
                Amount = model.Payment.Amount.ToString()
            };

            var churchMerchantAccount = Work.ChurchMerchantAccount.GetByChurchId(model.Church.Id);

            ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(churchMerchantAccount.ApiUsername, churchMerchantAccount.ApiPassword);

            var transactionResponse = new TransactionResponse();

            if (givingModel.AccountType == DigitalPaymentMethods.Card)
            {
                var cardTransactionRequest = new CardTransactionRequest
                {
                    tokenized_card = new TokenizedCard
                    {
                        card_info_key = givingModel.AccountGUID,
                        amount = givingModel.Amount,
                        transaction_type = TransactionTypeShortCode.Auth
                    }
                };

                transactionResponse = await nuveiHelper.ProcessCreditCardTransactionAsync(cardTransactionRequest, apiCredentials);

                model.Payment.PaymentStatus = Responses.GetApiTransactionResponse(transactionResponse?.result);

                if (model.Payment.PaymentStatus != APIStatuses.Success)
                {
                    string apiErrorMessage = Responses.HandleApiTransactionFailure(transactionResponse);
                    message = $"There was a problem submitting your donation. Please try again with a different payment method. Thank you. (Error: {apiErrorMessage})";
                    return new CompleteViewModel() { Success = false, Message = message };
                }

                model.Payment.AccountScheduleGUID = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : string.Empty;
                var result = Work.Payment.Create(model.Payment);
                givingModel.Payment = result.Data;

                SendPaymentStatusEmail(givingModel, transactionResponse);

                if (result.ResultType == ResultType.Success)
                {
                    returnObj.Church = model.Church;
                    returnObj.FundId = model.Payment.FundId;
                    returnObj.CampusName = campusName;
                    returnObj.Guest = false;
                    returnObj.PaymentAmount = model.Payment.Amount.ToString();
                    returnObj.PaymentOccurrence = PaymentOccurrence.OneTime;
                    returnObj.Success = true;
                    returnObj.Message = message;
                    returnObj.ProcessingFee = model.IncludeProcessingFee ? processingFees : 0;

                    return returnObj;
                }

                message = Responses.GetApiTransactionResponse(transactionResponse?.result) != APIStatuses.Success ? transactionResponse.result_message : message;
            }
            else
            {
                var checkTransactionRequest = new CheckTransactionRequest
                {
                    tokenized_check = new TokenizedCheck
                    {
                        check_info_key = givingModel.AccountGUID,
                        amount = givingModel.Amount,
                        transaction_type = TransactionTypeShortCode.Auth
                    }
                };

                transactionResponse = await nuveiHelper.ProcessCheckTransactionAsync(checkTransactionRequest, apiCredentials);

                model.Payment.PaymentStatus = Responses.GetApiTransactionResponse(transactionResponse?.result);

                if (model.Payment.PaymentStatus != APIStatuses.Success)
                {
                    string apiErrorMessage = Responses.HandleApiTransactionFailure(transactionResponse);
                    message = $"There was a problem submitting your donation. Please try again with a different payment method. Thank you. (Error: {apiErrorMessage})";
                    return new CompleteViewModel() { Success = false, Message = message };
                }

                model.Payment.AccountScheduleGUID = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : null;
                var result = Work.Payment.Create(model.Payment);
                givingModel.Payment = result.Data;

                SendPaymentStatusEmail(givingModel, transactionResponse);

                if (result.ResultType == ResultType.Success)
                {
                    returnObj.Church = model.Church;
                    returnObj.FundId = model.Payment.FundId;
                    returnObj.CampusName = campusName;
                    returnObj.Guest = false;
                    returnObj.PaymentAmount = model.Payment.Amount.ToString();
                    returnObj.PaymentOccurrence = PaymentOccurrence.OneTime;
                    returnObj.Success = true;
                    returnObj.Message = message;
                    returnObj.ProcessingFee = model.IncludeProcessingFee ? processingFees : 0;

                    return returnObj;
                }

                message = Responses.GetApiTransactionResponse(transactionResponse?.result) != APIStatuses.Success ? transactionResponse.result_message : message;
            }

            return new CompleteViewModel() { Success = false, Message = message };
        }

        public CompleteViewModel CreateRecurringPayment(GivingSignUpViewModel model, decimal processingFees, string campusName)
        {
            var user = model.Phone.IsNotNullOrEmpty() ? Work.User.GetByPhone(model.Phone) : Work.User.GetByEmail(model.Email);
            var church = Work.Church.Get(model.Church.Id);
            var message = !string.IsNullOrEmpty(church?.GivingThankYouText) ? church.GivingThankYouText : string.Empty;
            var returnObj = new CompleteViewModel();

            SetRecurringPaymentDetails(model, user?.Id);
            SetGiftEndingReasonDetails(model.ScheduledPayment);

            var result = Work.ScheduledPayment.Create(model.ScheduledPayment);

            if (result.ResultType == ResultType.Success)
            {
                returnObj.Church = model.Church;
                returnObj.FundId = model.Payment.FundId;
                returnObj.CampusName = campusName;
                returnObj.Guest = false;
                returnObj.PaymentAmount = model.Payment.Amount.ToString();
                returnObj.PaymentOccurrence = PaymentOccurrence.Recurring;
                returnObj.Success = true;
                returnObj.Message = message;
                returnObj.ProcessingFee = model.IncludeProcessingFee ? processingFees : 0;
                return returnObj;
            }

            message = result.Message;

            return new CompleteViewModel() { Success = false, Message = message };
        }

        private static void SetRecurringPaymentDetails(GivingSignUpViewModel model, string userId)
        {
            model.ScheduledPayment.Id = Utilities.GenerateUniqueId();
            model.ScheduledPayment.UserId = userId;
            model.ScheduledPayment.IncludeProcessingFee = model.IncludeProcessingFee;
            model.ScheduledPayment.Amount = model.Payment.Amount;
            model.ScheduledPayment.Frequency = PaymentOccurrence.Recurring;
            model.ScheduledPayment.CampusId = model.Payment.CampusId;
            model.ScheduledPayment.FundId = model.Payment.FundId;
            model.ScheduledPayment.ChurchId = model.Church?.Id; // Assuming ChurchId is a property in ScheduledPayment
            model.ScheduledPayment.PaymentMethod = model.Payment.PaymentMethod;
        }

        private static void SetGiftEndingReasonDetails(ScheduledPayment payment)
        {
            switch (payment.GiftEndingReason)
            {
                case GiftEndingReasons.WhenICancelIt:
                    payment.RecurringEndDate = null;
                    payment.MaxGifts = null;
                    break;
                case GiftEndingReasons.OnASpecificDate:
                    payment.MaxGifts = null;
                    break;
                case GiftEndingReasons.AfterMaxNumberofGifts:
                    payment.RecurringEndDate = null;
                    break;
            }
        }

        private Fund GetFund(GivingSignUpViewModel model)
        {
            var fund = Work.Fund.Get(model.Payment.FundId);

            if (fund.IsNotNullOrEmpty() && !fund.Closed)
            {
                return fund;
            }

            return Work.Fund.GetByName(model.Church.Id, GivingFunds.General);
        }

        public LeadApiRequest MapToLeadRequestModel(ChurchMerchantAccountVM model)
        {
            const string appTemplateId = "a0R0x0000029lhoEAA";
            const string terminalTemplateIds = "a140x00000TcN00";
            //const string achTemplateId = "a0RDW000002Jw1K2AS";

            return new LeadApiRequest
            {
                correlation_id = model.CorrelationId,
                application_template_id = appTemplateId,
                terminal_template_ids = terminalTemplateIds,
                //ach_template_id = achTemplateId,
                company = new Company
                {
                    company_legal_name = !string.IsNullOrEmpty(model.Church.LegalName) ? model.Church.LegalName : string.Empty,
                    company_dba_name = !string.IsNullOrEmpty(model.Church.LegalName) ? model.Church.LegalName : string.Empty,
                    company_ownership_type = !string.IsNullOrEmpty(model.Account.BusinessType) ? model.Account.BusinessType : string.Empty,
                    company_federal_tax_id = !string.IsNullOrEmpty(model.Church.TaxIdNumber) ? model.Church.TaxIdNumber.Replace("-", string.Empty) : string.Empty,
                    legal_address = new LegalAddress
                    {
                        address = !string.IsNullOrEmpty(model.Church.PhysicalAddress1) ? model.Church.PhysicalAddress1 : string.Empty,
                        postal_code = !string.IsNullOrEmpty(model.Church.PhysicalZip) ? model.Church.PhysicalZip : string.Empty,
                        city = !string.IsNullOrEmpty(model.Church.PhysicalCity) ? model.Church.PhysicalCity : string.Empty,
                        state = !string.IsNullOrEmpty(model.Church.PhysicalState) ? model.Church.PhysicalState : string.Empty,
                    }
                },
                owner_Principal = new OwnerPrincipal
                {
                    first_name = !string.IsNullOrEmpty(model.Account.RespContactFirstName) ? model.Account.RespContactFirstName : string.Empty,
                    middle_name = string.Empty,
                    last_name = !string.IsNullOrEmpty(model.Account.RespContactLastName) ? model.Account.RespContactLastName : string.Empty,
                    email_address = !string.IsNullOrEmpty(model.Account.RespContactEmail) ? model.Account.RespContactEmail : string.Empty,
                    phone = !string.IsNullOrEmpty(model.Account.RespContactPhone) ? model.Account.RespContactPhone.PhoneFriendly() : string.Empty,
                    date_of_birth = !string.IsNullOrEmpty(Convert.ToDateTime(model.Account.RespContactDOB).ToShortDateString()) ? Convert.ToDateTime(model.Account.RespContactDOB).ToShortDateString() : string.Empty,
                    social_security_number = !string.IsNullOrEmpty(model.Account.RespContactSSN) ? model.Account.RespContactSSN.Replace("-", string.Empty) : string.Empty,
                    drivers_license_number = !string.IsNullOrEmpty(model.Account.RespContactDLN) ? model.Account.RespContactDLN : string.Empty,
                },
                bank = new Bank
                {
                    bank_routing_number = !string.IsNullOrEmpty(model.Account.RoutingNumber) ? model.Account.RoutingNumber : string.Empty,
                    bank_account_number = !string.IsNullOrEmpty(model.Account.AccountNumber) ? model.Account.AccountNumber : string.Empty,
                }
            };
        }
    }
}