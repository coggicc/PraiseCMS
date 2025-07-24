using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class HouseholdOperations : GenericRepository
    {
        public HouseholdOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        #region Household
        public Household Get(string id)
        {
            return Read<Household>().FirstOrDefault(x => x.Id == id);
        }

        public Household GetByName(string name)
        {
            return Read<Household>().FirstOrDefault(x => x.Name.ToLower().Trim() == name.ToLower().Trim() && x.ChurchId == SessionVariables.CurrentChurch.Id);
        }

        public List<Household> Get(IEnumerable<string> Ids)
        {
            return Read<Household>().Where(x => Ids.Contains(x.Id)).ToList();
        }

        public List<Household> GetAll(string churchId)
        {
            return Read<Household>().Where(x => x.ChurchId == churchId).OrderBy(x => x.Name).ToList();
        }
        #endregion

        #region CRUD
        public Result<Household> Create(Household entity)
        {
            try
            {
                Create<Household>(entity);
                SaveChanges();
                return new Result<Household>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Household>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Household> Update(Household entity)
        {
            try
            {
                Update<Household>(entity);
                SaveChanges();
                return new Result<Household>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Household>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Household> Delete(Household entity)
        {
            try
            {
                Delete<Household>(entity);
                SaveChanges();
                return new Result<Household>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Household>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Household> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                var householdMembers = MembersByHousehold(id);
                try
                {
                    foreach (var member in householdMembers)
                    {
                        Delete(member);
                    }
                }
                catch (Exception ex)
                {
                    ExceptionLogger.LogException(ex);
                    return new Result<Household>
                    {
                        Exception = ex,
                        Message = Constants.DeleteExceptionMessage,
                        ResultType = ResultType.Exception
                    };
                }
                Delete<Household>(entity);
                SaveChanges();
                return new Result<Household>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Household>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        #region Household Member
        public HouseholdMember GetMember(string id)
        {
            return Read<HouseholdMember>().FirstOrDefault(x => x.Id == id && x.IsActive);
        }

        public List<HouseholdMember> MembersByHousehold(string id)
        {
            return Read<HouseholdMember>().Where(x => x.HouseholdId == id && x.IsActive).ToList();
        }

        public List<HouseholdMemberVM> MembersByHouseholdWithName(string ids)
        {
            var id = ids.SplitToList();
            return (from HM in Db.HouseholdMembers
                    join P in Db.People on HM.PersonId equals P.Id
                    where HM.IsActive && id.Contains(HM.HouseholdId)
                    orderby P.FirstName
                    select new
                    {
                        HM.CreatedBy,
                        HM.HouseholdId,
                        HM.IsHeadofHousehold,
                        HM.CreatedDate,
                        HM.ModifiedBy,
                        HM.ModifiedDate,
                        HM.Id,
                        HM.IsActive,
                        HM.PersonId,
                        HM.FamilyRole,
                        MemberName = P.FirstName + " " + P.LastName
                    }).ToList().Select(x => new HouseholdMemberVM()
                    {
                        CreatedBy = x.CreatedBy,
                        HouseholdId = x.HouseholdId,
                        IsHeadofHousehold = x.IsHeadofHousehold,
                        CreatedDate = x.CreatedDate,
                        ModifiedBy = x.ModifiedBy,
                        ModifiedDate = x.ModifiedDate,
                        Id = x.Id,
                        IsActive = x.IsActive,
                        PersonId = x.PersonId,
                        MemberName = x.MemberName,
                        FamilyRole = x.FamilyRole,
                    }).ToList();
        }

        public HouseholdMember GetHeadOfHousehold(string id)
        {
            return Read<HouseholdMember>().FirstOrDefault(x => x.HouseholdId == id && x.IsHeadofHousehold && x.IsActive);
        }

        public List<HouseholdMemberVM> MembersByHouseholds(string id)
        {
            var ids = id.Split(',').ToArray();
            var member = Read<HouseholdMember>().Where(x => ids.Contains(x.HouseholdId) && x.IsActive).ToList();

            return member.Select(q => new HouseholdMemberVM()
            {
                IsHeadofHousehold = q.IsHeadofHousehold,
                PersonId = q.PersonId,
                IsActive = q.IsActive,
                HouseholdId = q.HouseholdId,
                Id = q.Id,
                FamilyRole = q.FamilyRole,
                CreatedBy = q.CreatedBy,
                CreatedDate = q.CreatedDate,
                ModifiedBy = q.ModifiedBy,
                ModifiedDate = q.ModifiedDate
            }).ToList();
        }
        #endregion

        #region CRUD Household Member
        public Result<HouseholdMember> CreateMember(HouseholdMember entity)
        {
            try
            {
                Create(entity);
                SaveChanges();
                return new Result<HouseholdMember>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<HouseholdMember>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<HouseholdMember> UpdateMember(HouseholdMember entity)
        {
            try
            {
                Update(entity);
                SaveChanges();
                return new Result<HouseholdMember>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<HouseholdMember>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<HouseholdMember> DeleteMember(string id)
        {
            try
            {
                var entity = GetMember(id);
                Delete(entity);
                SaveChanges();
                return new Result<HouseholdMember>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<HouseholdMember>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        #region Household Dashboard
        public HouseholdDashboard GetPeopleAndHouseholds(string param)
        {
            var dashboard = new HouseholdDashboard
            {
                People = Work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id).OrderBy(x => x.FullName).Where(x => x.Display.ContainsIgnoreCase(param.ToLower().Trim())).ToList(),
                Households = GetAll(SessionVariables.CurrentChurch.Id).Where(x => x.Display.ContainsIgnoreCase(param.ToLower().Trim())).OrderBy(x => x.Name).ToList()
            };

            if (!dashboard.Households.Any()) return dashboard;

            var ids = string.Join(",", dashboard.Households.Select(q => q.Id));
            dashboard.HouseholdMembers = MembersByHouseholdWithName(ids);
            var users = Work.User.GetAllByPersonIds(dashboard.HouseholdMembers.Select(q => q.PersonId).ToList()).Where(z => z.PersonId.IsNotNullOrEmpty()).ToList();
            dashboard.HouseholdMembers.Select(d => { d.UserId = users.Any(q => q.PersonId.Equals(d.PersonId)) ? users.First(q => q.PersonId.Equals(d.PersonId)).Id : null; return d; }).ToList();

            return dashboard;
        }
        #endregion

        #region PersonHousehold
        public List<PersonHousehold> GetAllHouseholdsByPerson(string personId)
        {
            var householdMembers = Read<HouseholdMember>().Where(q => q.PersonId.Equals(personId)).ToList();
            var households = Get(householdMembers.Select(q => q.HouseholdId));
            var model = householdMembers.Select(q => new PersonHousehold
            {
                HouseholdMember = q,
                Household = households.Any(x => x.Id.Equals(q.HouseholdId)) ? households.First(x => x.Id.Equals(q.HouseholdId)) : new Household()
            }).ToList();

            return model.OrderBy(q => q.Household.Display).ToList();
        }

        public List<PersonHousehold> GetAllHouseholdsByPersonIds(IEnumerable<string> personIds)
        {
            var householdMembers = Read<HouseholdMember>().Where(q => personIds.Contains(q.PersonId)).ToList();
            var households = Get(householdMembers.Select(q => q.HouseholdId));
            var model = householdMembers.Select(q => new PersonHousehold
            {
                HouseholdMember = q,
                Household = households.Any(x => x.Id.Equals(q.HouseholdId)) ? households.First(x => x.Id.Equals(q.HouseholdId)) : new Household()
            }).ToList();

            return model.OrderBy(q => q.Household.Display).ToList();
        }
        #endregion
    }
}