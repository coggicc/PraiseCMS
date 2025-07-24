using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Interfaces;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using Constants = PraiseCMS.Shared.Shared.Constants;

namespace PraiseCMS.BusinessLayer
{
    public class ReportOperations : GenericRepository
    {
        public ReportOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public Report Get(string id)
        {
            return Read<Report>().FirstOrDefault(x => x.Id == id);
        }

        public string GetName(string id)
        {
            return Read<Report>().Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault();
        }

        public Report GetWithDates(string id)
        {
            return Read<Report>().FirstOrDefault(x => x.Id == id && x.StartDate != null && x.EndDate != null);
        }

        public ReportListView GetByUserId(string userId, string churchId)
        {
            var model = new ReportListView
            {
                Reports = Read<Report>().OrderBy(x => x.Name).ToList() ?? new List<Report>(),
                ReportCategories = Read<ReportCategory>().ToList() ?? new List<ReportCategory>(),
                FavoriteReports = Read<FavoriteReport>().Where(x => x.UserId == userId).ToList() ?? new List<FavoriteReport>()
            };
            model.Reports.AddRange(GetFixedReportsForChurch(churchId));

            return model;
        }

        public ReportListView GetDashboard(string churchId, string userId, DateRange dateRange)
        {
            var model = new ReportListView
            {
                Reports = Read<Report>().Where(x => x.ChurchId == churchId).OrderBy(x => x.Name).ToList() ?? new List<Report>(),
                ReportCategories = Read<ReportCategory>().ToList() ?? new List<ReportCategory>(),
                FavoriteReports = Read<FavoriteReport>().Where(x => x.UserId == userId).ToList() ?? new List<FavoriteReport>(),
                OfflineGiving = Work.OfflineGiving.GetAll(churchId, null, dateRange),
                Payments = Work.Payment.GetAll(churchId, null, dateRange),
                PrayerRequests = Work.PrayerRequest.GetAll(churchId, dateRange),
                Salvations = Work.Salvation.GetAll(churchId, dateRange),
                SmallGroups = Work.SmallGroup.GetAll(churchId, dateRange),
                ReportGroup = Work.Report.GetAllGroups(userId),
                UserReportGroups = Read<UserReportGroups>().Where(x => x.UserId == userId).ToList()
            };

            //There was a problem with duplicate reports because the fixed reports were already in the
            //reports table and now being added again on line 53 with the add range text.

            //var fixedReports = Work.Report.GetFixedReportsForChurch(churchId);
            //foreach (var item in fixedReports)
            //{
            //    if (!model.Reports.Contains(item))
            //    {
            //        model.Reports.Add(item);
            //    }
            //}

            model.Reports.AddRange(Work.Report.GetFixedReportsForChurch(churchId));

            decimal combinedPaymentsOfflineGivingTotal = 0;

            model.SalvationsTotal = (model.Salvations?.Sum(x => x.Total) ?? 0).ToNumberString();
            model.SmallGroupsTotal = (model.SmallGroups?.Count ?? 0).ToNumberString();
            model.PrayerRequestsTotal = (model.PrayerRequests?.Count ?? 0).ToNumberString();

            combinedPaymentsOfflineGivingTotal = (model.Payments?.Sum(x => x.Amount) ?? 0) +
                          (model.OfflineGiving?.Sum(x => x.Amount) ?? 0);

            model.GivingTotal = combinedPaymentsOfflineGivingTotal.ToCurrencyString();

            return model;
        }

        public List<ReportCategory> GetCurrentReports(string churchId, string userId)
        {
            var modules = Work.Module.GetAll();
            var favoriteReports = Read<FavoriteReport>().Where(x => x.UserId.Equals(userId)).ToList();
            var reports = Read<Report>().Where(x => x.ChurchId.Equals(churchId) && x.ReportUrl != null).ToList();
            reports.AddRange(GetFixedReportsForChurch(churchId));

            foreach (var report in reports)
            {
                report.Favorite = favoriteReports.Any(f => f.ReportId.Equals(report.Id));
                report.ModuleId = modules.FindAll(z => z.Name.EqualsIgnoreCase(report.Name)).Select(s => s.Id).FirstOrDefault();
            }

            var reportCategories = Read<ReportCategory>().ToList();

            foreach (var category in reportCategories)
            {
                category.Reports = reports.Where(r => r.ReportCategoryId.Equals(category.Id)).ToList();
            }

            return reportCategories;
        }

        #region Fixed Reports
        public FixedReport GetFixedReport(string id)
        {
            //Get SYSTEM fixed report
            return Read<FixedReport>().FirstOrDefault(x => x.Id.Equals(id));
        }

        public List<FixedReport> GetFixedReports()
        {
            //Get all SYSTEM fixed reports
            return Read<FixedReport>().OrderBy(x => x.Name).ToList();
        }

        public List<Report> GetFixedReportsForChurch(string churchId)
        {
            var fixedReports = Work.Report.GetFixedReports();
            return fixedReports.Select(x => new Report
            {
                Id = x.Id,
                ReportCategoryId = x.ReportCategoryId,
                ChurchId = churchId,
                Name = x.Name,
                Description = x.Description,
                ReportUrl = x.URL,
                ReportType = ReportTypes.Fixed,
                ReportTypeId = x.Id,
                CreatedDate = x.CreatedDate,
                CreatedBy = x.CreatedBy,
                ModifiedDate = null,
                ModifiedBy = null,
                StartDate = new DateTime(DateTime.Now.Year, 1, 1),
                EndDate = new DateTime(DateTime.Now.Year, 12, 31),
                IsDefaultDateRange = false
            }).ToList();
        }

        //In case there are no fixed reports
        //public List<FixedReport> CreateFixedReportsNewChurch()
        //{
        //    var reports = db.Reports.Where(x => x.ChurchId.Equals("2549461115046617d658544d4e9b6a") && x.ReportType.Equals("Fixed")).ToList();
        //    var fixedReports = new List<FixedReport>();

        //    foreach (var item in reports)
        //    {
        //        var report = new FixedReport
        //        {
        //            Id = Utilities.GenerateUniqueId(),
        //            ReportCategoryId = item.ReportCategoryId,
        //            Name = item.Name,
        //            URL = item.ReportUrl,
        //            Description = item.Description,
        //            CreatedDate = item.CreatedDate,
        //            CreatedBy = item.CreatedBy
        //        };
        //        fixedReports.Add(report);
        //        db.FixedReports.Add(report);
        //    }
        //    db.SaveChanges();

        //    return fixedReports;
        //}
        #endregion

        #region Favorite Reports
        public FavoriteReport GetFavoriteReport(string id)
        {
            return Read<FavoriteReport>().FirstOrDefault(x => x.Id == id);
        }

        public List<FavoriteReport> GetAllFavoriteReports(string userId)
        {
            return Read<FavoriteReport>().Where(x => x.UserId == userId).ToList();
        }

        public Result<bool> MakeFavorite(string id, string userId)
        {
            try
            {
                var favorite = Read<FavoriteReport>().FirstOrDefault(x => x.ReportId == id && x.UserId == userId);

                if (favorite.IsNotNull())
                {
                    Delete(favorite);
                }
                else
                {
                    favorite = new FavoriteReport
                    {
                        Id = Utilities.GenerateUniqueId(),
                        UserId = userId,
                        ReportId = id
                    };
                    Create(favorite);
                }

                SaveChanges();

                return new Result<bool>
                {
                    ResultType = ResultType.Success,
                    Message = "Favorite reports updated."
                };
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException.IsNotNullOrEmpty() ? ex.InnerException?.Message : ex.Message;

                return new Result<bool>
                {
                    ResultType = ResultType.Failure,
                    Message = $"{Constants.DefaultErrorMessage} {msg}"
                };
            }
        }
        #endregion        

        #region Report Settings
        public ReportSettings GetSetting(string id)
        {
            return Read<ReportSettings>().FirstOrDefault(x => x.Id == id);
        }

        public ReportSettings GetSettingByUser(string userId)
        {
            return Read<ReportSettings>().FirstOrDefault(x => x.UserId == userId);
        }

