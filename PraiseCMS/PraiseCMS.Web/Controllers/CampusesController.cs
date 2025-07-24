using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Models;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Attributes;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    [RequireUser]
    
    [RequirePermission(ModuleId = "9320982602755ea0f9a4ba47d7a34e")]
    public class CampusesController : BaseController
    {
        public ActionResult Index()
        {
            var dateRange = new DateRange
            {
                StartDate = DateTime.Parse($"01/01/{DateTime.Now.Year}"),
                EndDate = DateTime.Parse($"12/31/{DateTime.Now.Year}")
            };

            var model = work.Campus.GetCampusesView(SessionVariables.CurrentChurch.Id, dateRange);

            return View(model);
        }

        public ActionResult Dashboard(string id)
        {
            var model = work.Campus.GetDashboardById(SessionVariables.CurrentChurch.Id, id);
            return View(model);
        }

        #region CRUD
        [HttpGet]
        public ActionResult _CreateCampus()
        {
            var campus = new Campus()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };

            return PartialView("_CreateEdit", campus);
        }

        [HttpPost]
        public ActionResult _CreateCampus(Campus campus)
        {
            if (ModelState.IsValid)
            {
                work.Campus.Create(campus);
                //add new campus in session
                SessionVariables.Campuses.Add(campus);
                CreateAlertMessage("The new campus has been created.", AlertMessageTypes.Success, AlertMessageIcons.Success);

                return AjaxRedirectTo("/campuses");
            }

            return PartialView("_CreateEdit", campus);
        }

        [HttpGet]
        public ActionResult _EditCampus(string id)
        {
            var campus = work.Campus.Get(id);
            return PartialView("_CreateEdit", campus);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditCampus(Campus campus)
        {
            if (ModelState.IsValid)
            {
                work.Campus.Update(SessionVariables.CurrentUser.User.Id, campus);

                if (SessionVariables.Campuses.Any(q => q.Id == campus.Id))
                {
                    //add new campus in session
                    SessionVariables.Campuses.Remove(SessionVariables.Campuses.FirstOrDefault(q => q.Id == campus.Id));
                    SessionVariables.Campuses.Add(campus);

                    if (SessionVariables.CurrentCampus.IsNotNullOrEmpty() && SessionVariables.CurrentCampus.Id == campus.Id)
                    {
                        SessionVariables.CurrentCampus = campus;
                    }
                }

                CreateAlertMessage(Constants.SavedMessage, AlertMessageTypes.Success, AlertMessageIcons.Success);

                return AjaxRedirectTo("/campuses");
            }

            return PartialView("_CreateEdit", campus);
        }

        [HttpGet]
        public ActionResult Delete(string id)
        {
            work.Campus.Delete(id);
            return RedirectToAction("index");
        }
        #endregion

        [RequireUser]
        public void UpdateCurrentCampus(string id)
        {
            SessionVariables.CurrentCampus = SessionVariables.Campuses.FirstOrDefault(x => x.Id == id);
            var uSettings = work.UserSetting.GetByUserId(SessionVariables.CurrentUser.User.Id);

            if (uSettings == null)
            {
                work.Campus.CreateSettingsByCampusId(SessionVariables.CurrentChurch.Id, SessionVariables.CurrentUser.User.Id, id);
            }
            else
            {
                uSettings.PrimaryChurchCampusId = id;
                uSettings.ModifiedDate = DateTime.Now;
                uSettings.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                work.UserSetting.Update(uSettings);
            }
        }

        public ActionResult Giving(string campusId)
        {
            var model = work.Campus.GetGivingDashboard(campusId, SessionVariables.CurrentChurch.Id);
            return View(model);
        }

        #region Building
        public ActionResult Buildings()
        {
            return View(work.Building.GetAll(SessionVariables.CurrentChurch.Id));
        }

        public ActionResult BuildingDetails(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var building = work.Building.Get(id);
            if (building == null)
            {
                return HttpNotFound();
            }

            return View(building);
        }

        [HttpGet]
        public ActionResult _CreateBuilding()
        {
            var model = new BuildingViewModel
            {
                //var building = new Building()
                //{
                //    Id = Utilities.GenerateUniqueId(),
                //    ChurchId = SessionVariables.CurrentChurch.Id,
                //    CreatedBy = SessionVariables.CurrentUser.User.Id,
                //    CreatedDate = DateTime.Now
                //};

                BuildingId = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id
            };

            return PartialView("_CreateEditBuilding", model);
        }

        [HttpPost]
        public ActionResult _CreateBuilding(BuildingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var building = new Building
                {
                    Id = model.BuildingId,
                    ChurchId = model.ChurchId,
                    CampusId = model.CampusId,
                    BuildingName = model.BuildingName
                    // Assign other properties as needed
                };

                work.Building.CreateBuilding(building);
                return AjaxRedirectTo("/campuses/buildings");
            }

            return PartialView("_CreateEditBuilding", model);
        }

        [HttpGet]
        public ActionResult _EditBuilding(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Retrieve the building by ID
            var building = work.Building.Get(id);
            if (building == null)
            {
                return HttpNotFound();
            }

            // Map the building entity to the view model for editing
            var model = new BuildingViewModel
            {
                BuildingId = building.Id,
                ChurchId = building.ChurchId,
                CampusId = building.CampusId,
                BuildingName = building.BuildingName
                // Map other properties as needed
            };

            return PartialView("_CreateEditBuilding", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditBuilding(BuildingViewModel model)
        {
            if (ModelState.IsValid)
            {
                var building = work.Building.Get(model.BuildingId);
                if (building == null)
                {
                    return HttpNotFound();
                }

                building.BuildingName = model.BuildingName;
                building.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                building.ModifiedDate = DateTime.Now;

                work.Building.UpdateBuilding(building);
                return AjaxRedirectTo("/campuses/buildings");
            }

            return PartialView("_CreateEditBuilding", model);
        }

        [HttpGet]
        public ActionResult DeleteBuilding(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var building = work.Building.Get(id);

            if (building == null)
            {
                return HttpNotFound();
            }

            work.Building.DeleteBuilding(building);

            return RedirectToAction("Buildings");
        }
        #endregion

        #region Floors
        public ActionResult Floors()
        {
            return View(work.Floor.GetAll(SessionVariables.CurrentChurch.Id, null, true));
        }

        public ActionResult FloorDetails(string floorId)
        {
            if (floorId == null)
            {
                // If no building ID is provided, return a bad request or redirect to an error page
                // For example:
                // return RedirectToAction("Error", "Home");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = work.Floor.Get(floorId);
            return View(model);
        }

        [HttpGet]
        public ActionResult _CreateFloor()
        {
            var building = new Floor()
            {
                Id = Utilities.GenerateUniqueId(),
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };
            ViewData["Buildings"] = work.Building.GetAll(building.ChurchId);

            return PartialView("_CreateEditFloor", building);
        }

        [HttpPost]
        public ActionResult _CreateFloor(Floor floor)
        {
            if (ModelState.IsValid)
            {
                work.Floor.CreateFloor(floor);
                return AjaxRedirectTo("/campuses/floors");
            }

            ViewData["Buildings"] = work.Building.GetAll(floor.ChurchId);

            return PartialView("_CreateEditFloor", floor);
        }

        [HttpGet]
        public ActionResult _EditFloor(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var floor = work.Floor.Get(id);

            if (floor == null)
            {
                return HttpNotFound();
            }

            ViewData["Buildings"] = work.Building.GetAll(floor.ChurchId);

            return PartialView("_CreateEditFloor", floor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditFloor(Floor floor)
        {
            if (ModelState.IsValid)
            {
                work.Floor.UpdateFloor(floor);
                return AjaxRedirectTo("/campuses/floors");
            }

            ViewData["Buildings"] = work.Building.GetAll(floor.ChurchId);

            return PartialView("_CreateEditFloor", floor);
        }

        [HttpGet]
        public ActionResult DeleteFloor(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var floor = work.Floor.Get(id);

            if (floor == null)
            {
                return HttpNotFound();
            }

            work.Floor.DeleteFloor(floor);
            return RedirectToAction("Floors");
        }
        #endregion

        #region Rooms
        public ActionResult Rooms()
        {
            ViewData["Buildings"] = work.Building.GetAll(SessionVariables.CurrentChurch.Id);
            ViewData["Floors"] = work.Floor.GetAll(SessionVariables.CurrentChurch.Id);
            return View(work.Room.GetAll(SessionVariables.CurrentChurch.Id));
        }

        public ActionResult RoomDetails(string roomId)
        {
            if (roomId == null)
            {
                // If no building ID is provided, return a bad request or redirect to an error page
                // For example:
                // return RedirectToAction("Error", "Home");
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var model = work.Room.Get(roomId);
            return View(model);
        }

        public ActionResult GetRooms(string campusId, string buildingId, string floorId)
        {
            return PartialView("_RoomsList", work.Room.GetAll(SessionVariables.CurrentChurch.Id, campusId, buildingId, floorId));
        }

        [HttpGet]
        public ActionResult _CreateRoom()
        {
            var room = new Room()
            {
                Id = Utilities.GenerateUniqueId(),
                Status = true,
                ChurchId = SessionVariables.CurrentChurch.Id,
                CreatedBy = SessionVariables.CurrentUser.User.Id,
                CreatedDate = DateTime.Now
            };
            ViewData["Buildings"] = work.Building.GetAll(room.ChurchId);
            ViewData["Floors"] = work.Floor.GetAll(room.ChurchId);

            return PartialView("_CreateEditRoom", room);
        }

        [HttpPost]
        public ActionResult _CreateRoom(Room room)
        {
            if (ModelState.IsValid)
            {
                work.Room.Create(room);
                return AjaxRedirectTo("/campuses/rooms");
            }

            ViewData["Buildings"] = work.Building.GetAll(room.ChurchId);
            ViewData["Floors"] = work.Floor.GetAll(room.ChurchId);

            return PartialView("_CreateEditRoom", room);
        }

        [HttpGet]
        public ActionResult _EditRoom(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var room = work.Room.Get(id);

            if (room == null)
            {
                return HttpNotFound();
            }

            ViewData["Buildings"] = work.Building.GetAll(room.ChurchId);
            ViewData["Floors"] = work.Floor.GetAll(room.ChurchId);

            return PartialView("_CreateEditRoom", room);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult _EditRoom(Room room)
        {
            if (ModelState.IsValid)
            {
                room.ModifiedDate = DateTime.Now;
                room.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                work.Room.Update(room);

                return AjaxRedirectTo("/campuses/rooms");
            }

            ViewData["Buildings"] = work.Building.GetAll(room.ChurchId);
            ViewData["Floors"] = work.Floor.GetAll(room.ChurchId);

            return PartialView("_CreateEditRoom", room);
        }

        [HttpGet]
        public ActionResult DeleteRoom(string id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var room = work.Room.Get(id);

            if (room == null)
            {
                return HttpNotFound();
            }

            work.Room.Delete(room);

            return RedirectToAction("Rooms");
        }
        #endregion
    }
}