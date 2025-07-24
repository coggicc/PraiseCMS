using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.DataAccess.Singletons;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;

namespace PraiseCMS.BusinessLayer
{
    public class PrayerRequestOperations : GenericRepository
    {
        private const string _confidentialPrayerRequestModule = "Confidential Prayer Request";

        public PrayerRequestOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public PrayerRequest Get(string id)
        {
            return Read<PrayerRequest>().FirstOrDefault(x => x.Id == id);
        }

        public List<PrayerRequest> Get(List<string> ids)
        {
            return Read<PrayerRequest>().Where(x => ids.Contains(x.Id)).ToList();
        }

        public List<PrayerRequest> GetAll(string churchId, DateRange dateRange)
        {
            return Read<PrayerRequest>()
                .Where(x => x.ChurchId == churchId
                    && DbFunctions.TruncateTime(x.CreatedDate) >= DbFunctions.TruncateTime(dateRange.StartDate)
                    && DbFunctions.TruncateTime(x.CreatedDate) <= DbFunctions.TruncateTime(dateRange.EndDate))
                .ToList();
        }

        public IEnumerable<PrayerRequest> GetAll(string churchId)
        {
            return Read<PrayerRequest>().Where(x => x.ChurchId == churchId);
        }

        public IEnumerable<PrayerRequest> GetAllByCampusId(string campusId)
        {
            return Read<PrayerRequest>().Where(x => x.CampusId == campusId);
        }

        public List<PrayerRequest> GetAllByRequestsAndPersonIds(IEnumerable<PrayerRequest> prayerRequests, List<Person> people)
        {
            return prayerRequests.Select(x => { x.Person = people.Find(q => q.Id.Equals(x.PersonId)); return x; }).ToList();
        }

        public PrayerRequestsView GetDashboard(
            string churchId,
            string campusId,
            string request,
            string categoryId,
            string inboxType,
            List<Permissions> permissions,
            string sortType = SortOrders.Descending,
            string keyword = null,
            int pageNumber = 1,
            int pageSize = 25)
        {
            // Fetch and filter prayer requests
            var prayerRequests = GetAllFiltered(churchId, campusId, request, categoryId, keyword, permissions).ToList();

            // Calculate total requests and counts once
            int totalRequests = prayerRequests.Count;
            int notPrayedOverCount = prayerRequests.Count(x => !x.PrayedOver);

            // Apply inbox settings and sorting
            var sortedRequests = ApplyInboxSettings(prayerRequests, inboxType, sortType);

            // Apply pagination
            var paginatedRequests = sortedRequests
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Map Person objects to Prayer Requests
            // Filter out null or empty PersonIds before creating the dictionary
            var personIds = paginatedRequests
                .Where(pr => !string.IsNullOrEmpty(pr.PersonId))
                .Select(pr => pr.PersonId)
                .Distinct()
                .ToList();

            var persons = Work.Person.GetAllByPersonIds(personIds).ToDictionary(p => p.Id);

            // Assign Person and OrderNumber in one loop
            for (int i = 0; i < paginatedRequests.Count; i++)
            {
                var prayerRequest = paginatedRequests[i];

                if (!string.IsNullOrEmpty(prayerRequest.PersonId) && persons.TryGetValue(prayerRequest.PersonId, out var person))
                {
                    prayerRequest.Person = person;
                }

                prayerRequest.OrderNumber = ((pageNumber - 1) * pageSize) + 1 + i;
            }

            return new PrayerRequestsView
            {
                PrayerRequests = paginatedRequests,
                NotPrayedOverCount = notPrayedOverCount,
                TotalPrayerRequests = totalRequests,
                FilterKeyword = keyword,
                Page = pageNumber,
                From = ((pageNumber - 1) * pageSize) + 1,
                To = Math.Min(pageNumber * pageSize, totalRequests),
                TotalPage = (int)Math.Ceiling(totalRequests / (double)pageSize),
                CurrentUrl = BuildCurrentUrl(request),
                PrayerRequestType = request,
                ReadAction = paginatedRequests.Any(q => !q.Read) ? PrayerRequestStatuses.Read : PrayerRequestStatuses.Unread,
                PrayedOverAction = paginatedRequests.Any(q => !q.PrayedOver) ? PrayerRequestStatuses.PrayedOver : PrayerRequestStatuses.NotPrayedOver,
                Newest = sortType == SortOrders.Descending ? "active" : string.Empty,
                Oldest = sortType == SortOrders.Ascending ? "active" : string.Empty,
                PrayerRequestCategories = GetAllCategories()
            };
        }

