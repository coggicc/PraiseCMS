using PraiseCMS.API.Helpers;
using PraiseCMS.API.Models;
using PraiseCMS.API.Services;
using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static PraiseCMS.Shared.Methods.ExtensionMethods;

namespace PraiseCMS.BusinessLayer
{
    public class SubscriptionOperations : GenericRepository
    {
        public TokenManager tokenManager;
        public NuveiHelper nuveiHelper;

        public SubscriptionOperations(ApplicationDbContext db, Work work) : base(db, work)
        {
            tokenManager = new TokenManager();
            nuveiHelper = new NuveiHelper(tokenManager);
        }

        public List<Subscription> GetAll(string churchId)
        {
            return Read<Subscription>().Where(x => x.ChurchId == churchId).ToList();
        }

        public List<Subscription> GetAll()
        {
            return Read<Subscription>().ToList();
        }

        public Subscription GetActiveSubscription(string churchId)
        {
            return Read<Subscription>().FirstOrDefault(x => x.ChurchId == churchId && x.IsActive);
        }

        public List<Subscription> GetActiveSubscriptionsByChurchIdList(List<string> churchIds)
        {
            return Read<Subscription>()
                .Where(x => churchIds.Contains(x.ChurchId) && x.IsActive)
                .ToList();
        }

        public Subscription GetNextSubscription(string churchId)
        {
            return Read<Subscription>().FirstOrDefault(x => x.ChurchId == churchId && !x.IsActive && x.StartDate == null && x.EndDate == null);
        }

