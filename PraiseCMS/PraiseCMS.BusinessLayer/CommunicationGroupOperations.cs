using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class CommunicationGroupOperations : GenericRepository
    {
        public CommunicationGroupOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public CommunicationGroup Get(string id)
        {
            return Read<CommunicationGroup>().FirstOrDefault(x => x.Id == id);
        }

        public IEnumerable<CommunicationGroup> Get(List<string> ids)
        {
            return Read<CommunicationGroup>().Where(x => ids.Contains(x.Id)).ToList();
        }

        public IEnumerable<CommunicationGroup> GetByPerson(string id)
        {
            var peoples = Read<CommunicationGroupsPeople>().Where(x => x.PersonId.Equals(id) && x.IsActive).ToList();
            var groups = Get(peoples.Select(q => q.CommunicationGroupId).Distinct().ToList());
            groups.Select(x =>
            {
                x.GroupsPersonId = peoples.Find(q => q.CommunicationGroupId.Equals(x.Id)).Id;
                x.DisableEmailNotifications = peoples.Find(q => q.CommunicationGroupId.Equals(x.Id)).DisableEmailNotifications;
                x.DisableTextNotifications = peoples.Find(q => q.CommunicationGroupId.Equals(x.Id)).DisableTextNotifications;
                x.DisableSystemNotifications = peoples.Find(q => q.CommunicationGroupId.Equals(x.Id)).DisableSystemNotifications;
                return x;
            }).ToList();

            return groups;
        }

        public CommunicationGroupsPeople GetGroupMember(string id)
        {
            return Read<CommunicationGroupsPeople>().FirstOrDefault(x => x.Id == id && x.IsActive);
        }

        public CommunicationGroupsPeople GetMemberByGroupIdAndPersonId(string groupId, string personId)
        {
            return Read<CommunicationGroupsPeople>().FirstOrDefault(x => x.CommunicationGroupId.Equals(groupId) && x.PersonId.Equals(personId) && x.IsActive);
        }

        public CommunicationGroupsPeople GetGroupMemberWithName(string id)
        {
            return (from CGM in Db.CommunicationGroupsPeople
                          join P in Db.People on CGM.PersonId equals P.Id
                          where CGM.IsActive && CGM.Id == id
                          orderby P.FirstName
                          select new
                          {
                              CGM.CreatedBy,
                              CGM.CommunicationGroupId,
                              CGM.CreatedDate,
                              CGM.ModifiedBy,
                              CGM.ModifiedDate,
                              CGM.Id,
                              CGM.IsActive,
                              CGM.PersonId,
                              MemberName = P.FirstName + " " + P.LastName
                          }).ToList().Select(x => new CommunicationGroupsPeople
                          {
                              CreatedBy = x.CreatedBy,
                              CommunicationGroupId = x.CommunicationGroupId,
                              CreatedDate = x.CreatedDate,
                              ModifiedBy = x.ModifiedBy,
                              ModifiedDate = x.ModifiedDate,
                              Id = x.Id,
                              IsActive = x.IsActive,
                              PersonId = x.PersonId,
                              MemberName = x.MemberName,
                          }).FirstOrDefault();
        }

        public List<CommunicationGroupsPeople> GetMemberByGroupIds(string ids)
        {
            var list = ids.SplitToList();
            return Read<CommunicationGroupsPeople>().Where(x => list.Contains(x.CommunicationGroupId) && x.IsActive).ToList();
        }

        public bool Override(string id, string key, bool value)
        {
            var data = Read<CommunicationGroupsPeople>().FirstOrDefault(x => x.Id.Equals(id) && x.IsActive);

            if (data != null)
            {
                switch (key.ToUpper())
                {
                    case "EMAIL":
                        if (value)
                        {
                            data.DisableEmailNotifications = true;
                        }
                        else
                        {
                            data.DisableEmailNotifications = null;
                        }
                        break;
                    case "TEXT":
                        if (value)
                        {
                            data.DisableTextNotifications = true;
                        }
                        else
                        {
                            data.DisableTextNotifications = null;
                        }
                        break;
                    case "SYSTEMNOTIFICATION":
                        if (value)
                        {
                            data.DisableSystemNotifications = true;
                        }
                        else
                        {
                            data.DisableSystemNotifications = null;
                        }
                        break;

                }
                Update(data);
                SaveChanges();

                return true;
            }

            return false;
        }

        public List<CommunicationGroupsPeople> GetMemberByGroupsWithName(string ids)
        {
            var id = ids.SplitToList();
            var members = Read<CommunicationGroupsPeople>().Where(x => id.Contains(x.CommunicationGroupId)).ToList();
            var people = Work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id, members.Select(x => x.PersonId).ToList());
            members.Select(x => { x.Person = people.Find(p => p.Id.Equals(x.PersonId)); return x; }).ToList();
            members.Select(x => { x.MemberName = x.Person.Display; x.PhoneNumber = x.Person.PhoneNumber; x.Email = x.Person.Email; return x; }).ToList();

            return members;
        }

        public CommunicationGroup GetByName(string name)
        {
            return Read<CommunicationGroup>().FirstOrDefault(x => x.Name.ToLower().Trim() == name.ToLower().Trim() && x.ChurchId == SessionVariables.CurrentChurch.Id);
        }
        public Result<CommunicationGroup> Create(CommunicationGroup entity)
        {
            try
            {
                Create<CommunicationGroup>(entity);
                SaveChanges();
                return new Result<CommunicationGroup>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<CommunicationGroup>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<CommunicationGroupsPeople> CreateGroupMember(CommunicationGroupsPeople entity)
        {
            try
            {
                Create(entity);
                SaveChanges();
                return new Result<CommunicationGroupsPeople>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<CommunicationGroupsPeople>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<CommunicationGroupsPeople> CreateGroupMember(IEnumerable<CommunicationGroupsPeople> entity)
        {
            try
            {
                Create(entity);
                SaveChanges();
                return new Result<CommunicationGroupsPeople> { ResultType = ResultType.Success };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<CommunicationGroupsPeople>
                {
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<CommunicationGroup> Delete(CommunicationGroup entity)
        {
            try
            {
                Delete<CommunicationGroup>(entity);
                SaveChanges();
                return new Result<CommunicationGroup>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<CommunicationGroup>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<CommunicationGroup> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<CommunicationGroup>(entity);
                SaveChanges();
                return new Result<CommunicationGroup>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<CommunicationGroup>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<CommunicationGroupsPeople> DeleteGroupMember(CommunicationGroupsPeople entity)
        {
            try
            {
                Delete(entity);
                SaveChanges();
                return new Result<CommunicationGroupsPeople>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<CommunicationGroupsPeople>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<CommunicationGroupsPeople> DeleteGroupMember(string id)
        {
            try
            {
                var entity = GetGroupMember(id);
                Delete(entity);
                SaveChanges();
                return new Result<CommunicationGroupsPeople>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<CommunicationGroupsPeople>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<CommunicationGroupsPeople> InactivePersonInGroup(string groupId, string personId)
        {
            try
            {
                var entity = GetMemberByGroupIdAndPersonId(groupId, personId);

                if (entity.IsNotNullOrEmpty())
                {
                    entity.IsActive = false;
                    Update(entity);
                    SaveChanges();
                }

                return new Result<CommunicationGroupsPeople>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<CommunicationGroupsPeople>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<CommunicationGroup> Update(CommunicationGroup entity)
        {
            try
            {
                Update<CommunicationGroup>(entity);
                SaveChanges();
                return new Result<CommunicationGroup>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<CommunicationGroup>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<CommunicationGroupsPeople> UpdateGroupMember(CommunicationGroupsPeople entity)
        {
            try
            {
                Update(entity);
                SaveChanges();
                return new Result<CommunicationGroupsPeople>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<CommunicationGroupsPeople>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public List<CommunicationGroup> GetAll(string churchId, bool includePeople = false)
        {
            var group = Read<CommunicationGroup>().Where(x => x.ChurchId == churchId).OrderBy(x => x.Name).ToList();

            if (includePeople)
            {
                var member = GetMemberByGroupsWithName(string.Join(",", group.Select(x => x.Id)));
                group.Select(x => { x.CommunicationGroupsPeople = member.FindAll(m => m.CommunicationGroupId.Equals(x.Id)); return x; }).ToList();
            }

            return group;
        }

        public ResponseModel Communicate(CommunicateWithGroupModel model)
        {
            try
            {
                var groupMembers = GetMemberByGroupsWithName(model.SelectedGroupId).Where(x => x.IsActive).ToList();
                var emails = groupMembers.FindAll(x => x.DisableEmailNotifications.IsNullOrEmpty()).Select(x => x.Person).Where(x => x.Email.IsNotNullOrEmpty()).Select(x => x.Email).ToList();
                var phoneNumbers = groupMembers.FindAll(x => x.DisableTextNotifications.IsNullOrEmpty()).Select(x => x.Person).Where(x => x.PhoneNumber.IsNotNullOrEmpty()).Select(x => x.PhoneNumber).ToList();
                var users = Work.User.GetAllByPersonIds(groupMembers.Select(x => x.PersonId).ToList());
                var groupName = !string.IsNullOrEmpty(model.SelectedGroupId) ? Work.CommunicationGroup.Get(model.SelectedGroupId).Display : string.Empty;
                var notifications = users.Select(x => new Notification
                {
                    Id = Utilities.GenerateUniqueId(),
                    CreatedBy = SessionVariables.CurrentUser.User.Id,
                    CreatedDate = DateTime.Now,
                    Type = $"CommunicationGroup-{groupName}",
                    TypeId = model.SelectedGroupId,
                    ChurchId = SessionVariables.CurrentChurch.Id,
                    Name = model.Subject,
                    Viewed = false,
                    AssignedToUserId = x.Id
                });
                Work.Notification.Create(notifications);

                if (emails.Any())
                {
                    var email = new Email
                    {
                        Id = Utilities.GenerateUniqueId(),
                        Message = EmailTemplates.General.Replace("{message}", model.Message),
                        Bcc = string.Join(",", emails),
                        Attachments = null,
                        Subject = model.Subject,
                        CreatedBy = SessionVariables.CurrentUser?.User.Id != null ? SessionVariables.CurrentUser.User.Id : string.Empty,
                        CreatedDate = DateTime.Now,
                        Type = $"CommunicationGroup-{groupName}",
                        TypeId = model.SelectedGroupId,
                    };
                    Emailer.SendEmail(email, null, null, new Domain { EmailLogo = SessionVariables.CurrentChurch.Logo, Name = SessionVariables.CurrentChurch.Display, EmailReplyAddress = SessionVariables.CurrentChurch.Email, EmailDisplay = SessionVariables.CurrentChurch.Display }, SessionVariables.CurrentChurch);
                }

                if (phoneNumbers.Any())
                {
                    var smsMessage = new SmsMessage
                    {
                        Id = Utilities.GenerateUniqueId(),
                        To = string.Join(",", phoneNumbers),
                        Message = $"{model.Subject}\n\n{model.Message}",
                        CreatedDate = DateTime.Now,
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        Type = $"CommunicationGroup-{groupName}",
                        TypeId = model.SelectedGroupId,
                    };
                    Utilities.SendMessage(smsMessage);
                }
                return new ResponseModel { Success = true };
            }
            catch (Exception ex)
            {
                return new ResponseModel { Success = false, Message = ex.Message };
            }
        }
    }
}