        public IEnumerable<PrayerRequest> GetAllFiltered(string churchId, string campusId, string request, string categoryId, string keyword, List<Permissions> permissions)
        {
            var query = Read<PrayerRequest>().Where(x => x.ChurchId == churchId);

            if (!string.IsNullOrEmpty(campusId))
            {
                query = query.Where(x => x.CampusId == campusId);
            }

            if (!string.IsNullOrEmpty(request))
            {
                switch (request)
                {
                    case PrayerRequestStatuses.Unread:
                        query = query.Where(x => !x.Read && !x.PrayedOver);
                        break;
                    case PrayerRequestStatuses.Confidential:
                        query = query.Where(x => x.Confidential && !x.PrayedOver);
                        break;
                    case PrayerRequestStatuses.HighPriority:
                        query = query.Where(x => x.HighPriority && !x.PrayedOver);
                        break;
                    case PrayerRequestStatuses.Responded:
                        query = query.Where(x => x.Responded && !x.PrayedOver);
                        break;
                    case PrayerRequestStatuses.FollowUpRequired:
                        query = query.Where(x => x.FollowUpRequired && !x.PrayedOver);
                        break;
                    case PrayerRequestStatuses.Starred:
                        query = query.Where(x => x.Starred && !x.PrayedOver);
                        break;
                    case PrayerRequestStatuses.PrayedOver:
                        query = query.Where(x => x.PrayedOver);
                        break;
                    default:
                        // No specific filtering applied, but exclude PrayedOver
                        query = query.Where(x => !x.PrayedOver);
                        break;
                }
            }
            else
            {
                // If no 'request' is provided, exclude PrayedOver requests
                query = query.Where(x => !x.PrayedOver);
            }

            if (!string.IsNullOrEmpty(categoryId))
            {
                query = categoryId.Equals("uncategorized")
                    ? query.Where(x => string.IsNullOrEmpty(x.CategoryId))
                    : query.Where(x => x.CategoryId == categoryId);
            }

            if (!string.IsNullOrEmpty(keyword))
            {
                query = query.Where(x => x.Message.Contains(keyword.Trim()));
            }

            var module = Work.Module.GetByName(_confidentialPrayerRequestModule);
            if (module.IsNotNullOrEmpty() && permissions.Any())
            {
                var accessLevel = Utilities.GetAccessLevel(module.Id, permissions);
                if (accessLevel.Equals(Operations.NoAccess))
                {
                    query = query.Where(x => !x.Confidential);
                }
            }

            return query;
        }

        private string BuildCurrentUrl(string request)
        {
            var queryParams = new List<string>();
            const string baseUrl = "/prayerRequests";

            if (!string.IsNullOrEmpty(request))
            {
                queryParams.Add($"request={request}");
            }

            return queryParams.Any()
                ? $"{baseUrl}?{string.Join("&", queryParams)}"
                : baseUrl;
        }

        public List<PrayerRequest> ApplyInboxSettings(IEnumerable<PrayerRequest> prayerRequests, string inboxType, string sortType)
        {
            List<PrayerRequest> prioritizedRequests;

            switch (inboxType)
            {
                case InboxType.UnreadFirst:
                    prioritizedRequests = prayerRequests.Where(x => !x.Read).ToList();
                    break;
                case InboxType.StarredFirst:
                    prioritizedRequests = prayerRequests.Where(x => x.Starred).ToList();
                    break;
                default:
                    return SortByCreatedDate(prayerRequests.ToList(), sortType);
            }

            // Sort prioritized requests by date
            prioritizedRequests = SortByCreatedDate(prioritizedRequests, sortType);

            // Get remaining requests and sort them by date
            var remainingRequests = SortByCreatedDate(prayerRequests.Except(prioritizedRequests).ToList(), sortType);

            // Combine prioritized requests with the remaining ones
            prioritizedRequests.AddRange(remainingRequests);

            return prioritizedRequests;
        }

