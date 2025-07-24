using PraiseCMS.BusinessLayer.Repository;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Services;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.BusinessLayer
{
    public class PersonOperations : GenericRepository
    {
        public PersonOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        #region CRUD
        public Result<Person> Create(Person entity)
        {
            try
            {
                entity.CreatedDate = DateTime.Now;
                entity.CreatedBy = SessionVariables.CurrentUser?.User.IsNotNullOrEmpty() == true ? SessionVariables.CurrentUser.User.Id : Constants.System;
                Create<Person>(entity);
                SaveChanges();
                return new Result<Person>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Person>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Person> Update(Person entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                Update<Person>(entity);
                SaveChanges();
                return new Result<Person>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Person>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Person> Delete(Person entity)
        {
            try
            {
                Delete<Person>(entity);
                SaveChanges();
                return new Result<Person>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Person>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        public Result<Person> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<Person>(entity);
                SaveChanges();
                return new Result<Person>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Person>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        public Person Get(string id)
        {
            var person = Read<Person>().FirstOrDefault(x => x.Id == id);

            if (person == null) return null;

            var user = Work.User.GetByPersonId(person.Id);

            if (user != null)
            {
                person.UserId = user.Id;

                person.Email = string.IsNullOrEmpty(person.Email) ? user.Email : person.Email;
                person.PhoneNumber = string.IsNullOrEmpty(person.PhoneNumber) ? user.PhoneNumber : person.PhoneNumber;
                person.Address1 = string.IsNullOrEmpty(person.Address1) ? user.Address1 : person.Address1;
                person.Address2 = string.IsNullOrEmpty(person.Address2) ? user.Address2 : person.Address2;
                person.City = string.IsNullOrEmpty(person.City) ? user.City : person.City;
                person.State = string.IsNullOrEmpty(person.State) ? user.State : person.State;
                person.Zip = string.IsNullOrEmpty(person.Zip) ? user.Zip : person.Zip;

                // Retrieve user settings only if needed
                var userSettings = Work.UserSetting.GetByUserId(user.Id);
                person.ProfileImage = string.IsNullOrEmpty(person.ProfileImage)
                    ? userSettings?.ProfileImage ?? person.ProfileImage
                    : person.ProfileImage;
            }

            return person;
        }

        public Person GetByUserId(string userId)
        {
            var user = Work.User.Get(userId);

            if (user?.PersonId != null)
            {
                return Read<Person>().FirstOrDefault(x => x.Id == user.PersonId);
            }

            return new Person();
        }

        public List<Person> GetAllByPersonIds(string churchId, IEnumerable<string> personIds = null)
        {
            // Get a distinct list of Person IDs related to the given church
            var peopleIds = Read<ChurchPerson>()
                .Where(x => x.ChurchId.Equals(churchId))
                .Select(x => x.PersonId)
                .ToHashSet(); // Use HashSet to ensure distinct IDs

            // Query for Persons related to the church
            var peopleQuery = GetAllByPersonIds(peopleIds);

            // Filter Persons based on the provided personIds if not null
            if (personIds?.Any() == true)
            {
                peopleQuery = peopleQuery.Where(x => personIds.Contains(x.Id));
            }

            var people = peopleQuery.ToList();
            var peopleIdsList = people.Select(p => p.Id).ToList();

            // Retrieve Users by the filtered Person IDs
            var usersDictionary = Work.User.GetAllByPersonIds(peopleIdsList)
                .Where(u => !string.IsNullOrEmpty(u.PersonId))
                .ToDictionary(u => u.PersonId);

            // Retrieve UserSettings by the User IDs
            var userSettingsDictionary = Work.UserSetting.GetByUserId(usersDictionary.Values.Select(u => u.Id).ToList())
                .ToDictionary(us => us.UserId);

            // Update the Person properties based on User and UserSettings
            foreach (var person in people)
            {
                if (usersDictionary.TryGetValue(person.Id, out var user))
                {
                    person.UserId = user.Id;
                    person.Email = string.IsNullOrEmpty(person.Email) ? user.Email : person.Email;
                    person.PhoneNumber = string.IsNullOrEmpty(person.PhoneNumber) ? user.PhoneNumber : person.PhoneNumber;
                    person.Address1 = string.IsNullOrEmpty(person.Address1) ? user.Address1 : person.Address1;
                    person.Address2 = string.IsNullOrEmpty(person.Address2) ? user.Address2 : person.Address2;
                    person.City = string.IsNullOrEmpty(person.City) ? user.City : person.City;
                    person.State = string.IsNullOrEmpty(person.State) ? user.State : person.State;
                    person.Zip = string.IsNullOrEmpty(person.Zip) ? user.Zip : person.Zip;

                    if (!string.IsNullOrEmpty(person.UserId) && userSettingsDictionary.TryGetValue(person.UserId, out var setting))
                    {
                        person.ProfileImage = string.IsNullOrEmpty(person.ProfileImage) ? setting.ProfileImage : person.ProfileImage;
                    }
                }
            }

            return people.OrderBy(x => x.Display).ToList(); // Ensuring distinct and ordered result
        }

        public IQueryable<Person> GetAllByPersonIds(IEnumerable<string> ids = null)
        {
            return Read<Person>().Where(x => ids.Contains(x.Id));
        }

        public IEnumerable<Person> GetAllByUserIds(List<string> userIds)
        {
            var user = Read<ApplicationUser>().Where(x => userIds.Contains(x.Id)).ToList();

            if (user.IsNotNullOrEmpty() && user.Any())
            {
                var ids = user.Select(q => q.PersonId).ToList();

                return Read<Person>().Where(x => ids.Contains(x.Id)).ToList();
            }

            return new List<Person>();
        }

        public List<Person> GetAllByChurchId(string churchId)
        {
            // Get a list of Person IDs related to the given church
            var peopleIds = Read<ChurchPerson>()
                .Where(x => x.ChurchId.Equals(churchId))
                .Select(x => x.PersonId)
                .ToHashSet(); // Use HashSet for O(1) lookups

            // Query for Persons related to the church
            var people = GetAllByPersonIds(peopleIds);

            return people.ToList();
        }

        public List<Person> GetAllWhereChurchIdNotNull()
        {
            // Get a list of Person IDs where church Id is not null
            var peopleIds = Read<ChurchPerson>()
                .Where(x => !string.IsNullOrEmpty(x.ChurchId))
                .Select(x => x.PersonId)
                .ToHashSet(); // Use HashSet for O(1) lookups

            // Query for Persons related to the church
            var people = GetAllByPersonIds(peopleIds);

            return people.ToList();
        }

        public Person GetByEmailAndPhone(string email, string phone)
        {
            if (email.IsNotNullOrEmpty())
            {
                var byEmail = GetByEmail(email);

                if (byEmail.IsNotNull())
                {
                    return byEmail;
                }
            }

            if (phone.IsNotNullOrEmpty())
            {
                var byPhone = GetByPhone(phone);

                if (byPhone.IsNotNull())
                {
                    return byPhone;
                }
            }

            return null;
        }

        public Person GetByEmailAndPhoneAndName(string email, string phone, string firstName, string lastName)
        {
            if (email.IsNotNullOrEmpty())
            {
                var byEmail = GetByEmail(email);

                if (byEmail.IsNotNull())
                {
                    return byEmail;
                }
            }

            if (phone.IsNotNullOrEmpty())
            {
                var byPhone = GetByPhone(phone);

                if (byPhone.IsNotNull())
                {
                    return byPhone;
                }
            }

            if (firstName.IsNotNullOrEmpty() && lastName.IsNotNullOrEmpty())
            {
                var byName = GetByName(firstName, lastName);

                if (byName.IsNotNull())
                {
                    return byName;
                }
            }

            return null;
        }

        public Person GetByEmail(string email)
        {
            return Read<Person>().FirstOrDefault(x => x.Email == email);
        }

        public Person GetByPhone(string phone)
        {
            return Read<Person>().FirstOrDefault(x => x.PhoneNumber == phone);
        }

        public Person GetByName(string firstName, string lastName)
        {
            return Read<Person>().FirstOrDefault(x => x.FirstName.Equals(firstName) && x.LastName.Equals(lastName));
        }

        public List<Person> GetDeceasedPeople(string churchId, DateRange dateRange)
        {
            var churchPeople = Read<ChurchPerson>()
                .Where(x => x.ChurchId.Equals(churchId))
                .Select(x => x.PersonId)
            .ToList();

            return Read<Person>()
                .Where(x => churchPeople.Contains(x.Id)
                    && x.DeceasedDate != null
                    && x.DeceasedDate >= dateRange.StartDate
                    && x.DeceasedDate <= dateRange.EndDate)
                .OrderByDescending(x => x.DeceasedDate)
            .ToList();
        }

        public IEnumerable<Person> GetAllByName(string name)
        {
            var people = GetAllByPersonIds(SessionVariables.CurrentChurch.Id);
            // Normalize the name to lowercase and trim it
            var normalizedName = name.ToLower().Trim();

            // Filter the people based on the normalized name
            var filteredPeople = people
                .Where(x => ($"{x.FirstName.ToLower().Trim()} {x.LastName.ToLower().Trim()}").Contains(normalizedName))
                .ToList();

            var users = Work.User.GetAllByPersonIds(filteredPeople.Select(q => q.Id).ToList()).Where(z => z.PersonId.IsNotNullOrEmpty());
            filteredPeople.Select(d => { d.UserId = users.Any(q => q.PersonId.Equals(d.Id)) ? users.First(q => q.PersonId.Equals(d.Id)).Id : null; return d; }).ToList();

            return filteredPeople;
        }

        public List<UpcomingBirthdaysVM> GetUpcomingBirthdays(string churchId)
        {
            var people = GetAllByChurchId(churchId);
            var results = new List<UpcomingBirthdaysVM>();

            foreach (var person in people)
            {
                var date = Convert.ToDateTime(person.DOB);

                if (date.Month == DateTime.Now.Month || date.Month == DateTime.Now.Month + 1)
                {
                    var dob = new DateTime(DateTime.Now.Year, date.Month, date.Day);
                    var diff = (dob - DateTime.Now).Days;

                    if (diff >= 0 && diff <= 30)
                    {
                        results.Add(new UpcomingBirthdaysVM
                        {
                            Id = person.Id,
                            Display = person.Display,
                            DOB = date,
                            RemainingDays = diff,
                            ProfileImageURL = person.ProfileImageURL
                        });
                    }
                }
            }

            return results.OrderBy(x => x.RemainingDays).ToList();
        }

        #region ChurchPerson
        public ChurchPerson GetByPersonId(string personId)
        {
            return Read<ChurchPerson>().FirstOrDefault(x => x.PersonId.Equals(personId));
        }

        public List<ChurchPerson> GetAllByPersonId(string personId)
        {
            return Read<ChurchPerson>().Where(x => x.PersonId.Equals(personId)).ToList();
        }

        public Result<ChurchPerson> CreateChurchPerson(ChurchPerson entity)
        {
            try
            {
                entity.CreatedBy = !string.IsNullOrEmpty(entity.CreatedBy) ? entity.CreatedBy : Constants.System;
                entity.CreatedDate = entity.CreatedDate != DateTime.MinValue ? entity.CreatedDate : DateTime.Now;

                Create(entity);
                SaveChanges();
                return new Result<ChurchPerson>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<ChurchPerson>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion

        #region CRUD Relationship Model
        public List<Relationship> GetAllRelatives(string personId, bool includePerson = false)
        {
            var relatives = Read<Relationship>().Where(x => x.PersonId.Equals(personId)).ToList();

            if (includePerson)
            {
                var people = GetAllByPersonIds(SessionVariables.CurrentChurch.Id, relatives.Select(x => x.RelativePersonId));
                relatives.Select(x => { x.RelativePerson = people.Find(q => q.Id.Equals(x.RelativePersonId)); return x; }).ToList();
            }

            return relatives;
        }

        public ResponseModel AddUpdateRelationship(IEnumerable<Relationship> model)
        {
            try
            {
                if (model.Any())
                {
                    var relatives = GetAllRelatives(model.First().PersonId).AsEnumerable();

                    if (relatives.Any())
                    {
                        Delete(relatives);
                    }

                    Create(model);
                    SaveChanges();
                    return new ResponseModel { Success = true };
                }

                return new ResponseModel { Success = false, Message = "Add at least once record" };
            }
            catch (Exception ex)
            {
                return new ResponseModel { Success = false, Message = ex.Message };
            }
        }

        public ResponseModel DeleteRelationships(string personId)
        {
            try
            {
                var relatives = GetAllRelatives(personId).AsEnumerable();

                if (relatives.Any())
                {
                    Delete(relatives);
                    SaveChanges();
                }

                return new ResponseModel { Success = true, Message = $"{relatives.Count().Pluralize("Relationship")} has been deleted." };
            }
            catch (Exception ex)
            {
                return new ResponseModel { Success = false, Message = $"{Constants.DefaultErrorMessage} {ex.Message}" };
            }
        }
        #endregion

        #region Super Admin Only
        public List<string> GetActivePersonIds(string churchId)
        {
            var personIds = Read<Person>().Join(
                Read<ChurchPerson>(),
                person => person.Id,
                churchPerson => churchPerson.PersonId,
                (person, churchPerson) => new { person.Id, person.IsActive, churchPerson.ChurchId }
            ).Where(p => p.ChurchId == churchId && p.IsActive)
             .Select(p => p.Id)
             .ToList();

            if (personIds.Count == 0)
                throw new Exception("No active people found for the given church.");

            return personIds;
        }
        #endregion
    }
}