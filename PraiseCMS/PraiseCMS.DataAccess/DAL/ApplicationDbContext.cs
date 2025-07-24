using Microsoft.AspNet.Identity.EntityFramework;
using PraiseCMS.DataAccess.Models;
using System.Data.Entity;

namespace PraiseCMS.DataAccess.DAL
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext()
            : base("DefaultConnection", throwIfV1Schema: false)
        {
        }

        public static ApplicationDbContext Create()
        {
            return new ApplicationDbContext();
        }

        public DbSet<AttachmentSD> Attachments { get; set; }
        public DbSet<Attendance> Attendance { get; set; }
        public DbSet<Baptism> Baptisms { get; set; }
        public DbSet<BlogPost> BlogPosts { get; set; }
        public DbSet<BlogCategory> BlogCategories { get; set; }
        public DbSet<BlogTag> BlogTags { get; set; }
        public DbSet<BlogPostTag> BlogPostTags { get; set; }
        public DbSet<Building> Buildings { get; set; }
        public DbSet<Campus> Campuses { get; set; }
        public DbSet<CheckIn> CheckIns { get; set; }
        public DbSet<Church> Churches { get; set; }
        public DbSet<ChurchEvent> ChurchEvents { get; set; }
        public DbSet<ChurchEventScheduler> ChurchEventScheduler { get; set; }
        public DbSet<ChurchEventTime> ChurchEventTime { get; set; }
        public DbSet<ChurchEventType> ChurchEventTypes { get; set; }
        public DbSet<ChurchMerchantAccount> ChurchMerchantAccounts { get; set; }
        public DbSet<ChurchUser> ChurchUsers { get; set; }
        public DbSet<CommunicationGroup> CommunicationGroup { get; set; }
        public DbSet<CommunicationGroupsPeople> CommunicationGroupsPeople { get; set; }
        public DbSet<CommunicationHistory> CommunicationHistory { get; set; }
        public DbSet<CustomizedDashboard> CustomizedDashboard { get; set; }
        public DbSet<DashboardTemplate> DashboardTemplate { get; set; }
        public DbSet<DashboardTemplatePermission> DashboardTemplatePermission { get; set; }
        public DbSet<DashboardWidget> DashboardWidget { get; set; }
        public DbSet<DashboardWidgetSortOrder> DashboardWidgetSortOrder { get; set; }
        public DbSet<Denomination> Denominations { get; set; }
        public DbSet<Domain> Domains { get; set; }
        public DbSet<Email> Emails { get; set; }
        public DbSet<EmailCampaign> EmailCampaigns { get; set; }
        public DbSet<EmailTemplate> EmailTemplates { get; set; }
        public DbSet<Equipment> Equipment { get; set; }
        public DbSet<EquipmentCategory> EquipmentCategories { get; set; }
        public DbSet<EventAttendance> EventAttendance { get; set; }
        public DbSet<EventSD> Event { get; set; }
        public DbSet<FavoriteReport> FavoriteReports { get; set; }
        public DbSet<FixedReport> FixedReports { get; set; }
        public DbSet<Floor> Floors { get; set; }
        public DbSet<Folder> Folders { get; set; }
        public DbSet<FoodPantry> FoodPantry { get; set; }
        public DbSet<Fund> Funds { get; set; }
        public DbSet<Household> Households { get; set; }
        public DbSet<HouseholdMember> HouseholdMembers { get; set; }
        public DbSet<IPBlacklist> IPBlacklist { get; set; }
        public DbSet<IPWhitelist> IPWhitelist { get; set; }
        public DbSet<LatLong> LatLongs { get; set; }
        public DbSet<Lead> Leads { get; set; }
        public DbSet<Log> Logs { get; set; }
        public DbSet<MessageRequest> MessageRequests { get; set; }
        public DbSet<MessageRequestCategory> MessageRequestCategories { get; set; }
        public DbSet<Modules> Modules { get; set; }
        public DbSet<Note> Notes { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<OfflineGiving> OfflineGiving { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<PaymentMethodAccount> PaymentMethodAccounts { get; set; }
        public DbSet<Permissions> Permissions { get; set; }
        public DbSet<Person> People { get; set; }
        public DbSet<ChurchPerson> ChurchPeople { get; set; }
        public DbSet<PrayerRequest> PrayerRequests { get; set; }
        public DbSet<PrayerRequestCategory> PrayerRequestCategories { get; set; }
        public DbSet<Relationship> Relationships { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<ReportCategory> ReportCategories { get; set; }
        public DbSet<ReportGroup> ReportGroups { get; set; }
        public DbSet<ReportGroupEmailSetting> ReportGroupEmailSettings { get; set; }
        public DbSet<ReportSettings> ReportSettings { get; set; }
        public DbSet<RoleModules> RoleModules { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Salvation> Salvations { get; set; }
        public DbSet<ScheduledPayment> ScheduledPayments { get; set; }
        public DbSet<Sermon> Sermons { get; set; }
        public DbSet<SermonNote> SermonNotes { get; set; }
        public DbSet<SermonSeries> SermonSeries { get; set; }
        public DbSet<SermonTopic> SermonTopics { get; set; }
        public DbSet<ServeTeamMember> ServeTeamMembers { get; set; }
        public DbSet<ServiceArea> ServiceAreas { get; set; }
        public DbSet<SmallGroup> SmallGroups { get; set; }
        public DbSet<SmallGroupAttendee> SmallGroupAttendees { get; set; }
        public DbSet<SmallGroupCategory> SmallGroupCategories { get; set; }
        public DbSet<SmallGroupCategoryType> SmallGroupCategoryTypes { get; set; }
        public DbSet<SmsMessage> SmsMessages { get; set; }
        public DbSet<Subscription> Subscription { get; set; }
        public DbSet<SubscriptionTransaction> SubscriptionTransactions { get; set; }
        public DbSet<SubscriptionType> SubscriptionType { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TagPerson> TagPeople { get; set; }
        public DbSet<TaskSD> Tasks { get; set; }
        public DbSet<UserMerchantAccount> UserMerchantAccounts { get; set; }
        public DbSet<UserReportGroups> UserReportGroups { get; set; }
        public DbSet<UserSetting> UserSettings { get; set; }
        public DbSet<Widget> Widget { get; set; }
        public DbSet<WidgetCategory> WidgetCategoriesCombine { get; set; }
        public DbSet<WidgetCategoryType> WidgetCategory { get; set; }
        public DbSet<WidgetCategoryRole> WidgetCategoryRole { get; set; }
        public DbSet<WidgetPermission> WidgetPermission { get; set; }
    }
}