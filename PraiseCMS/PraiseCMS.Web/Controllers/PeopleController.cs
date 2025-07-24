using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]

    [RequirePermission(ModuleId = "87345006275f4bcae00d144f98a62a")]
    public class PeopleController : BaseController
    {
        public ActionResult Index()
        {
            var model = new PeopleDashboard
            {
                People = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id).OrderByDescending(x => x.CreatedDate).Take(5).OrderBy(x => x.Display).ToList()
            };

            return View(model);
        }

        public ActionResult List(string filterKeyword)
        {
            var model = new PeopleDashboard
            {
                People = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id)
            };

            if (!string.IsNullOrEmpty(filterKeyword))
            {
                ViewBag.userFilterKeyword = filterKeyword;
                filterKeyword = filterKeyword.ToLower().Trim();
                var filterList = new List<Person>();
                filterList.AddRange(model.People.Where(x => !filterList.Select(s => s.Id).Contains(x.Id) && !string.IsNullOrEmpty(x.FullName) && x.FullName.ContainsIgnoreCase(filterKeyword)));
                filterList.AddRange(model.People.Where(x => !filterList.Select(s => s.Id).Contains(x.Id) && !string.IsNullOrEmpty(x.FirstName) && x.FirstName.ContainsIgnoreCase(filterKeyword)));
                filterList.AddRange(model.People.Where(x => !filterList.Select(s => s.Id).Contains(x.Id) && !string.IsNullOrEmpty(x.LastName) && x.LastName.ContainsIgnoreCase(filterKeyword)));
                filterList.AddRange(model.People.Where(x => !filterList.Select(s => s.Id).Contains(x.Id) && !string.IsNullOrEmpty(x.Email) && x.Email.ContainsIgnoreCase(filterKeyword)));
                filterList.AddRange(model.People.Where(x => !filterList.Select(s => s.Id).Contains(x.Id) && !string.IsNullOrEmpty(x.PhoneNumber) && x.PhoneNumber.ContainsIgnoreCase(filterKeyword)));
                model.People = filterList;
            }

            var users = work.User.GetAllByPersonIds(model.People.Select(q => q.Id).ToList()).Where(z => z.PersonId.IsNotNullOrEmpty());
            model.People.Select(d => { d.UserId = users.Any(q => q.PersonId.Equals(d.Id)) ? users.First(q => q.PersonId.Equals(d.Id)).Id : null; return d; }).ToList();

            return View(model);
        }

        [HttpGet]
        public ActionResult _CreatePerson()
        {
            var model = new Person()
            {
                Id = Utilities.GenerateUniqueId(),
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now,
                IsActive = true
            };

            return PartialView("_CreatePerson", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _CreatePerson(Person model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return PartialView("_CreatePerson", model);
                }

                var _person = work.Person.GetByEmailAndPhone(model.Email, model.PhoneNumber);

                if (_person.IsNotNullOrEmpty())
                {
                    CreateAlertMessage("Uh-oh! A person has already been created with the same email or phone number. You cannot create a duplicate person.", AlertMessageTypes.Warning, AlertMessageIcons.Warning);
                    return PartialView("_CreatePerson", model);
                }

                var result = work.Person.Create(model);

                if (result.ResultType.Equals(ResultType.Success))
                {
                    work.Person.CreateChurchPerson(new ChurchPerson
                    {
                        Id = Utilities.GenerateUniqueId(),
                        PersonId = model.Id,
                        ChurchId = SessionVariables.CurrentChurch.Id,
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now
                    });

                    return AjaxRedirectTo("/users/userprofile?Id=" + model.Id + "&type=person");
                }
                else
                {
                    CreateAlertMessage($"{result.Message} Error:{result.Exception.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
                }

                return PartialView("_CreatePerson", model);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                CreateAlertMessage($"{Constants.DefaultErrorMessage} {ex.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                return PartialView("_CreatePerson", model);
            }
        }

        //Called when adding a donor from the bulk donor giving modal
        [HttpPost]
        public ActionResult CreatePerson(Person person)
        {
            try
            {
                var _person = work.Person.GetByEmailAndPhoneAndName(person.Email, person.PhoneNumber, person.FirstName, person.LastName);

                if (_person.IsNotNullOrEmpty())
                {
                    return Json(new { Success = false, Message = "Uh-oh! A person has already been created with the same email or phone number. You cannot create a duplicate person." });
                }

                person.Id = Utilities.GenerateUniqueId();
                person.IsActive = true;
                person.CreatedBy = SessionVariables.CurrentUser.User.Id;
                person.CreatedDate = DateTime.Now;
                var result = work.Person.Create(person);

                if (result.ResultType.Equals(ResultType.Success))
                {
                    work.Person.CreateChurchPerson(new ChurchPerson
                    {
                        Id = Utilities.GenerateUniqueId(),
                        PersonId = person.Id,
                        ChurchId = SessionVariables.CurrentChurch.Id,
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now
                    });

                    return Json(new { Success = true, Model = result.Data });
                }

                return Json(new { Success = false, Message = $"{result.Message} Error:{result.Exception.Message}" });
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return Json(new { Success = false, Message = $"{Constants.DefaultErrorMessage} {ex.Message}" });
            }
        }

        #region Households
        public ActionResult Households(string filterKeyword)
        {
            var dashboard = new HouseholdDashboard();

            if (!string.IsNullOrEmpty(filterKeyword))
            {
                ViewBag.userFilterKeyword = filterKeyword;
                dashboard.Households = work.Household.GetAll(SessionVariables.CurrentChurch.Id).OrderBy(x => x.Name).Where(x => x.Display.ContainsIgnoreCase(filterKeyword.ToLower().Trim())).ToList();
            }
            else
            {
                dashboard.Households = work.Household.GetAll(SessionVariables.CurrentChurch.Id).OrderBy(x => x.Name).ToList();
            }

            var ids = string.Join(",", dashboard.Households.Select(q => q.Id));
            dashboard.HouseholdMembers = work.Household.MembersByHouseholdWithName(ids);
            var users = work.User.GetAllByPersonIds(dashboard.HouseholdMembers.Select(q => q.PersonId).ToList()).Where(z => z.PersonId.IsNotNullOrEmpty()).ToList();
            dashboard.HouseholdMembers.Select(d => { d.UserId = users.Any(q => q.PersonId.Equals(d.PersonId)) ? users.First(q => q.PersonId.Equals(d.PersonId)).Id : null; return d; }).ToList();

            return View(dashboard);
        }

        [HttpGet]
        public ActionResult _CreateHousehold()
        {
            var model = new Household
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedDate = DateTime.Now,
                CreatedBy = SessionVariables.CurrentUser.User.Id
            };

            return PartialView("_CreateEditHousehold", model);
        }

        [HttpPost]
        public ActionResult _CreateHousehold(Household model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEditHousehold", model);
            }

            work.Household.Create(model);

            return AjaxRedirectTo("/people/householdmembers/" + model.Id);
        }

        [HttpGet]
        public ActionResult _EditHousehold(string id)
        {
            var model = work.Household.Get(id);
            return PartialView("_CreateEditHousehold", model);
        }

        [HttpPost]
        public ActionResult _EditHousehold(Household model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEditHousehold", model);
            }

            model.ModifiedDate = DateTime.Now;
            model.ModifiedBy = SessionVariables.CurrentUser.User.Id;
            work.Household.Update(model);

            return AjaxRedirectTo("/people/households");
        }

        [HttpGet]
        public ActionResult DeleteHousehold(string id)
        {
            work.Household.Delete(id);
            CreateAlertMessage("The household has been deleted.", AlertMessageTypes.Success, AlertMessageIcons.Success);

            return RedirectToAction(nameof(Households));
        }

        #endregion

        #region Household Members
        public ActionResult HouseholdMembers(string id, string returnUrl = "")
        {
            ViewBag.ReturnUrl = returnUrl;
            var dashboard = new HouseholdDashboard();
            dashboard.Households.Add(work.Household.Get(id));
            dashboard.HouseholdMembers = work.Household.MembersByHouseholdWithName(id);
            var users = work.User.GetAllByPersonIds(dashboard.HouseholdMembers.Select(q => q.PersonId).ToList()).Where(z => z.PersonId.IsNotNullOrEmpty()).ToList();
            dashboard.HouseholdMembers.Select(d => { d.UserId = users.Any(q => q.PersonId.Equals(d.PersonId)) ? users.First(q => q.PersonId.Equals(d.PersonId)).Id : null; return d; }).ToList();

            return View(dashboard);
        }

        [HttpGet]
        public ActionResult _CreateHouseholdMember(string householdId)
        {
            var member = new HouseholdMember
            {
                Id = Utilities.GenerateUniqueId(),
                IsActive = true,
                HouseholdId = householdId,
                CreatedDate = DateTime.Now,
                CreatedBy = SessionVariables.CurrentUser.User.Id
            };
            var existingMembers = work.Household.MembersByHousehold(householdId).Select(s => s.PersonId).ToList();
            var model = new HouseholdMemberViewModel()
            {
                Member = member,
                Peoples = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id).Where(x => !existingMembers.Contains(x.Id)).ToList(),
                Mode = PeopleSelectionMode.System
            };
            var head = work.Household.GetHeadOfHousehold(householdId);
            ViewBag.hasHead = head.IsNotNullOrEmpty();

            return PartialView("_CreateEditHouseholdMember", model);
        }

        [HttpPost]
        public ActionResult _CreateHouseholdMember(HouseholdMemberViewModel model)
        {
            if (model.Mode.Equals(PeopleSelectionMode.Manual))
            {
                var person = work.Person.GetByEmailAndPhone(model.People.Email, model.People.PhoneNumber);

                if (person.IsNullOrEmpty())
                {
                    person = new Person()
                    {
                        Id = Utilities.GenerateUniqueId(),
                        CreatedDate = DateTime.Now,
                        FirstName = model.People.FirstName,
                        LastName = model.People.LastName,
                        Email = model.People.Email,
                        PhoneNumber = model.People.PhoneNumber,
                        IsActive = true
                    };
                    work.Person.Create(person);
                    work.Person.CreateChurchPerson(new ChurchPerson
                    {
                        Id = Utilities.GenerateUniqueId(),
                        PersonId = person.Id,
                        ChurchId = SessionVariables.CurrentChurch.Id,
                        CreatedBy = SessionVariables.CurrentUser.User.Id,
                        CreatedDate = DateTime.Now
                    });
                }
                model.Member.PersonId = person.Id;
            }

            if (model.Member.FamilyRole.IsNullOrEmpty())
            {
                model.Member.FamilyRole = FamilyRoles.Unassigned;
            }

            model.Member.IsHeadofHousehold = model.Member.FamilyRole.Equals(FamilyRoles.HeadOfHousehold);

            var result = work.Household.CreateMember(model.Member);

            if (result.ResultType.Equals(ResultType.Success))
            {
                CreateAlertMessage("The member has been added to the household.", AlertMessageTypes.Success, AlertMessageIcons.Success);
            }
            else
            {
                CreateAlertMessage($"{result.Message} Error:{result.Exception.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return RedirectToAction(nameof(HouseholdMembers), new { id = model.Member.HouseholdId });
        }

        [HttpGet]
        public ActionResult _EditHouseholdMember(string id)
        {
            var member = work.Household.GetMember(id);
            var existingMembers = work.Household.MembersByHousehold(member.HouseholdId).Where(q => !q.PersonId.Equals(member.PersonId)).Select(s => s.PersonId).ToList();
            var model = new HouseholdMemberViewModel()
            {
                Member = member,
                Peoples = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id).Where(x => !existingMembers.Contains(x.Id)).ToList()
            };
            var head = work.Household.GetHeadOfHousehold(member.HouseholdId);
            ViewBag.hasHead = head.IsNotNullOrEmpty() && !member.IsHeadofHousehold;

            return PartialView("_CreateEditHouseholdMember", model);
        }

        [HttpPost]
        public ActionResult _EditHouseholdMember(HouseholdMemberViewModel model)
        {
            model.Member.ModifiedDate = DateTime.Now;
            model.Member.ModifiedBy = SessionVariables.CurrentUser.User.Id;

            if (model.Member.FamilyRole.IsNullOrEmpty())
            {
                model.Member.FamilyRole = FamilyRoles.Unassigned;
            }

            model.Member.IsHeadofHousehold = model.Member.FamilyRole.Equals(FamilyRoles.HeadOfHousehold);
            var result = work.Household.UpdateMember(model.Member);

            if (result.ResultType.Equals(ResultType.Success))
            {
                CreateAlertMessage("The household member has been updated.", AlertMessageTypes.Success, AlertMessageIcons.Success);
            }
            else
            {
                CreateAlertMessage($"{result.Message} Error:{result.Exception.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return RedirectToAction(nameof(HouseholdMembers), new { id = model.Member.HouseholdId });
        }

        [HttpGet]
        public ActionResult DeleteHouseholdMember(string id)
        {
            var member = work.Household.GetMember(id);
            work.Household.DeleteMember(id);
            CreateAlertMessage("The household member has been removed.", AlertMessageTypes.Success, AlertMessageIcons.Success);

            return RedirectToAction("householdmembers", new { id = member.HouseholdId });
        }

        public ActionResult GetPeopleAndHouseholds(string param)
        {
            var model = new HouseholdDashboard();

            if (param.IsNotNullOrEmpty())
            {
                model = work.Household.GetPeopleAndHouseholds(param);
            }

            return PartialView("_PeopleAndHouseholds", model);
        }
        #endregion

        #region CommunicationGroups
        public ActionResult CommunicationGroups(string filterKeyword)
        {
            var dashboard = new CommunicationGroupsDashboard();

            if (!string.IsNullOrEmpty(filterKeyword))
            {
                ViewBag.userFilterKeyword = filterKeyword;
                dashboard.CommunicationGroups = work.CommunicationGroup.GetAll(SessionVariables.CurrentChurch.Id).OrderBy(x => x.Name).Where(x => x.Display.ContainsIgnoreCase(filterKeyword.ToLower().Trim())).ToList();
            }
            else
            {
                dashboard.CommunicationGroups = work.CommunicationGroup.GetAll(SessionVariables.CurrentChurch.Id).OrderBy(x => x.Name).ToList();
            }

            var ids = string.Join(",", dashboard.CommunicationGroups.Select(q => q.Id));
            dashboard.CommunicationGroupsPeoples = work.CommunicationGroup.GetMemberByGroupsWithName(ids);

            return View(dashboard);
        }

        [HttpGet]
        public ActionResult _CreateCommunicationGroup()
        {
            var model = new CommunicationGroup
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedDate = DateTime.Now,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                EnableEmail = true,
                EnableText = true,
                EnableSystemNotification = true
            };

            return PartialView("_CreateEditCommunicationGroup", model);
        }

        [HttpPost]
        public ActionResult _CreateCommunicationGroup(CommunicationGroup model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEditCommunicationGroup", model);
            }

            if (model.EnableEmail || model.EnableText || model.EnableSystemNotification)
            {
                work.CommunicationGroup.Create(model);

                return AjaxRedirectTo("/people/CommunicationGroups");
            }

            CreateAlertMessage("Please select at least once contact method.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

            return PartialView("_CreateEditCommunicationGroup", model);
        }

        [HttpGet]
        public ActionResult _EditCommunicationGroup(string id)
        {
            var model = work.CommunicationGroup.Get(id);
            return PartialView("_CreateEditCommunicationGroup", model);
        }

        [HttpPost]
        public ActionResult _EditCommunicationGroup(CommunicationGroup model)
        {
            if (!ModelState.IsValid)
            {
                return PartialView("_CreateEditCommunicationGroup", model);
            }

            if (model.EnableEmail || model.EnableText || model.EnableSystemNotification)
            {
                model.ModifiedDate = DateTime.Now;
                model.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                work.CommunicationGroup.Update(model);

                return AjaxRedirectTo("/people/CommunicationGroups");
            }

            CreateAlertMessage("Please select at least once contact method.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

            return PartialView("_CreateEditCommunicationGroup", model);
        }

        [HttpGet]
        public ActionResult DeleteCommunicationGroup(string id)
        {
            var group = work.CommunicationGroup.Get(id);
            work.CommunicationGroup.Delete(group);
            CreateAlertMessage($"{group.Display} group has been deleted.", AlertMessageTypes.Success, AlertMessageIcons.Success);

            return RedirectToAction(nameof(CommunicationGroups));
        }
        #endregion

        #region CommunicationGroupPeoples
        public ActionResult CommunicationGroupPeoples(string id)
        {
            var dashboard = new CommunicationGroupsDashboard();
            dashboard.CommunicationGroups.Add(work.CommunicationGroup.Get(id));
            dashboard.CommunicationGroupsPeoples = work.CommunicationGroup.GetMemberByGroupsWithName(id);

            var users = work.User.GetAllByPersonIds(dashboard.CommunicationGroupsPeoples.Select(q => q.PersonId).ToList()).Where(z => z.PersonId.IsNotNullOrEmpty()).ToList();

            dashboard.CommunicationGroupsPeoples.Select(d =>
            {
                d.UserId = users.Any(q => q.PersonId.Equals(d.PersonId)) ? users.First(q => q.PersonId.Equals(d.PersonId)).Id : null;
                d.Email = d.Email.IsNullOrEmpty() ? users.Any(q => q.PersonId.Equals(d.PersonId)) ? users.First(q => q.PersonId.Equals(d.PersonId)).Email : null : d.Email;
                d.PhoneNumber = d.PhoneNumber.IsNullOrEmpty() ? users.Any(q => q.PersonId.Equals(d.PersonId)) ? users.First(q => q.PersonId.Equals(d.PersonId)).PhoneNumber : null : d.PhoneNumber;
                return d;
            }).ToList();

            return View(dashboard);
        }

        [HttpGet]
        public ActionResult _CreateCommunicationGroupPeople(string groupId)
        {
            var existingMembers = work.CommunicationGroup.GetMemberByGroupIds(groupId).Select(s => s.PersonId).ToList();
            var model = new CommunicationGroupsPeopleModel()
            {
                GroupId = groupId,
                Peoples = work.Person.GetAllByPersonIds(SessionVariables.CurrentChurch.Id).Where(x => !existingMembers.Contains(x.Id)).ToList()
            };

            return PartialView("_CreateEditCommunicationGroupPeople", model);
        }

        [HttpPost]
        public ActionResult _CreateCommunicationGroupPeople(CommunicationGroupsPeopleModel model)
        {
            var members = model.Members.Select(q => new CommunicationGroupsPeople
            {
                Id = Utilities.GenerateUniqueId(),
                PersonId = q,
                IsActive = true,
                CommunicationGroupId = model.GroupId,
                CreatedDate = DateTime.Now,
                CreatedBy = SessionVariables.CurrentUser.User.Id
            }).ToList();
            var result = work.CommunicationGroup.CreateGroupMember(members);

            if (result.ResultType.Equals(ResultType.Success))
            {
                CreateAlertMessage($"The {(model.Members.Count() > 1 ? "members have" : "member has")} been added to the group.", AlertMessageTypes.Success, AlertMessageIcons.Success);
            }
            else
            {
                CreateAlertMessage($"{result.Message} Error:{result.Exception.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
            }

            return RedirectToAction(nameof(CommunicationGroupPeoples), new { id = model.GroupId });
        }

        //[HttpGet]
        //public ActionResult _EditCommunicationGroupPeople(string id)
        //{
        //    var member = work.CommunicationGroup.GetGroupMember(id);
        //    var existingMembers = work.CommunicationGroup.GetMemberByGroupIds(member.CommunicationGroupId).Where(q => !q.PersonId.Equals(member.PersonId)).Select(s => s.PersonId).ToList();
        //    var model = new CommunicationGroupsPeopleModel()
        //    {
        //        Member = member,
        //        Person = work.Person.GetAll(SessionVariables.CurrentChurch.Id).Where(x => !existingMembers.Contains(x.Id)).ToList()
        //    };

        //    return PartialView("_CreateEditCommunicationGroupPeople", model);
        //}

        //[HttpPost]
        //public ActionResult _EditCommunicationGroupPeople(CommunicationGroupsPeopleModel model)
        //{
        //    model.Member.ModifiedDate = DateTime.Now;
        //    model.Member.ModifiedBy = SessionVariables.CurrentUser.User.Id;
        //    var result = work.CommunicationGroup.UpdateGroupMember(model.Member);

        //    if (result.ResultType.Equals(ResultType.Success))
        //    {
        //        CreateAlertMessage($"The group member has been updated.", AlertMessageTypes.Success, AlertMessageIcons.Success);
        //    }
        //    else
        //    {
        //        CreateAlertMessage($"{result.Message} Error:{result.Exception.Message}", AlertMessageTypes.Failure, AlertMessageIcons.Failure);
        //    }

        //    return RedirectToAction(nameof(PeopleController.CommunicationGroupPeoples), new { id = model.Member.CommunicationGroupId });
        //}

        [HttpGet]
        public ActionResult DeleteCommunicationGroupPeople(string id)
        {
            var member = work.CommunicationGroup.GetGroupMemberWithName(id);
            work.CommunicationGroup.DeleteGroupMember(id);
            CreateAlertMessage($"{member.MemberName} has been removed from the group.", AlertMessageTypes.Success, AlertMessageIcons.Success);

            return RedirectToAction(nameof(CommunicationGroupPeoples), new { id = member.CommunicationGroupId });
        }

        [HttpPost]
        [AllowAnonymous]
        [OverrideActionFilters]
        public ActionResult Override(string id, string key, bool value)
        {
            return Json(work.CommunicationGroup.Override(id, key, value));
        }

        [HttpGet]
        public ActionResult CommunicateWithGroup()
        {
            var model = new CommunicateWithGroupModel
            {
                CommunicationGroup = work.CommunicationGroup.GetAll(SessionVariables.CurrentChurch.Id, includePeople: true)
            };

            return PartialView("_Communication", model);
        }

        [HttpPost]
        public ActionResult CommunicateWithGroup(CommunicateWithGroupModel model)
        {
            var result = work.CommunicationGroup.Communicate(model);

            return result.Success ? Json(new { Success = true }) : Json(new { Success = false, result.Message });
        }
        #endregion        
    }
}