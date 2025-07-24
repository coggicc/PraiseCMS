using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class EquipmentOperation : GenericRepository
    {
        public EquipmentOperation(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        #region Equipment
        public Equipment Get(string id)
        {
            return Read<Equipment>().FirstOrDefault(x => x.Id == id);
        }
        #endregion

        #region CRUD Equipment
        public void Create(Equipment entity)
        {
            Create<Equipment>(entity);
            SaveChanges();
        }

        public void Update(Equipment entity)
        {
            entity.ModifiedDate = DateTime.Now;
            entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;

            Update<Equipment>(entity);
            SaveChanges();
        }

        public void Delete(string id)
        {
            var d = Get(id);
            Delete<Equipment>(d);
            SaveChanges();
        }

        public void Delete(Equipment entity)
        {
            Delete<Equipment>(entity);
            SaveChanges();
        }
        #endregion

        #region Equipment Category
        public EquipmentCategory GetCategory(string id)
        {
            return Read<EquipmentCategory>().FirstOrDefault(x => x.Id == id);
        }

        public List<EquipmentCategory> GetAllCategoriesByChurch(string churchId)
        {
            return Read<EquipmentCategory>().Where(x => string.IsNullOrEmpty(x.ChurchId) || x.ChurchId == churchId).OrderBy(x => x.Name).ToList();
        }
        #endregion

        #region CRUD Equipment Category
        public void CreateCategory(EquipmentCategory category)
        {
            Create(category);
            SaveChanges();
        }

        public void UpdateCategory(EquipmentCategory category)
        {
            Update(category);
            SaveChanges();
        }

        public void DeleteCategory(string id)
        {
            var entity = GetCategory(id);
            Delete(entity);
            SaveChanges();
        }
        #endregion        

        public EquipmentView GetWithCategory(string churchId)
        {
            return new EquipmentView
            {
                EquipmentList = Db.Equipment.Where(x => x.ChurchId == churchId).ToList(),
                EquipmentCategories = Db.EquipmentCategories.Where(x => string.IsNullOrEmpty(x.ChurchId) || x.ChurchId == churchId).OrderBy(x => x.Name).ToList()
            };
        }

        public EquipmentView GetByCategory(string categoryId)
        {
            var model = new EquipmentView
            {
                EquipmentList = Db.Equipment.Where(x => x.ChurchId.Equals(SessionVariables.CurrentChurch.Id)).ToList(),
                EquipmentCategories = Db.EquipmentCategories.Where(x => x.ChurchId.Equals(SessionVariables.CurrentChurch.Id)).OrderBy(x => x.Name).ToList()
            };

            if (!string.IsNullOrEmpty(categoryId))
            {
                model.EquipmentList = model.EquipmentList.Where(x => x.EquipmentCategoryId.Equals(categoryId)).ToList();
                model.EquipmentCategories = Db.EquipmentCategories.Where(x => x.Id.Equals(categoryId) || x.ParentId.Equals(categoryId)).OrderBy(x => x.Name).ToList();
            }

            return model;
        }

        public EquipmentView GetWithCategorybyName(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new EquipmentView();
            }

            var model = new EquipmentView
            {
                EquipmentList = Db.Equipment.Where(x => x.ChurchId.Equals(SessionVariables.CurrentChurch.Id) && !string.IsNullOrEmpty(x.Name) && x.Name.ToUpper().Trim().Contains(name.ToUpper().Trim())).OrderBy(q => q.Name).ToList(),
                EquipmentCategories = Db.EquipmentCategories.Where(x => x.ChurchId.Equals(SessionVariables.CurrentChurch.Id) && !string.IsNullOrEmpty(x.Name) && x.Name.ToUpper().Trim().Contains(name.ToUpper().Trim())).OrderBy(q => q.Name).ToList()
            };
            //Add equipments related to categories
            var equipmentsIds = model.EquipmentList.Select(c => c.Id).ToList();
            var equipmentCategoryIds = model.EquipmentCategories.Select(c => c.Id).ToList();
            model.EquipmentList = Db.Equipment.Where(x => x.ChurchId.Equals(SessionVariables.CurrentChurch.Id) && !equipmentsIds.Contains(x.Id) && equipmentCategoryIds.Contains(x.EquipmentCategoryId)).OrderBy(q => q.Name).ToList();

            return model;
        }
    }
}