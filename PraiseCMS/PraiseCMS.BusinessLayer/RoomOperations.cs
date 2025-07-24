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
    public class RoomOperations : GenericRepository
    {
        public RoomOperations(ApplicationDbContext db, Work work)
            : base(db, work)
        {
        }

        public Room Get(string id)
        {
            return Read<Room>().FirstOrDefault(x => x.Id == id);
        }

        public List<Room> GetAll(string churchId, string campusId = null, string buildingId = null, string floorId = null)
        {
            var iQrooms = Read<Room>().Where(x => x.ChurchId.Equals(churchId));

            if (campusId.IsNotNullOrEmpty())
            {
                iQrooms = iQrooms.Where(x => x.CampusId.Equals(campusId));
            }

            if (buildingId.IsNotNullOrEmpty())
            {
                iQrooms = iQrooms.Where(x => x.BuildingId.Equals(buildingId));
            }

            if (floorId.IsNotNullOrEmpty())
            {
                iQrooms = iQrooms.Where(x => x.FloorId.Equals(floorId));
            }

            var rooms = iQrooms.OrderBy(q => q.Name).ToList();
            var buildings = Work.Building.GetAll(churchId, rooms.Select(x => x.BuildingId));
            var floors = Work.Floor.GetAll(churchId, rooms.Select(x => x.FloorId));
            rooms.Select(x => { x.Building = buildings.Find(a => a.Id.Equals(x.BuildingId)); x.Floor = floors.Find(a => a.Id.Equals(x.FloorId)); return x; }).ToList();

            return rooms;
        }

        #region CRUD
        public Result<Room> Create(Room entity)
        {
            try
            {
                Create<Room>(entity);
                SaveChanges();
                return new Result<Room>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Room>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.CreateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Room> Update(Room entity)
        {
            try
            {
                entity.ModifiedDate = DateTime.Now;
                entity.ModifiedBy = SessionVariables.CurrentUser.User.Id;

                Update<Room>(entity);
                SaveChanges();
                return new Result<Room>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Room>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.UpdateExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Room> Delete(Room entity)
        {
            try
            {
                Delete<Room>(entity);
                SaveChanges();
                return new Result<Room>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Room>
                {
                    Data = entity,
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }

        public Result<Room> Delete(string id)
        {
            try
            {
                var entity = Get(id);
                Delete<Room>(entity);
                SaveChanges();
                return new Result<Room>
                {
                    Data = entity,
                    ResultType = ResultType.Success
                };
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return new Result<Room>
                {
                    Exception = ex,
                    Message = Constants.DeleteExceptionMessage,
                    ResultType = ResultType.Exception
                };
            }
        }
        #endregion
    }
}