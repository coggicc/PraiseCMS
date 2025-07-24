using Newtonsoft.Json.Linq;
using PraiseCMS.DataAccess.Models.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Logs")]
    public class Log
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("Church")]
        public string ChurchId { get; set; }

        [DisplayName("Controller")]
        public string Controller { get; set; }

        [DisplayName("Action")]
        public string Action { get; set; }

        [DisplayName("Status")]
        public string Status { get; set; }

        [DisplayName("Parameter")]
        public string Parameter { get; set; }

        [DisplayName("Type")]
        public string Type { get; set; }

        [DisplayName("Type Id")]
        public string TypeId { get; set; }

        [DisplayName("Auto Save")]
        public bool Autosave { get; set; }

        [DisplayName("UserId")]
        public string UserId { get; set; }

        [DisplayName("Created Date")]
        public DateTime CreatedDate { get; set; }

        [DisplayName("Created By")]
        public string CreatedBy { get; set; }

        [DisplayName("ClassName")]
        public string ClassName { get; set; }

        [DisplayName("MethodName")]
        public string MethodName { get; set; }
    }

    public class LogView
    {
        public LogView()
        {
            Logs = new List<LogsViewModel>();
            Users = new List<ApplicationUser>();
        }

        public Log Log { get; set; }
        public ApplicationUser User { get; set; }
        public List<LogsViewModel> Logs { get; set; }
        public List<ApplicationUser> Users { get; set; }
    }

    public class LogViewModel : Pagination
    {
        public LogViewModel()
        {
            Log = new Log();
            Person = new ApplicationUser();
        }

        public IEnumerable<KeyValuePair<string, JValue>> LogFields { get; set; }
        public Log Log { get; set; }
        public ApplicationUser Person { get; set; }
        public string DataType { get; set; }
        public int TotalLogs { get; set; }
    }
    public class LogListViewModel : Pagination
    {
        public LogListViewModel()
        {
            Logs = new List<Log>();
            ControllerList = new List<SelectListItems>();
            ChurchList = new List<SelectListItems>();
            TypeList = new List<SelectListItems>();
        }

        public List<Log> Logs { get; set; }
        public int TotalLogs { get; set; }
        public List<SelectListItems> ControllerList { get; set; }
        public List<SelectListItems> ChurchList { get; set; }
        public List<SelectListItems> TypeList { get; set; }
        public string Controller { get; set; }
        public string Church { get; set; }
        public string Type { get; set; }
    }
}