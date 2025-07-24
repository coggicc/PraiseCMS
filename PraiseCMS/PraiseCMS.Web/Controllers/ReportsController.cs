using ChartJS.Helpers.MVC;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.DataAccess.Singletons;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using PraiseCMS.Web.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]

    [RequirePermission(ModuleId = "1080633919ada9e25feeb84455a998")]
    public class ReportsController : BaseController
    {
        public ActionResult Index()
        {
            var model = work.Report.GetDashboard(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.Id, ExtensionMethods.GetCurrentYearDateRange());
            var modules = work.Module.GetAll();
            model.Reports.Select(x => { x.Class = modules.Any(q => q.Name.Equals(x.Name)) ? $"module-{modules.Find(q => q.Name.Equals(x.Name)).Id}" : string.Empty; return x; }).ToList();

            return View(model);
        }

        #region Manage Reports
        public ActionResult ViewReport(string id, string type)
        {
            var result = GetSingleReport(id, type, out var startDate, out var endDate);

            ViewBag.startDate = Convert.ToDateTime(startDate).ToShortDateString();
            ViewBag.endDate = Convert.ToDateTime(endDate).ToShortDateString();

            return View(result);
        }

        [HttpPost]
        public ActionResult ViewReport(string campusIds, string reportId, string category, string dateRange)
        {
            var result = GetSingleReport(campusIds, reportId, category, dateRange, out var startDate, out var endDate);

            ViewBag.startDate = DateTime.Parse(startDate).ToShortDateString();
            ViewBag.endDate = DateTime.Parse(endDate).ToShortDateString();

            return View(result);
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [RequirePermission(ModuleId = "7687864678d021d4159a6b4db0a399")]
        public ActionResult Manage()
        {
            return View(work.Report.GetByUserId(SessionVariables.CurrentUser.User.Id, SessionVariables.CurrentChurch.Id));
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult CreateReport(string categoryId, string name)
        {
            var report = new Report()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                ReportCategoryId = !string.IsNullOrEmpty(categoryId) ? categoryId : string.Empty,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            var reportView = new ReportView
            {
                CategoryName = name,
                Report = report,
                IsNew = true,
                ReportSettings = work.Report.GetSettingByUser(SessionVariables.CurrentUser.User.Id),
                ReportCategories = work.Report.GetAllCategories()
            };

            return View("CreateEdit", reportView);
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpPost]
        public ActionResult CreateReport(ReportView reportView)
        {
            if (ModelState.IsValid)
            {
                if (reportView.Report.ReportType.Equals(ReportTypes.Fixed) && string.IsNullOrEmpty(reportView.Report.ReportUrl))
                {
                    ModelState.AddModelError("Report.ReportUrl", "The report URL is required");
                    CreateAlertMessage("You must provide a report URL for fixed reports.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                    return View("CreateEdit", reportView);
                }

                if (reportView.Report.ReportType.Equals("Custom") && string.IsNullOrEmpty(reportView.Report.Query))
                {
                    ModelState.AddModelError("Report.Query", "The report query is required");
                    CreateAlertMessage("You must provide a report query for custom reports.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                    return View("CreateEdit", reportView);
                }

                if (reportView.Report.ReportType.Equals(ReportTypes.Fixed))
                {
                    work.Report.CreateFixed(new FixedReport
                    {
                        Id = Utilities.GenerateUniqueId(),
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now,
                        Description = reportView.Report.Description,
                        Name = reportView.Report.Name,
                        ReportCategoryId = reportView.Report.ReportCategoryId,
                        URL = reportView.Report.ReportUrl
                    });
                }
                else
                {
                    work.Report.Create(reportView.Report);
                }

                //SessionVariables.ReportCategories = work.Report.GetCurrentReports(SessionVariables.CurrentChurch.Id);
                CreateAlertMessage(Constants.SavedMessage, AlertMessageTypes.Success, AlertMessageIcons.Success);

                return RedirectToAction("index");
            }

            reportView.ReportSettings = work.Report.GetSettingByUser(SessionVariables.CurrentUser.User.Id);
            CreateAlertMessage("Please correct the errors below.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

            return View("CreateEdit", reportView);
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult EditReport(string id, string type)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Report report;

            if (type.IsNotNullOrEmpty() && type.EqualsIgnoreCase(ReportTypes.Fixed))
            {
                var fixedReport = work.Report.GetFixedReport(id);

                if (fixedReport.IsNotNullOrEmpty())
                {
                    report = new Report
                    {
                        ChurchId = SessionVariables.CurrentChurch.Id,
                        Id = fixedReport.Id,
                        ReportUrl = fixedReport.URL,
                        Description = fixedReport.Description,
                        ReportCategoryId = fixedReport.ReportCategoryId,
                        Name = fixedReport.Name,
                        ReportType = ReportTypes.Fixed,
                        IsDefaultDateRange = false,
                        StartDate = Convert.ToDateTime($"01/01/{DateTime.Now.Year}"),
                        EndDate = Convert.ToDateTime($"12/31/{DateTime.Now.Year}"),
                        CreatedBy = fixedReport.CreatedBy,
                        CreatedDate = fixedReport.CreatedDate,
                        ModifiedBy = fixedReport.ModifiedBy,
                        ModifiedDate = fixedReport.ModifiedDate
                    };
                }
                else
                {
                    return HttpNotFound();
                }
            }
            else
            {
                report = work.Report.Get(id);

                if (report.IsNullOrEmpty())
                {
                    return HttpNotFound();
                }
            }

            var reportSetting = work.Report.GetSettingByUser(SessionVariables.CurrentUser.User.Id);

            if (report.IsDefaultDateRange)
            {
                report.StartDate = reportSetting.DateFrom;
                report.EndDate = reportSetting.DateEnd;
            }

            var reportView = new ReportView
            {
                Report = report,
                IsNew = false,
                ReportSettings = reportSetting
            };

            return View("CreateEdit", reportView);
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditReport(ReportView reportView)
        {
            if (!ModelState.IsValid)
            {
                return View("CreateEdit", reportView);
            }

            if (reportView.Report.ReportType.Equals(ReportTypes.Fixed) && string.IsNullOrEmpty(reportView.Report.ReportUrl))
            {
                ModelState.AddModelError("Report.ReportUrl", "The report URL is required");
                CreateAlertMessage("You must provide a report URL for fixed reports.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                return View("CreateEdit", reportView);
            }

            if (reportView.Report.ReportType.Equals("Custom") && string.IsNullOrEmpty(reportView.Report.Query))
            {
                ModelState.AddModelError("Report.Query", "The report query is required");
                CreateAlertMessage("You must provide a report query for custom reports.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                return View("CreateEdit", reportView);
            }

            if (reportView.Report.ReportType.Equals(ReportTypes.Fixed))
            {
                var report = work.Report.GetFixedReport(reportView.Report.Id);
                report.ModifiedDate = DateTime.Now;
                report.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                report.Name = reportView.Report.Name;
                report.Description = reportView.Report.Description;
                report.ReportCategoryId = reportView.Report.ReportCategoryId;
                report.URL = reportView.Report.ReportUrl;
                work.Report.UpdateFixed(report);
            }
            else
            {
                var report = work.Report.Get(reportView.Report.Id);
                report.ModifiedDate = DateTime.Now;
                report.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                report.StartDate = reportView.Report.StartDate;
                report.EndDate = reportView.Report.EndDate;
                report.IsDefaultDateRange = reportView.Report.IsDefaultDateRange;
                report.Name = reportView.Report.Name;
                report.Description = reportView.Report.Description;
                report.ReportCategoryId = reportView.Report.ReportCategoryId;
                report.GraphType = reportView.Report.GraphType;
                report.Query = reportView.Report.Query;
                report.Instructions = reportView.Report.Instructions;
                report.XAxisColumn = reportView.Report.XAxisColumn;
                report.YAxisColumn = reportView.Report.YAxisColumn;
                report.XAxisTitle = reportView.Report.XAxisTitle;
                report.YAxisTitle = reportView.Report.YAxisTitle;
                report.YMultiAxisTitle = reportView.Report.YMultiAxisTitle;
                report.ReportUrl = reportView.Report.ReportUrl;
                work.Report.Update(report);
            }

            return RedirectToAction("index");
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult DeleteReport(string id, string type)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (type.IsNotNullOrEmpty() && type.EqualsIgnoreCase(ReportTypes.Fixed))
            {
                work.Report.DeleteFixed(id);
            }
            else
            {
                work.Report.Delete(id);
            }

            return RedirectToAction("index");
        }

        [HttpGet]
        public ActionResult _Favorite(string id)
        {
            var result = work.Report.MakeFavorite(id, SessionVariables.CurrentUser.User.Id);
            SessionVariables.ReportCategories = work.Report.GetCurrentReports(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.Id);

            return Json(new { Success = result.ResultType.Equals(ResultType.Success), result.Message }, JsonRequestBehavior.AllowGet);
        }
        #endregion

        public ActionResult ReportSearch(string ReportSearch)
        {
            var url = $"{ApplicationCache.Instance.EnvironmentConfiguration.Url}/reports/viewreport?Id={ReportSearch}";
            return Redirect(url);
        }

        [ChildActionOnly]
        public ActionResult _PrayerRequests(ReportDashboard dashboard)
        {
            var reportData = work.Payment.GetAll(SessionVariables.CurrentChurch.Id);
            var reportBuilder = new ReportBuilder();
            var chart = ReportHelper.GetVerticalBarChart(reportBuilder);
            ViewBag.Chart = new MvcHtmlString(chart.Draw("PrayerRequestsChart"));

            return PartialView("_PrayerRequests", reportData);
        }

        [ChildActionOnly]
        public ActionResult _Giving(ReportDashboard dashboard)
        {
            var reportData = work.Payment.GetAll(SessionVariables.CurrentChurch.Id);
            var reportBuilder = work.Report.GetGivingReportData(dashboard, SessionVariables.CurrentChurch.Id);
            var chart = ReportHelper.GetVerticalBarChart(reportBuilder);
            ViewBag.Chart = new MvcHtmlString(chart.Draw("myChart"));

            return PartialView("_Giving", reportData);
        }

        public ReportVM Generate(ReportDashboard dashboard)
        {
            var result = new ReportVM();

            #region Date filters
            var reportSettings = work.Report.GetSettingByUser(SessionVariables.CurrentUser.User.Id);

            var startDate = Convert.ToDateTime(dashboard.StartDate);
            var endDate = Convert.ToDateTime(dashboard.EndDate);

            if (dashboard.StartDate.IsNull())
            {
                if (reportSettings.IsNotNull())
                {
                    startDate = reportSettings.DateFrom;
                }
                else
                {
                    startDate = Convert.ToDateTime($"{DateTime.Today.Year}/01/01").Date;

                    if (DateTime.Today.Month >= 7)
                    {
                        startDate = Convert.ToDateTime($"{DateTime.Today.Year}/07/01").Date;
                    }
                }
            }

            if (dashboard.EndDate.IsNull())
            {
                if (reportSettings.IsNotNull())
                {
                    endDate = reportSettings.DateEnd;
                }
                else
                {
                    endDate = Convert.ToDateTime($"{DateTime.Today.Year}/06/30").Date;

                    if (DateTime.Today.Month >= 7)
                    {
                        endDate = Convert.ToDateTime($"{DateTime.Today.Year}/12/30").Date;
                    }
                }
            }

            if (!string.IsNullOrEmpty(dashboard.PresetDateRange))
            {
                switch (dashboard.PresetDateRange)
                {
                    case PresetDateRange.Week:
                        startDate = DateTime.Now.AddDays(-7);
                        break;
                    case PresetDateRange.Month:
                        startDate = DateTime.Now.AddMonths(-1);
                        break;
                    case PresetDateRange.Year:
                        startDate = DateTime.Now.AddYears(-1);
                        break;
                    default:
                        startDate = DateTime.MinValue;
                        break;
                }
            }
            #endregion

            result.Giving = GetReport(new ReportDashboard
            {
                Campus = dashboard.Campus,
                EndDate = endDate.ToString(),
                ReportId = dashboard.ReportId,
                StartDate = startDate.ToString(),
                Tab = ReportCategories.Giving
            });

            result.PrayerRequests = GetReport(new ReportDashboard
            {
                Campus = dashboard.Campus,
                EndDate = endDate.ToString(),
                ReportId = dashboard.ReportId,
                StartDate = startDate.ToString(),
                Tab = ReportCategories.PrayerRequests
            });

            result.Attendance = GetReport(new ReportDashboard
            {
                Campus = dashboard.Campus,
                EndDate = endDate.ToString(),
                ReportId = dashboard.ReportId,
                StartDate = startDate.ToString(),
                Tab = ReportCategories.Attendance
            });

            result.Custom = GetReport(new ReportDashboard
            {
                Campus = dashboard.Campus,
                EndDate = endDate.ToString(),
                ReportId = dashboard.ReportId,
                StartDate = startDate.ToString(),
                Tab = ReportCategories.Custom
            });

            var favoriteReportCharts = new List<ChartViewModel>();
            //var favoriteReports = (from f in db.FavoriteReports
            //                       join r in db.Reports on f.ReportId equals r.Id
            //                       join c in db.ReportCategories on r.ReportCategoryId equals c.Id
            //                       where f.UserId == SessionVariables.CurrentUser.User.Id
            //                       select new { f.ReportId, ReportCategoryName = c.Name }).ToList();

            var favoriteReports = (from f in work.Report.GetAllFavoriteReports(SessionVariables.CurrentUser.User.Id)
                                   let r = work.Report.Get(f.ReportId) // Fetch the report by its ID
                                   let c = work.Report.GetAllCategories().FirstOrDefault(cat => cat.Id == r.ReportCategoryId) // Fetch the category by its ID
                                   where r != null && c != null // Ensure that the report and category exist
                                   select new { f.ReportId, ReportCategoryName = c.Name }).ToList();

            foreach (var item in favoriteReports)
            {
                var chart = GetReport(new ReportDashboard
                {
                    Campus = dashboard.Campus,
                    EndDate = endDate.ToString(),
                    ReportId = dashboard.ReportId,
                    StartDate = startDate.ToString(),
                    Tab = ReportCategories.Favorites
                });

                favoriteReportCharts.Add(new ChartViewModel { Chart = chart.ToString(), ReportCategoryName = item.ReportCategoryName });
            }

            result.Favorites = favoriteReportCharts;

            return result;
        }

        public ReportVM Generate(string reportId = null)
        {
            var result = new ReportVM();

            #region Date filters
            var reportSettings = work.Report.GetSettingByUser(SessionVariables.CurrentUser.User.Id);

            DateTime startDate;
            DateTime endDate;

            if (reportSettings.IsNotNull())
            {
                startDate = reportSettings.DateFrom;
            }
            else
            {
                startDate = Convert.ToDateTime($"{DateTime.Today.Year}/01/01").Date;

                if (DateTime.Today.Month >= 7)
                {
                    startDate = Convert.ToDateTime($"{DateTime.Today.Year}/07/01").Date;
                }
            }

            if (reportSettings.IsNotNull())
            {
                endDate = reportSettings.DateEnd;
            }
            else
            {
                endDate = Convert.ToDateTime($"{DateTime.Today.Year}/06/30").Date;

                if (DateTime.Today.Month >= 7)
                {
                    endDate = Convert.ToDateTime($"{DateTime.Today.Year}/12/30").Date;
                }
            }
            #endregion

            var favoriteReportCharts = new List<ChartViewModel>();
            var favoriteReports = (from f in work.Report.GetAllFavoriteReports(SessionVariables.CurrentUser.User.Id)
                                   let r = work.Report.Get(f.ReportId) // Fetch the report by its ID
                                   let c = work.Report.GetAllCategories().FirstOrDefault(cat => cat.Id == r.ReportCategoryId) // Fetch the category by its ID
                                   where r != null && c != null // Ensure that the report and category exist
                                   select new { f.ReportId, ReportCategoryName = c.Name }).ToList();

            foreach (var item in favoriteReports)
            {
                var chart = GetReport(new ReportDashboard
                {
                    Tab = item.ReportCategoryName,
                    ReportId = item.ReportId,
                    StartDate = startDate.ToString(),
                    EndDate = endDate.ToString()
                });

                favoriteReportCharts.Add(new ChartViewModel { Chart = chart.ToString(), ReportCategoryName = item.ReportCategoryName });
            }

            result.Favorites = favoriteReportCharts;

            result.Giving = GetReport(new ReportDashboard
            {
                Tab = ReportCategories.Giving,
                ReportId = reportId,
                StartDate = startDate.ToString(),
                EndDate = endDate.ToString()
            });

            result.PrayerRequests = GetReport(new ReportDashboard
            {
                Tab = ReportCategories.PrayerRequests,
                ReportId = reportId,
                StartDate = startDate.ToString(),
                EndDate = endDate.ToString()
            });

            result.Attendance = GetReport(new ReportDashboard
            {
                Tab = ReportCategories.Attendance,
                ReportId = reportId,
                StartDate = startDate.ToString(),
                EndDate = endDate.ToString()
            });

            result.Custom = GetReport(new ReportDashboard
            {
                Tab = ReportCategories.Custom,
                ReportId = reportId,
                StartDate = startDate.ToString(),
                EndDate = endDate.ToString()
            });

            return result;
        }

        public ReportModel GetSingleReport(string id, string type, out string startDate, out string endDate)
        {
            #region Default date range
            var report = work.Report.Get(id);

            if (report.IsDefaultDateRange)
            {
                var reportSettings = work.Report.GetSettingByUser(SessionVariables.CurrentUser.User.Id);

                if (reportSettings.IsNotNull())
                {
                    startDate = reportSettings.DateFrom.ToString();
                    endDate = reportSettings.DateEnd.ToString();
                }
                else
                {
                    startDate = DateTime.Parse($"01/01/{DateTime.Now.Year}").ToString();
                    endDate = DateTime.Parse($"12/31/{DateTime.Now.Year}").ToString();
                }
            }
            else
            {
                startDate = report.StartDate.ToString();
                endDate = report.EndDate.ToString();
            }

            if (startDate.IsNullOrEmpty() && startDate.IsNullOrEmpty())
            {
                var date = Utilities.GetDateRange();
                startDate = date.StartDate.ToString();
                endDate = date.EndDate.ToString();
            }
            #endregion

            var result = new ReportModel
            {
                ReportName = report.Name,
                ReportId = id,
                Category = type
            };
            var campus = new List<string>();

            if (!SessionVariables.CurrentUser.IsAdmin)
            {
                campus.Add(SessionVariables.CurrentUser.Settings.PrimaryChurchCampusId);
            }

            result.Report = GetReport(new ReportDashboard
            {
                Report = report,
                Tab = type,
                ReportId = id,
                StartDate = startDate,
                EndDate = endDate,
                Campus = campus
            }).ToString();

            return result;
        }

        public ReportModel GetSingleReport(string campusIds, string reportId, string category, string dateRange, out string startDate, out string endDate)
        {
            #region Date range filters
            var report = work.Report.Get(reportId);

            if (dateRange.IsNotNullOrEmpty())
            {
                var dates = dateRange.ToDateRange();
                startDate = dates.StartDate.ToString();
                endDate = dates.EndDate.ToString();
            }
            else
            {
                if (report.IsDefaultDateRange)
                {
                    var reportSettings = work.Report.GetSettingByUser(SessionVariables.CurrentUser.User.Id);
                    startDate = reportSettings?.DateFrom.ToString();
                    endDate = reportSettings?.DateEnd.ToString();
                }
                else
                {
                    startDate = report.StartDate.ToString();
                    endDate = report.EndDate.ToString();
                }
            }

            if (startDate.IsNullOrEmpty() && endDate.IsNullOrEmpty())
            {
                var date = Utilities.GetDateRange();
                startDate = date.StartDate.ToString();
                endDate = date.EndDate.ToString();
            }
            #endregion

            var result = new ReportModel
            {
                ReportName = report.Name,
                ReportId = reportId,
                Category = category,
                CampusIds = campusIds
            };

            if (!SessionVariables.CurrentUser.IsAdmin)
            {
                result.CampusIdList.Add(SessionVariables.CurrentUser.Settings.PrimaryChurchCampusId);
            }
            else
            {
                result.CampusIdList = campusIds.DeserializeToList();
            }

            result.Report = GetReport(new ReportDashboard
            {
                Report = report,
                Tab = category,
                ReportId = reportId,
                StartDate = startDate,
                EndDate = endDate,
                Campus = result.CampusIdList
            }).ToString();

            return result;
        }

        private object GetReport(ReportDashboard dashboard)
        {
            var report = dashboard.Report;
            var campus = dashboard.Campus;
            var startDate = Convert.ToDateTime(dashboard.StartDate);
            var endDate = dashboard.EndDate.IsNotNullOrEmpty() && !dashboard.EndDate.Contains("Invalid") ? Convert.ToDateTime(dashboard.EndDate) : startDate.AddDays(+1);

            var campuses = dashboard.Campus.IsNotNull() && dashboard.Campus.Count > 0 ? SessionVariables.Campuses.Where(x => campus.Contains(x.Id)).ToList() : SessionVariables.Campuses;

            var data = new List<ReportViewModel>();
            //var reportData = new DataAccess.BLL.ReportOperations();
            var category = work.Report.GetCategory(report.ReportCategoryId);

            string sqlQuery = null;

            if (category.Name == ReportCategories.Attendance)
            {
                if (report.Query.IndexOf("from eventattendance", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    sqlQuery = work.Report.GetEventAttendanceReportDashboard(report.Query, SessionVariables.CurrentChurch, campuses, startDate, endDate);
                }
                else
                {
                    sqlQuery = work.Report.GetAttendanceReportDashboard(report.Query, SessionVariables.CurrentChurch, campuses, startDate, endDate);
                }
            }
            else if (category.Name == ReportCategories.Giving)
            {
                sqlQuery = work.Report.GetGivingReportDashboard(report.Query, SessionVariables.CurrentChurch, campuses, startDate, endDate);
            }
            else if (category.Name == ReportCategories.PrayerRequests)
            {
                sqlQuery = work.Report.GetPrayerRequestsReportDashboard(report.Query, SessionVariables.CurrentChurch, campuses, startDate, endDate);
            }
            else
            {
                sqlQuery = report.Query;
            }

            var result = adoData.ReadViaQuery(sqlQuery);

            if (report.GraphType.IsNullOrEmpty())
            {
                return RenderPartialToString("_Custom", result.ToDynamic(), ControllerContext);
            }

            var reportAxisColumn = work.Report.GetReportAxisColumns(report, result, startDate, endDate);

            return ReportGenerator.GenerateChartForCustomReport(reportAxisColumn);
        }

        #region Manage Groups
        public ActionResult Groups()
        {
            var reportGroups = work.Report.GetAllGroups(SessionVariables.CurrentUser.User.Id) ?? new List<ReportGroup>();
            return View(reportGroups);
        }

        [HttpGet]
        public ActionResult _CreateGroup()
        {
            var group = new ReportGroup()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                UserId = SessionVariables.CurrentUser.User.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            return PartialView("_CreateEditGroup", group);
        }

        [HttpPost]
        public ActionResult _CreateGroup(ReportGroup group)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEditGroup", group);
            }

            work.Report.CreateGroup(group);

            return AjaxRedirectTo("/reports/groups");
        }

        [HttpGet]
        public ActionResult _EditGroup(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var group = work.Report.GetGroup(id);

            if (group == null)
            {
                return HttpNotFound();
            }

            return PartialView("_CreateEditGroup", group);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditGroup(ReportGroup group)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEditGroup", group);
            }

            group.ModifiedDate = DateTime.Now;
            group.ModifiedBy = SessionVariables.CurrentUser.User.Id;

            work.Report.UpdateGroup(group);

            return AjaxRedirectTo("/reports/groups");
        }

        [HttpGet]
        public ActionResult DeleteGroup(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            work.Report.DeleteGroup(id);

            return RedirectToAction("groups");
        }

        public ActionResult GetGroupsPartial(string id)
        {
            var data = work.Report.GetReportGroups(id, SessionVariables.CurrentUser.User.Id, SessionVariables.CurrentChurch.Id);
            return PartialView(data);
        }

        public void AddRemoveReportGroup(string reportId, string groupId)
        {
            work.Report.AddRemoveReportGroup(reportId, groupId, SessionVariables.CurrentUser.User.Id, SessionVariables.CurrentChurch.Id);
        }
        #endregion

        #region Manage Group Email Settings
        [HttpGet]
        public ActionResult _CreateGroupEmailSettings(string reportGroupId)
        {
            if (string.IsNullOrEmpty(reportGroupId))
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var groupEmailSettings = new ReportGroupEmailSetting()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                UserId = SessionVariables.CurrentUser.User.Id,
                ReportGroupId = reportGroupId,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            var model = new ReportGroupEmailSettingsView
            {
                ReportGroupEmailSettings = groupEmailSettings,
                Users = work.User.GetUsersByChurch(SessionVariables.CurrentChurch.Id)
            };

            return PartialView("_CreateEditGroupEmailSettings", model);
        }

        [HttpPost]
        public ActionResult _CreateGroupEmailSettings(ReportGroupEmailSettingsView model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEditGroupEmailSettings", model.ReportGroupEmailSettings);
            }

            work.Report.CreateGroupEmailSetting(model.ReportGroupEmailSettings);

            return AjaxRedirectTo("/reports");
        }

        [HttpGet]
        public ActionResult _EditGroupEmailSettings(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var groupEmailSettings = work.Report.GetGroupEmailSetting(id);

            if (groupEmailSettings == null)
            {
                return HttpNotFound();
            }

            var model = new ReportGroupEmailSettingsView
            {
                ReportGroupEmailSettings = groupEmailSettings,
                Users = work.User.GetUsersByChurch(SessionVariables.CurrentChurch.Id)
            };

            return PartialView("_CreateEditGroupEmailSettings", model.ReportGroupEmailSettings);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditGroupEmailSettings(ReportGroupEmailSettingsView model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEditGroup", model.ReportGroupEmailSettings);
            }

            model.ReportGroupEmailSettings.ModifiedDate = DateTime.Now;
            model.ReportGroupEmailSettings.ModifiedBy = SessionVariables.CurrentUser.User.Id;

            work.Report.UpdateGroupEmailSetting(model.ReportGroupEmailSettings);

            return AjaxRedirectTo("/reports");
        }

        [HttpGet]
        public ActionResult DeleteGroupEmailSettings(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            work.Report.DeleteGroupEmailSetting(id);

            return RedirectToAction("groups");
        }
        #endregion

        #region Manage Categories
        [RequireRole(Role = Roles.SuperAdmin)]
        public ActionResult Categories()
        {
            var reportCategories = work.Report.GetAllCategories(SessionVariables.CurrentChurch.Id) ?? new List<ReportCategory>();
            return View(reportCategories);
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult CreateCategory()
        {
            var category = new ReportCategory()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            return PartialView("_CreateEditCategory", category);
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpPost]
        public ActionResult CreateCategory(ReportCategory category)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEditCategory", category);
            }

            work.Report.CreateCategory(category);
            return AjaxRedirectTo("/reports/categories");
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult EditCategory(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var category = work.Report.GetCategory(id);

            if (category == null)
            {
                return HttpNotFound();
            }

            return PartialView("_CreateEditCategory", category);
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCategory(ReportCategory category)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEditCategory", category);
            }

            category.ModifiedDate = DateTime.Now;
            category.ModifiedBy = SessionVariables.CurrentUser.User.Id;

            work.Report.UpdateCategory(category);

            return AjaxRedirectTo("/reports/categories");
        }

        [RequireRole(Role = Roles.SuperAdmin)]
        [HttpGet]
        public ActionResult DeleteCategory(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            work.Report.DeleteCategory(id);

            return RedirectToAction("categories");
        }
        #endregion

        #region Settings
        [RequirePermission(ModuleId = "14990257708c6034ad1d1d4e5d9af6")]
        public ActionResult Settings()
        {
            var setting = work.Report.GetSettingByUser(SessionVariables.CurrentUser.User.Id);

            if (setting.IsNullOrEmpty())
            {
                setting = new ReportSettings
                {
                    UserId = SessionVariables.CurrentUser.User.Id,
                    DateFrom = new DateTime(DateTime.Now.Year, 1, 1),
                    DateEnd = new DateTime(DateTime.Now.Year, 12, 31),
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now
                };
                work.Report.CreateOrUpdateSetting(setting, SessionVariables.CurrentUser.User.Id);
            }

            ViewBag.startDate = setting.DateFrom.ToShortDateString();
            ViewBag.endDate = setting.DateEnd.ToShortDateString();

            return View(setting);
        }

        [HttpPost]
        public ActionResult Settings(string Id, string dateRange, string CreatedBy, DateTime? CreatedDate)
        {
            var dates = dateRange.ToDateRange();
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();

            var reportSettings = new ReportSettings
            {
                Id = Id,
                UserId = SessionVariables.CurrentUser.User.Id,
                DateFrom = dates.StartDate,
                DateEnd = dates.EndDate,
                CreatedBy = CreatedBy ?? SessionVariables.CurrentUser.User.Id,
                CreatedDate = CreatedDate ?? DateTime.Now
            };

            var settings = work.Report.CreateOrUpdateSetting(reportSettings, SessionVariables.CurrentUser.User.Id);

            if (settings.ResultType == ResultType.Success)
            {
                CreateAlertMessage(settings.Message, AlertMessageTypes.Success, AlertMessageIcons.Success);
                return RedirectToAction("Settings");
            }

            CreateAlertMessage(settings.Message, AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            return View(settings.Data);
        }
        #endregion

        #region Base Reports
        [RequirePermission(ModuleId = "1473588347af62779d2cdd48479ae8")]
        public ActionResult AttendanceSummary(string dateRangeTitle, string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, null, dateRange);
            var model = new AttendanceSummaryModel
            {
                AllAttendance = work.Attendance.GetAll(SessionVariables.CurrentChurch.Id, dates),
                AttendanceByDate = work.Attendance.GetAll(SessionVariables.CurrentChurch.Id, dates)
            };
            ViewBag.ShowDateRangePicker = true;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            model.DateRange = dateRange;

            return View(model);
        }

        [RequirePermission(ModuleId = "1537168360aba20b87b11d491c8694")]
        public ActionResult BaptismReport(string id, string dateRangeTitle, string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, id, dateRange);
            var users = work.User.GetBaptismUsers(dates);

            ViewBag.ShowDateRangePicker = true;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            ViewBag.DateRange = dates.CombinedDate;

            return View(users);
        }

        [RequirePermission(ModuleId = "2232019043fb615ed5a1b24b038011")]
        public ActionResult BaptismSummary(string dateRangeTitle, string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, null, dateRange);
            var model = new BaptismSummaryModel
            {
                AllBaptisms = work.Baptism.GetAll(SessionVariables.CurrentChurch.Id, dates),
                BaptismsByDate = work.Baptism.GetAll(SessionVariables.CurrentChurch.Id, dates)
            };
            ViewBag.ShowDateRangePicker = true;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            model.DateRange = dateRange;

            return View(model);
        }

        [RequirePermission(ModuleId = "3998741559c257bd8f07fd47d79cee")]
        public ActionResult CampusGivingReport(string campusId, string startDate, string endDate, string dateRangeTitle, string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, null, dateRange);

            if (startDate.IsNotNullOrEmpty() && dateRange.IsNullOrEmpty())
            {
                dates.StartDate = DateTime.Parse(startDate);
            }

            if (endDate.IsNotNullOrEmpty() && dateRange.IsNullOrEmpty())
            {
                dates.EndDate = DateTime.Parse(endDate);
            }

            var model = work.Report.GetCampusGivingSummaryDashboard(campusId, dates);

            ViewBag.ShowDateRangePicker = true;
            ViewBag.CampusId = campusId;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            model.DateRange = dates.CombinedDate;

            return View(model);
        }

        [RequirePermission(ModuleId = "19393488452497525f4dca43a2b9c4")]
        public ActionResult DeceasedReport(string id, string dateRangeTitle, string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, id, dateRange);
            var people = work.Person.GetDeceasedPeople(SessionVariables.CurrentChurch.Id, dates);

            ViewBag.ShowDateRangePicker = true;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            ViewBag.DateRange = dates.CombinedDate;

            return View(people);
        }

        [RequirePermission(ModuleId = "4930816743bcc0c7b5f18e4e8a9053")]
        public ActionResult DigitalGivingSummary(string id, string dateRangeTitle, string dateRange)
        {
            if (!SessionVariables.CurrentChurch.HasMerchantAccount)
            {
                CreateAlertMessage("Digital giving has not been enabled yet but can be enabled <a href='/settings/giving' class='font-weight-bold'>here</a>.", AlertMessageTypes.Info, AlertMessageIcons.Info);
            }

            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, id, dateRange);
            //var model = work.Report.GetDigitalGivingView(SessionVariables.CurrentChurch.Id, dates);
            var model = work.Report.GetGivingSummaryDashboard(SessionVariables.CurrentChurch.Id, dates, includeDigitalGiving: true, includeOfflineGiving: false);

            ViewBag.ShowDateRangePicker = true;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            model.DateRange = dates.CombinedDate;

            return View(model);
        }

        [RequirePermission(ModuleId = "3219453410e393b9f5c8eb4e3e8164")]
        public ActionResult DonorDemographicsReport(string dateRangeTitle, string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, null, dateRange);
            var model = work.Report.GetDonorsModel(SessionVariables.CurrentChurch.Id, dates);

            ViewBag.ShowDateRangePicker = true;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            model.DateRange = dates.CombinedDate;

            return View(model);
        }

        [RequirePermission(ModuleId = "2806835873416e969a7294417ea016")]
        public ActionResult DonorStatusReport()
        {
            // Define the start and end date for the current year
            var currentYear = DateTime.Now.Year;
            var dates = new DateRange
            {
                StartDate = new DateTime(currentYear, 1, 1),
                EndDate = new DateTime(currentYear, 12, 31)
            };

            // Get the report view model with the date range
            var model = work.Report.GetDonorStatusReportViewModel(SessionVariables.CurrentChurch.Id, dates);

            // Pass the model to the view
            return View(model);
        }

        [HttpGet]
        public ActionResult _FundReportFilter()
        {
            var model = new FundReportFilter
            {
                Funds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id)
            };
            return PartialView(model);
        }

        [HttpPost]
        public ActionResult _FundReportFilter(FundReportFilter model)
        {
            if (ModelState.IsValid)
            {
                return AjaxRedirectTo($"/reports/fundreport?fundId={model.FundId}&startDate={Url.Encode(model.StartDate)}&endDate={Url.Encode(model.EndDate)}");
            }

            model.Funds = work.Fund.GetAll(SessionVariables.CurrentChurch.Id);

            return PartialView("_FundReportFilter", model);
        }

        [RequirePermission(ModuleId = "9686681144d175c5dc858f4b9db0d2")]
        public ActionResult FundReport(string fundId, string type, string startDate, string endDate, string dateRangeTitle, string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, null, dateRange);

            if (startDate.IsNotNullOrEmpty() && dateRange.IsNullOrEmpty())
            {
                dates.StartDate = DateTime.Parse(startDate);
            }

            if (endDate.IsNotNullOrEmpty() && dateRange.IsNullOrEmpty())
            {
                dates.EndDate = DateTime.Parse(endDate);
            }

            var model = work.Report.GetFundSummaryView(SessionVariables.CurrentChurch.Id, fundId, dates);

            ViewBag.ShowDateRangePicker = true;
            ViewBag.fundId = fundId;
            ViewBag.type = type;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            model.DateRange = dates.CombinedDate;

            return View(model);
        }

        [RequirePermission(ModuleId = "3998741559c257bd8f07fd47d79cee")]
        public ActionResult FundReportDetails(string fundId, string type, string startDate, string endDate, string dateRangeTitle, string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, null, dateRange);

            if (startDate.IsNotNullOrEmpty() && dateRange.IsNullOrEmpty())
            {
                dates.StartDate = DateTime.Parse(startDate);
            }

            if (endDate.IsNotNullOrEmpty() && dateRange.IsNullOrEmpty())
            {
                dates.EndDate = DateTime.Parse(endDate);
            }

            var model = work.Report.GetFundDetailView(SessionVariables.CurrentChurch.Id, fundId, type, dates);

            ViewBag.ShowDateRangePicker = true;
            ViewBag.fundId = fundId;
            ViewBag.type = type;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            model.DateRange = dates.CombinedDate;

            return View(model);
        }

        [RequirePermission(ModuleId = "0006992687c594f4ad39c849c5aa85")]
        public ActionResult GivingSummary(string id, string dateRangeTitle, string dateRange)
        {
            if (!SessionVariables.CurrentChurch.HasMerchantAccount)
            {
                CreateAlertMessage("Digital giving has not been enabled. Only offline giving will appear below. Enable digital giving <a href='/settings/giving' class='font-weight-bold'>here</a>.", AlertMessageTypes.Info, AlertMessageIcons.Info);
            }

            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, id, dateRange);
            var model = work.Report.GetGivingSummaryDashboard(SessionVariables.CurrentChurch.Id, dates, true, true);

            ViewBag.ShowDateRangePicker = true;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            model.DateRange = dates.CombinedDate;

            return View(model);
        }

        [RequirePermission(ModuleId = "7622500380d791d1d3b8ef4d9eb56d")]
        public ActionResult OfflineGivingSummary(string id, string dateRangeTitle, string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, id, dateRange);
            var model = work.Report.GetGivingSummaryDashboard(SessionVariables.CurrentChurch.Id, dates, includeDigitalGiving: false, includeOfflineGiving: true);

            ViewBag.ShowDateRangePicker = true;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            model.DateRange = dates.CombinedDate;

            return View(model);
        }

        [RequirePermission(ModuleId = "135324978724bd32fa37d9492180a6")]
        public ActionResult PrayerRequestSummary(string dateRangeTitle, string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, null, dateRange);
            var model = work.Report.GetPrayerRequestsSummary(SessionVariables.CurrentChurch.Id, dates, includeAverageResponseTimes: true);

            ViewBag.ShowDateRangePicker = true;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            model.DateRange = dates.CombinedDate;

            return View(model);
        }

        [RequirePermission(ModuleId = "199677549505460a24dd6a45b8b0d8")]
        public ActionResult PrayerRequestSenders(string dateRangeTitle, string dateRange)
        {
            // Fetch the date range for the report
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, null, dateRange);

            // Get the prayer request summary for the specified date range
            var model = work.Report.GetPrayerRequestsSummary(SessionVariables.CurrentChurch.Id, dates, includeAverageResponseTimes: false);

            // Set ViewBag properties for use in the view
            ViewBag.ShowDateRangePicker = true;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            model.DateRange = dates.CombinedDate;

            // Return the view with the model
            return View(model);
        }

        [RequirePermission(ModuleId = "24027794037419def324ec4c8a9b4b")]
        public ActionResult PrayerRequestResponseSummary(string dateRangeTitle, string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, null, dateRange);
            var model = work.Report.GetPrayerRequestResponseSummary(SessionVariables.CurrentChurch.Id, dates);

            ViewBag.ShowDateRangePicker = true;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            model.DateRange = dates.CombinedDate;

            return View(model);
        }

        [RequirePermission(ModuleId = "09935651172d6211e1a03a41dd847f")]
        public ActionResult SalvationSummary(string dateRangeTitle, string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, null, dateRange);
            var model = new SalvationSummaryModel
            {
                AllSalvations = work.Salvation.GetAll(SessionVariables.CurrentChurch.Id, dates),
                SalvationsByDate = work.Salvation.GetAll(SessionVariables.CurrentChurch.Id, dates)
            };
            ViewBag.ShowDateRangePicker = true;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            model.DateRange = dateRange;

            return View(model);
        }

        [RequirePermission(ModuleId = "9495757836c9ad894ca6e149af8db2")]
        public ActionResult TopDonationsPastFiveYears()
        {
            const int years = 5;
            const int takeRecordForEachYear = 5;
            ViewBag.DonationRecords = takeRecordForEachYear;
            var donations = work.Report.GetTopDonationsByYears(SessionVariables.CurrentChurch.Id, years, takeRecordForEachYear, SessionVariables.CurrentUser.IsSuperAdmin);

            return View("topdonationspastfiveyears", donations);
        }

        [RequirePermission(ModuleId = "8567849171030d48c1f0c6423fbf95")]
        public ActionResult TopDonors(string id, string dateRangeTitle, string dateRange)
        {
            const int recordCount = 25;
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, id, dateRange);
            var donors = work.Report.GetTopDonors(SessionVariables.CurrentChurch.Id, recordCount, dates, SessionVariables.CurrentUser.IsSuperAdmin);

            ViewBag.ShowDateRangePicker = true;
            ViewBag.dateRangeTitle = dateRangeTitle;
            ViewBag.startDate = dates.StartDate.ToShortDateString();
            ViewBag.endDate = dates.EndDate.ToShortDateString();
            ViewBag.DateRange = dates.CombinedDate;

            return View(donors);
        }

        [RequirePermission(ModuleId = "92332366516f6ef23bdb6a4b57852f")]
        public ActionResult TopDonorsPastFiveYears()
        {
            const int years = 5;
            const int takeRecordForEachYear = 5;
            var donations = work.Report.GetTopDonorsByYears(SessionVariables.CurrentChurch.Id, years, takeRecordForEachYear, SessionVariables.CurrentUser.IsSuperAdmin);
            return View("topdonorspastfiveyears", donations);
        }

        [RequirePermission(ModuleId = "1753373774496296655da04636b86e")]
        public ActionResult UpcomingBirthdays()
        {
            var users = work.Person.GetUpcomingBirthdays(SessionVariables.CurrentChurch.Id);
            return View(users);
        }
        #endregion

        //Used only on Reports/Index page
        [ChildActionOnly]
        public ActionResult WidgetReport(string reportType, string CampusIds, string dateRange)
        {
            var reportData = work.Report.GetGivingReport(reportType, SessionVariables.CurrentChurch.Id, CampusIds, dateRange);
            var result = ReportGenerator.GenerateChart(new List<Campus>(), reportData, reportType);
            return PartialView("WidgetReport", result.ToString());
        }

        #region Export Reports
        public ActionResult ExportAttendanceSummaryToCSV(string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, null, dateRange);
            var attendance = work.Attendance.GetAll(SessionVariables.CurrentChurch.Id, dates);

            var rows = new List<string> { "Campus name,Attendance Count" };

            foreach (var campus in SessionVariables.Campuses.OrderBy(x => x.Display))
            {
                var total = attendance.Where(q => q.CampusId.IsNotNullOrEmpty() && q.CampusId.Equals(campus.Id)).Select(x => x.Total).Sum();
                rows.Add($"{campus.Display},{total}");
            }

            var totalNoCampus = attendance.Where(q => q.CampusId.IsNullOrEmpty()).Select(x => x.Total).Sum();
            if (totalNoCampus > 0)
            {
                rows.Add($"[No Campus Assigned],{totalNoCampus}");
            }

            var totalAttendance = attendance.Select(x => x.Total).Sum();
            rows.Add($"Total,{totalAttendance}");

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("AttendanceSummaryReport", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportBaptismReportToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var users = work.User.GetBaptismUsers(dates);
            var rows = new List<string>() { "Name,Baptism Date" };

            foreach (var user in users)
            {
                var baptismDate = Convert.ToDateTime(user.Person.BaptismDate).ToShortDateString();
                rows.Add($"{user.Display},{baptismDate}");
            }

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("BaptismReport", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportBaptismSummaryToCSV(string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, null, dateRange);
            var baptisms = work.Baptism.GetAll(SessionVariables.CurrentChurch.Id, dates);

            var rows = new List<string> { "Campus name,Baptism Count" };

            foreach (var campus in SessionVariables.Campuses.OrderBy(x => x.Display))
            {
                var total = baptisms.Where(q => q.CampusId.IsNotNullOrEmpty() && q.CampusId.Equals(campus.Id)).Select(x => x.Total).Sum();
                rows.Add($"{campus.Display},{total}");
            }

            var totalNoCampus = baptisms.Where(q => q.CampusId.IsNullOrEmpty()).Select(x => x.Total).Sum();
            if (totalNoCampus > 0)
            {
                rows.Add($"[No Campus Assigned],{totalNoCampus}");
            }

            var totalBaptisms = baptisms.Select(x => x.Total).Sum();
            rows.Add($"Total,{totalBaptisms}");

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("BaptismSummaryReport", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportCampusDigitalGivingSummaryToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetDigitalGivingView(SessionVariables.CurrentChurch.Id, dates);

            var rows = new List<string> { "Campus,Total,Online,Text Message" };

            foreach (var campus in model.Campuses.OrderBy(x => x.Name))
            {
                var campusDigitalGiving = model.DigitalGiving.Where(x => x.CampusId == campus.Id);
                var totalCampusDigitalGiving = campusDigitalGiving.Sum(x => x.Amount);
                var totalCampusOnlineGiving = campusDigitalGiving.Where(x => x.DigitalPaymentType == DigitalPaymentTypes.Online).Sum(x => x.Amount);
                var totalCampusTextMessageGiving = campusDigitalGiving.Where(x => x.DigitalPaymentType == DigitalPaymentTypes.TextToGive).Sum(x => x.Amount);
                rows.Add($"{campus.Display},{totalCampusDigitalGiving},{totalCampusOnlineGiving},{totalCampusTextMessageGiving}");
            }

            var noCampusDigitalGiving = model.DigitalGiving.Where(x => string.IsNullOrEmpty(x.CampusId));
            var noCampusDigitalGivingTotal = noCampusDigitalGiving.Sum(x => x.Amount);
            var noCampusOnlineGivingTotal = noCampusDigitalGiving.Where(x => x.DigitalPaymentType == DigitalPaymentTypes.Online).Sum(x => x.Amount);
            var noCampusTextMessageGivingTotal = noCampusDigitalGiving.Where(x => x.DigitalPaymentType == DigitalPaymentTypes.TextToGive).Sum(x => x.Amount);
            rows.Add($"[No Campus Assigned],{noCampusDigitalGivingTotal},{noCampusOnlineGivingTotal},{noCampusTextMessageGivingTotal}");

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("DigitalGivingSummaryByCampus", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportCampusGivingReportToCSV(string campusId, string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetCampusGivingSummaryDashboard(campusId, dates);
            var rows = new List<string>() { "Fund,Total,Digital,Offline" };

            foreach (var fund in model.Funds.OrderBy(x => x.Display))
            {
                var total = model.TotalGiving.Where(x => !string.IsNullOrEmpty(x.FundId) && x.FundId.Equals(fund.Id)).Sum(x => x.Amount);
                var onlineTotalGiving = model.DigitalGiving.Where(x => x.FundId == fund.Id).Sum(x => x.Amount);
                var offlineTotalGiving = model.OfflineGiving.Where(x => x.FundId == fund.Id).Sum(x => x.Amount);
                rows.Add($"{fund.Display},{total},{onlineTotalGiving},{offlineTotalGiving}");
            }

            var data = string.Join("\r\n", rows);
            var campus = !string.IsNullOrEmpty(campusId) ? $"{work.Campus.Get(campusId).Display.FilenameFriendlyLower()}_" : string.Empty;
            var suggestedFilename = Utilities.CombineFileName($"{campus}CampusGivingReport", dates.CombinedPlainDate, FileExtension.csv).ToLower();

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportCampusGivingSummaryToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetGivingSummaryDashboard(SessionVariables.CurrentChurch.Id, dates);

            var rows = new List<string> { "Campus,Total,Digital,Offline" };

            foreach (var campus in model.Campuses)
            {
                var totalCampusGiving = model.TotalGiving.Where(x => !string.IsNullOrEmpty(x.CampusId) && x.CampusId.Equals(campus.Id)).Sum(x => x.Amount);
                var totalCampusDigitalGiving = model.DigitalGiving.Where(x => !string.IsNullOrEmpty(x.CampusId) && x.CampusId.Equals(campus.Id)).Sum(x => x.Amount);
                var totalCampusOfflineGiving = model.OfflineGiving.Where(x => !string.IsNullOrEmpty(x.CampusId) && x.CampusId.Equals(campus.Id)).Sum(x => x.Amount);
                rows.Add($"{campus.Display},{totalCampusGiving},{totalCampusDigitalGiving},{totalCampusOfflineGiving}");
            }

            var noCampusTotalGiving = model.TotalGiving.Where(x => string.IsNullOrEmpty(x.CampusId)).Sum(x => x.Amount);
            var noCampusDigitalGiving = model.DigitalGiving.Where(x => string.IsNullOrEmpty(x.CampusId)).Sum(x => x.Amount);
            var noCampusOfflineGiving = model.OfflineGiving.Where(x => string.IsNullOrEmpty(x.CampusId)).Sum(x => x.Amount);
            rows.Add($"[No Campus Assigned],{noCampusTotalGiving},{noCampusDigitalGiving},{noCampusOfflineGiving}");

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("GivingSummaryByCampus", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportCampusOfflineGivingSummaryToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetOfflineGivingView(SessionVariables.CurrentChurch.Id, dates);

            var rows = new List<string> { "Campus,Total,Offering Plate,Drop-Off,Mailed" };

            foreach (var campus in SessionVariables.Campuses.OrderBy(x => x.Name))
            {
                var campusGiving = model.OfflineGiving.Where(x => x.CampusId == campus.Id);
                var totalCampusGiving = campusGiving.Sum(x => x.Amount);
                var totalOfferingPlateGiving = campusGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.OfferingPlate).Sum(x => x.Amount);
                var totalDropOffGiving = campusGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.DropOff).Sum(x => x.Amount);
                var totalMailedGiving = campusGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.Mail).Sum(x => x.Amount);

                rows.Add($"{campus.Display},{totalCampusGiving},{totalOfferingPlateGiving},{totalDropOffGiving},{totalMailedGiving}");
            }

            var noCampusOfflineGiving = model.OfflineGiving.Where(x => string.IsNullOrEmpty(x.CampusId));
            var noCampusOfflineGivingTotal = noCampusOfflineGiving.Sum(x => x.Amount);
            var noCampusOfferingPlateGiving = noCampusOfflineGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.OfferingPlate).Sum(x => x.Amount);
            var noCampusDropOffGiving = noCampusOfflineGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.DropOff).Sum(x => x.Amount);
            var noCampusMailedGiving = noCampusOfflineGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.Mail).Sum(x => x.Amount);
            rows.Add($"[No Campus Assigned],{noCampusOfflineGivingTotal},{noCampusOfferingPlateGiving},{noCampusDropOffGiving},{noCampusMailedGiving}");

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("OfflineGivingSummaryByCampus", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportDeceasedReportToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var people = work.Person.GetDeceasedPeople(SessionVariables.CurrentChurch.Id, dates);
            var rows = new List<string>() { "Name,Deceased Date" };

            foreach (var person in people)
            {
                var deceasedDate = person.DeceasedDate?.ToShortDateString() ?? string.Empty;
                rows.Add($"{person.Display},{deceasedDate}");
            }

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("DeceasedReport", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        #region Export Donor Demographics Report 
        public ActionResult ExportDonationsByGenderToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetDonorsModel(SessionVariables.CurrentChurch.Id, dates);
            var rows = new List<string> { "Gender,Count,%,Avg. Donation" };

            // Loop over each gender and use pre-calculated stats
            foreach (var item in Constants.Genders.Concat(new[] { new KeyValuePair<string, string>(string.Empty, "Not Provided") }))
            {
                var genderKey = string.IsNullOrEmpty(item.Key) ? "Not Provided" : item.Key;
                var stat = model.GenderStats.ContainsKey(genderKey) ? model.GenderStats[genderKey] : new GenderStat();

                rows.Add($"{item.Value},{stat.Count},{stat.Percentage:0.00}%,{stat.AverageDonation.ToCurrencyString()}");
            }

            // Add the total row
            rows.Add($"Total,{model.TotalGenderCount},{model.TotalGenderPercentage.ToFormattedPercentage()},{model.TotalGenderAverageDonation.ToCurrencyString()}");

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("DonationsByGender", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportDonationsByMaritalStatusToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetDonorsModel(SessionVariables.CurrentChurch.Id, dates);
            var rows = new List<string> { "Marital Status,Count,%,Avg. Donation" };

            // Loop over each marital status and use pre-calculated stats
            foreach (var item in MaritalStatuses.Items)
            {
                var column1 = item.Equals(MaritalStatuses.Other) ? "Other/Not Provided" : item;
                var stat = model.MaritalStatusStats.ContainsKey(item) ? model.MaritalStatusStats[item] : new MaritalStatusStat();

                rows.Add($"{column1},{stat.Count},{stat.Percentage.ToFormattedPercentage()},{stat.AverageDonation.ToCurrencyString()}");
            }

            // Add the total row
            rows.Add($"Totals,{model.TotalMaritalStatusCount},{model.TotalMaritalStatusPercentage.ToFormattedPercentage()},{model.TotalMaritalStatusAverageDonation.ToCurrencyString()}");

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("DonationsByMaritalStatus", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportDonationsByAgeGroupToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetDonorsModel(SessionVariables.CurrentChurch.Id, dates);
            var rows = new List<string> { "Age Group,Count,%,Avg. Donation" };

            // Loop over each age group and use pre-calculated stats
            foreach (var item in AgeGroupDemographics.Items)
            {
                var column1 = item.Equals(AgeGroupDemographics.Other) ? "Other/Not Provided" : item;
                var stat = model.AgeGroupStats.ContainsKey(item) ? model.AgeGroupStats[item] : new AgeGroupStat();

                rows.Add($"{column1},{stat.Count},{stat.Percentage.ToFormattedPercentage()},{stat.AverageDonation.ToCurrencyString()}");
            }

            // Add the total row
            rows.Add($"Totals,{model.TotalAgeGroupCount},{model.TotalAgeGroupPercentage.ToFormattedPercentage()},{model.TotalAgeGroupAverageDonation.ToCurrencyString()}");

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("DonationsByAgeGroup", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportDonationsByEthnicityToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetDonorsModel(SessionVariables.CurrentChurch.Id, dates);
            var rows = new List<string> { "Ethnicity,Count,%,Avg. Donation" };

            // Loop over each ethnicity and use pre-calculated stats
            foreach (var item in EthnicTypes.Items.OrderBy(x => x))
            {
                var ethnicKey = item.Equals(EthnicTypes.Other) ? "Other/Not Provided" : item;
                var stat = model.EthnicTypeStats.ContainsKey(item) ? model.EthnicTypeStats[item] : new EthnicTypeStat();

                rows.Add($"{ethnicKey},{stat.Count},{stat.Percentage:0.00}%,{stat.AverageDonation.ToCurrencyString()}");
            }

            // Add the total row
            rows.Add($"Totals,{model.TotalEthnicTypeCount},{model.TotalEthnicTypePercentage.ToFormattedPercentage()},{model.TotalEthnicTypeAverageDonation.ToCurrencyString()}");

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("DonationsByEthnicity", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportDonationsByEducationToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetDonorsModel(SessionVariables.CurrentChurch.Id, dates);
            var rows = new List<string> { "Education,Count,%,Avg. Donation" };

            // Loop over each education type and use pre-calculated stats
            foreach (var item in EducationTypes.Items)
            {
                var educationKey = item.Equals(EducationTypes.Other) ? "Other/Not Provided" : item;
                var stat = model.EducationTypeStats.ContainsKey(item) ? model.EducationTypeStats[item] : new EducationTypeStat();

                rows.Add($"{educationKey},{stat.Count},{stat.Percentage:0.00}%,{stat.AverageDonation.ToCurrencyString()}");
            }

            // Add the total row
            rows.Add($"Totals,{model.TotalEducationTypeCount},{model.TotalEducationTypePercentage.ToFormattedPercentage()},{model.TotalEducationTypeAverageDonation.ToCurrencyString()}");

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("DonationsByEducation", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportDonationsByEmploymentStatusToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetDonorsModel(SessionVariables.CurrentChurch.Id, dates);
            var rows = new List<string> { "Employment Status,Count,%,Avg. Donation" };

            // Loop over each employment status and use pre-calculated stats
            foreach (var item in EmploymentStatuses.Items)
            {
                var employmentStatusKey = item.Equals(EmploymentStatuses.Other) ? "Other/Not Provided" : item;
                var stat = model.EmploymentStatusStats.ContainsKey(item) ? model.EmploymentStatusStats[item] : new EmploymentStatusStat();

                rows.Add($"{employmentStatusKey},{stat.Count},{stat.Percentage:0.00}%,{stat.AverageDonation.ToCurrencyString()}");
            }

            // Add the total row
            rows.Add($"Totals,{model.TotalEmploymentStatusCount},{model.TotalEmploymentStatusPercentage.ToFormattedPercentage()},{model.TotalEmploymentStatusAverageDonation.ToCurrencyString()}");

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("DonationsByEmploymentStatus", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }
        #endregion

        #region Export Donor Status Report
        public ActionResult ExportDonorStatusReportToCSV()
        {
            // Define the start and end date for the current year
            var currentYear = DateTime.Now.Year;
            var dates = new DateRange
            {
                StartDate = new DateTime(currentYear, 1, 1),
                EndDate = new DateTime(currentYear, 12, 31)
            };

            // Get the report view model with the date range
            var model = work.Report.GetDonorStatusReportViewModel(SessionVariables.CurrentChurch.Id, dates);

            var rows = new List<string>
            {
                "Donor Status,Count,Avg. Donation"
            };

            AddDonorType(rows, "First Time Donors", model.FirstTimeDonorCount, model.FirstTimeAverageDonation);
            AddDonorType(rows, "Second Time Donors", model.SecondTimeDonorsCount, model.SecondTimeAverageDonation);
            AddDonorType(rows, "Occasional Donors", model.OccasionalDonorsCount, model.OccasionalAverageDonation);
            AddDonorType(rows, "Regular Donors", model.RegularDonorsCount, model.RegularAverageDonation);
            AddDonorType(rows, "Recurring Donors", model.RecurringDonorsCount, model.RecurringAverageDonation);
            AddDonorType(rows, "Inactive Donors", model.InActiveDonorsCount, model.InActiveAverageDonation);
            AddDonorType(rows, "Total Donors", model.TotalDonorCount, model.TotalAverageDonation);

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("DonorStatusReport", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        private void AddDonorType(List<string> rows, string title, int count, decimal averageDonation)
        {
            rows.Add($"{title},{count},{averageDonation.ToCurrencyString()}");
        }
        #endregion

        public ActionResult ExportFundDigitalGivingSummaryToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetDigitalGivingView(SessionVariables.CurrentChurch.Id, dates);

            var rows = new List<string> { "Fund,Total,Online,Text Message" };

            foreach (var fund in model.Funds.OrderBy(x => x.Display))
            {
                var digitalGiving = model.DigitalGiving.Where(x => x.FundId == fund.Id);
                var totalDigitalGiving = digitalGiving.Sum(x => x.Amount);
                var totalOnlineGiving = digitalGiving.Where(x => x.DigitalPaymentType == DigitalPaymentTypes.Online).Sum(x => x.Amount);
                var totalTextMessageGiving = digitalGiving.Where(x => x.DigitalPaymentType == DigitalPaymentTypes.TextToGive).Sum(x => x.Amount);
                rows.Add($"{fund.Display},{totalDigitalGiving},{totalOnlineGiving},{totalTextMessageGiving}");
            }

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("DigitalGivingSummaryByFund", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportFundGivingSummaryToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetGivingSummaryDashboard(SessionVariables.CurrentChurch.Id, dates);

            var rows = new List<string> { "Fund,Total,Digital,Offline" };

            foreach (var fund in model.Funds.OrderBy(x => x.Display))
            {
                var totalFundGiving = model.TotalGiving.Where(x => x.FundId.Equals(fund.Id)).Sum(x => x.Amount);
                var totalFundDigitalGiving = model.DigitalGiving.Where(x => x.FundId.Equals(fund.Id)).Sum(x => x.Amount);
                var totalFundOfflineGiving = model.OfflineGiving.Where(x => x.FundId.Equals(fund.Id)).Sum(x => x.Amount);
                rows.Add($"{fund.Display},{totalFundGiving},{totalFundDigitalGiving},{totalFundOfflineGiving}");
            }

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("GivingSummaryByFund", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportFundOfflineGivingSummaryToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetOfflineGivingView(SessionVariables.CurrentChurch.Id, dates);

            var rows = new List<string> { "Fund,Total,Offering Plate,Drop-Off,Mailed" };

            foreach (var fund in model.Funds.OrderBy(x => x.Display))
            {
                var fundGiving = model.OfflineGiving.Where(x => x.FundId == fund.Id);

                var totalFundGiving = fundGiving.Sum(x => x.Amount);
                var totalOfferingPlateGiving = fundGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.OfferingPlate).Sum(x => x.Amount);
                var totalDropOffGiving = fundGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.DropOff).Sum(x => x.Amount);
                var totalMailedGiving = fundGiving.Where(x => x.OfflinePaymentType == OfflinePaymentTypes.Mail).Sum(x => x.Amount);

                rows.Add($"{fund.Display},{totalFundGiving},{totalOfferingPlateGiving},{totalDropOffGiving},{totalMailedGiving}");
            }

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("OfflineGivingSummaryByFund", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportFundReportToCSV(string fundId, string type, string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetFundSummaryView(SessionVariables.CurrentChurch.Id, fundId, dates);

            var rows = new List<string> { "Campus,Total,Total Digital,Total Offline" };

            foreach (var campus in model.Campuses)
            {
                var totalCampusGiving = model.TotalGiving.Where(x => x.CampusId == campus.Id).Sum(x => x.Amount);
                var totalCampusDigitalGiving = model.DigitalGiving.Where(x => x.CampusId == campus.Id).Sum(x => x.Amount);
                var totalCampusOfflineGiving = model.OfflineGiving.Where(x => x.CampusId == campus.Id).Sum(x => x.Amount);

                rows.Add($"{campus.Display},{totalCampusGiving},{totalCampusDigitalGiving},{totalCampusOfflineGiving}");
            }

            var noCampusTotalGiving = model.TotalGiving.Where(x => string.IsNullOrEmpty(x.CampusId)).Sum(x => x.Amount);
            var noCampusDigitalGiving = model.DigitalGiving.Where(x => string.IsNullOrEmpty(x.CampusId)).Sum(x => x.Amount);
            var noCampusOfflineGiving = model.OfflineGiving.Where(x => string.IsNullOrEmpty(x.CampusId)).Sum(x => x.Amount);

            rows.Add($"[No Campus Assigned],{noCampusTotalGiving},{noCampusDigitalGiving},{noCampusOfflineGiving}");

            var data = string.Join("\r\n", rows);

            var fund = !string.IsNullOrEmpty(fundId) ? $"{work.Fund.Get(fundId).Display.FilenameFriendlyLower()}_" : string.Empty;
            type = !string.IsNullOrEmpty(type) ? $"{type}_" : string.Empty;
            var suggestedFilename = Utilities.CombineFileName($"{fund}{type}FundReport", dates.CombinedPlainDate, FileExtension.csv).ToLower();

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportFundReportDetailsToCSV(string fundId, string type, string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetFundDetailView(SessionVariables.CurrentChurch.Id, fundId, type, dates);

            var rows = new List<string>();
            var fundName = !string.IsNullOrEmpty(fundId) ? $"{work.Fund.Get(fundId).Display.FilenameFriendlyLower()}_" : string.Empty;
            var paymentType = !string.IsNullOrEmpty(type) ? $"{type}_" : string.Empty;
            var suggestedFilename = Utilities.CombineFileName($"{fundName}{paymentType}FundReportDetails", dates.CombinedPlainDate, FileExtension.csv).ToLower();

            var campusHeaders = type == PaymentMethodTypes.Digital
                ? new[] { "Campus", "Total", "Online", "Text Message" }
                : new[] { "Campus", "Total", "Offering Plate", "Drop Off", "Mail" };

            rows.Add(string.Join(",", campusHeaders));

            foreach (var campus in model.Campuses)
            {
                var campusId = campus.Id;
                var totalCampusGiving = type == PaymentMethodTypes.Digital
                    ? model.DigitalGiving.Where(x => x.CampusId == campusId).Sum(x => x.Amount)
                    : model.OfflineGiving.Where(x => x.CampusId == campusId).Sum(x => x.Amount);

                var onlineGiving = type == PaymentMethodTypes.Digital
                    ? model.DigitalGiving.Where(x => x.CampusId == campusId && x.DigitalPaymentType == DigitalPaymentTypes.Online).Sum(x => x.Amount)
                    : 0;

                var textMessageGiving = type == PaymentMethodTypes.Digital
                    ? model.DigitalGiving.Where(x => x.CampusId == campusId && x.DigitalPaymentType == DigitalPaymentTypes.TextToGive).Sum(x => x.Amount)
                    : model.OfflineGiving.Where(x => x.CampusId == campusId && (x.OfflinePaymentType == OfflinePaymentTypes.DropOff || x.OfflinePaymentType == OfflinePaymentTypes.Mail)).Sum(x => x.Amount);

                var offeringPlateGiving = type == PaymentMethodTypes.Digital ? 0
                    : model.OfflineGiving.Where(x => x.CampusId == campusId && x.OfflinePaymentType == OfflinePaymentTypes.OfferingPlate).Sum(x => x.Amount);

                var dropOffGiving = type == PaymentMethodTypes.Digital ? 0
                    : model.OfflineGiving.Where(x => x.CampusId == campusId && x.OfflinePaymentType == OfflinePaymentTypes.DropOff).Sum(x => x.Amount);

                var mailGiving = type == PaymentMethodTypes.Digital ? 0
                    : model.OfflineGiving.Where(x => x.CampusId == campusId && x.OfflinePaymentType == OfflinePaymentTypes.Mail).Sum(x => x.Amount);

                var campusRow = $"{campus.Display},{totalCampusGiving},{onlineGiving},{textMessageGiving},{offeringPlateGiving},{dropOffGiving},{mailGiving}";
                rows.Add(campusRow);
            }

            var noCampusTotal = type == PaymentMethodTypes.Digital
                ? model.DigitalGiving.Where(x => string.IsNullOrEmpty(x.CampusId)).Sum(x => x.Amount)
                : model.OfflineGiving.Where(x => string.IsNullOrEmpty(x.CampusId)).Sum(x => x.Amount);

            var noCampusOnlineGiving = type == PaymentMethodTypes.Digital
                ? model.DigitalGiving.Where(x => string.IsNullOrEmpty(x.CampusId) && x.DigitalPaymentType == DigitalPaymentTypes.Online).Sum(x => x.Amount)
                : 0;

            var noCampusTextMessageGiving = type == PaymentMethodTypes.Digital
                ? model.DigitalGiving.Where(x => string.IsNullOrEmpty(x.CampusId) && x.DigitalPaymentType == DigitalPaymentTypes.TextToGive).Sum(x => x.Amount)
                : model.OfflineGiving.Where(x => string.IsNullOrEmpty(x.CampusId)).Sum(x => x.Amount);

            var noCampusRow = $"[No Campus Assigned],{noCampusTotal},{noCampusOnlineGiving},{noCampusTextMessageGiving},0,0,0";
            rows.Add(noCampusRow);

            var data = string.Join("\r\n", rows);
            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportPrayerRequestsByCategoryToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var prayerRequests = work.PrayerRequest.GetAll(SessionVariables.CurrentChurch.Id, dates);
            var categories = work.PrayerRequest.GetAllCategories();
            var rows = new List<string> { "Type,Total" };

            foreach (var category in categories)
            {
                var count = prayerRequests.Count(q => q.CategoryId.IsNotNullOrEmpty() && q.CategoryId.Equals(category.Id));
                rows.Add($"{category.Display},{count}");
            }

            var total = prayerRequests.Count;
            rows.Add($"Total,{total}");

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("PrayerRequestsByCategory", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportPrayerRequestsByTypeToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetPrayerRequestsSummary(SessionVariables.CurrentChurch.Id, dates, includeAverageResponseTimes: true);

            // Initialize the CSV content with headers
            var rows = new List<string> { "Type,Total" };

            // Extract counts from StatusCounts dictionary
            var statusCounts = model.StatusCounts;

            // Add rows for each type
            foreach (var status in new[] { StatusKeys.ConfidentialRequests, StatusKeys.FollowUpRequiredRequests, StatusKeys.HighPriorityRequests })
            {
                if (statusCounts.TryGetValue(status, out var counts))
                {
                    rows.Add($"{status.Replace("Requests", string.Empty)},{counts.ByDate}");
                }
            }

            // Add total row
            if (statusCounts.TryGetValue(StatusKeys.TotalRequests, out var totalCount))
            {
                rows.Add($"Total Prayer Requests,{totalCount.ByDate}");
            }

            // Join rows into a CSV string
            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("PrayerRequestsByType", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportPrayerRequestsResponseSummaryToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetPrayerRequestResponseSummary(SessionVariables.CurrentChurch.Id, dates);
            const string defaultTime = "N/A";

            var rows = new List<string>
            {
                "Response Type,Total,Avg. Response Time"
            };

            // Define response types and their display names
            var responseTypes = new Dictionary<string, string>
            {
                { FollowUpStatuses.Incomplete, FollowUpStatuses.Incomplete },
                { FollowUpStatuses.AttemptedToContact, FollowUpStatuses.AttemptedToContact },
                { FollowUpStatuses.Completed, FollowUpStatuses.Completed }
            };

            // Get the count of follow-up required requests
            var followUpRequiredCount = model.StatusCounts[StatusKeys.FollowUpRequiredRequests].ByDate;
            rows.Add($"Follow Up Required,{followUpRequiredCount},{defaultTime}"); // Placeholder for avg response time

            // Retrieve the average response times from the model
            var avgResponseTimes = model.AverageResponseTimes;

            // Iterate over response types to populate the rows
            foreach (var responseType in responseTypes)
            {
                var count = model.PrayerRequestsByDate
                    .Count(q => q.FollowUpRequired && q.FollowUpStatus.IsNotNullOrEmpty() && q.FollowUpStatus.EqualsIgnoreCase(responseType.Value));

                // Default to an empty string for average response time
                string avgTime = defaultTime;

                // Retrieve the average response time from the dictionary if it exists
                if (avgResponseTimes.TryGetValue(responseType.Value, out var avg))
                {
                    avgTime = avg;
                }

                // Add the row to the list
                rows.Add($"{responseType.Key},{count},{avgTime}");
            }

            // Convert rows to CSV format
            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("PrayerRequestsResponseSummary", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportPrayerRequestSendersToCSV(string dateRange)
        {
            var dates = dateRange.SplitDates();
            var model = work.Report.GetPrayerRequestsSummary(SessionVariables.CurrentChurch.Id, dates, includeAverageResponseTimes: false);

            // Extract the sender counts from the model
            var uniqueSendersCount = model.SenderCounts.UniqueSendersByDate;
            var repeatSendersCount = model.SenderCounts.RepeatSendersByDate;
            var totalSendersCount = model.SenderCounts.TotalSendersByDate;

            var rows = new List<string>
            {
                "Type,Total",
                $"Unique,{uniqueSendersCount}",
                $"Repeat,{repeatSendersCount}",
                $"Total,{totalSendersCount}"
            };

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("PrayerRequestsBySender", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportSalvationSummaryToCSV(string dateRange)
        {
            var dates = work.Report.GetReportDateRange(SessionVariables.CurrentUser.User.Id, null, dateRange);
            var salvations = work.Salvation.GetAll(SessionVariables.CurrentChurch.Id, dates);

            var rows = new List<string> { "Campus name,Attendance Count" };

            foreach (var campus in SessionVariables.Campuses.OrderBy(x => x.Display))
            {
                var total = salvations.Where(q => q.CampusId.IsNotNullOrEmpty() && q.CampusId.Equals(campus.Id)).Sum(x => x.Total);
                rows.Add($"{campus.Display},{total}");
            }

            var totalNoCampus = salvations.Where(q => q.CampusId.IsNullOrEmpty()).Sum(x => x.Total);
            if (totalNoCampus > 0)
            {
                rows.Add($"[No Campus Assigned],{totalNoCampus}");
            }

            var totalSalvations = salvations.Select(x => x.Total).Sum();
            rows.Add($"Total,{totalSalvations}");

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("SalvationsSummaryReport", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportTopDonationsPastFiveYearsToCSV()
        {
            const int years = 5;
            const int takeRecordForEachYear = 5;
            var result = work.Report.GetTopDonationsByYears(SessionVariables.CurrentChurch.Id, years, takeRecordForEachYear);
            var rows = new List<string>() { "Donor,Type,Amount" };

            if (result.Any())
            {
                foreach (var donations in result.OrderByDescending(x => x.Year).ToList())
                {
                    rows.Add(donations.Year.ToString());

                    foreach (var item in donations.Donations.OrderByDescending(x => x.TotalAmount))
                    {
                        rows.Add($"{item.Donor.Display},{item.GivingType},{item.TotalAmount}");
                    }
                }
            }

            var data = string.Join("\r\n", rows);
            const string suggestedFilename = "TopDonationsPastFiveYears.csv";

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportTopDonorsToCSV(string dateRange)
        {
            const int recordCount = 25;
            var dates = dateRange.SplitDates();
            var donors = work.Report.GetTopDonors(SessionVariables.CurrentChurch.Id, recordCount, dates, SessionVariables.CurrentUser.IsSuperAdmin);
            var rows = new List<string>() { "Donor,Total,Digital,Offline" };

            if (donors.Any())
            {
                foreach (var item in donors)
                {
                    rows.Add($"{item.Donor.Display},{item.TotalAmount},{item.DigitalGivingAmount},{item.OfflineGivingAmount}");
                }
            }

            var data = string.Join("\r\n", rows);
            var suggestedFilename = Utilities.CombineFileName("TopDonors", dates.CombinedPlainDate, FileExtension.csv);

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportTopDonorsPastFiveYearsToCSV()
        {
            const int years = 5;
            const int takeRecordForEachYear = 5;
            var result = work.Report.GetTopDonorsByYears(SessionVariables.CurrentChurch.Id, years, takeRecordForEachYear, SessionVariables.CurrentUser.IsSuperAdmin);
            var rows = new List<string>() { "Donor,Digital,Offline,Total" };

            if (result.Any())
            {
                foreach (var donations in result.OrderByDescending(x => x.Year).ToList())
                {
                    rows.Add(donations.Year.ToString());

                    foreach (var item in donations.Donations.OrderByDescending(x => x.TotalAmount))
                    {
                        rows.Add($"{item.Donor.Display},{item.TotalDigitalAmount},{item.TotalOfflineAmount},{item.TotalAmount}");
                    }
                }
            }

            var data = string.Join("\r\n", rows);
            const string suggestedFilename = "TopDonorsPastFiveYears.csv";

            return ExportToCsv(data, suggestedFilename);
        }

        public ActionResult ExportUpcomingBirthdaysToCSV()
        {
            var people = work.Person.GetUpcomingBirthdays(SessionVariables.CurrentChurch.Id);
            var rows = new List<string>() { "Name,Birthday,Days Until" };

            foreach (var person in people)
            {
                rows.Add($"{person.Display},{person.DOB.ToShortDateString()},{person.RemainingDays}");
            }

            var data = string.Join("\r\n", rows);
            const string suggestedFilename = "UpcomingBirthdays.csv";

            return ExportToCsv(data, suggestedFilename);
        }
        #endregion
    }
}