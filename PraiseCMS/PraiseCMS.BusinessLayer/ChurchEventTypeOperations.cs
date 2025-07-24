using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class ChurchEventTypeOperations : GenericRepository
    {
        public ChurchEventTypeOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public ChurchEventType Get(string id)
        {
            return Read<ChurchEventType>().FirstOrDefault(x => x.Id == id);
        }

        public List<ChurchEventType> GetAll(string churchId, string createdBy = null, bool isDeleted = false)
        {
            var query = Read<ChurchEventType>();

            // Apply the IsDeleted filter if specified
            if (isDeleted)
            {
                query = query.Where(x => x.IsDeleted);
            }
            else
            {
                // If isDeleted is not provided, exclude deleted records by default
                query = query.Where(x => !x.IsDeleted);
            }

            // Apply the CreatedBy filter if specified
            if (!string.IsNullOrEmpty(createdBy))
            {
                query = query.Where(x => x.CreatedBy == createdBy);
            }

            return query.OrderBy(x => x.Type).ThenBy(x => x.Type).ToList();
        }

        public ChurchEventTypesViewModel GetCustomName(string id)
        {
            var Event = Read<ChurchEventType>().FirstOrDefault(x => x.Id == id);

            return new ChurchEventTypesViewModel()
            {
                CalendarColor = !string.IsNullOrEmpty(Event.CalendarColor) ? Event.CalendarColor : "primary",
                Id = Event.Id,
                Type = !string.IsNullOrEmpty(Event.Type) ? Event.Type : Event.Type,
            };
        }

        public List<ChurchEventTypesViewModel> GetAllViewModel(string churchId, List<string> ids = null, bool includeCustomName = false)
        {
            var baseQuery = from cet in Db.ChurchEventTypes
                            join ce in Db.ChurchEvents
                            on new { ChurchEventTypeId = cet.Id, ChurchId = churchId } equals new { ce.ChurchEventTypeId, ce.ChurchId } into customEvents
                            from ce in customEvents.DefaultIfEmpty()
                            where !cet.IsDeleted && (ce == null || !ce.IsDeleted)
                            select new ChurchEventTypesViewModel
                            {
                                Id = cet.Id,
                                Type = cet.Type,
                                CustomEventName = ce != null && includeCustomName && !string.IsNullOrEmpty(ce.CustomEventName) ? ce.CustomEventName : null,
                                CombinedEventName = (ce != null && includeCustomName && !string.IsNullOrEmpty(ce.CustomEventName))
                                    ? ce.CustomEventName + " (" + cet.Type + ")"
                                    : cet.Type, // Use base type if no custom name
                                CalendarColor = cet.CalendarColor,
                                Description = ce != null && !string.IsNullOrEmpty(ce.Description) ? ce.Description : null
                            };

            // Apply filtering for specific ids after loading into memory
            var result = baseQuery.ToList();
            if (ids != null)
            {
                result = result.Where(cet => ids.Contains(cet.Id)).ToList();
            }

            return result.Distinct().OrderBy(x => x.Type).ToList();
        }

        #region CRUD
        public void Create(ChurchEventTypeView model)
        {
            Db.ChurchEventTypes.Add(model.ChurchEventType);
            Db.SaveChanges();
        }

        public ChurchEventTypeView CreateChurchEventTypeModel(string churchId, string userId)
        {
            var model = new ChurchEventTypeView
            {
                ChurchEventType = new ChurchEventType()
                {
                    Id = Utilities.GenerateUniqueId(),
                    CreatedBy = userId,
                    CreatedDate = DateTime.Now
                },
                CommonEventType = GetAll(churchId).Any() ? new List<string>() : ChurchEvents.Items.OrderBy(q => q).ToList()
            };

            return model;
        }

        public void Update(ChurchEventType model)
        {
            Update<ChurchEventType>(model);
            Db.SaveChanges();
        }

        public void Delete(string id)
        {
            var cvt = Work.ChurchEventType.Get(id);
            cvt.IsDeleted = true;
            Update<ChurchEventType>(cvt);
            SaveChanges();
        }

        public void Delete(ChurchEventType entity)
        {
            entity.IsDeleted = true;
            Update<ChurchEventType>(entity);
            SaveChanges();
        }
        #endregion
    }
}