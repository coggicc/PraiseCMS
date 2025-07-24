using PraiseCMS.DataAccess.Models.Base;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace PraiseCMS.DataAccess.Models
{
    [Table("Domains")]
    public class Domain : BaseModel
    {
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        [DisplayName("ID")]
        public string Id { get; set; }

        [StringLength(250)]
        [DisplayName("Name")]
        public string Name { get; set; }

        [StringLength(250)]
        [DisplayName("Base URL")]
        public string BaseUrl { get; set; }

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [DisplayName("Number")]
        public int Number { get; set; }

        [DisplayName("Active")]
        public bool Active { get; set; }

        [DisplayName("Navbar Logo")]
        public string NavbarLogo { get; set; }

        [DisplayName("Auth Logo")]
        public string AuthLogo { get; set; }

        [DisplayName("Favicon")]
        public string Favicon { get; set; }

        [AllowHtml]
        [DisplayName("Custom CSS")]
        public string CustomCss { get; set; }

        [DisplayName("Style Sheet Number")]
        public int StyleSheetNumber { get; set; }

        [DisplayName("Google Analytics Number")]
        public string GoogleAnalyticsNumber { get; set; }

        [StringLength(50)]
        [DisplayName("Site Title")]
        public string SiteTitle { get; set; }

        [StringLength(50)]
        [DisplayName("Email Display")]
        public string EmailDisplay { get; set; }

        [StringLength(50)]
        [DisplayName("Email Reply Address")]
        public string EmailReplyAddress { get; set; }

        [DisplayName("Email Logo")]
        public string EmailLogo { get; set; }

        [DisplayName("Native Auth Logo")]
        public string NativeAuthLogo { get; set; }

        public string Display => "Domain #" + Number;
    }
}