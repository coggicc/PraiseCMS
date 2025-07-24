using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.Shared.Methods;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class SearchOperations : GenericRepository
    {
        public SearchOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public List<SearchResults> GetSearchResults(string query)
        {
            var resultList = new List<SearchResults>();

            if (query.IsNullOrEmpty())
            {
                return resultList;
            }

            var people = Work.Person.GetAllByName(query);
            var userSettings = Work.User.GetUsersSetting();
            var eventTypes = Work.ChurchEvents.GetAllByName(query);
            var events = Work.ChurchEvents.GetAllByType(string.Join(",", eventTypes.Select(q => q.Id).ToArray()));
            var calendarEvents = Work.Event.GetEventsByName(query);
            var reports = Work.Report.GetSystemReportsByName(query);

            if (people.IsNotNullOrEmpty())
            {
                resultList.AddRange(people.Select(q => new SearchResults
                {
                    Category = "Users",
                    Name = q.Display,
                    Url = q.UserId.IsNotNullOrEmpty() ? $"/users/userprofile?Id={q.UserId}&type=user" : $"/users/userprofile?Id={q.Id}&type=person",
                    Image = q.UserId.IsNotNullOrEmpty() && userSettings.Any(x => x.UserId.Equals(q.UserId)) && userSettings.FirstOrDefault(x => x.UserId.Equals(q.UserId)).ProfileImage.IsNotNullOrEmpty()
                        ? userSettings.FirstOrDefault(x => x.UserId.Equals(q.UserId)).ProfileImage
                        : q.ProfileImage
                }));
            }

            // Add event types to the result list if not empty
            //if (eventTypes.IsNotNullOrEmpty())
            //{
            //    resultList.AddRange(eventTypes.Select(q => new SearchResults
            //    {
            //        Category = "Event Types",
            //        Name = q.Type,
            //        Url = q.Id
            //    }));
            //}

            if (events.IsNotNullOrEmpty())
            {
                resultList.AddRange(events.Select(q => new SearchResults
                {
                    Category = "Events",
                    Name = q.DisplayName,
                    Url = q.Id
                }));
            }

            if (calendarEvents.IsNotNullOrEmpty())
            {
                resultList.AddRange(calendarEvents.Select(q => new SearchResults
                {
                    Category = "Calendar Events",
                    Name = q.DisplayWithDate,
                    Url = q.Id
                }));
            }

            // Add equipments to the result list if not empty
            //if (equipmentCategory.IsNotNullOrEmpty() && equipmentCategory.EquipmentList.IsNotNullOrEmpty())
            //{
            //    resultList.AddRange(equipmentCategory.EquipmentList.Select(q => new SearchResults
            //    {
            //        Category = "Equipment",
            //        Name = q.Name,
            //        Url = q.Id
            //    }));
            //}

            //// Add equipment categories to the result list if not empty
            //if (equipmentCategory.IsNotNullOrEmpty() && equipmentCategory.EquipmentCategories.IsNotNullOrEmpty())
            //{
            //    resultList.AddRange(equipmentCategory.EquipmentCategories.Select(q => new SearchResults
            //    {
            //        Category = "Equipment Categories",
            //        Name = q.Name,
            //        Url = q.Id
            //    }));
            //}

            if (reports.IsNotNullOrEmpty())
            {
                resultList.AddRange(reports.Select(q => new SearchResults
                {
                    Category = "Reports",
                    Name = q.Text,
                    Url = q.Value
                }));
            }

            return resultList;
        }

        public List<SearchResults> GetSearchResults(string category, string query)
        {
            var resultList = new List<SearchResults>();

            if (query.IsNullOrEmpty())
            {
                return resultList;
            }

            switch (category.ToLower().Trim())
            {
                case "users":
                    var people = Work.Person.GetAllByName(query);
                    var userSettings = Work.User.GetUsersSetting();

                    resultList.AddRange(people.Where(p => p.IsNotNullOrEmpty()).Select(q => new SearchResults
                    {
                        Category = "Users",
                        Name = q.Display,
                        Url = q.UserId.IsNotNullOrEmpty() ? $"/users/userprofile?Id={q.UserId}&type=user" : $"/users/userprofile?Id={q.Id}&type=person",
                        Image = q.UserId.IsNotNullOrEmpty() && userSettings.Any(x => x.UserId.Equals(q.UserId)) && userSettings.FirstOrDefault(x => x.UserId.Equals(q.UserId)).ProfileImage.IsNotNullOrEmpty()
                            ? userSettings.FirstOrDefault(x => x.UserId.Equals(q.UserId)).ProfileImage : q.ProfileImage
                    }));
                    break;

                case "events":
                    var eventTypes = Work.ChurchEvents.GetAllByName(query);
                    var events = Work.ChurchEvents.GetAllByType(string.Join(",", eventTypes.Select(q => q.Id)));

                    resultList.AddRange(events.Where(e => e.IsNotNullOrEmpty()).Select(q => new SearchResults
                    {
                        Category = "Events",
                        Name = q.DisplayName,
                        Url = $"/churchevents/dashboard/{q.Id}"
                    }));
                    break;

                case "calendar events":
                    var calendarEvents = Work.Event.GetEventsByName(query);

                    resultList.AddRange(calendarEvents.Where(c => c.IsNotNullOrEmpty()).Select(q => new SearchResults
                    {
                        Category = "Calendar Events",
                        Name = q.DisplayWithDate,
                        Url = q.Id
                    }));
                    break;

                //case "equipment":
                //    var equipmentCategory = Work.Equipment.GetWithCategorybyName(query);
                //    var equipmentList = equipmentCategory?.EquipmentList;

                //    resultList.AddRange(equipmentList.Where(eq => eq.IsNotNullOrEmpty()).Select(q => new SearchResults
                //    {
                //        Category = "Equipment",
                //        Name = q.Display,
                //        Url = q.Id,
                //    }));
                //    break;

                //case "equipment categories":
                //    if (equipmentCategory.IsNotNullOrEmpty() && equipmentCategory.EquipmentCategories.IsNotNullOrEmpty())
                //    {
                //        resultList.AddRange(equipmentCategory.EquipmentCategories.Select(q => new SearchResults()
                //        {
                //            Category = "Equipment Categories",
                //            Name = q.Display,
                //            Url = q.Id,
                //        }).ToList());
                //    }
                //    break;

                case "reports":
                    var reports = Work.Report.GetSystemReportsByName(query);

                    resultList.AddRange(reports.Where(r => r.IsNotNullOrEmpty()).Select(q => new SearchResults
                    {
                        Category = "Reports",
                        Name = q.Text,
                        Url = q.Value
                    }));
                    break;
            }

            return resultList;
        }
    }
}