        public Result<Subscription> Create(Subscription entity)
        {
            try
            {
                Create<Subscription>(entity);
                SaveChanges();

                //set subscription in session while creating new one
                SessionVariables.SetSubscriptions(entity.ChurchId);

                return new Result<Subscription>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Subscription>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Subscription> Update(Subscription entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = Constants.System;

                Update<Subscription>(entity);
                SaveChanges();
                return new Result<Subscription>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Subscription>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public SubscriptionType GetSubscriptionTypeByName(string name)
        {
            return Read<SubscriptionType>().FirstOrDefault(x => x.Name == name);
        }

        public IEnumerable<SubscriptionType> GetAllSubscriptionTypes()
        {
            return Read<SubscriptionType>().Where(x => x.IsActive).ToList();
        }

        public SubscriptionType GetSubscriptionType(string id)
        {
            return Read<SubscriptionType>().FirstOrDefault(x => x.Id == id);
        }

        public Result<SubscriptionTransaction> CreateTransaction(SubscriptionTransaction entity)
        {
            try
            {
                entity.TransactionId = Utilities.GenerateUniqueId(15);
                Create(entity);
                SaveChanges();

                return new Result<SubscriptionTransaction>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<SubscriptionTransaction>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public IEnumerable<SubscriptionTransaction> TransactionBySubscriptionIds(IEnumerable<string> subscriptionIds)
        {
            return Read<SubscriptionTransaction>().Where(x => subscriptionIds.Contains(x.SubscriptionId)).ToList();
        }

        public IEnumerable<BillingActivityVM> SubscriptionsWithTransactions(string churchId)
        {
            var church = Work.Church.Get(id: churchId);
            var allSubscriptions = GetAll(churchId).Where(q => q.StartDate.IsNotNullOrEmpty()).OrderByDescending(q => q.CreatedDate).ToList();
            var paymentMethods = Work.PaymentMethodAccount.GetAll(church.DonorGUID);
            var allTransactions = TransactionBySubscriptionIds(allSubscriptions.Select(q => q.Id)).ToList();
            allTransactions = allTransactions.Select(d => { d.PaymentMethod = paymentMethods.Any(q => q.AccountGUID.Equals(d.PaymentMethod)) ? paymentMethods.FirstOrDefault(q => q.AccountGUID.Equals(d.PaymentMethod))?.PaymentMethodPreview : string.Empty; return d; }).ToList();
            var result = allSubscriptions.Select(item => new BillingActivityVM() { Subscription = item, Transactions = allTransactions.Where(q => q.SubscriptionId.Equals(item.Id)).OrderByDescending(q => q.TransactionDate) }).ToList();

            return result.OrderByDescending(q => q.Subscription.CreatedDate);
        }

        public SubscriptionTransactionVM TransactionInvoice(Church church, string id)
        {
            var model = new SubscriptionTransactionVM()
            {
                Transaction = Read<SubscriptionTransaction>().FirstOrDefault(q => q.Id.Equals(id)),
                Church = church
            };
            var paymentMethod = Work.PaymentMethodAccount.GetByAccountGUID(model.Transaction.PaymentMethod);
            model.Transaction.PaymentMethod = paymentMethod.IsNotNullOrEmpty() ? paymentMethod.PaymentMethodPreview : "Not Available";

            return model;
        }

        public void SendExceptionEmail(Church church, string exception)
        {
            var churchName = !string.IsNullOrEmpty(church.Name) ? church.Name : church.LegalName;

            var obj = logsRepository.JsonConverter("Exception Message", exception,
                       "Church", churchName, "Phone", church.Phone, "Email", church.Email, "Date", DateTime.Now.ToShortDateAndTimeString());
            logsRepository.LogData("RenewSubscriptions", "AutoRenewSubscriptionJob", "Error renewing church subscription", church.Id, LogStatuses.Error, obj, hasSession: false);

            var message = $"<b>Church:</b> {churchName}<br><b>Phone:</b> {church.Phone}<br><b>Email:</b> {church.Email}<br><b>Date:</b> {DateTime.Now.ToShortDateAndTimeString()}<br><b>Error:</b> {exception}";

            var email = new Email()
            {
                Id = Utilities.GenerateUniqueId(),
                Message = message,
                To = "SupportEmail".AppSetting("info@praisecms.com"),
                Attachments = null,
                Subject = "Error - Church Subscription Renewal",
                CreatedBy = Constants.System,
                CreatedDate = DateTime.Now
            };
            Emailer.SendEmail(email);
        }

        public void SendRenewalEmail(Subscription subscription, SubscriptionTransaction transaction, Church church)
        {
            // If there's no PrimaryUserId, skip sending the email
            if (string.IsNullOrEmpty(church.PrimaryUserId))
                return;

            var churchAdmin = Work.User.Get(id: church.PrimaryUserId);

            // If user is not found or has no email, skip sending
            if (churchAdmin == null || string.IsNullOrEmpty(churchAdmin.Email))
                return;

            var content = EmailTemplates.subscription_Renewal_Body
            .Replace("{subscription_period}", subscription.BillingPlan.Equals(BillingType.Monthly) ? "1 Month" : "12 Months")
            .Replace("{subscription_type}", subscription.BillingPlan.Equals(BillingType.Free) ? "Basic" : "Premium")
            .Replace("{monthly_price}", Utilities.GetMonthlySubscriptionAmount().ToCurrencyString())
            .Replace("{annually_price}", Utilities.GetAnnualSubscriptionAmount().ToCurrencyString());

            if (subscription.BillingPlan.Equals(BillingType.Free))
            {
                content = content.Replace("{expiry_date}", string.Empty); // Basic plan does not have an expiration date
            }
            else
            {
                content = content.Replace("{expiry_date}",
                    subscription.EndDate.HasValue
                        ? $"Subscription expires on {subscription.EndDate.Value.ToShortDateString()}"
                        : "Subscription expiration date not available");
            }

            var email = new Email
            {
                Id = Utilities.GenerateUniqueId(),
                Message = content,
                To = churchAdmin.Email,
                Attachments = null,
                Subject = $"Receipt for your Praise CMS subscription renewal{(transaction?.TransactionId != null ? $" ({transaction.TransactionId})" : string.Empty)}",
                CreatedBy = Constants.System,
                CreatedDate = DateTime.Now
            };

            Emailer.SendEmail(email);
        }

        public void SubscriptionCanceledEmail(Subscription subscription, Church church)
        {
            // Skip if no primary user assigned
            if (string.IsNullOrEmpty(church.PrimaryUserId))
                return;

            var churchAdmin = Work.User.Get(id: church.PrimaryUserId);

            // Skip if admin not found or doesn't have an email
            if (churchAdmin == null || string.IsNullOrEmpty(churchAdmin.Email))
                return;

            var content = EmailTemplates.SubscriptionCancelled_Body
                .Replace("{subscription_period}", subscription.BillingPlan.Equals(BillingType.Monthly) ? "1 Month" : "12 Months")
                .Replace("{church_name}", church.Display)
                .Replace("{renew_url}", "{site-url}" + $"/settings/changeplan/{church.Id}")
                .Replace("{subscription_type}", subscription.BillingPlan.Equals(BillingType.Free) ? "Basic" : "Premium")
                .Replace("{monthly_price}", Utilities.GetMonthlySubscriptionAmount().ToCurrencyString())
                .Replace("{annually_price}", Utilities.GetAnnualSubscriptionAmount().ToCurrencyString());

            if (subscription.BillingPlan.Equals(BillingType.Free))
            {
                content = content.Replace("{expiry_date}", string.Empty);
            }
            else
            {
                content = content.Replace("{expiry_date}",
                    subscription.EndDate.HasValue
                        ? $"Subscription expires on {subscription.EndDate.Value.ToShortDateString()}"
                        : "Subscription expiration date not available");
            }

            var email = new Email
            {
                Id = Utilities.GenerateUniqueId(),
                Message = content,
                To = churchAdmin.Email,
                Attachments = null,
                Subject = "Your Praise CMS subscription has been canceled.",
                CreatedBy = Constants.System,
                CreatedDate = DateTime.Now
            };

            Emailer.SendEmail(email);
        }

        public void FreeTrialEmail(string message, string subject, Church church)
        {
            if (message.IsNotNullOrEmpty() && church.IsNotNullOrEmpty() && church.PrimaryUserId.IsNotNullOrEmpty())
            {
                var churchAdmin = Work.User.Get(id: church.PrimaryUserId);
                var content = EmailTemplates.General_With_Heading.Replace("{message}", message)
                     .Replace("{user_display}", churchAdmin.IsNotNullOrEmpty() ? churchAdmin.FirstName : Constants.User);

                var email = new Email()
                {
                    Id = Utilities.GenerateUniqueId(),
                    Message = content,
                    To = churchAdmin.Email,
                    Attachments = null,
                    Subject = subject,
                    CreatedBy = Constants.System,
                    CreatedDate = DateTime.Now
                };

                if (churchAdmin.IsNotNullOrEmpty() && churchAdmin.Email.IsNotNullOrEmpty())
                {
                    Emailer.SendEmail(email);
                }
            }
        }

        #region Auto Renew Subscription
        public async Task<Result<SubscriptionTransaction>> CreateTransaction(Subscription model)
        {
            try
            {
                var givingVM = new GivingViewModel();
                var transaction = new SubscriptionTransaction();
                var returnModel = new Result<SubscriptionTransaction>();
                var church = Work.Church.Get(id: model.ChurchId);
                var paymentMethods = Work.Payment.GetPaymentMethodsDropdownList(church.DonorGUID);
                var praiseMerchantAccount = Work.ChurchMerchantAccount.GetPraiseChurchAccount();

                //API credentials need to be for Praise Church since this is for the church paying Praise
                ApiCredentials apiCredentials = nuveiHelper.GetApiCredentials(praiseMerchantAccount.ApiUsername, praiseMerchantAccount.ApiPassword);

                if (praiseMerchantAccount.IsNotNullOrEmpty())
                {
                    var fund = Work.Fund.GetByName(praiseMerchantAccount.ChurchId, PraiseFunds.Subscriptions);

                    if (paymentMethods.Any(q => q.IsPrimary))
                    {
                        givingVM = new GivingViewModel()
                        {
                            //Should pass Praise churchId because for subscription charges we need to use the credentials of Praise church merchant account
                            ChurchId = praiseMerchantAccount.ChurchId,
                            DonorGUID = church.DonorGUID,
                            AccountGUID = paymentMethods.FirstOrDefault(q => q.IsPrimary)?.Value,
                        };

                        switch (model.BillingPlan)
                        {
                            case BillingType.Monthly:
                                givingVM.Payment = new Payment() { Amount = Utilities.GetMonthlySubscriptionAmount() };
                                break;
                            case BillingType.Annually:
                                givingVM.Payment = new Payment() { Amount = "AnnualSubscriptionFee".AppSetting(0m) };
                                break;
                        }

                        var paymentAccount = Work.PaymentMethodAccount.GetByAccountGUID(givingVM.AccountGUID);

                        if (paymentAccount.IsNotNullOrEmpty())
                        {
                            if (paymentAccount.AccountType == DigitalPaymentMethods.Card)
                            {
                                givingVM.Payment.Amount += (givingVM.Payment.Amount * 2 / 100) + (decimal)0.25;
                            }
                            else if (paymentAccount.AccountType == DigitalPaymentMethods.ACH)
                            {
                                givingVM.Payment.Amount += (decimal)0.25;
                            }
                        }

                        var transactionResponse = new TransactionResponse();

                        if (paymentAccount.AccountType == DigitalPaymentMethods.Card)
                        {
                            var cardTransactionRequest = new CardTransactionRequest
                            {
                                tokenized_card = new TokenizedCard
                                {
                                    card_info_key = givingVM.AccountGUID,
                                    amount = givingVM.Payment.Amount.ToString(),
                                    transaction_type = TransactionTypeShortCode.Auth
                                }
                            };

                            transactionResponse = await nuveiHelper.ProcessCreditCardTransactionAsync(cardTransactionRequest, apiCredentials);

                            givingVM.Payment.PaymentStatus = Responses.GetApiTransactionResponse(transactionResponse?.result);
                            givingVM.Payment.AccountScheduleGUID = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : string.Empty;

                            var result = Work.Payment.Create(givingVM.Payment);

                            givingVM.Payment = result.Data;

                            Work.Giving.SendPaymentStatusEmail(givingVM, transactionResponse);

                            if (givingVM.Payment.PaymentStatus != APIStatuses.Success)
                            {
                                var obj = logsRepository.JsonConverter("Exception Message", transactionResponse.result_message, "Exception Code", transactionResponse.result_description,
                                    "Fund Id", fund.Id, "Amount", givingVM.Payment.Amount.ToCurrencyString(), "AccountGUID", givingVM.AccountGUID, "DonorGUID", givingVM.DonorGUID);
                                logsRepository.LogData("CreateTransaction", "AutoRenewSubscription", "Create Transaction for Church Subscription", model.ChurchId, LogStatuses.Error, obj, hasSession: false);
                            }

                            return new Result<SubscriptionTransaction>
                            {
                                Data = transaction,
                                ResultType = ResultType.Failure,
                                Message = transactionResponse.result_message
                            };
                        }
                        else
                        {
                            var checkTransactionRequest = new CheckTransactionRequest
                            {
                                tokenized_check = new TokenizedCheck
                                {
                                    check_info_key = givingVM.AccountGUID,
                                    amount = givingVM.Payment.Amount.ToString(),
                                    transaction_type = TransactionTypeShortCode.Auth
                                }
                            };

                            transactionResponse = await nuveiHelper.ProcessCheckTransactionAsync(checkTransactionRequest, apiCredentials);

                            givingVM.Payment.PaymentStatus = Responses.GetApiTransactionResponse(transactionResponse?.result);
                            givingVM.Payment.AccountScheduleGUID = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : string.Empty;
                            var result = Work.Payment.Create(givingVM.Payment);
                            givingVM.Payment = result.Data;

                            Work.Giving.SendPaymentStatusEmail(givingVM, transactionResponse);

                            if (givingVM.Payment.PaymentStatus != APIStatuses.Success)
                            {
                                var obj = logsRepository.JsonConverter("Exception Message", transactionResponse.result_message, "Exception Code", transactionResponse.result_description,
                                    "Fund Id", fund.Id, "Amount", givingVM.Payment.Amount.ToCurrencyString(), "AccountGUID", givingVM.AccountGUID, "DonorGUID", givingVM.DonorGUID);
                                logsRepository.LogData("CreateTransaction", "AutoRenewSubscription", "Create Transaction for Church Subscription", model.ChurchId, LogStatuses.Error, obj, hasSession: false);

                                return new Result<SubscriptionTransaction>
                                {
                                    Data = transaction,
                                    ResultType = ResultType.Failure,
                                    Message = transactionResponse.result_message
                                };
                            }
                        }

                        transaction = new SubscriptionTransaction()
                        {
                            Id = Utilities.GenerateUniqueId(),
                            Amount = givingVM.Payment.Amount,
                            BillingType = model.BillingPlan,
                            CreatedBy = Constants.System,
                            CreatedDate = DateTime.Now,
                            PaymentMethod = givingVM.AccountGUID,
                            SubscriptionId = model.Id,
                            TransactionDate = DateTime.Now,
                            AccountScheduleGUID = !string.IsNullOrEmpty(transactionResponse.payment_reference_number) ? transactionResponse.payment_reference_number : string.Empty
                        };

                        Work.Subscription.CreateTransaction(transaction);

                        return new Result<SubscriptionTransaction>
                        {
                            Data = transaction,
                            ResultType = ResultType.Success
                        };
                    }

                    returnModel.Message = $"{Constants.DefaultErrorMessage} No primary payment method found.";
                    returnModel.ResultType = ResultType.Failure;

                    return returnModel;
                }

                return new Result<SubscriptionTransaction>
                {
                    Data = transaction,
                    Message = $"{Constants.DefaultErrorMessage} No merchant account was found for Praise.",
                    ResultType = ResultType.Failure,
                };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task RenewSubscriptions()
        {
            var churches = Work.Church.GetAllActive().OrderByDescending(q => q.CreatedDate).ToList();
            var allSubscription = GetAll();
            var subscriptionTypes = GetAllSubscriptionTypes();
            var freePlan = subscriptionTypes.FirstOrDefault(q => q.Name.EqualsIgnoreCase(PlanType.Free));
            var standardPlan = subscriptionTypes.FirstOrDefault(q => q.Name.EqualsIgnoreCase(PlanType.Premium));

            foreach (var church in churches)
            {
                try
                {
                    var churchSubscriptions = allSubscription.Where(q => q.ChurchId.Equals(church.Id)).ToList();

                    //If the church has an active plan and expired yesterday or before
                    if (churchSubscriptions.Any(q => q.IsActive && q.EndDate.IsNotNullOrEmpty() && Convert.ToDateTime(q.EndDate).Date < DateTime.Now.Date))
                    {
                        //if user changed the plan, deactivate the current and start a new plan
                        if (churchSubscriptions.Any(q => q.StartDate.IsNullOrEmpty() && q.EndDate.IsNullOrEmpty() && !q.IsActive))
                        {
                            //Inactive the previously activated plan
                            var activePlan = churchSubscriptions.FirstOrDefault(q => q.IsActive);

                            if (activePlan.IsNotNullOrEmpty())
                            {
                                activePlan.IsActive = false;
                            }

                            Update(activePlan);

                            //Got new plan selected by the user and started operation on this
                            var newPlan = churchSubscriptions.FirstOrDefault(q => q.StartDate.IsNullOrEmpty() && q.EndDate.IsNullOrEmpty() && !q.IsActive);

                            //if user selected the free plan, no transaction is needed and free plan starts
                            if (newPlan.BillingPlan.Equals(BillingType.Free))
                            {
                                newPlan.IsActive = true;
                                newPlan.StartDate = DateTime.Now;
                                newPlan.ModifiedDate = DateTime.Now;
                                newPlan.ModifiedBy = Constants.System;
                                FreeTrialEmail(SubscriptionNotificationMessages.SubscriptionExpired, SubscriptionNotificationSubjects.SubscriptionExpired, church);
                            }

                            //if user selected the premium plan, create transaction and start plan based on user selection (Monthly/Annually)
                            else
                            {
                                var result = await CreateTransaction(newPlan);
                                newPlan.StartDate = DateTime.Now;
                                newPlan.EndDate = newPlan.BillingPlan.EqualsIgnoreCase(BillingType.Monthly) ? DateTime.Now.AddMonths(1).AddDays(-1) : DateTime.Now.AddYears(1).AddDays(-1);
                                newPlan.IsActive = true;
                                newPlan.ModifiedDate = DateTime.Now;
                                newPlan.ModifiedBy = Constants.System;

                                if (result.ResultType == ResultType.Failure)
                                {
                                    newPlan.PlanTypeId = freePlan.Id;
                                    newPlan.BillingPlan = BillingType.Free;
                                    newPlan.EndDate = null;
                                    FreeTrialEmail(SubscriptionNotificationMessages.SubscriptionExpired, SubscriptionNotificationSubjects.SubscriptionExpired, church);
                                    SendExceptionEmail(church, result.Message);
                                }
                                else
                                {
                                    SendRenewalEmail(newPlan, result.Data, church);
                                }
                            }
                            Update(newPlan);
                        }

                        //if the plan is unchanged, then renew the plan and extend the expiration date (should not free trial)
                        else if (churchSubscriptions.Any(q => q.IsActive && !q.FreeTrial && q.EndDate.IsNotNullOrEmpty() && Convert.ToDateTime(q.EndDate).Date < DateTime.Now.Date))
                        {
                            var currentPlan = churchSubscriptions.FirstOrDefault(q => q.IsActive);
                            var result = await CreateTransaction(currentPlan);

                            //If transaction failed, convert the premium plan into free plan with a null EndDate because the free plan doesn't have expiration date.
                            if (result.ResultType.Equals(ResultType.Failure))
                            {
                                currentPlan.IsActive = false;
                                Update(currentPlan);

                                var subscription = new Subscription()
                                {
                                    Id = Utilities.GenerateUniqueId(),
                                    ChurchId = church.Id,
                                    PlanTypeId = freePlan.Id,
                                    CreatedBy = Constants.System,
                                    CreatedDate = DateTime.Now,
                                    FreeTrial = false,
                                    StartDate = DateTime.Now,
                                    EndDate = null,
                                    BillingPlan = BillingType.Free,
                                    IsActive = true
                                };
                                Work.Subscription.Create(subscription);
                                FreeTrialEmail(SubscriptionNotificationMessages.SubscriptionExpired, SubscriptionNotificationSubjects.SubscriptionExpired, church);
                                SendExceptionEmail(church, result.Message);
                            }
                            else
                            {
                                var extendedDate = currentPlan.BillingPlan.EqualsIgnoreCase(BillingType.Monthly) ? Convert.ToDateTime(currentPlan.EndDate).Date.AddMonths(1).AddDays(-1) : Convert.ToDateTime(currentPlan.EndDate).Date.AddYears(1).AddDays(-1);
                                currentPlan.IsActive = true;
                                currentPlan.EndDate = extendedDate;
                                Update(currentPlan);
                                SendRenewalEmail(currentPlan, result.Data, church);
                            }
                        }

                        //Free trial and expired yesterday
                        else if (churchSubscriptions.Any(q => q.IsActive && q.FreeTrial && q.EndDate.IsNotNullOrEmpty() && Convert.ToDateTime(q.EndDate).Date < DateTime.Now.Date))
                        {
                            var currentPlan = churchSubscriptions.FirstOrDefault(q => q.IsActive);
                            currentPlan.IsActive = false;
                            Update(currentPlan);

                            var newPlan = new Subscription()
                            {
                                Id = Utilities.GenerateUniqueId(),
                                ChurchId = church.Id,
                                PlanTypeId = standardPlan.Id,
                                CreatedBy = Constants.System,
                                CreatedDate = DateTime.Now,
                                FreeTrial = false,
                                StartDate = DateTime.Now,
                                EndDate = DateTime.Now.AddMonths(1).AddDays(-1),
                                BillingPlan = BillingType.Monthly,
                                IsActive = true
                            };
                            Work.Subscription.Create(newPlan);

                            var result = await CreateTransaction(newPlan);

                            //If transaction failed, convert the premium plan into free plan with a null EndDate because the free plan doesn't have expiration date.
                            if (result.ResultType.Equals(ResultType.Failure))
                            {
                                newPlan.PlanTypeId = freePlan.Id;
                                newPlan.EndDate = null;
                                newPlan.BillingPlan = BillingType.Free;
                                Work.Subscription.Update(newPlan);
                                SendExceptionEmail(church, result.Message);
                                FreeTrialEmail(SubscriptionNotificationMessages.FreeTrialExpired, SubscriptionNotificationSubjects.FreeTrialExpired, church);
                            }
                            else
                            {
                                SendRenewalEmail(currentPlan, result.Data, church);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    SendExceptionEmail(church, ex.Message);
                }
            }
        }

        public void SendReminderEmail()
        {
            const string emailBody = SubscriptionNotificationMessages.FreeTrialExpiringSoon;
            const string subject = SubscriptionNotificationSubjects.FreeTrialExpiringSoon;

            var churches = Db.Churches.Where(q => q.IsActive).ToList();
            var allSubscriptions = Db.Subscription
                .Where(q => q.IsActive && q.FreeTrial)
                .ToList();

            // Map days to their corresponding reminder days
            var dayMappings = new Dictionary<int, string>
            {
                { 16, "14" },
                { 23, "7" },
                { 27, "3" }
            };

            foreach (var subscription in allSubscriptions)
            {
                var startDate = Convert.ToDateTime(subscription.StartDate);
                var daysElapsed = (DateTime.Today - startDate.Date).TotalDays;

                if (dayMappings.TryGetValue(Convert.ToInt32(daysElapsed), out var dayCount))
                {
                    var church = churches.FirstOrDefault(q => q.Id.Equals(subscription.ChurchId));

                    if (church != null)
                    {
                        FreeTrialEmail(emailBody.Replace("{dayCount}", dayCount), subject, church);
                    }
                }
            }
        }
        #endregion
    }
}