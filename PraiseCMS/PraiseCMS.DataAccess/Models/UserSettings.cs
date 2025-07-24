using PraiseCMS.DataAccess.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace PraiseCMS.DataAccess.Models
{
    [Table("UserSettings")]
    public class UserSetting : BaseModel
    {
        [DisplayName("Id")]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public string Id { get; set; }

        [DisplayName("User")]
        public string UserId { get; set; }
        public string ProfileImage { get; set; }

        [DisplayName("Primary Church")]
        public string PrimaryChurchId { get; set; }

        [DisplayName("Primary Church Location")]
        public string PrimaryChurchCampusId { get; set; }

        [DisplayName("Dashboard Template Id")]
        public string DashboardTemplateId { get; set; }

        [DisplayName("Dark Mode Enabled")]
        public bool DarkModeEnabled { get; set; }

        [DisplayName("Paperless Giving")]
        public bool PaperlessGiving { get; set; }

        [DisplayName("Sidebar Collapsed")]
        public bool SidebarCollapsed { get; set; }

        [DisplayName("Full Width View")]
        public bool FullWidthView { get; set; } = true;
    }
}