        public Result<ReportSettings> CreateSetting(ReportSettings entity)
        {
            try
            {
                Create(entity);
                SaveChanges();
                return new Result<ReportSettings>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ReportSettings>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ReportSettings> CreateOrUpdateSetting(ReportSettings entity, string userId)
        {
            try
            {
                if (entity.Id.IsNullOrEmpty())
                {
                    entity.Id = Utilities.GenerateUniqueId();
                    entity.CreatedBy = userId;
                    entity.CreatedDate = DateTime.Now;
                    Create(entity);
                }
                else
                {
                    entity.ModifiedBy = userId;
                    entity.ModifiedDate = DateTime.Now;
                    UpdateSetting(entity);
                }

                SaveChanges();

                return new Result<ReportSettings>
                {
                    Message = "The default date range has been updated.",
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ReportSettings>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ReportSettings> UpdateSetting(ReportSettings entity)
        {
            try
            {
                Update(entity);
                SaveChanges();
                return new Result<ReportSettings>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ReportSettings>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ReportSettings> DeleteSetting(string id)
        {
            try
            {
                var entity = GetSetting(id);
                Delete(entity);
                SaveChanges();
                return new Result<ReportSettings>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ReportSettings>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<ReportSettings> DeleteSetting(ReportSettings entity)
        {
            try
            {
                Delete(entity);
                SaveChanges();
                return new Result<ReportSettings>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ReportSettings>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        #region Report Category
        public ReportCategory GetCategory(string id)
        {
            return Read<ReportCategory>().FirstOrDefault(x => x.Id == id);
        }

        public List<ReportCategory> GetAllCategories(string churchId)
        {
            return Read<ReportCategory>().Where(x => x.ChurchId == churchId).OrderBy(x => x.Name).ToList();
        }

        public List<ReportCategory> GetAllCategories()
        {
            return Read<ReportCategory>().OrderBy(x => x.Name).ToList();
        }

        public ReportCategory CreateCategory(ReportCategory entity)
        {
            Create(entity);
            SaveChanges();
            return entity;
        }

        public ReportCategory CreateOrUpdateCategory(ReportCategory entity)
        {
            if (entity.Id.IsNullOrEmpty())
            {
                Create(entity);
            }
            else
            {
                UpdateCategory(entity);
            }

            SaveChanges();
            return entity;
        }

        public void UpdateCategory(ReportCategory entity)
        {
            Update(entity);
            SaveChanges();
        }

        public void DeleteCategory(string id)
        {
            var entity = GetCategory(id);
            Delete(entity);
            SaveChanges();
        }

        public void DeleteCategory(ReportCategory entity)
        {
            Delete(entity);
            SaveChanges();
        }
        #endregion

        #region Report Group
        public List<ReportGroup> GetAllGroups(string userId)
        {
            return Read<ReportGroup>().Where(x => x.UserId == userId).OrderBy(x => x.Name).ToList();
        }

        public ReportGroup GetGroup(string id)
        {
            return Read<ReportGroup>().FirstOrDefault(x => x.Id == id);
        }

        public void DeleteGroup(string id)
        {
            var entity = GetGroup(id);
            Delete(entity);
            SaveChanges();
        }

        public void DeleteGroup(ReportGroup entity)
        {
            Delete(entity);
            SaveChanges();
        }

        public ReportGroup CreateGroup(ReportGroup entity)
        {
            Create(entity);
            SaveChanges();
            return entity;
        }

        public ReportGroup CreateOrUpdateGroup(ReportGroup entity)
        {
            if (entity.Id.IsNullOrEmpty())
            {
                Create(entity);
            }
            else
            {
                UpdateGroup(entity);
            }

            SaveChanges();
            return entity;
        }

        public void UpdateGroup(ReportGroup entity)
        {
            Update(entity);
            SaveChanges();
        }

        public ReportGroupPartialVM GetReportGroups(string reportId, string userId, string churchId)
        {
            return new ReportGroupPartialVM
            {
                ReportId = reportId,
                ReportGroup = Read<ReportGroup>().Where(x => x.ChurchId == churchId && x.UserId == userId).OrderBy(x => x.Name).ToList(),
                UserReportGroups = Read<UserReportGroups>().Where(x => x.ChurchId == churchId && x.UserId == userId).ToList()
            };
        }

        public Result<UserReportGroups> AddRemoveReportGroup(string reportId, string groupId, string userId, string churchId)
        {
            try
            {
                var reportName = Work.Report.GetName(reportId);
                var groupName = Read<ReportGroup>().Where(x => x.Id == groupId).Select(x => x.Name).FirstOrDefault();
                var userGroup = Read<UserReportGroups>().FirstOrDefault(x => x.ReportId == reportId && x.GroupId == groupId && x.UserId == userId && x.ChurchId == churchId);

                if (userGroup.IsNotNull())
                {
                    Delete(userGroup);
                    SaveChanges();

                    return new Result<UserReportGroups>()
                    {
                        ResultType = ResultType.Success,
                        Message = $"{reportName} has been removed from the group {groupName}."
                    };
                }

                var obj = new UserReportGroups()
                {
                    ChurchId = churchId,
                    GroupId = groupId,
                    Id = Utilities.GenerateUniqueId(),
                    ReportId = reportId,
                    UserId = userId
                };
                Create(obj);
                SaveChanges();

                return new Result<UserReportGroups>
                {
                    Data = obj,
                    ResultType = ResultType.Success,
                    Message = $"{reportName} has been added to the group {groupName}."
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<UserReportGroups>
                {
                    ResultType = ResultType.Exception,
                    Exception = ex
                };
            }
        }
        #endregion

        #region Report Group Email Settings
        public List<ReportGroupEmailSetting> GetAllGroupEmailSettings(string userId)
        {
            return Read<ReportGroupEmailSetting>().Where(x => x.UserId == userId).ToList();
        }

        public ReportGroupEmailSetting GetGroupEmailSetting(string id)
        {
            return Read<ReportGroupEmailSetting>().FirstOrDefault(x => x.Id == id);
        }

        public void DeleteGroupEmailSetting(string id)
        {
            var entity = GetGroupEmailSetting(id);
            Delete(entity);
            SaveChanges();
        }

        public void DeleteGroupEmailSetting(ReportGroupEmailSetting entity)
        {
            Delete(entity);
            SaveChanges();
        }

        public ReportGroupEmailSetting CreateGroupEmailSetting(ReportGroupEmailSetting entity)
        {
            Create(entity);
            SaveChanges();
            return entity;
        }

        public ReportGroupEmailSetting CreateOrUpdateGroupEmailSetting(ReportGroupEmailSetting entity)
        {
            if (entity.Id.IsNullOrEmpty())
            {
                Create(entity);
            }
            else
            {
                UpdateGroupEmailSetting(entity);
            }

            SaveChanges();
            return entity;
        }

        public void UpdateGroupEmailSetting(ReportGroupEmailSetting entity)
        {
            Update(entity);
            SaveChanges();
        }
        #endregion

        #region Giving Reports
        public List<SelectListItem> GetSystemReportsByName(string name)
        {
            return GivingReports.SystemReports().Where(q => q.Text.ToLower().Trim().Contains(name.ToLower().Trim())).ToList();
        }

        public GivingSummaryDashboard GetGivingSummaryDashboard(string churchId, DateRange dateRange, bool includeDigitalGiving = true, bool includeOfflineGiving = true)
        {
            var model = new GivingSummaryDashboard
            {
                DigitalGiving = includeDigitalGiving ? Work.Payment.GetAll(churchId, null, dateRange) : new List<Payment>(),
                OfflineGiving = includeOfflineGiving ? Work.OfflineGiving.GetAll(churchId, null, dateRange) : new List<OfflineGiving>(),
                Funds = Work.Fund.GetAll(churchId),
                Campuses = Work.Campus.GetAll(churchId)
            };

            if (includeDigitalGiving)
            {
                AddGiftsToTotalGiving(model.TotalGiving, model.DigitalGiving); // Uses CreatedDate by default
            }

            if (includeOfflineGiving)
            {
                AddGiftsToTotalGiving(model.TotalGiving, model.OfflineGiving, getDate: gift => gift.DateReceived);
            }

            // Calculate totals for no campus
            model.NoCampusTotals = CalculateGivingByCampus(model, string.Empty);

            // Calculate totals by campus
            model.CampusTotals = model.Campuses.Select(c => CalculateGivingByCampus(model, c.Id)).ToList();

            // Calculate totals by fund
            model.FundTotals = model.Funds.Select(f => CalculateGivingByFund(model, f.Id)).ToList();

            // Prepare the funds data for the view
            model.TithesFundData = PrepareFundData(model, FundDisplayData.Tithes);
            model.GeneralFundData = PrepareFundData(model, FundDisplayData.General);
            model.MissionsFundData = PrepareFundData(model, FundDisplayData.Missions);

            return model;
        }

        public GivingSummaryDashboard GetDigitalGivingView(string churchId, DateRange dateRange)
        {
            return new GivingSummaryDashboard
            {
                DigitalGiving = Work.Payment.GetAll(churchId, null, dateRange),
                Funds = Work.Fund.GetAll(churchId),
                Campuses = Work.Campus.GetAll(churchId)
            };
        }

        public GivingSummaryDashboard GetOfflineGivingView(string churchId, DateRange dateRange)
        {
            return new GivingSummaryDashboard
            {
                OfflineGiving = Work.OfflineGiving.GetAll(churchId, null, dateRange),
                Funds = Work.Fund.GetAll(churchId)
            };
        }

        public FundReportDashboard GetFundSummaryView(string churchId, string fundId, DateRange dateRange)
        {
            var model = new FundReportDashboard
            {
                DigitalGiving = Work.Payment.GetAll(churchId, fundId, dateRange),
                OfflineGiving = Work.OfflineGiving.GetAll(churchId, fundId, dateRange),
                Campuses = Work.Campus.GetAll(churchId),
                Fund = Work.Fund.Get(fundId)
            };

            AddGiftsToTotalGiving(model.TotalGiving, model.DigitalGiving); // Uses CreatedDate by default
            AddGiftsToTotalGiving(model.TotalGiving, model.OfflineGiving, getDate: gift => gift.DateReceived);

            var gsdModel = new GivingSummaryDashboard
            {
                DigitalGiving = model.DigitalGiving,
                OfflineGiving = model.OfflineGiving,
                TotalGiving = model.TotalGiving
            };

            // Calculate totals for no campus
            model.NoCampusTotals = CalculateGivingByCampus(gsdModel, string.Empty);

            // Calculate totals by campus
            model.CampusTotals = model.Campuses.Select(c => CalculateGivingByCampus(gsdModel, c.Id)).ToList();

            // Calculate totals by fund
            model.SingleFundTotals = CalculateGivingByFund(gsdModel, fundId);

            return model;
        }

        public FundReportDashboard GetFundDetailView(string churchId, string fundId, string type, DateRange dates)
        {
            var model = new FundReportDashboard
            {
                Campuses = Work.Campus.GetAll(churchId),
                Fund = Work.Fund.Get(fundId)
            };

            if (!string.IsNullOrEmpty(type))
            {
                if (type.Equals(PaymentMethodTypes.Digital))
                {
                    model.PaymentMethodType = PaymentMethodTypes.Digital;
                    model.DigitalGiving = Work.Payment.GetAll(churchId, fundId, dates);

                    AddGiftsToTotalGiving(model.TotalGiving, model.DigitalGiving); // Uses CreatedDate by default
                }
                else if (type.Equals(PaymentMethodTypes.Offline))
                {
                    model.PaymentMethodType = PaymentMethodTypes.Offline;
                    model.OfflineGiving = Work.OfflineGiving.GetAll(churchId, fundId, dates);

                    AddGiftsToTotalGiving(model.TotalGiving, model.OfflineGiving, getDate: gift => gift.DateReceived);
                }
            }

            // Calculate totals for no campus
            model.NoCampusTotals = CalculateGivingByCampus(model, string.Empty);

            // Calculate totals by campus
            model.CampusTotals = model.Campuses.Select(c => CalculateGivingByCampus(model, c.Id)).ToList();

            // Calculate totals for a single fund
            if (model.Fund != null)
            {
                model.SingleFundTotals = CalculateGivingByFund(model, model.Fund.Id);
            }

            model.FundTotals = new List<GivingByFund>
            {
                model.SingleFundTotals
            };

            return model;
        }

        public GivingSummaryDashboard GetCampusGivingSummaryDashboard(string campusId, DateRange dates)
        {
            var campus = Work.Campus.Get(campusId);

            // Initialize the model with campus and fund information
            var model = new GivingSummaryDashboard
            {
                DigitalGiving = Work.Payment.GetAllByCampusId(campusId, dates),
                OfflineGiving = Work.OfflineGiving.GetAllByCampusId(campusId, dates),
                CurrentCampus = campus,
                Funds = campus != null ? Work.Fund.GetAll(campus.ChurchId) : new List<Fund>()
            };

            // Consolidate the adding of gifts into TotalGiving
            AddGiftsToTotalGiving(model.TotalGiving, model.DigitalGiving); // Uses CreatedDate by default
            AddGiftsToTotalGiving(model.TotalGiving, model.OfflineGiving, getDate: gift => gift.DateReceived);

            return model;
        }

        //Used only in widgets on Reports/Index page
        public List<ReportViewModel> GetGivingReport(string reportType, string churchId, string campusIdsPlain, string dateRange)
        {
            var result = new List<ReportViewModel>();
            var campusIds = campusIdsPlain.DeserializeToList();
            var dates = dateRange.ToDateRange();
            var payments = Read<Payment>().Where(x => x.ChurchId == churchId && x.CreatedDate >= dates.StartDate && x.CreatedDate <= dates.EndDate).ToList();

            if (campusIds.Count > 0)
            {
                payments = payments.Where(x => campusIds.Contains(x.CampusId)).ToList();
            }

            var total = payments.Count;

            if (reportType == GivingReportType.LYBUNT)
            {
                var startDate = Convert.ToDateTime($"01/01/{DateTime.Now.Year - 1}");
                var endDate = Convert.ToDateTime($"12/31/{DateTime.Now.Year}");

                var bothYears = payments.Where(d => d.CreatedDate >= startDate && d.CreatedDate <= endDate).Select(x => x.UserId).Distinct().Count();
                var percent = Utilities.GetPercent(bothYears, bothYears);
                result.Add(new ReportViewModel { Total = percent, Year = startDate.Year, Title = "Both This &amp; Last Year" });

                // var lastYear = payments.Where(d => d.CreatedDate >= startDate && d.CreatedDate <= lastYearEndDate).Select(x => x.UserId).Distinct().Count();
                percent = 100 - percent;
                result.Add(new ReportViewModel { Total = percent, Year = endDate.Year, Title = "Last Year Only" });
            }
            else if (reportType == GivingReportType.DigitalPaymentMethodType)
            {
                var card = payments.Count(x => x.DigitalPaymentMethod == DigitalPaymentMethods.Card);
                var percent = Utilities.GetPercent(card, total);
                result.Add(new ReportViewModel { Total = percent, Title = "Credit/Debit Cards" });

                // var ach = payments.Where(x => x.DigitalPaymentMethod == DigitalPaymentMethods.ACH).Count();
                percent = 100 - percent;
                result.Add(new ReportViewModel { Total = percent, Title = "Bank Accounts" });
            }
            else if (reportType == GivingReportType.DigitalPaymentType)
            {
                var online = payments.Count(x => x.DigitalPaymentType == DigitalPaymentTypes.Online);
                var percent = Utilities.GetPercent(online, total);
                result.Add(new ReportViewModel { Total = percent, Title = "Online" });

                //var textToGive = payments.Where(x => x.DigitalPaymentType == DigitalPaymentTypes.TextToGive).Count();
                percent = 100 - percent;
                result.Add(new ReportViewModel { Total = percent, Title = "Text to Give" });
            }
            else if (reportType == GivingReportType.DonorType)
            {
                var distinct = payments.Select(x => x.UserId).Distinct().ToList();
                var firstTime = new List<int>();
                var previous = new List<int>();

                foreach (var countRecords in distinct.Select(uid => payments.Count(x => x.UserId == uid)))
                {
                    if (countRecords > 1)
                    {
                        previous.Add(countRecords);
                    }
                    else
                    {
                        firstTime.Add(countRecords);
                    }
                }

                var percent = Utilities.GetPercent(firstTime.Count, distinct.Count);
                result.Add(new ReportViewModel { Total = firstTime.Count, Title = "First-Time" });

                percent = 100 - percent;
                result.Add(new ReportViewModel { Total = percent, Title = "Previous" });
            }
            else if (reportType == GivingReportType.ManualPaymentType)
            {
                var totalPercent = 0;
                var paymentType = Read<OfflineGiving>().Where(x => x.ChurchId == churchId).ToList();

                if (campusIds.Count > 0)
                {
                    paymentType = paymentType.Where(x => campusIds.Contains(x.CampusId)).ToList();
                }

                total = paymentType.Count;

                var dropOff = paymentType.Count(x => x.OfflinePaymentType == OfflinePaymentTypes.DropOff);
                var percent = Utilities.GetPercent(dropOff, total);
                result.Add(new ReportViewModel { Total = percent, Title = OfflinePaymentTypes.DropOff });
                totalPercent = percent;

                var offeringPlate = paymentType.Count(x => x.OfflinePaymentType == OfflinePaymentTypes.OfferingPlate);
                percent = Utilities.GetPercent(offeringPlate, total);
                result.Add(new ReportViewModel { Total = percent, Title = OfflinePaymentTypes.OfferingPlate });
                totalPercent += percent;

                percent = 100 - totalPercent;
                result.Add(new ReportViewModel { Total = percent, Title = OfflinePaymentTypes.Mail });
            }
            else if (reportType == GivingReportType.PaymentType)
            {
                var paymentType = Read<OfflineGiving>().Count(x => x.ChurchId == churchId);
                var grandTotal = total + paymentType;

                var percent = Utilities.GetPercent(total, grandTotal);
                result.Add(new ReportViewModel { Total = percent, Title = "Digital" });

                percent = 100 - percent;
                result.Add(new ReportViewModel { Total = percent, Title = "Offline" });
            }
            else if (reportType == GivingReportType.SYBUNT)
            {
                var lastYearDate = DateTime.Parse($"01/01/{DateTime.Now.Year} 12:00:00 AM");

                var totalUsers = payments.Select(x => x.UserId).Distinct().Count();
                var lastYearUsers = payments.Where(x => x.CreatedDate >= lastYearDate).Select(x => x.UserId).Distinct().Count();

                var noneGivingUserInCurrentYear = totalUsers - lastYearUsers;
                // var restUserCounts = totalUsers - noneGivingUserInCurrentYear;

                var percent = Utilities.GetPercent(noneGivingUserInCurrentYear, totalUsers);
                result.Add(new ReportViewModel { Total = percent, Title = "Given in Prior Years Only" });

                percent = 100 - percent;
                result.Add(new ReportViewModel { Total = percent, Title = "Given this Year and in Prior Years" });
            }
            else if (reportType == GivingReportType.TimeADay)
            {
                var totalPercent = 0;

                var startTime = DateTime.Parse(EarlyMorningTimes.Start).TimeOfDay;
                var endTime = DateTime.Parse(EarlyMorningTimes.Stop).TimeOfDay;
                var earlyMorning = payments.Count(x => x.CreatedDate.TimeOfDay >= startTime && x.CreatedDate.TimeOfDay <= endTime);
                var percent = Utilities.GetPercent(earlyMorning, total);
                result.Add(new ReportViewModel { Total = percent, Title = TimeOfTheDay.EarlyMorning });
                totalPercent = percent;

                startTime = DateTime.Parse(MorningTimes.Start).TimeOfDay;
                endTime = DateTime.Parse(MorningTimes.Stop).TimeOfDay;
                var morning = payments.Count(x => x.CreatedDate.TimeOfDay >= startTime && x.CreatedDate.TimeOfDay <= endTime);
                percent = Utilities.GetPercent(morning, total);
                result.Add(new ReportViewModel { Total = percent, Title = TimeOfTheDay.Morning });
                totalPercent += percent;

                startTime = DateTime.Parse(AfternoonTimes.Start).TimeOfDay;
                endTime = DateTime.Parse(AfternoonTimes.Stop).TimeOfDay;
                var afternoon = payments.Count(x => x.CreatedDate.TimeOfDay >= startTime && x.CreatedDate.TimeOfDay <= endTime);
                percent = Utilities.GetPercent(afternoon, total);
                result.Add(new ReportViewModel { Total = percent, Title = TimeOfTheDay.Afternoon });
                totalPercent += percent;

                startTime = DateTime.Parse(EveningTimes.Start).TimeOfDay;
                endTime = DateTime.Parse(EveningTimes.Stop).TimeOfDay;
                var evening = payments.Count(x => x.CreatedDate.TimeOfDay >= startTime && x.CreatedDate.TimeOfDay <= endTime);
                percent = Utilities.GetPercent(evening, total);
                result.Add(new ReportViewModel { Total = percent, Title = TimeOfTheDay.Evening });
                totalPercent += percent;

                //startTime = DateTime.Parse(OvernightTimes.Start).TimeOfDay;
                //endTime = DateTime.Parse(OvernightTimes.Stop).TimeOfDay;
                //var overnight = payments.Where(x => x.CreatedDate.TimeOfDay >= startTime && x.CreatedDate.TimeOfDay <= endTime).Count();
                //startTime = DateTime.Parse("00:00 AM").TimeOfDay;
                //overnight += payments.Where(x => x.CreatedDate.TimeOfDay == startTime).Count();
                percent = 100 - totalPercent;
                result.Add(new ReportViewModel { Total = percent, Title = TimeOfTheDay.Overnight });
            }
            else if (reportType == GivingReportType.WeekDay)
            {
                var totalPercent = 0;
                var sunday = payments.Count(x => x.CreatedDate.DayOfWeek == DayOfWeek.Sunday);
                var percent = Utilities.GetPercent(sunday, total);
                result.Add(new ReportViewModel { Total = percent, Title = DaysOfTheWeek.Sunday });
                totalPercent = percent;

                var monday = payments.Count(x => x.CreatedDate.DayOfWeek == DayOfWeek.Monday);
                percent = Utilities.GetPercent(monday, total);
                result.Add(new ReportViewModel { Total = percent, Title = DaysOfTheWeek.Monday });
                totalPercent += percent;

                var tuesday = payments.Count(x => x.CreatedDate.DayOfWeek == DayOfWeek.Tuesday);
                percent = Utilities.GetPercent(tuesday, total);
                result.Add(new ReportViewModel { Total = percent, Title = DaysOfTheWeek.Tuesday });
                totalPercent += percent;

                var wednesday = payments.Count(x => x.CreatedDate.DayOfWeek == DayOfWeek.Wednesday);
                percent = Utilities.GetPercent(wednesday, total);
                result.Add(new ReportViewModel { Total = percent, Title = DaysOfTheWeek.Wednesday });
                totalPercent += percent;

                var thursday = payments.Count(x => x.CreatedDate.DayOfWeek == DayOfWeek.Thursday);
                percent = Utilities.GetPercent(thursday, total);
                result.Add(new ReportViewModel { Total = percent, Title = DaysOfTheWeek.Thursday });
                totalPercent += percent;

                var friday = payments.Count(x => x.CreatedDate.DayOfWeek == DayOfWeek.Friday);
                percent = Utilities.GetPercent(friday, total);
                result.Add(new ReportViewModel { Total = percent, Title = DaysOfTheWeek.Friday });
                totalPercent += percent;

                // var saturday = payments.Where(x => x.CreatedDate.DayOfWeek == DayOfWeek.Saturday).Count();
                percent = 100 - totalPercent;
                result.Add(new ReportViewModel { Total = percent, Title = DaysOfTheWeek.Saturday });
            }

            return result;
        }

        public List<TopDonorsVM> GetTopDonors(string churchId, int listSize, DateRange dateRange, bool isSuperAdmin = false)
        {
            // Create dictionaries to hold donor details
            var personCache = new Dictionary<string, Person>();
            var userCache = new Dictionary<string, ApplicationUser>();

            // Step 1: Retrieve Data
            var offlineGiving = Work.OfflineGiving.GetAllWithDonors(churchId, dateRange);
            var digitalGiving = Work.Payment.GetAll(churchId, null, dateRange);

            // Extract unique UserIds for digital giving
            var digitalUserIds = digitalGiving.Select(d => d.UserId).Distinct().ToList();

            // Retrieve all relevant users in bulk
            var users = Work.User.GetAll(digitalUserIds);

            // Cache the users
            foreach (var user in users)
            {
                userCache[user.Id] = user;
            }

            // Retrieve all relevant persons in bulk
            var personIds = offlineGiving.Select(og => og.PersonId)
                .Union(users.Select(u => u.PersonId))
                .Distinct()
                .ToList();

            var persons = Work.Person.GetAllByPersonIds(personIds);

            // Cache the persons
            foreach (var person in persons)
            {
                personCache[person.Id] = person;
            }

            // Step 2: Aggregate Giving Amounts
            var aggregatedOfflineGiving = offlineGiving
                .GroupBy(g => g.PersonId)
                .Select(group => new
                {
                    DonorId = group.Key,
                    OfflineGivingAmount = group.Sum(g => g.Amount)
                });

            var aggregatedDigitalGiving = digitalGiving
                .Join(users,
                      digital => digital.UserId,
                      user => user.Id,
                      (digital, user) => new { digital, user })
                .GroupBy(g => g.user.PersonId)
                .Select(group => new
                {
                    DonorId = group.Key,
                    DigitalGivingAmount = group.Sum(g => g.digital.Amount)
                });

            // Step 3: Combine Offline and Digital Giving
            var combinedGiving = aggregatedOfflineGiving.Any() ?
                (from offline in aggregatedOfflineGiving
                 join digital in aggregatedDigitalGiving on offline.DonorId equals digital.DonorId into gj
                 from digital in gj.DefaultIfEmpty()
                 select new
                 {
                     offline.DonorId,
                     offline.OfflineGivingAmount,
                     DigitalGivingAmount = digital != null ? digital.DigitalGivingAmount : 0
                 }).ToList() :
                aggregatedDigitalGiving.Select(digital => new
                {
                    digital.DonorId,
                    OfflineGivingAmount = 0m,
                    digital.DigitalGivingAmount
                }).ToList();

            // Step 4: Sort by Total Amount
            combinedGiving.Sort((a, b) => (b.OfflineGivingAmount + b.DigitalGivingAmount).CompareTo(a.OfflineGivingAmount + a.DigitalGivingAmount));

            // Step 5: Select Top # Donors
            var topDonors = combinedGiving.Take(listSize);

            // Step 6: Pass Data to View
            return topDonors.Select((donor, index) => new TopDonorsVM
            {
                Donor = isSuperAdmin
                    ? personCache.TryGetValue(donor.DonorId, out var person) ? person : new Person { Display = "[No Donor Found]" }
                    : new Person { Display = GetAnonymizedDonorName(index) },
                OfflineGivingAmount = donor.OfflineGivingAmount,
                DigitalGivingAmount = donor.DigitalGivingAmount,
                TotalAmount = donor.OfflineGivingAmount + donor.DigitalGivingAmount
            }).ToList();
        }

        private string GetAnonymizedDonorName(int index)
        {
            // Generate a donor name based on the index
            return $"Donor {index + 1}";
        }

        public List<TopDonationVM> GetTopDonorsByYears(string churchId, int years, int takeRecordForEachYear, bool isSuperAdmin = false)
        {
            List<TopDonationVM> topDonationsByYears = new List<TopDonationVM>();

            // Create dictionaries to hold donor details
            var donorIndexMapping = new Dictionary<string, string>();
            var personCache = new Dictionary<string, Person>();
            var userCache = new Dictionary<string, ApplicationUser>();

            // Initialize a counter for donor names
            int donorCounter = 1;

            // Iterate through each of the past X years
            for (int i = 0; i < years; i++)
            {
                int currentYear = DateTime.Now.Year - i;

                // Create a DateRange object for the current year
                var dateRange = new DateRange
                {
                    StartDate = new DateTime(currentYear, 1, 1),
                    EndDate = new DateTime(currentYear, 12, 31)
                };

                // Retrieve data for the current year
                var offlineGivingForYear = Work.OfflineGiving.GetAllWithDonors(churchId, dateRange);
                var digitalGivingForYear = Work.Payment.GetAllForYear(churchId, currentYear);

                // Collect all person IDs from offline giving and all user IDs from digital giving
                var personIds = offlineGivingForYear.Select(og => og.PersonId).Distinct().ToList();
                var userIds = digitalGivingForYear.Select(dg => dg.UserId).Distinct().ToList();

                // Retrieve all relevant persons and users in bulk
                var persons = Work.Person.GetAllByPersonIds(personIds);
                var users = Work.User.GetAll(userIds);

                // Cache the persons and users
                foreach (var person in persons)
                {
                    personCache[person.Id] = person;
                }

                foreach (var user in users)
                {
                    userCache[user.Id] = user;
                }

                // Aggregate giving amounts for the current year
                var aggregatedOfflineGiving = offlineGivingForYear
                    .GroupBy(g => g.PersonId)
                    .Select(group => new
                    {
                        DonorId = group.Key,
                        OfflineGivingAmount = group.Sum(g => g.Amount)
                    });

                var aggregatedDigitalGiving = digitalGivingForYear
                    .Join(users,
                          digital => digital.UserId,
                          user => user.Id,
                          (digital, user) => new { digital, user })
                    .GroupBy(g => g.user.PersonId)
                    .Select(group => new
                    {
                        DonorId = group.Key,
                        DigitalGivingAmount = group.Sum(g => g.digital.Amount)
                    });

                var combinedGiving = aggregatedOfflineGiving.Any() ?
                    (from offline in aggregatedOfflineGiving
                     join digital in aggregatedDigitalGiving on offline.DonorId equals digital.DonorId into gj
                     from digital in gj.DefaultIfEmpty()
                     select new
                     {
                         offline.DonorId,
                         offline.OfflineGivingAmount,
                         DigitalGivingAmount = digital != null ? digital.DigitalGivingAmount : 0
                     }).ToList() :
                    aggregatedDigitalGiving.Select(digital => new
                    {
                        digital.DonorId,
                        OfflineGivingAmount = 0m,
                        digital.DigitalGivingAmount
                    }).ToList();

                combinedGiving.Sort((a, b) => (b.OfflineGivingAmount + b.DigitalGivingAmount).CompareTo(a.OfflineGivingAmount + a.DigitalGivingAmount));

                // Create or retrieve donor index mapping
                var topDonorsForYear = combinedGiving.Take(takeRecordForEachYear)
                    .Select(donor =>
                    {
                        if (!donorIndexMapping.ContainsKey(donor.DonorId))
                        {
                            // Assign a new anonymized name if not already mapped
                            donorIndexMapping[donor.DonorId] = GetAnonymizedDonorName(donorCounter++);
                        }

                        var person = personCache.TryGetValue(donor.DonorId, out var p) ? p : new Person { Display = "[No Donor Found]" };

                        return new TopDonationModel
                        {
                            Donor = isSuperAdmin ? person : new Person { Display = donorIndexMapping[donor.DonorId] },
                            TotalAmount = donor.OfflineGivingAmount + donor.DigitalGivingAmount,
                            TotalDigitalAmount = donor.DigitalGivingAmount,
                            TotalOfflineAmount = donor.OfflineGivingAmount
                        };
                    }).ToList();

                var topDonationVM = new TopDonationVM
                {
                    Year = currentYear,
                    Donations = topDonorsForYear
                };

                topDonationsByYears.Add(topDonationVM);
            }

            return topDonationsByYears;
        }

        public List<TopDonationVM> GetTopDonationsByYears(string churchId, int years, int takeRecordForEachYear, bool isSuperAdmin = false)
        {
            List<TopDonationVM> topDonationsPastFiveYears = new List<TopDonationVM>();

            // Create dictionaries to hold donor details
            var donorIndexMapping = new Dictionary<string, string>();
            var personCache = new Dictionary<string, Person>();
            var userCache = new Dictionary<string, ApplicationUser>();

            // Initialize a counter for donor names
            int donorCounter = 1;

            // Iterate through each of the past X years
            for (int i = 0; i < years; i++)
            {
                int currentYear = DateTime.Now.Year - i;

                // Create a DateRange object for the current year
                var dateRange = new DateRange
                {
                    StartDate = new DateTime(currentYear, 1, 1),
                    EndDate = new DateTime(currentYear, 12, 31)
                };

                // Retrieve digital and offline giving for the current year
                var offlineGivingForYear = Work.OfflineGiving.GetAllWithDonors(churchId, dateRange);
                var digitalGivingForYear = Work.Payment.GetAllForYear(churchId, currentYear);

                // Collect all person IDs and user IDs from the donations
                var personIds = offlineGivingForYear.Select(og => og.PersonId).Distinct().ToList();
                var userIds = digitalGivingForYear.Select(dg => dg.UserId).Distinct().ToList();

                // Retrieve all relevant persons and users in bulk
                var persons = Work.Person.GetAllByPersonIds(personIds);
                var users = Work.User.GetAll(userIds);

                // Cache the persons and users
                foreach (var person in persons)
                {
                    personCache[person.Id] = person;
                }

                foreach (var user in users)
                {
                    userCache[user.Id] = user;
                }

                // Process offline giving for the current year
                var topDonorsOffline = offlineGivingForYear
                    .Select(og =>
                    {
                        if (!donorIndexMapping.ContainsKey(og.PersonId))
                        {
                            donorIndexMapping[og.PersonId] = isSuperAdmin
                                ? personCache.TryGetValue(og.PersonId, out var person) ? person.Display : "[No Donor Found]"
                                : GetAnonymizedDonorName(donorCounter++);
                        }

                        return new TopDonationModel
                        {
                            Donor = new Person { Display = donorIndexMapping[og.PersonId] },
                            GivingType = PaymentMethodTypes.Offline,
                            TotalAmount = og.Amount,
                            TotalOfflineAmount = og.Amount,
                            TotalDigitalAmount = 0 // Initialize to 0 since this is offline giving
                        };
                    });

                // Process digital giving for the current year
                var topDonorsDigital = digitalGivingForYear
                    .Select(dg =>
                    {
                        if (userCache.TryGetValue(dg.UserId, out var user))
                        {
                            var personId = user.PersonId;
                            if (!donorIndexMapping.ContainsKey(personId))
                            {
                                donorIndexMapping[personId] = isSuperAdmin
                                    ? personCache.TryGetValue(personId, out var person) ? person.Display : "[No Donor Found]"
                                    : GetAnonymizedDonorName(donorCounter++);
                            }

                            return new TopDonationModel
                            {
                                Donor = new Person { Display = donorIndexMapping[personId] },
                                GivingType = PaymentMethodTypes.Digital,
                                TotalAmount = dg.Amount,
                                TotalDigitalAmount = dg.Amount,
                                TotalOfflineAmount = 0 // Initialize to 0 since this is digital giving
                            };
                        }

                        // If UserId is null or Person not found, return a placeholder Person object
                        return new TopDonationModel
                        {
                            Donor = new Person { Display = isSuperAdmin ? "[No Donor Found]" : GetAnonymizedDonorName(donorCounter++) },
                            GivingType = PaymentMethodTypes.Digital,
                            TotalAmount = dg.Amount,
                            TotalDigitalAmount = dg.Amount,
                            TotalOfflineAmount = 0 // Initialize to 0 since this is digital giving
                        };
                    });

                // Combine results for offline and digital giving and take top records for the current year
                var combinedGiving = topDonorsOffline.Concat(topDonorsDigital)
                    .OrderByDescending(d => d.TotalAmount) // Order by total amount in descending order
                    .Take(takeRecordForEachYear) // Take top records for the current year
                    .ToList();

                // Create TopDonationVM for the current year
                var topDonationVM = new TopDonationVM
                {
                    Year = currentYear,
                    Donations = combinedGiving
                };

                // Add TopDonationVM to the list
                topDonationsPastFiveYears.Add(topDonationVM);
            }

            return topDonationsPastFiveYears;
        }

        public async Task<List<TopDonationVM>> GetTopDonationsByYearsAsync(string churchId, int years, int takeRecordForEachYear)
        {
            List<TopDonationVM> topDonationsPastFiveYears = new List<TopDonationVM>();

            for (int i = 0; i < years; i++)
            {
                int currentYear = DateTime.Now.Year - i;

                // Create a DateRange object for the current year
                var dateRange = new DateRange
                {
                    StartDate = new DateTime(currentYear, 1, 1),
                    EndDate = new DateTime(currentYear, 12, 31)
                };

                // Retrieve offline giving data asynchronously
                var offlineGivingForYear = await Work.OfflineGiving.GetAllWithDonorsAsync(churchId, dateRange);

                // Retrieve digital giving data asynchronously
                var digitalGivingForYear = await Work.Payment.GetAllForYearAsync(churchId, currentYear);

                // Process and combine giving data
                var combinedGiving = offlineGivingForYear
                    .Select(og => new TopDonationModel
                    {
                        Donor = Work.Person.Get(og.PersonId) ?? new Person { Display = "[No Donor Found]" },
                        GivingType = PaymentMethodTypes.Offline,
                        TotalAmount = og.Amount,
                        TotalOfflineAmount = og.Amount,
                        TotalDigitalAmount = 0
                    })
                    .Concat(digitalGivingForYear
                        .Select(dg =>
                        {
                            var user = Work.User.Get(dg.UserId);
                            var person = user != null ? Work.Person.Get(user.PersonId) : null;

                            return new TopDonationModel
                            {
                                Donor = person ?? new Person { Display = "[No Donor Found]" },
                                GivingType = PaymentMethodTypes.Digital,
                                TotalAmount = dg.Amount,
                                TotalDigitalAmount = dg.Amount,
                                TotalOfflineAmount = 0
                            };
                        }))
                    .OrderByDescending(d => d.TotalAmount)
                    .Take(takeRecordForEachYear)
                    .ToList();

                var topDonationVM = new TopDonationVM
                {
                    Year = currentYear,
                    Donations = combinedGiving
                };

                topDonationsPastFiveYears.Add(topDonationVM);
            }

            return topDonationsPastFiveYears;
        }

        #region Donor Demographics Report
        public DonorsModel GetDonorsModel(string churchId, DateRange dates = null)
        {
            var model = new DonorsModel();
            var digitalGiving = Work.Payment.GetAll(churchId);
            var offlineGiving = Work.OfflineGiving.GetAll(churchId);
            var users = Work.User.GetAllByChurchId(churchId);
            var activeGifts = Work.ScheduledPayment.GetAllUnprocessed(churchId);

            // Adding gifts to total giving
            AddGiftsToTotalGiving(model.TotalGiving, digitalGiving,
                gift => GetPaymentType(gift),
                gift => gift.Frequency.Equals(PaymentOccurrence.Recurring) && gift.ScheduledPaymentId.IsNotNullOrEmpty()
                    ? activeGifts.Any(a => a.Id.Equals(gift.ScheduledPaymentId) && !a.IsProcessed)
                        ? PaymentOccurrence.Recurring
                        : PaymentOccurrence.OneTime
                    : PaymentOccurrence.OneTime,
                gift => users.Find(z => z.Id.Equals(gift.UserId))?.PersonId,
                gift => gift.CreatedDate // Use CreatedDate for digital giving
            );

            AddGiftsToTotalGiving(model.TotalGiving, offlineGiving,
                gift => GetPaymentType(gift),
                gift => PaymentOccurrence.OneTime,
                gift => gift.PersonId,
                gift => gift.DateReceived // Use DateReceived for offline giving
            );

            model.People = Work.Person.GetAllByPersonIds(churchId, model.TotalGiving.Select(x => x.PersonId).Distinct());

            // Filter data by date range if provided
            if (dates.IsNotNullOrEmpty())
            {
                model.TotalGivingByDate = model.TotalGiving.Where(x => x.CreatedDate.Date >= dates.StartDate.Date && x.CreatedDate.Date <= dates.EndDate.Date).ToList();
                model.PeopleByDate = model.People.Where(x => model.TotalGivingByDate.Select(q => q.PersonId).Contains(x.Id)).ToList();
            }
            else
            {
                model.TotalGivingByDate = model.TotalGiving;
                model.PeopleByDate = model.People;
            }

            // Calculate statistics
            CalculateGenderStats(model);
            CalculateMaritalStatusStats(model);
            CalculateAgeGroupStats(model);
            CalculateEthnicTypeStats(model);
            CalculateEducationTypeStats(model);
            CalculateEmploymentStatusStats(model);

            return model;
        }

        private void CalculateGenderStats(DonorsModel model)
        {
            var genderKeys = Constants.Genders.Keys.Concat(new[] { "Not Provided" }).ToList();
            model.GenderStats = new Dictionary<string, GenderStat>();

            decimal totalDonationsAcrossAllGenders = 0m;
            int totalDonationsCountAcrossAllGenders = 0;

            foreach (var genderKey in genderKeys)
            {
                IEnumerable<Person> peopleByGender;
                IEnumerable<decimal> totalGivingForGender;

                if (genderKey == "Not Provided")
                {
                    peopleByGender = model.PeopleByDate.Where(q => string.IsNullOrEmpty(q.Gender));
                    totalGivingForGender = model.TotalGivingByDate
                        .Where(x => x.PersonId.IsNotNullOrEmpty() && peopleByGender.Select(p => p.Id).Contains(x.PersonId))
                        .Select(x => x.Amount);
                }
                else
                {
                    peopleByGender = model.PeopleByDate.Where(q => q.Gender.EqualsIgnoreCase(genderKey));
                    totalGivingForGender = model.TotalGivingByDate
                        .Where(x => x.PersonId.IsNotNullOrEmpty() && peopleByGender.Select(p => p.Id).Contains(x.PersonId))
                        .Select(x => x.Amount);
                }

                var genderDonationCount = totalGivingForGender.Count();
                var averageDonation = genderDonationCount > 0 ? totalGivingForGender.Average() : 0m;

                model.GenderStats[genderKey] = new GenderStat
                {
                    Count = peopleByGender.Count(),
                    Percentage = peopleByGender.Any() ? Utilities.GetPercent(peopleByGender.Count(), model.PeopleByDate.Count) : 0,
                    AverageDonation = averageDonation
                };

                // Aggregate totals for overall average calculation
                totalDonationsAcrossAllGenders += totalGivingForGender.Sum();
                totalDonationsCountAcrossAllGenders += genderDonationCount;
            }

            // Update totals
            model.TotalGenderCount = model.GenderStats.Values.Sum(x => x.Count);
            model.TotalGenderPercentage = model.GenderStats.Values.Sum(x => x.Percentage);
            model.TotalGenderAverageDonation = totalDonationsCountAcrossAllGenders > 0
                ? totalDonationsAcrossAllGenders / totalDonationsCountAcrossAllGenders
                : 0m;
        }

        private void CalculateMaritalStatusStats(DonorsModel model)
        {
            // Default any null or empty MaritalStatus to "Other/Not Provided"
            foreach (var person in model.PeopleByDate)
            {
                if (string.IsNullOrEmpty(person.MaritalStatus))
                {
                    person.MaritalStatus = MaritalStatuses.Other;
                }
            }

            // Initialize Marital Status Statistics
            model.MaritalStatusStats = new Dictionary<string, MaritalStatusStat>();
            decimal totalDonationsAcrossAllMaritalStatuses = 0m;
            int totalDonationsCountAcrossAllMaritalStatuses = 0;

            foreach (var maritalStatus in MaritalStatuses.Items)
            {
                var peopleByMaritalStatus = model.PeopleByDate.Where(q => q.MaritalStatus.EqualsIgnoreCase(maritalStatus));
                var totalGivingForMaritalStatus = model.TotalGivingByDate
                    .Where(x => x.PersonId.IsNotNullOrEmpty() && peopleByMaritalStatus.Select(p => p.Id).Contains(x.PersonId))
                    .Select(x => x.Amount);

                var maritalStatusDonationCount = totalGivingForMaritalStatus.Count();

                model.MaritalStatusStats[maritalStatus] = new MaritalStatusStat
                {
                    MaritalStatus = maritalStatus,
                    Count = peopleByMaritalStatus.Count(),
                    Percentage = peopleByMaritalStatus.Any() ? Utilities.GetPercent(peopleByMaritalStatus.Count(), model.PeopleByDate.Count) : 0,
                    AverageDonation = maritalStatusDonationCount > 0 ? totalGivingForMaritalStatus.Average() : 0m
                };

                // Aggregate totals for overall average calculation
                totalDonationsAcrossAllMaritalStatuses += totalGivingForMaritalStatus.Sum();
                totalDonationsCountAcrossAllMaritalStatuses += maritalStatusDonationCount;
            }

            model.TotalMaritalStatusCount = model.MaritalStatusStats.Sum(x => x.Value.Count);
            model.TotalMaritalStatusPercentage = model.MaritalStatusStats.Sum(x => x.Value.Percentage);
            model.TotalMaritalStatusAverageDonation = totalDonationsCountAcrossAllMaritalStatuses > 0
                ? totalDonationsAcrossAllMaritalStatuses / totalDonationsCountAcrossAllMaritalStatuses
                : 0m;
        }

        private void CalculateAgeGroupStats(DonorsModel model)
        {
            // Initialize Age Group Statistics
            model.AgeGroupStats = new Dictionary<string, AgeGroupStat>();
            int totalPeopleCount = model.PeopleByDate.Count;
            decimal totalDonationsAcrossAllAgeGroups = 0m;
            int totalDonationsCountAcrossAllAgeGroups = 0;

            foreach (var ageGroup in AgeGroupDemographics.Items)
            {
                var peopleByAge = GetPeopleByAgeGroup(model.PeopleByDate, ageGroup);
                var totalGivingForAgeGroup = model.TotalGivingByDate
                    .Where(x => x.PersonId.IsNotNullOrEmpty() && peopleByAge.Select(p => p.Id).Contains(x.PersonId))
                    .Select(x => x.Amount);

                var ageGroupDonationCount = totalGivingForAgeGroup.Count();

                model.AgeGroupStats[ageGroup] = new AgeGroupStat
                {
                    AgeGroup = ageGroup,
                    Count = peopleByAge.Count,
                    Percentage = peopleByAge.Any() ? Utilities.GetPercent(peopleByAge.Count, model.PeopleByDate.Count) : 0,
                    AverageDonation = ageGroupDonationCount > 0 ? totalGivingForAgeGroup.Average() : 0m
                };

                // Aggregate totals for overall average calculation
                totalDonationsAcrossAllAgeGroups += totalGivingForAgeGroup.Sum();
                totalDonationsCountAcrossAllAgeGroups += ageGroupDonationCount;
            }

            // Calculate overall totals for all age groups
            model.TotalAgeGroupCount = model.AgeGroupStats.Sum(x => x.Value.Count);
            model.TotalAgeGroupPercentage = model.AgeGroupStats.Sum(x => x.Value.Percentage);

            // Calculate the overall average donation
            model.TotalAgeGroupAverageDonation = totalDonationsCountAcrossAllAgeGroups > 0
                ? totalDonationsAcrossAllAgeGroups / totalDonationsCountAcrossAllAgeGroups
                : 0m;
        }

        private void CalculateEthnicTypeStats(DonorsModel model)
        {
            // Initialize Ethnic Type Statistics
            model.EthnicTypeStats = new Dictionary<string, EthnicTypeStat>();
            decimal totalDonationsAcrossAllEthnicTypes = 0m;
            int totalDonationsCountAcrossAllEthnicTypes = 0;

            foreach (var ethnicType in EthnicTypes.Items)
            {
                var peopleByEthnicType = model.PeopleByDate.Where(q => (q.Ethnicity.IsNotNullOrEmpty() && q.Ethnicity.EqualsIgnoreCase(ethnicType)) || (q.Ethnicity.IsNullOrEmpty() && ethnicType.EqualsIgnoreCase(EthnicTypes.Other)));
                var totalGivingForEthnicType = model.TotalGivingByDate
                    .Where(x => x.PersonId.IsNotNullOrEmpty() && peopleByEthnicType.Select(p => p.Id).Contains(x.PersonId))
                    .Select(x => x.Amount);

                var ethnicTypeDonationCount = totalGivingForEthnicType.Count();

                model.EthnicTypeStats[ethnicType] = new EthnicTypeStat
                {
                    EthnicType = ethnicType,
                    Count = peopleByEthnicType.Count(),
                    Percentage = peopleByEthnicType.Any() ? Utilities.GetPercent(peopleByEthnicType.Count(), model.PeopleByDate.Count) : 0,
                    AverageDonation = ethnicTypeDonationCount > 0 ? totalGivingForEthnicType.Average() : 0m
                };

                // Aggregate totals for overall average calculation
                totalDonationsAcrossAllEthnicTypes += totalGivingForEthnicType.Sum();
                totalDonationsCountAcrossAllEthnicTypes += ethnicTypeDonationCount;
            }

            model.TotalEthnicTypeCount = model.EthnicTypeStats.Sum(x => x.Value.Count);
            model.TotalEthnicTypePercentage = model.EthnicTypeStats.Sum(x => x.Value.Percentage);
            model.TotalEthnicTypeAverageDonation = totalDonationsCountAcrossAllEthnicTypes > 0
                ? totalDonationsAcrossAllEthnicTypes / totalDonationsCountAcrossAllEthnicTypes
                : 0m;
        }

        private void CalculateEducationTypeStats(DonorsModel model)
        {
            // Initialize Education Type Statistics
            model.EducationTypeStats = new Dictionary<string, EducationTypeStat>();
            decimal totalDonationsAcrossAllEducationTypes = 0m;
            int totalDonationsCountAcrossAllEducationTypes = 0;

            foreach (var educationType in EducationTypes.Items)
            {
                var peopleByEducationType = model.PeopleByDate.Where(q => (q.Education.IsNotNullOrEmpty() && q.Education.EqualsIgnoreCase(educationType)) || (q.Education.IsNullOrEmpty() && educationType.EqualsIgnoreCase(EducationTypes.Other)));
                var totalGivingForEducationType = model.TotalGivingByDate
                    .Where(x => x.PersonId.IsNotNullOrEmpty() && peopleByEducationType.Select(p => p.Id).Contains(x.PersonId))
                    .Select(x => x.Amount);

                var educationTypeDonationCount = totalGivingForEducationType.Count();

                model.EducationTypeStats[educationType] = new EducationTypeStat
                {
                    EducationType = educationType,
                    Count = peopleByEducationType.Count(),
                    Percentage = peopleByEducationType.Any() ? Utilities.GetPercent(peopleByEducationType.Count(), model.PeopleByDate.Count) : 0,
                    AverageDonation = educationTypeDonationCount > 0 ? totalGivingForEducationType.Average() : 0m
                };

                // Aggregate totals for overall average calculation
                totalDonationsAcrossAllEducationTypes += totalGivingForEducationType.Sum();
                totalDonationsCountAcrossAllEducationTypes += educationTypeDonationCount;
            }

            model.TotalEducationTypeCount = model.EducationTypeStats.Sum(x => x.Value.Count);
            model.TotalEducationTypePercentage = model.EducationTypeStats.Sum(x => x.Value.Percentage);
            model.TotalEducationTypeAverageDonation = totalDonationsCountAcrossAllEducationTypes > 0
                ? totalDonationsAcrossAllEducationTypes / totalDonationsCountAcrossAllEducationTypes
                : 0m;
        }

        private void CalculateEmploymentStatusStats(DonorsModel model)
        {
            // Initialize Employment Status Statistics
            model.EmploymentStatusStats = new Dictionary<string, EmploymentStatusStat>();
            decimal totalDonationsAcrossAllEmploymentStatuses = 0m;
            int totalDonationsCountAcrossAllEmploymentStatuses = 0;

            foreach (var employmentStatus in EmploymentStatuses.Items)
            {
                var peopleByEmploymentStatus = model.PeopleByDate.Where(q => (q.EmploymentStatus.IsNotNullOrEmpty() && q.EmploymentStatus.EqualsIgnoreCase(employmentStatus)) || (q.EmploymentStatus.IsNullOrEmpty() && employmentStatus.EqualsIgnoreCase(EmploymentStatuses.Other)));
                var totalGivingForEmploymentStatus = model.TotalGivingByDate
                    .Where(x => x.PersonId.IsNotNullOrEmpty() && peopleByEmploymentStatus.Select(p => p.Id).Contains(x.PersonId))
                    .Select(x => x.Amount);

                var employmentStatusDonationCount = totalGivingForEmploymentStatus.Count();

                model.EmploymentStatusStats[employmentStatus] = new EmploymentStatusStat
                {
                    EmploymentStatus = employmentStatus,
                    Count = peopleByEmploymentStatus.Count(),
                    Percentage = peopleByEmploymentStatus.Any() ? Utilities.GetPercent(peopleByEmploymentStatus.Count(), model.PeopleByDate.Count) : 0,
                    AverageDonation = employmentStatusDonationCount > 0 ? totalGivingForEmploymentStatus.Average() : 0m
                };

                // Aggregate totals for overall average calculation
                totalDonationsAcrossAllEmploymentStatuses += totalGivingForEmploymentStatus.Sum();
                totalDonationsCountAcrossAllEmploymentStatuses += employmentStatusDonationCount;
            }

            model.TotalEmploymentStatusCount = model.EmploymentStatusStats.Sum(x => x.Value.Count);
            model.TotalEmploymentStatusPercentage = model.EmploymentStatusStats.Sum(x => x.Value.Percentage);
            model.TotalEmploymentStatusAverageDonation = totalDonationsCountAcrossAllEmploymentStatuses > 0
                ? totalDonationsAcrossAllEmploymentStatuses / totalDonationsCountAcrossAllEmploymentStatuses
                : 0m;
        }

        private List<Person> GetPeopleByAgeGroup(List<Person> people, string ageGroup)
        {
            switch (ageGroup)
            {
                case AgeGroupDemographics.FifteenNineteen:
                    return people.Where(q => q.Age.IsNotNullOrEmpty() && q.Age >= 15 && q.Age <= 19).ToList();
                case AgeGroupDemographics.TwentyTwentyFour:
                    return people.Where(q => q.Age.IsNotNullOrEmpty() && q.Age >= 20 && q.Age <= 24).ToList();
                case AgeGroupDemographics.TwentyFiveTwentyNine:
                    return people.Where(q => q.Age.IsNotNullOrEmpty() && q.Age >= 25 && q.Age <= 29).ToList();
                case AgeGroupDemographics.Thirties:
                    return people.Where(q => q.Age.IsNotNullOrEmpty() && q.Age >= 30 && q.Age <= 39).ToList();
                case AgeGroupDemographics.Forties:
                    return people.Where(q => q.Age.IsNotNullOrEmpty() && q.Age >= 40 && q.Age <= 49).ToList();
                case AgeGroupDemographics.Fifties:
                    return people.Where(q => q.Age.IsNotNullOrEmpty() && q.Age >= 50 && q.Age <= 59).ToList();
                case AgeGroupDemographics.Sixties:
                    return people.Where(q => q.Age.IsNotNullOrEmpty() && q.Age >= 60 && q.Age <= 69).ToList();
                case AgeGroupDemographics.Seventies:
                    return people.Where(q => q.Age.IsNotNullOrEmpty() && q.Age >= 70 && q.Age <= 79).ToList();
                case AgeGroupDemographics.Eighties:
                    return people.Where(q => q.Age.IsNotNullOrEmpty() && q.Age >= 80 && q.Age <= 89).ToList();
                case AgeGroupDemographics.Other:
                    return people.Where(q => q.Age.IsNullOrEmpty() || q.Age < 15 || q.Age >= 90).ToList();
                default:
                    return new List<Person>();
            }
        }
        #endregion

        #region Donor Status Report
        public DonorStatusReportViewModel GetDonorStatusReportViewModel(string churchId, DateRange dates = null)
        {
            var donorData = new DonorStatusReportViewModel
            {
                TotalGiving = new List<TotalGivingItem>(),
                People = new List<Person>(),
                TotalGivingByDate = new List<TotalGivingItem>(),
                PeopleByDate = new List<Person>(),
                DateRange = dates?.ToString() ?? "No date range specified"
            };

            // Fetch data
            var (digitalGiving, offlineGiving, users, activeGifts) = FetchData(churchId);

            // Add gifts to total giving
            AddGiftsToTotalGiving(donorData.TotalGiving, digitalGiving,
                gift => GetPaymentType(gift),
                gift => gift.Frequency.Equals(PaymentOccurrence.Recurring) && gift.ScheduledPaymentId.IsNotNullOrEmpty()
                    ? activeGifts.Any(a => a.Id.Equals(gift.ScheduledPaymentId) && !a.IsProcessed)
                        ? PaymentOccurrence.Recurring
                        : PaymentOccurrence.OneTime
                    : PaymentOccurrence.OneTime,
                gift => users.Find(z => z.Id.Equals(gift.UserId))?.PersonId,
                gift => gift.CreatedDate // Use CreatedDate for digital giving
            );

            // For offline giving (use DateReceived)
            AddGiftsToTotalGiving(donorData.TotalGiving, offlineGiving,
                gift => GetPaymentType(gift),
                gift => PaymentOccurrence.OneTime,
                gift => gift.PersonId,
                gift => gift.DateReceived // Use DateReceived for offline giving
            );

            // Fetch people data based on unique PersonIds
            var personIds = donorData.TotalGiving.Select(x => x.PersonId).Distinct();
            donorData.People = Work.Person.GetAllByPersonIds(churchId, personIds);

            // Filter data by date range if provided
            if (dates != null)
            {
                FilterData(donorData, dates);
            }
            else
            {
                donorData.TotalGivingByDate = donorData.TotalGiving;
                donorData.PeopleByDate = donorData.People;
            }

            // Compute statistics
            CalculateStatistics(donorData);

            return donorData;
        }

        private (List<Payment> DigitalGiving, List<OfflineGiving> OfflineGiving, List<ApplicationUser> Users, List<ScheduledPayment> ActiveGifts) FetchData(string churchId)
        {
            var digitalGiving = Work.Payment.GetAll(churchId);
            var offlineGiving = Work.OfflineGiving.GetAll(churchId);
            var users = Work.User.GetAllByChurchId(churchId);
            var activeGifts = Work.ScheduledPayment.GetAllUnprocessed(churchId);

            return (digitalGiving, offlineGiving, users, activeGifts);
        }

        private void FilterData(DonorStatusReportViewModel donorData, DateRange dates)
        {
            donorData.TotalGivingByDate = donorData.TotalGiving
                .Where(x => x.CreatedDate.Date >= dates.StartDate.Date && x.CreatedDate.Date <= dates.EndDate.Date)
                .ToList();
            donorData.PeopleByDate = donorData.People
                .Where(x => donorData.TotalGivingByDate.Select(q => q.PersonId).Contains(x.Id))
                .ToList();
        }

        private void CalculateStatistics(DonorStatusReportViewModel donorData)
        {
            var oneYearAgo = DateTime.Now.AddYears(-1).Date;
            var oneTimeGivingFilter = donorData.TotalGivingByDate
                .Where(q => q.Frequency.Equals(PaymentOccurrence.OneTime) && q.CreatedDate.Date > oneYearAgo && !string.IsNullOrEmpty(q.PersonId));

            donorData.TotalDonorCount = donorData.PeopleByDate.Count;

            donorData.FirstTimeDonorCount = oneTimeGivingFilter
                .GroupBy(x => x.PersonId)
                .Where(x => x.Count() == 1 && !x.Any(z => z.CreatedDate.Date < oneYearAgo))
                .Count();

            donorData.FirstTimeAverageDonation = oneTimeGivingFilter
                .GroupBy(x => x.PersonId)
                .Where(x => x.Count() == 1 && !x.Any(z => z.CreatedDate.Date < oneYearAgo))
                .SelectMany(x => x.Select(s => s.Amount))
                .DefaultIfEmpty(0m)
                .Average();

            donorData.RepeatDonorCount = donorData.TotalGivingByDate
                .Where(q => !string.IsNullOrEmpty(q.PersonId))
                .GroupBy(x => x.PersonId)
                .Where(x => x.Count() > 1)
                .Count();

            donorData.SecondTimeDonorsCount = oneTimeGivingFilter
                .GroupBy(x => x.PersonId)
                .Where(x => x.Count() == 2 && !x.Any(z => z.CreatedDate.Date < oneYearAgo))
                .Count();

            donorData.SecondTimeAverageDonation = oneTimeGivingFilter
                .GroupBy(x => x.PersonId)
                .Where(x => x.Count() == 2)
                .SelectMany(x => x.Select(s => s.Amount))
                .DefaultIfEmpty(0m)
                .Average();

            donorData.OccasionalDonorsCount = oneTimeGivingFilter
                .GroupBy(x => x.PersonId)
                .Where(x => x.Count() > 2 && x.Count() < 12)
                .Count();

            donorData.OccasionalAverageDonation = oneTimeGivingFilter
                .GroupBy(x => x.PersonId)
                .Where(x => x.Count() > 2 && x.Count() < 12)
                .SelectMany(x => x.Select(s => s.Amount))
                .DefaultIfEmpty(0m)
                .Average();

            donorData.RegularDonorsCount = oneTimeGivingFilter
                .GroupBy(x => x.PersonId)
                .Where(x => x.Count() > 11)
                .Count();

            donorData.RegularAverageDonation = oneTimeGivingFilter
                .GroupBy(x => x.PersonId)
                .Where(x => x.Count() > 11)
                .SelectMany(x => x.Select(s => s.Amount))
                .DefaultIfEmpty(0m)
                .Average();

            donorData.RecurringDonorsCount = oneTimeGivingFilter
                .GroupBy(x => x.PersonId)
                .Count();

            donorData.RecurringAverageDonation = oneTimeGivingFilter
                .Select(s => s.Amount)
                .DefaultIfEmpty(0m)
                .Average();

            //Determine inactive donors
            // Step 1: Identify donors who gave before the last 12 months
            var pastDonors = donorData.TotalGiving
                .Where(q => q.CreatedDate.Date < oneYearAgo && !string.IsNullOrEmpty(q.PersonId))
                .GroupBy(x => x.PersonId)
                .Select(x => x.Key)
                .ToList();

            // Step 2: Identify donors who gave within the last 12 months
            var recentDonors = donorData.TotalGivingByDate
                .Where(q => q.CreatedDate.Date >= oneYearAgo && !string.IsNullOrEmpty(q.PersonId))
                .GroupBy(x => x.PersonId)
                .Select(x => x.Key)
                .ToList();

            // Step 3: Inactive donors are those who are in pastDonors but not in recentDonors
            var inactiveDonors = pastDonors.Except(recentDonors);

            // Calculate inactive donor count
            donorData.InActiveDonorsCount = inactiveDonors.Count();

            // Calculate average donation for inactive donors
            donorData.InActiveAverageDonation = donorData.TotalGiving
                .Where(q => inactiveDonors.Contains(q.PersonId))
                .Select(s => s.Amount)
                .DefaultIfEmpty(0m)
                .Average();

            donorData.TotalAverageDonation = donorData.TotalGivingByDate
                    .Where(q => q.CreatedDate.Date > oneYearAgo && !string.IsNullOrEmpty(q.PersonId))
                    .Select(s => s.Amount)
                    .DefaultIfEmpty(0m)
                    .Average();

            donorData.FirstTimePercentage = Utilities.GetPercent(donorData.FirstTimeDonorsCount, donorData.TotalDonorCount);
            donorData.SecondTimePercentage = Utilities.GetPercent(donorData.SecondTimeDonorsCount, donorData.TotalDonorCount);
            donorData.OccasionalPercentage = Utilities.GetPercent(donorData.OccasionalDonorsCount, donorData.TotalDonorCount);
            donorData.RegularPercentage = Utilities.GetPercent(donorData.RegularDonorsCount, donorData.TotalDonorCount);
            donorData.RecurringPercentage = Utilities.GetPercent(donorData.RecurringDonorsCount, donorData.TotalDonorCount);
            donorData.InActivePercentage = Utilities.GetPercent(donorData.InActiveDonorsCount, donorData.TotalDonorCount);
            donorData.TotalPercentage = Utilities.GetPercent(donorData.TotalDonorCount, donorData.TotalDonorCount);
        }
        #endregion

        private FundData PrepareFundData(GivingSummaryDashboard model, FundDisplaySettings settings)
        {
            var fund = model.Funds.FirstOrDefault(x => x.Name.ContainsIgnoreCase(settings.FundName));
            if (fund == null)
            {
                return null;
            }

            var fundUrl = $"/reports/fundreport?fundId={fund.Id}&type={settings.Type}";
            var fundAmount = model.TotalGiving.Where(x => x.FundId == fund.Id).Sum(x => x.Amount).ToCurrencyString();

            return new FundData
            {
                FundUrl = fundUrl,
                FundAmount = fundAmount,
                BgClass = settings.BgClass,
                IconClass = settings.IconClass
            };
        }

        private GivingByCampus CalculateGivingByCampus<T>(T model, string campusId) where T : IGivingDashboard
        {
            // Check if campusId is provided or we're calculating for "No Campus"
            bool isNoCampus = string.IsNullOrEmpty(campusId);

            return new GivingByCampus
            {
                CampusId = campusId,
                TotalGiving = model.TotalGiving
                    .Where(x => isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId)
                    .Sum(x => x.Amount).ToCurrencyString(),
                DigitalGiving = model.DigitalGiving
                    .Where(x => isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId)
                    .Sum(x => x.Amount).ToCurrencyString(),
                OfflineGiving = model.OfflineGiving
                    .Where(x => isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId)
                    .Sum(x => x.Amount).ToCurrencyString(),
                OnlineGiving = model.DigitalGiving
                    .Where(x => x.DigitalPaymentType == DigitalPaymentTypes.Online
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),
                TextMessageGiving = model.DigitalGiving
                    .Where(x => x.DigitalPaymentType == DigitalPaymentTypes.TextToGive
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),
                OfferingPlateGiving = model.OfflineGiving
                    .Where(x => x.OfflinePaymentType == OfflinePaymentTypes.OfferingPlate
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),
                DropOffGiving = model.OfflineGiving
                    .Where(x => x.OfflinePaymentType == OfflinePaymentTypes.DropOff
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),
                MailedGiving = model.OfflineGiving
                    .Where(x => x.OfflinePaymentType == OfflinePaymentTypes.Mail
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),

                // DigitalGiving breakdown
                OnlineCardGiving = model.DigitalGiving
                    .Where(x => x.DigitalPaymentType == DigitalPaymentTypes.Online
                                && x.DigitalPaymentMethod == DigitalPaymentMethods.Card
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),
                OnlineAchGiving = model.DigitalGiving
                    .Where(x => x.DigitalPaymentType == DigitalPaymentTypes.Online
                                && x.DigitalPaymentMethod == DigitalPaymentMethods.ACH
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),
                TextMessageCardGiving = model.DigitalGiving
                    .Where(x => x.DigitalPaymentType == DigitalPaymentTypes.TextToGive
                                && x.DigitalPaymentMethod == DigitalPaymentMethods.Card
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),
                TextMessageAchGiving = model.DigitalGiving
                    .Where(x => x.DigitalPaymentType == DigitalPaymentTypes.TextToGive
                                && x.DigitalPaymentMethod == DigitalPaymentMethods.ACH
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),

                // OfflineGiving breakdown
                OfferingPlateCashGiving = model.OfflineGiving
                    .Where(x => x.OfflinePaymentType == OfflinePaymentTypes.OfferingPlate
                                && x.OfflinePaymentMethod == OfflinePaymentMethods.Cash
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),
                OfferingPlateCheckGiving = model.OfflineGiving
                    .Where(x => x.OfflinePaymentType == OfflinePaymentTypes.OfferingPlate
                                && x.OfflinePaymentMethod == OfflinePaymentMethods.Check
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),
                DropOffCashGiving = model.OfflineGiving
                    .Where(x => x.OfflinePaymentType == OfflinePaymentTypes.DropOff
                                && x.OfflinePaymentMethod == OfflinePaymentMethods.Cash
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),
                DropOffCheckGiving = model.OfflineGiving
                    .Where(x => x.OfflinePaymentType == OfflinePaymentTypes.DropOff
                                && x.OfflinePaymentMethod == OfflinePaymentMethods.Check
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),
                MailedCashGiving = model.OfflineGiving
                    .Where(x => x.OfflinePaymentType == OfflinePaymentTypes.Mail
                                && x.OfflinePaymentMethod == OfflinePaymentMethods.Cash
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),
                MailedCheckGiving = model.OfflineGiving
                    .Where(x => x.OfflinePaymentType == OfflinePaymentTypes.Mail
                                && x.OfflinePaymentMethod == OfflinePaymentMethods.Check
                                && (isNoCampus ? string.IsNullOrEmpty(x.CampusId) : x.CampusId == campusId))
                    .Sum(x => x.Amount).ToCurrencyString(),
            };
        }

        private GivingByFund CalculateGivingByFund(IGivingDashboard model, string fundId)
        {
            return new GivingByFund
            {
                FundId = fundId,
                TotalGiving = model.TotalGiving.Where(x => x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                DigitalGiving = model.DigitalGiving.Where(x => x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                OfflineGiving = model.OfflineGiving.Where(x => x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                OnlineGiving = model.DigitalGiving.Where(x => x.DigitalPaymentType == DigitalPaymentTypes.Online && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                TextMessageGiving = model.DigitalGiving.Where(x => x.DigitalPaymentType == DigitalPaymentTypes.TextToGive && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                OfferingPlateGiving = model.OfflineGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.OfferingPlate && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                DropOffGiving = model.OfflineGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.DropOff && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                MailedGiving = model.OfflineGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.Mail && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),

                // DigitalGiving breakdown
                OnlineCardGiving = model.DigitalGiving.Where(x => x.DigitalPaymentType == DigitalPaymentTypes.Online && x.DigitalPaymentMethod == DigitalPaymentMethods.Card && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                OnlineAchGiving = model.DigitalGiving.Where(x => x.DigitalPaymentType == DigitalPaymentTypes.Online && x.DigitalPaymentMethod == DigitalPaymentMethods.ACH && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                TextMessageCardGiving = model.DigitalGiving.Where(x => x.DigitalPaymentType == DigitalPaymentTypes.TextToGive && x.DigitalPaymentMethod == DigitalPaymentMethods.Card && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                TextMessageAchGiving = model.DigitalGiving.Where(x => x.DigitalPaymentType == DigitalPaymentTypes.TextToGive && x.DigitalPaymentMethod == DigitalPaymentMethods.ACH && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),

                // OfflineGiving breakdown
                OfferingPlateCashGiving = model.OfflineGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.OfferingPlate && x.OfflinePaymentMethod == OfflinePaymentMethods.Cash && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                OfferingPlateCheckGiving = model.OfflineGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.OfferingPlate && x.OfflinePaymentMethod == OfflinePaymentMethods.Check && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                DropOffCashGiving = model.OfflineGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.DropOff && x.OfflinePaymentMethod == OfflinePaymentMethods.Cash && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                DropOffCheckGiving = model.OfflineGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.DropOff && x.OfflinePaymentMethod == OfflinePaymentMethods.Check && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                MailedCashGiving = model.OfflineGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.Mail && x.OfflinePaymentMethod == OfflinePaymentMethods.Cash && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
                MailedCheckGiving = model.OfflineGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.Mail && x.OfflinePaymentMethod == OfflinePaymentMethods.Check && x.FundId == fundId).Sum(x => x.Amount).ToCurrencyString(),
            };
        }

        // Helper method to add gifts to TotalGiving
        private void AddGiftsToTotalGiving<T>(
            List<TotalGivingItem> totalGiving,
            IEnumerable<T> gifts,
            Func<T, string> getPaymentType = null,
            Func<T, string> getFrequency = null,
            Func<T, string> getPersonId = null,
            Func<T, DateTime?> getDate = null) // Note that getDate now returns DateTime?
            where T : class, IGivingItem
        {
            foreach (var gift in gifts)
            {
                totalGiving.Add(new TotalGivingItem
                {
                    Amount = gift.Amount,
                    CreatedDate = getDate?.Invoke(gift) ?? gift.CreatedDate, // Use getDate or fallback to CreatedDate if null
                    CampusId = gift.CampusId,
                    FundId = gift.FundId,
                    PaymentType = getPaymentType != null ? getPaymentType(gift) : GetPaymentType(gift),
                    Frequency = getFrequency != null ? getFrequency(gift) : PaymentOccurrence.OneTime,
                    PersonId = getPersonId != null ? getPersonId(gift) : null
                });
            }
        }

        private string GetPaymentType<T>(T gift) where T : class
        {
            // Gets the DigitalPaymentType or OfflinePaymentType based on the type of gift passed in (Payment or OfflinePayment).
            var type = gift.GetType();
            var paymentTypeProperty = type.GetProperty("DigitalPaymentType") ?? type.GetProperty("OfflinePaymentType");
            return paymentTypeProperty?.GetValue(gift) as string;
        }

        //Not sure if this is used anymore. Verify before removing!
        public ReportBuilder GetGivingReportData(ReportDashboard dashboard, string churchId)
        {
            var reportData = Work.Payment.GetAll(churchId);

            var campuses = Work.Campus.GetAll(churchId);

            if (!string.IsNullOrEmpty(dashboard.StartDate))
            {
                var start = Convert.ToDateTime(dashboard.StartDate);
                reportData = reportData.Where(x => x.CreatedDate >= start).ToList();
            }

            if (!string.IsNullOrEmpty(dashboard.EndDate))
            {
                var end = Convert.ToDateTime(dashboard.EndDate);
                reportData = reportData.Where(x => x.CreatedDate <= end).ToList();
            }

            if (!string.IsNullOrEmpty(dashboard.PresetDateRange))
            {
                var fromDate = new DateTime();
                switch (dashboard.PresetDateRange)
                {
                    case PresetDateRange.Week:
                        fromDate = DateTime.Now.AddDays(-7);
                        break;
                    case PresetDateRange.Month:
                        fromDate = DateTime.Now.AddMonths(-1);
                        break;
                    case PresetDateRange.Year:
                        fromDate = DateTime.Now.AddYears(-1);
                        break;
                    default:
                        fromDate = DateTime.MinValue;
                        break;
                }

                reportData = reportData.Where(x => x.CreatedDate >= fromDate).ToList();
            }

            if (dashboard.Campus.Count > 0)
            {
                reportData = reportData.Where(x => dashboard.Campus.Contains(x.CampusId)).ToList();
            }

            var reportBuilder = new ReportBuilder
            {
                XAxisLabels = new List<string>(),
                ReportType = GraphTypes.BarGraph
            };

            var count = 0;
            //var borderColorZero = "black";
            const string borderColorZero = "#ffc0cb";
            const string borderColorOne = "green";
            const string borderColorTwo = "red";
            const string borderColorThree = "blue";
            const string borderColorFour = "orange";
            const string borderColorFive = "yellow";

            foreach (var campus in campuses)
            {
                reportBuilder.XAxisLabels.Add(campus.Name);

                var dataSet = new ReportDataSet
                {
                    Label = campus.Name,
                    BorderWidth = "2"
                };

                switch (count)
                {
                    case 0:
                        dataSet.BorderColor = borderColorZero;
                        break;
                    case 1:
                        dataSet.BorderColor = borderColorOne;
                        break;
                    case 2:
                        dataSet.BorderColor = borderColorTwo;
                        break;
                    case 3:
                        dataSet.BorderColor = borderColorThree;
                        break;
                    case 4:
                        dataSet.BorderColor = borderColorFour;
                        break;
                    case 5:
                        dataSet.BorderColor = borderColorFive;
                        break;
                    default:
                        dataSet.BorderColor = borderColorZero;
                        break;
                }
                count++;

                foreach (var payment in reportData.Where(x => x.CampusId == campus.Id).ToList())
                {
                    dataSet.LinearData.Add((int)Math.Round(payment.Amount, 0));
                }

                reportBuilder.ReportDataSets.Add(dataSet);
            }

            return reportBuilder;
        }
        #endregion

        #region Prayer Requests Reports
        //public PrayerRequestsSummary GetPrayerRequestsSummary(string churchId, DateRange dateRange, bool includeAverageResponseTimes)
        //{
        //    var allPrayerRequests = Work.PrayerRequest.GetAll(churchId, ExtensionMethods.GetCurrentYearDateRange());
        //    var categories = Work.PrayerRequest.GetAllCategories();
        //    var prayerRequestsByDate = Work.PrayerRequest.GetAll(churchId, dateRange);

        //    var model = new PrayerRequestsSummary
        //    {
        //        AllPrayerRequests = allPrayerRequests,
        //        Categories = categories,
        //        PrayerRequestsByDate = prayerRequestsByDate,
        //        TotalPrayerRequestsYTD = allPrayerRequests.Count,
        //        HighPriorityRequestsYTD = allPrayerRequests.Count(q => q.HighPriority),
        //        ConfidentialRequestsYTD = allPrayerRequests.Count(q => q.Confidential),
        //        FollowUpRequiredRequestsYTD = allPrayerRequests.Count(q => q.FollowUpRequired),
        //        HighPriorityRequestsByDate = prayerRequestsByDate.Count(q => q.HighPriority),
        //        HighPriorityRequestsNotPrayedOverByDate = prayerRequestsByDate.Count(q => q.HighPriority && !q.PrayedOver),
        //        ConfidentialRequestsByDate = prayerRequestsByDate.Count(q => q.Confidential),
        //        ConfidentialRequestsNotPrayedOverByDate = prayerRequestsByDate.Count(q => q.Confidential && !q.PrayedOver),
        //        FollowUpRequiredRequestsByDate = prayerRequestsByDate.Count(q => q.FollowUpRequired),
        //        FollowUpRequiredRequestsNotPrayedOverByDate = prayerRequestsByDate.Count(q => q.FollowUpRequired && !q.PrayedOver),
        //        FollowUpRequiredRequestsNotPrayedOverNotCompletedByDate = prayerRequestsByDate.Count(q => q.FollowUpRequired && !q.PrayedOver && q.FollowUpStatus != FollowUpStatuses.Completed),
        //        TotalRequestsByDate = prayerRequestsByDate.Count,
        //        TotalRequestsNotPrayedOverByDate = prayerRequestsByDate.Count(q => !q.PrayedOver),
        //        CategoryCounts = categories.ToDictionary(
        //            category => category.Id,
        //            category => prayerRequestsByDate.Count(q => !string.IsNullOrEmpty(q.CategoryId) && q.CategoryId.Equals(category.Id))
        //        ),
        //        UncategorizedRequestsByDate = prayerRequestsByDate.Count(q => string.IsNullOrEmpty(q.CategoryId)),
        //        AverageResponseTimes = includeAverageResponseTimes ? CalculateAverageResponseTimes(prayerRequestsByDate) : new Dictionary<string, string>()
        //    };

        //    return model;
        //}

        public PrayerRequestsSummary GetPrayerRequestsSummary(string churchId, DateRange dateRange, bool includeAverageResponseTimes)
        {
            var allPrayerRequests = Work.PrayerRequest.GetAll(churchId, ExtensionMethods.GetCurrentYearDateRange());
            var categories = Work.PrayerRequest.GetAllCategories();
            var prayerRequestsByDate = Work.PrayerRequest.GetAll(churchId, dateRange);

            var model = new PrayerRequestsSummary
            {
                AllPrayerRequests = allPrayerRequests,
                Categories = categories,
                PrayerRequestsByDate = prayerRequestsByDate,
                DateRange = dateRange.ToString(),

                // Populate YTD and ByDate counts for StatusCounts (bit fields)
                StatusCounts = new Dictionary<string, StatusCounts>
                {
                    [StatusKeys.TotalRequests] = new StatusCounts
                    {
                        YTD = allPrayerRequests.Count,
                        ByDate = prayerRequestsByDate.Count
                    },
                    [StatusKeys.HighPriorityRequests] = new StatusCounts
                    {
                        YTD = allPrayerRequests.Count(q => q.HighPriority),
                        ByDate = prayerRequestsByDate.Count(q => q.HighPriority)
                    },
                    [StatusKeys.ConfidentialRequests] = new StatusCounts
                    {
                        YTD = allPrayerRequests.Count(q => q.Confidential),
                        ByDate = prayerRequestsByDate.Count(q => q.Confidential)
                    },
                    [StatusKeys.FollowUpRequiredRequests] = new StatusCounts
                    {
                        YTD = allPrayerRequests.Count(q => q.FollowUpRequired),
                        ByDate = prayerRequestsByDate.Count(q => q.FollowUpRequired)
                    },
                    [StatusKeys.TotalNotPrayedOver] = new StatusCounts
                    {
                        YTD = allPrayerRequests.Count(q => !q.PrayedOver),
                        ByDate = prayerRequestsByDate.Count(q => !q.PrayedOver)
                    },
                    [StatusKeys.HighPriorityNotPrayedOver] = new StatusCounts
                    {
                        YTD = allPrayerRequests.Count(q => q.HighPriority && !q.PrayedOver),
                        ByDate = prayerRequestsByDate.Count(q => q.HighPriority && !q.PrayedOver)
                    },
                    [StatusKeys.ConfidentialNotPrayedOver] = new StatusCounts
                    {
                        YTD = allPrayerRequests.Count(q => q.Confidential && !q.PrayedOver),
                        ByDate = prayerRequestsByDate.Count(q => q.Confidential && !q.PrayedOver)
                    },
                    [StatusKeys.FollowUpRequiredNotPrayedOver] = new StatusCounts
                    {
                        YTD = allPrayerRequests.Count(q => q.FollowUpRequired && !q.PrayedOver),
                        ByDate = prayerRequestsByDate.Count(q => q.FollowUpRequired && !q.PrayedOver)
                    }
                },

                // Populate CategoryCounts with counts for each category
                CategoryCounts = categories.ToDictionary(
                    category => category.Id,
                    category => allPrayerRequests.Count(q => q.CategoryId == category.Id)
                ),

                // Populate sender counts
                SenderCounts = new SenderCounts
                {
                    TotalSendersYTD = allPrayerRequests.Select(q => q.PersonId).Distinct().Count(),
                    UniqueSendersYTD = allPrayerRequests.GroupBy(q => q.PersonId).Count(g => g.Count() == 1),
                    RepeatSendersYTD = allPrayerRequests.GroupBy(q => q.PersonId).Count(g => g.Count() > 1),
                    TotalSendersByDate = prayerRequestsByDate.Select(q => q.PersonId).Distinct().Count(),
                    UniqueSendersByDate = prayerRequestsByDate.GroupBy(q => q.PersonId).Count(g => g.Count() == 1),
                    RepeatSendersByDate = prayerRequestsByDate.GroupBy(q => q.PersonId).Count(g => g.Count() > 1)
                }
            };

            if (includeAverageResponseTimes)
            {
                model.AverageResponseTimes = CalculateAverageResponseTimes(prayerRequestsByDate);
            }

            return model;
        }

        //public PrayerRequestsSummary GetPrayerRequestReponseSummary(string churchId, DateRange dateRange)
        //{
        //    var allPrayerRequests = Work.PrayerRequest.GetAll(churchId, ExtensionMethods.GetCurrentYearDateRange());
        //    var prayerRequestsByDate = Work.PrayerRequest.GetAll(churchId, dateRange);

        //    var model = new PrayerRequestsSummary
        //    {
        //        PrayerRequestsByDate = prayerRequestsByDate,
        //        FollowUpRequiredRequestsByDate = prayerRequestsByDate.Count(q => q.FollowUpRequired),
        //        AverageResponseTimes = CalculateAverageResponseTimes(prayerRequestsByDate),
        //        TotalPrayerRequestsYTD = allPrayerRequests.Count,
        //        FollowUpRequiredRequestsYTD = allPrayerRequests.Count(q => q.FollowUpRequired)
        //    };

        //    return model;
        //}

        public PrayerRequestsSummary GetPrayerRequestResponseSummary(string churchId, DateRange dateRange)
        {
            var allPrayerRequests = Work.PrayerRequest.GetAll(churchId, ExtensionMethods.GetCurrentYearDateRange());
            var prayerRequestsByDate = Work.PrayerRequest.GetAll(churchId, dateRange);

            var incompleteCount = prayerRequestsByDate.Count(q => q.FollowUpRequired && q.FollowUpStatus.IsNotNullOrEmpty() && q.FollowUpStatus.EqualsIgnoreCase(FollowUpStatuses.Incomplete));
            var attemptedToContactCount = prayerRequestsByDate.Count(q => q.FollowUpRequired && q.FollowUpStatus.IsNotNullOrEmpty() && q.FollowUpStatus.EqualsIgnoreCase(FollowUpStatuses.AttemptedToContact));
            var completedCount = prayerRequestsByDate.Count(q => q.FollowUpRequired && q.FollowUpStatus.IsNotNullOrEmpty() && q.FollowUpStatus.EqualsIgnoreCase(FollowUpStatuses.Completed));

            var averageResponseTimes = CalculateAverageResponseTimes(prayerRequestsByDate);

            var followUpStatusCounts = new Dictionary<string, int>
            {
                [FollowUpStatuses.Incomplete] = incompleteCount,
                [FollowUpStatuses.AttemptedToContact] = attemptedToContactCount,
                [FollowUpStatuses.Completed] = completedCount
            };

            var model = new PrayerRequestsSummary
            {
                PrayerRequestsByDate = prayerRequestsByDate,
                StatusCounts = new Dictionary<string, StatusCounts>
                {
                    [StatusKeys.TotalRequests] = new StatusCounts
                    {
                        YTD = allPrayerRequests.Count,
                        ByDate = prayerRequestsByDate.Count
                    },
                    [StatusKeys.FollowUpRequiredRequests] = new StatusCounts
                    {
                        YTD = allPrayerRequests.Count(q => q.FollowUpRequired),
                        ByDate = prayerRequestsByDate.Count(q => q.FollowUpRequired)
                    }
                },
                AverageResponseTimes = averageResponseTimes,
                FollowUpStatusCounts = followUpStatusCounts
            };

            return model;
        }

        public Dictionary<string, string> CalculateAverageResponseTimes(IEnumerable<PrayerRequest> prayerRequestsByDate)
        {
            var result = new Dictionary<string, string>
            {
                { FollowUpStatuses.AttemptedToContact, "N/A" },
                { FollowUpStatuses.Completed, "N/A" }
            };

            var followUpRequiredRequests = prayerRequestsByDate.Where(q => q.FollowUpRequired);

            if (followUpRequiredRequests.Any(q => q.FollowUpStatus.IsNotNullOrEmpty()))
            {
                var attemptedRequests = followUpRequiredRequests.Where(q => q.FollowUpStatus.EqualsIgnoreCase(FollowUpStatuses.AttemptedToContact));
                if (attemptedRequests.Any())
                {
                    result[FollowUpStatuses.AttemptedToContact] = CalculateAverageTime(attemptedRequests);
                }

                var completedRequests = followUpRequiredRequests.Where(q => q.FollowUpStatus.EqualsIgnoreCase(FollowUpStatuses.Completed) && q.FollowUpCompleted);
                if (completedRequests.Any())
                {
                    result[FollowUpStatuses.Completed] = CalculateAverageTime(completedRequests);
                }
            }

            return result;
        }

        private string CalculateAverageTime(IEnumerable<PrayerRequest> requests)
        {
            // Step 1: Filter out requests where FollowUpDate is null
            var validRequests = requests
                .Where(q => q.FollowUpDate.HasValue)
                .ToList();

            // Step 2: Handle the case where no valid requests exist
            if (!validRequests.Any())
            {
                return "0 hours";
            }

            // Step 3: Calculate the average time difference in hours
            var averageHours = validRequests
                .Select(q => (q.FollowUpDate.Value - q.CreatedDate).TotalHours)
                .Average();

            // Step 4: Convert the average hours into days and hours
            var totalHours = Convert.ToInt32(averageHours);
            var days = totalHours / 24;
            var hours = totalHours % 24;

            // Step 5: Return the time in a human-readable format
            if (days >= 7)
            {
                // If the time is greater than or equal to a week, only show days
                return $"{days} {days.Pluralize("day")}";
            }
            else if (days > 0 && hours > 0)
            {
                // If the time is less than a week, show both days and hours
                return $"{days} {days.Pluralize("day")}, {hours} {hours.Pluralize("hour")}";
            }
            else if (days > 0)
            {
                // If there are only days (and less than a week), show days
                return $"{days} {days.Pluralize("day")}";
            }
            else
            {
                // If there are no days, just show hours
                return $"{hours} {hours.Pluralize("hour")}";
            }
        }
        #endregion        

        #region Report Helper
        public DateRange GetReportDateRange(string userId, string reportId, string dateRange)
        {
            var result = new DateRange();

            // If dateRange is provided, parse it and return the DateRange object
            if (!string.IsNullOrEmpty(dateRange))
            {
                var dates = dateRange.ToDateRange();
                result.StartDate = dates.StartDate;
                result.EndDate = dates.EndDate;
                return result;
            }

            // Fetch report with dates
            var report = GetWithDates(reportId);

            // Fetch report settings based on userId if dateRange is null or empty
            ReportSettings reportSettings = null;
            if (!string.IsNullOrEmpty(userId))
            {
                reportSettings = Work.Report.GetSettingByUser(userId);
            }

            // Determine StartDate
            result.StartDate = report?.StartDate
                ?? reportSettings?.DateFrom
                ?? new DateTime(DateTime.Now.Year, 1, 1);

            // Determine EndDate
            result.EndDate = report?.EndDate
                ?? reportSettings?.DateEnd
                ?? new DateTime(DateTime.Now.Year, 12, 31);

            return result;
        }
        #endregion

        #region BLL Report Operations
        public string GetAttendanceReportDashboard(string query, Church church, List<Campus> campuses, DateTime startDate, DateTime endDate)
        {
            if (!query.Contains("CreatedDate"))
            {
                var splitBeforeSelect = query.ToLower().SplitBefore("from");
                splitBeforeSelect += ", c.CreatedDate from ";
                var splitAfterSelect = query.ToLower().SplitAfter("from");
                query = $"{splitBeforeSelect}{splitAfterSelect}";
            }

            var where = $" WHERE c.CreatedDate BETWEEN '{startDate}' AND '{endDate}' ";

            if (church.IsNotNull() && !query.Contains("{church-id}"))
            {
                where += $" and c.ChurchId = '{church.Id}' ";
            }

            if (campuses.IsNotNull() && campuses.Count > 0 && !query.Contains("{campus-id}"))
            {
                var campusIds = campuses.Select(x => x.Id).ToList().CombineListToSQLString();
                where += $" and ci.CampusId in ({campusIds}) ";
            }

            var splitBefore = query.ToLower().SplitBefore("group by");
            var splitAfter = query.ToLower().SplitAfter("group by");
            return $"{splitBefore}{where}group by c.CreatedDate, {splitAfter}";
        }

        public string GetGivingReportDashboard(string query, Church church, List<Campus> campuses, DateTime startDate, DateTime endDate)
        {
            if (query.IsNullOrEmpty())
            {
                return query;
            }

            if (!query.Contains("CreatedDate"))
            {
                var splitBeforeSelect = query.ToLower().SplitBefore("from");
                splitBeforeSelect += ", p.CreatedDate from ";
                var splitAfterSelect = query.ToLower().SplitAfter("from");
                query = $"{splitBeforeSelect}{splitAfterSelect}";
            }

            var where = $" WHERE p.CreatedDate BETWEEN '{startDate}' AND '{endDate}' ";

            if (church.IsNotNull() && !query.Contains("{church-id}"))
            {
                where += $" and p.ChurchId = '{church.Id}' ";
            }

            if (campuses.IsNotNull() && campuses.Count > 0 && !query.Contains("{campus-id}"))
            {
                var campusIds = campuses.Select(x => x.Id).ToList().CombineListToSQLString();
                where += $" and p.CampusId in ({campusIds}) ";
            }

            var splitBefore = query.ToLower().SplitBefore("group by");
            var splitAfter = query.ToLower().SplitAfter("group by");

            return $"{splitBefore}{where}group by p.CreatedDate, {splitAfter}";
        }

        public string GetPrayerRequestsReportDashboard(string query, Church church, List<Campus> campuses, DateTime startDate, DateTime endDate)
        {
            if (!query.Contains("CreatedDate"))
            {
                var splitBeforeSelect = query.ToLower().SplitBefore("from");
                splitBeforeSelect += ", p.CreatedDate from ";
                var splitAfterSelect = query.ToLower().SplitAfter("from");
                query = $"{splitBeforeSelect}{splitAfterSelect}";
            }

            var where = $" WHERE p.CreatedDate BETWEEN '{startDate}' AND '{endDate}' ";

            if (church.IsNotNull() && !query.Contains("{church-id}"))
            {
                where += $" and p.ChurchId = '{church.Id}' ";
            }

            if (campuses.IsNotNull() && campuses.Count > 0 && !query.Contains("{campus-id}"))
            {
                var campusIds = campuses.Select(x => x.Id).ToList().CombineListToSQLString();
                where += $" and p.CampusId in ({campusIds}) ";
            }

            var splitBefore = query.ToLower().SplitBefore("group by");
            var splitAfter = query.ToLower().SplitAfter("group by");
            return $"{splitBefore}{where}group by p.CreatedDate, {splitAfter}";
        }

        public string GetEventAttendanceReportDashboard(string query, Church church, List<Campus> campuses, DateTime startDate, DateTime endDate)
        {
            // Ensure CreatedDate is included in the SELECT clause
            if (!query.ContainsIgnoreCase("CreatedDate"))
            {
                var splitBeforeSelect = query.SplitBefore("from");
                splitBeforeSelect += ", ea.CreatedDate from ";
                var splitAfterSelect = query.SplitAfter("from");
                query = $"{splitBeforeSelect}{splitAfterSelect}";
            }

            var where = $" WHERE ea.CreatedDate BETWEEN '{startDate:yyyy-MM-dd}' AND '{endDate:yyyy-MM-dd}' ";

            string result = null;

            if (query.ContainsIgnoreCase("group by"))
            {
                if (query.ContainsIgnoreCase("order by"))
                {
                    var splitOrderByBefore = query.SplitBefore("order by");
                    var splitOrderByAfter = query.SplitAfter("order by");
                    var splitBeforeGroupBy = splitOrderByBefore.SplitBefore("group by");
                    var splitAfterGroupBy = splitOrderByBefore.SplitAfter("group by");
                    result = $"{splitBeforeGroupBy}{where}group by ea.CreatedDate, {splitAfterGroupBy} order by{splitOrderByAfter}";
                }
                else
                {
                    var splitBeforeGroupBy = query.SplitBefore("group by");
                    var splitAfterGroupBy = query.SplitAfter("group by");
                    result = $"{splitBeforeGroupBy}{where}group by ea.CreatedDate, {splitAfterGroupBy}";
                }
            }
            else
            {
                if (query.ContainsIgnoreCase("order by"))
                {
                    var splitBeforeOrderBy = query.SplitBefore("order by");
                    var splitAfterOrderBy = query.SplitAfter("order by");
                    result = $"{splitBeforeOrderBy}{where}order by ea.CreatedDate, {splitAfterOrderBy}";
                }
            }

            return result;
        }

        public CustomReportBuilder GetReportAxisColumns(Report report, DataTable datatable, DateTime startDate, DateTime endDate)
        {
            var result = new CustomReportBuilder();
            var columns = datatable.Columns.Cast<DataColumn>().Select(x => x.ColumnName).ToList();
            var dateRange = Utilities.GetDateRange(startDate, endDate).Select(d => d.ToShortDateString()).Distinct().ToList();
            var dataSetKeyword = columns.FirstOrDefault(x => ReportUtilities.DataSetKeywords().Contains(x.ToLower()))
                                 ?? columns.FirstOrDefault(x => !ReportUtilities.YAxisColumnKeywords().Contains(x.ToLower()));
            var dataSetLabels = datatable.AsEnumerable().Select(dataRow => dataRow[dataSetKeyword].ToString()).Distinct().ToList();
            var yAxisColumnKeyword = report.YAxisColumn ?? columns.FirstOrDefault(x => ReportUtilities.YAxisColumnKeywords().Contains(x.ToLower()));
            var yAxisColumnList = datatable.AsEnumerable().Select(dataRow => dataRow[yAxisColumnKeyword].ToString()).ToList();
            var createdDateKeyword = columns.FirstOrDefault(x => ReportUtilities.CreatedDateKeywords().Contains(x.ToLower()));
            var campusKeyword = columns.FirstOrDefault(x => ReportUtilities.CampusKeywords().Contains(x.ToLower()));
            var campusList = campusKeyword == null ? new List<string>() : datatable.AsEnumerable().Select(dataRow => dataRow[campusKeyword].ToString()).ToList();

            foreach (DataRow dataRow in datatable.Rows)
            {
                var total = dataRow[yAxisColumnKeyword].ToString();
                var createdDate = createdDateKeyword == null ? null : dataRow[createdDateKeyword].ToString();
                var campusId = campusKeyword == null ? null : dataRow[campusKeyword].ToString();
                var label = dataRow[dataSetKeyword].ToString();

                result.Record.Add(new ChartRecordModel
                {
                    Title = label,
                    Total = total.ToInt32(),
                    CreatedDate = createdDate == null ? default : Convert.ToDateTime(createdDate),
                    CampusId = campusId
                });
            }

            result.DataSetLabels = dataSetLabels.OrderBy(x => x).ToList();
            result.Campuses = campusList.Any() ? Db.Campuses.Where(x => campusList.Contains(x.Id)).ToList() : new List<Campus>();
            result.XAxisTitle = report.XAxisTitle;
            result.YAxisTitle = report.YAxisTitle;
            result.YMultiAxisTitle = report.YMultiAxisTitle;
            result.GraphType = report.GraphType;
            result.XAxisColumns = dateRange;
            result.YAxisColumns = yAxisColumnList;
            result.StartDate = startDate;
            result.EndDate = endDate;
            result.Tab = ReportCategories.Custom;

            return result;
        }
        #endregion

        #region CRUD
        public Result<Report> Create(Report entity)
        {
            try
            {
                Create<Report>(entity);
                SaveChanges();
                return new Result<Report>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Report>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Report> Create(IEnumerable<Report> entity)
        {
            try
            {
                Create<Report>(entity);
                SaveChanges();
                return new Result<Report>
                {
                    List = entity.ToList(),
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Report>
                {
                    List = entity.ToList(),
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Report> Update(Report entity)
        {
            try
            {
                Update<Report>(entity);
                SaveChanges();
                return new Result<Report>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Report>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Report> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<Report>(entity);
                SaveChanges();
                return new Result<Report>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Report>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Report> Delete(Report entity)
        {
            try
            {
                Delete<Report>(entity);
                SaveChanges();
                return new Result<Report>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Report>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        #region CRUD Fixed Reports
        public Result<FixedReport> CreateFixed(IEnumerable<FixedReport> entity)
        {
            try
            {
                Create(entity);
                SaveChanges();
                return new Result<FixedReport>
                {
                    List = entity.ToList(),
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<FixedReport>
                {
                    List = entity.ToList(),
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<FixedReport> CreateFixed(FixedReport entity)
        {
            try
            {
                Create(entity);
                SaveChanges();
                return new Result<FixedReport>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<FixedReport>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<FixedReport> UpdateFixed(FixedReport entity)
        {
            try
            {
                Update(entity);
                SaveChanges();
                return new Result<FixedReport>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<FixedReport>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<FixedReport> DeleteFixed(string id)
        {
            try
            {
                var entity = GetFixedReport(id);
                Delete(entity);
                SaveChanges();
                return new Result<FixedReport>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<FixedReport>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<FixedReport> DeleteFixed(FixedReport entity)
        {
            try
            {
                Delete(entity);
                SaveChanges();
                return new Result<FixedReport>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<FixedReport>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        #region CRUD Favorite Reports
        public FavoriteReport CreateFavoriteReport(FavoriteReport entity)
        {
            Create(entity);
            SaveChanges();
            return entity;
        }

        public FavoriteReport CreateOrUpdateFavoriteReport(FavoriteReport entity)
        {
            if (entity.Id.IsNullOrEmpty())
            {
                Create(entity);
            }
            else
            {
                UpdateFavoriteReport(entity);
            }

            SaveChanges();

            return entity;
        }

        public void UpdateFavoriteReport(FavoriteReport entity)
        {
            Update(entity);
            SaveChanges();
        }

        public void DeleteFavoriteReport(string id)
        {
            var entity = GetFavoriteReport(id);
            Delete(entity);
            SaveChanges();
        }

        public void DeleteFavoriteReport(FavoriteReport entity)
        {
            Delete(entity);
            SaveChanges();
        }
        #endregion
    }
}