        private List<PrayerRequest> SortByCreatedDate(List<PrayerRequest> requests, string sortType)
        {
            return sortType == SortOrders.Ascending
                ? requests.OrderBy(x => x.CreatedDate).ToList()
                : requests.OrderByDescending(x => x.CreatedDate).ToList();
        }

        public SidebarViewModel GetSidebarData(string churchId)
        {
            // Fetch all categories
            var categories = GetAllCategories();

            // Calculate counts for each category
            var counts = new Dictionary<string, int>();
            foreach (var category in categories)
            {
                var count = GetCategoryRequestCount(churchId, category.Id);
                counts.Add(category.Id, count);
            }

            // Return data needed for the sidebar
            return new SidebarViewModel
            {
                Categories = categories,
                CategoryCounts = counts
            };
        }

        private int GetCategoryRequestCount(string churchId, string categoryId)
        {
            return Read<PrayerRequest>().Count(x => x.ChurchId == churchId && x.CategoryId == categoryId && !x.PrayedOver);
        }

        public PrayerRequestsView GetRequestListPartial(string churchId, string inboxType, List<Permissions> permissions, string relativeUrl)
        {
            var domain = ApplicationCache.Instance.EnvironmentConfiguration.Url;

            // Create a base URI with the domain
            var baseUri = new Uri(domain);

            // Combine base URI with the relative URL
            var fullUri = new Uri(baseUri, relativeUrl);

            // Use the combined URI to get the query string
            var query = fullUri.Query;

            var queryParameters = HttpUtility.ParseQueryString(query);

            var campusId = queryParameters.Get("campusId");
            var request = queryParameters.Get("request");
            var categoryId = queryParameters.Get("categoryId");
            var sortType = queryParameters.Get("sortType");
            var filterKeyword = queryParameters.Get("keyword");
            var pageNumber = queryParameters.Get("pageNumber") != null ? Convert.ToInt32(queryParameters.Get("pageNumber")) : 1;

            return GetDashboard(churchId, campusId, request, categoryId, inboxType, permissions, sortType, filterKeyword, pageNumber);
        }

        public List<ApplicationUser> GetPrayerRequestsPermittedUsers(string churchId)
        {
            var module = Work.Module.GetByName("Prayer Requests");
            var permission = Work.Permission.GetByModuleId(module.Id).Where(q => !q.OperationId.Equals(Operations.NoAccess)).ToList();
            var userIds = permission.FindAll(x => x.Type.EqualsIgnoreCase(PermissionType.User.ToString())).Select(s => s.TypeId).Distinct().ToList();
            var roleIds = permission.FindAll(x => x.Type.EqualsIgnoreCase(PermissionType.Role.ToString())).Select(s => s.TypeId).Distinct().ToList();
            var users = new List<ApplicationUser>();

            foreach (var roleId in roleIds)
            {
                users.AddRange(DAL.ReadUsersByRoleId(roleId));
            }

            users.AddRange(Work.User.GetAll(userIds));
            var churchUser = Work.User.GetAllUsersIdsByChurchId(churchId);

            return users.FindAll(x => churchUser.Contains(x.Id)).Distinct().ToList();
        }

        #region Prayer Request Category
        public List<PrayerRequestCategory> GetAllCategories()
        {
            return Read<PrayerRequestCategory>().OrderBy(x => x.Name).ToList();
        }

        public PrayerRequestCategory GetCategory(string id)
        {
            return Read<PrayerRequestCategory>().FirstOrDefault(x => x.Id == id);
        }

        public PrayerRequestCategory GetCategoryByName(string name)
        {
            return Read<PrayerRequestCategory>().FirstOrDefault(x => x.Name == name);
        }
        #endregion        

        public PrayerRequestVM CreateBasicExternalPrayerRequestModel(string churchId)
        {
            var prayerRequest = new PrayerRequest()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = churchId,
                CreatedDate = DateTime.Now,
                CreatedBy = Constants.System
            };

            return new PrayerRequestVM()
            {
                PrayerRequest = prayerRequest,
                Mode = PeopleSelectionMode.Manual
            };
        }

        public PrayerRequestVM GetCreateExternalPrayerRequestModel(string churchId)
        {
            var prayerRequest = new PrayerRequest()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = churchId,
                CreatedDate = DateTime.Now,
                CreatedBy = Constants.System
            };

