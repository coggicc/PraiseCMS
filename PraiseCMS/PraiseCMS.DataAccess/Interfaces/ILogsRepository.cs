using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Interfaces
{
    public interface ILogRepository
    {
        List<LogsViewModel> GetAllLogs();
        Log GetLogByID(string id);
        string JsonConverter(params dynamic[] Obj);
        void LogData(string actionName, string controller, string type, string typeID, string status, string parameters, bool hasSession = true);
    }
}