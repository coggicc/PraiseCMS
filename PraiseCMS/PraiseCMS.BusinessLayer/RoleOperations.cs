using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.Shared.Methods;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class RoleOperations : GenericRepository
    {
        public RoleOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public ApplicationRoles Get(string id)
        {
            return DAL.ReadRoleById(id);
        }

        public ApplicationRoles GetByName(string name)
        {
            return DAL.ReadRoleByName(name);
        }

        public List<ApplicationRoles> GetAll()
        {
            return DAL.ReadAllRoles();
        }

        public List<ApplicationRoles> GetAll(string churchId)
        {
            return churchId.IsNullOrEmpty() ? DAL.ReadAllRoles() : DAL.ReadAllRoles(churchId);
        }

        public List<ApplicationRoles> GetSystemRoles()
        {
            return Read<ApplicationRoles>().Where(x => x.ChurchId == null).ToList();
        }

        public void AddUserRole(string userId, string roleId)
        {
            DAL.InsertUserRole(new AspNetUserRoles { RoleId = roleId, UserId = userId });
        }

        public List<RoleUserCount> CountUsers(List<string> roleIds, string churchId)
        {
            var result = new List<RoleUserCount>();

            foreach (var id in roleIds)
            {
                result.Add(new RoleUserCount
                {
                    RoleId = id,
                    UserCount = Work.User.GetByRoleId(id, churchId).Count
                });
            }

            return result;
        }

        public RoleModules GetRoleModule(string roleId, string moduleId)
        {
            return Read<RoleModules>().FirstOrDefault(x => x.RoleId == roleId && x.ModuleId == moduleId);
        }
    }
}