            return new PrayerRequestVM()
            {
                PrayerRequest = prayerRequest,
                Categories = Work.PrayerRequest.GetAllCategories(),
                People = Work.Person.GetAllByPersonIds(prayerRequest.ChurchId),
                Mode = PeopleSelectionMode.Manual
            };
        }

        public void SendPrayedOverNotification(List<PrayerRequest> prayerRequests, Church church, ApplicationUser user)
        {
            var people = Work.Person.GetAllByPersonIds(church.Id, prayerRequests.Select(q => q.PersonId));
            var requestWithPeople = GetAllByRequestsAndPersonIds(prayerRequests, people);
            var withEmails = requestWithPeople.Where(q => q.Person.IsNotNullOrEmpty() && q.Person.Email.IsNotNullOrEmpty()).ToList();
            var withPhone = requestWithPeople.Where(q => q.Person.IsNotNullOrEmpty() && !withEmails.Select(x => x.Id).Contains(q.Id) && q.Person.PhoneNumber.IsNotNullOrEmpty()).ToList();
            var currentChurch = church;

            if (withEmails.Any())
            {
                withEmails.Select(d =>
                {
                    d.PrayedOver = ExtensionMethods.CheckEmailIsValid(d.Person.Email) && Emailer.SendEmail(
                         new Email()
                         {
                             Id = Utilities.GenerateUniqueId(),
                             Message = EmailTemplates.General.Replace("{message}", !string.IsNullOrEmpty(currentChurch.CompletedPrayerRequestEmailMessage) ? currentChurch.CompletedPrayerRequestEmailMessage : Constants.DefaultPrayerRequestPrayedOverEmailMessage),
                             To = d.Person.Email,
                             Attachments = null,
                             Subject = $"Prayer Request Prayed Over (Id: {d.Id})",
                             CreatedBy = user.Id,
                             CreatedDate = DateTime.Now,
                             Type = PrayerRequestStatuses.PrayedOver,
                             TypeId = d.Id
                         }, null,
                         new Domain()
                         {
                             EmailLogo = currentChurch.Logo,
                             Name = currentChurch.Display,
                             EmailReplyAddress = currentChurch.Email,
                             EmailDisplay = currentChurch.Display
                         }, currentChurch, false, currentChurch.Email, currentChurch.Display);
                    return d;
                }).ToList();
            }

            if (withPhone.Any())
            {
                withPhone.Select(d =>
                {
                    d.PrayedOver = d.Person.PhoneNumber.PhoneFriendly().Length == 10 && Utilities.SendMessage(new SmsMessage
                    {
                        Id = Utilities.GenerateUniqueId(),
                        To = d.Person.PhoneNumber,
                        Message = !string.IsNullOrEmpty(currentChurch.CompletedPrayerRequestTextMessage) ? currentChurch.CompletedPrayerRequestTextMessage : Constants.DefaultPrayerRequestPrayedOverTextMessage,
                        CreatedDate = DateTime.Now,
                        CreatedBy = user.Id,
                        Type = PrayerRequestStatuses.PrayedOver,
                        TypeId = d.Id
                    });
                    return d;
                }).ToList();
            }
        }

        #region CRUD
        public Result<PrayerRequest> Create(PrayerRequest entity)
        {
            try
            {
                Create<PrayerRequest>(entity);
                SaveChanges();
                return new Result<PrayerRequest>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<PrayerRequest>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<PrayerRequest> Update(PrayerRequest entity, string userId)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = !string.IsNullOrEmpty(userId) ? userId: Constants.System;
                Update<PrayerRequest>(entity);
                SaveChanges();
                return new Result<PrayerRequest>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<PrayerRequest>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public void Delete(string id)
        {
            var entity = Get(id);
            Delete(entity);
        }

        public void Delete(PrayerRequest entity)
        {
            Delete<PrayerRequest>(entity);
            SaveChanges();
        }
        #endregion

        #region CRUD Prayer Request Category
        public void CreateCategory(PrayerRequestCategory entity)
        {
            Create(entity);
            SaveChanges();
        }

        public void UpdateCategory(PrayerRequestCategory entity)
        {
            Update(entity);
            SaveChanges();
        }

        public void DeleteCategory(string id)
        {
            var entity = GetCategory(id);
            DeleteCategory(entity);
        }

        public void DeleteCategory(PrayerRequestCategory entity)
        {
            Delete(entity);
            SaveChanges();
        }
        #endregion        
    }
}