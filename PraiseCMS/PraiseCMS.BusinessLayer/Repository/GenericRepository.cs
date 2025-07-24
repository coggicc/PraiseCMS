using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Repository;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;

namespace PraiseCMS.BusinessLayer.Repository
{
    public class GenericRepository : IGenericRepository
    {
        public AdoDataAccess DAL;
        public Work Work { get; set; }
        protected ApplicationDbContext Db { get; set; }
        protected LogsRepository logsRepository;

        public GenericRepository(ApplicationDbContext dbContext, Work work)
        {
            Db = dbContext;
            Work = work;
            DAL = new AdoDataAccess();
            logsRepository = new LogsRepository();
        }

        public void Dispose()
        {
            Db.Dispose();
        }

        #region CRUD
        public IQueryable<T> Read<T>() where T : class
        {
            return Db.Set<T>().AsQueryable();
        }

        public int RunSQlCommand(string command)
        {
            return Db.Database.ExecuteSqlCommand(command);
        }

        public void Create<T>(T entity) where T : class
        {
            try
            {
                Db.Set<T>().Add(entity);
                LogData(entity, "Create");
            }
            catch (Exception ex)
            {
                LogDataException(entity, "Create", ex.StackTrace);
            }
        }

        public void Create<T>(IEnumerable<T> entities) where T : class
        {
            foreach (var entity in entities)
            {
                try
                {
                    Db.Set<T>().Add(entity);
                    LogData(entity, "Create");
                }
                catch (Exception ex)
                {
                    LogDataException(entity, "Create", ex.StackTrace);
                }
            }
        }

        public void Update<T>(T entity) where T : class
        {
            try
            {
                var entry = Db.Entry(entity);
                if (entry.State == EntityState.Detached)
                {
                    var entityType = typeof(T);
                    var entityProperties = entityType.GetProperties();
                    var primaryKeyProperty = entityProperties.FirstOrDefault(p => p.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));

                    if (primaryKeyProperty != null)
                    {
                        var primaryKeyValue = primaryKeyProperty.GetValue(entity);

                        var existingEntity = Db.Set<T>().Find(primaryKeyValue);

                        if (existingEntity != null)
                        {
                            Db.Entry(existingEntity).CurrentValues.SetValues(entity);
                        }
                        else
                        {
                            Db.Set<T>().Attach(entity);
                            entry.State = EntityState.Modified;
                        }
                    }
                }
                LogData(entity, "Edit");
            }
            catch (Exception ex)
            {
                LogDataException(entity, "Edit", ex.StackTrace);
            }
        }

        public void Update<T>(IEnumerable<T> entities) where T : class
        {
            foreach (var entity in entities)
            {
                try
                {
                    var entry = Db.Entry(entity);
                    Db.Set<T>().Attach(entity);
                    entry.State = EntityState.Modified;
                    LogData(entity, "Edit");
                }
                catch (Exception ex)
                {
                    LogDataException(entity, "Edit", ex.StackTrace);
                }
            }
        }

        public void Delete<T>(IEnumerable<T> entities) where T : class
        {
            foreach (var entity in entities)
            {
                try
                {
                    Db.Set<T>().Remove(entity);
                    LogData(entity, "Delete");
                }
                catch (Exception ex)
                {
                    LogDataException(entity, "Delete", ex.StackTrace);
                }
            }
        }

        public void Delete<T>(T entity) where T : class
        {
            try
            {
                Db.Set<T>().Remove(entity);
                LogData(entity, "Delete");
            }
            catch (Exception ex)
            {
                LogDataException(entity, "Delete", ex.StackTrace);
            }
        }

        public void SaveChanges()
        {
            try
            {
                Db.SaveChanges();
            }
            catch (DbUpdateException ex)
            {
                ExceptionLogger.LogDbUpdateException(ex);
                Db.Database.Log = Console.Write;
                // Handle DbUpdateException (base class for DbEntityValidationException, DbUpdateConcurrencyException, etc.)
                System.Diagnostics.Debug.WriteLine($"DbUpdateException: {ex.Message}");

                LogInnerExceptionDetails(ex.InnerException);
            }
            catch (SqlException ex)
            {
                ExceptionLogger.LogSqlException(ex);
                // Handle SqlException specifically
                System.Diagnostics.Debug.WriteLine($"SqlException: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"Error Number: {ex.Number}");
                System.Diagnostics.Debug.WriteLine($"Error State: {ex.State}");

                // Log affected columns, properties, etc. if available
                LogAffectedDetails(ex);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                // Handle other exceptions
                System.Diagnostics.Debug.WriteLine($"General Exception: {ex.Message}");
                // Additional handling based on other types of exceptions
            }
        }
        #endregion

        #region Logging
        private void LogInnerExceptionDetails(Exception ex)
        {
            if (ex != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner Exception: {ex.Message}");

                // Log affected columns, properties, etc. if available
                if (ex is SqlException sqlException)
                {
                    LogAffectedDetails(sqlException);
                }

                LogInnerExceptionDetails(ex.InnerException); // Recursive call
            }
        }

        private void LogAffectedDetails(SqlException ex)
        {
            // Log error message, error number, and SQL error state
            System.Diagnostics.Debug.WriteLine($"SqlException: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Error Number: {ex.Number}");
            System.Diagnostics.Debug.WriteLine($"Error State: {ex.State}");

            // Check if the error message contains information about the truncated column
            if (ex.Message.Contains("truncated") || ex.Message.Contains("terminated"))
            {
                var match = Regex.Match(ex.Message, "column '([^']*)'");
                if (match.Success)
                {
                    string affectedColumn = match.Groups[1].Value;
                    System.Diagnostics.Debug.WriteLine($"Affected Column: {affectedColumn}");
                }
            }

            // Log additional error details from SqlError collection
            foreach (SqlError error in ex.Errors)
            {
                System.Diagnostics.Debug.WriteLine($"Error Message: {error.Message}");
                System.Diagnostics.Debug.WriteLine($"Error Procedure: {error.Procedure}");
                System.Diagnostics.Debug.WriteLine($"Error Line Number: {error.LineNumber}");
            }
        }

        private void LogData<T>(T entity, string ObjectType)
        {
            var typeId = "-";
            try
            {
                var objEntity = (dynamic)entity;
                typeId = objEntity.Id;
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }

            try
            {
                var route = RoutData();
                var type = entity.GetType();
                var logsObj = entity.ConvertToJson();
                logsRepository.LogData(route.CurrentAction, route.CurrentController, $"{ObjectType} {type.Name}", typeId, LogStatuses.Done, logsObj);
            }
            catch (Exception exc)
            {
                ExceptionLogger.LogException(exc);
            }
        }

        private void LogDataException<T>(T entity, string state, string exception)
        {
            var typeId = "-";
            try
            {
                var objEntity = (dynamic)entity;
                typeId = objEntity.Id;
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }
            finally
            {
                var route = RoutData();
                logsRepository.LogData(route.CurrentAction, route.CurrentController, state, typeId, LogStatuses.Exception, exception);
            }
        }
        #endregion

        #region Route Helper
        public RouteData RoutData()
        {
            var httpContext = new HttpContextWrapper(HttpContext.Current);
            var routeData = System.Web.Routing.RouteTable.Routes.GetRouteData(httpContext);

            return new RouteData
            {
                CurrentAction = routeData.Values["action"].ToString(),
                CurrentController = routeData.Values["controller"].ToString()
            };
        }

        public class RouteData
        {
            public string CurrentController { get; set; }
            public string CurrentAction { get; set; }
        }
        #endregion
    }
}