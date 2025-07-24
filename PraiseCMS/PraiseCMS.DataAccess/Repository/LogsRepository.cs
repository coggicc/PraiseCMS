using Newtonsoft.Json;
using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Interfaces;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.DataAccess.Shared;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

namespace PraiseCMS.DataAccess.Repository
{
    public class LogsRepository : ILogRepository
    {
        private ApplicationDbContext _dbContext;

        public LogsRepository()
        {
        }

        public List<LogsViewModel> GetAllLogs()
        {
            return (from log in _dbContext.Logs
                    join church in _dbContext.Churches on log.ChurchId equals church.Id into churchGroup
                    from church in churchGroup.DefaultIfEmpty()
                    select new LogsViewModel
                    {
                        ID = log.Id,
                        Status = log.Status,
                        CreatedDate = log.CreatedDate
                    }).ToList();
        }

        public Log GetLogByID(string id)
        {
            if (_dbContext == null)
            {
                _dbContext = new ApplicationDbContext();
            }

            return _dbContext.Logs.FirstOrDefault(x => x.Id == id);
        }

        public void LogData(string actionName, string controller, string type, string typeID, string status, string parameters, bool hasSession = true)
        {
            try
            {
                var loggingEnabled = bool.Parse(ConfigurationManager.AppSettings["IsLoggingEnabled"]);

                if (loggingEnabled)
                {
                    if (_dbContext == null)
                    {
                        _dbContext = new ApplicationDbContext();
                    }

                    var stackTrace = new System.Diagnostics.StackTrace();
                    var frame = stackTrace.GetFrame(1);
                    var className = frame?.GetMethod()?.DeclaringType?.Name ?? string.Empty;
                    var methodName = frame?.GetMethod()?.Name ?? string.Empty;

                    var log = new Log
                    {
                        Id = Utilities.GenerateUniqueId(),
                        ChurchId = hasSession ? (SessionVariables.CurrentChurch?.Id) : null,
                        ClassName = className.SubstringIt(100),
                        MethodName = methodName.SubstringIt(100),
                        Controller = controller.SubstringIt(50),
                        Action = actionName.SubstringIt(150),
                        Status = status,
                        Parameter = parameters,
                        Type = type.SubstringIt(50),
                        TypeId = !string.IsNullOrEmpty(typeID) ? typeID : null,
                        Autosave = false,
                        UserId = hasSession ? (SessionVariables.CurrentUser?.User.Id) : null,
                        CreatedDate = DateTime.Now,
                        CreatedBy = hasSession ? (SessionVariables.CurrentUser?.User.Id) : Constants.System
                    };

                    _dbContext.Logs.Add(log);
                    _dbContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
            }
        }

        public string JsonConverter(params dynamic[] Obj)
        {
            var dictionary = new Dictionary<string, string>();

            try
            {
                for (var i = 0; i < Obj.Length; i += 2)
                {
                    string param = Obj[i]?.ToString() ?? string.Empty;
                    string value = Obj[i + 1]?.ToString() ?? string.Empty;
                    dictionary.Add(param, value);
                }

                return JsonConvert.SerializeObject(dictionary);
            }
            catch (Exception ex)
            {
                ExceptionLogger.LogException(ex);
                return null;
            }
        }
    }
}