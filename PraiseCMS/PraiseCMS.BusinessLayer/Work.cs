using Microsoft.Win32.SafeHandles;
using PraiseCMS.DataAccess.DAL;
using System;
using System.Runtime.InteropServices;

namespace PraiseCMS.BusinessLayer
{
    public class Work : IDisposable
    {
        bool disposed;
        readonly SafeHandle handle = new SafeFileHandle(IntPtr.Zero, true);
        private ApplicationDbContext _db;

        public Work()
        {
            _db = new ApplicationDbContext();
            InitOperations();
        }

        void InitOperations()
        {
            Account = new AccountOperations(_db, this);
            Attachment = new AttachmentOperations(_db, this);
            Attendance = new AttendanceOperations(_db, this);
            Baptism = new BaptismOperations(_db, this);
            Building = new BuildingOperations(_db, this);
            BlogPost = new BlogPostOperations(_db, this);
            BlogCategory = new BlogCategoryOperations(_db, this);
            BlogTag = new BlogTagOperations(_db, this);
            Calendar = new CalendarOperations(_db, this);
            Campus = new CampusOperations(_db, this);
            Church = new ChurchOperations(_db, this);
            ChurchEvent = new ChurchEventOperationsOLD(_db, this);
            ChurchEventType = new ChurchEventTypeOperations(_db, this);
            ChurchEvents = new ChurchEventOperations(_db, this);
            ChurchMerchantAccount = new ChurchMerchantAccountOperations(_db, this, Church);
            CommunicationGroup = new CommunicationGroupOperations(_db, this);
            DashboardTemplate = new DashboardTemplateOperations(_db, this);
            Denomination = new DenominationOperations(_db, this);
            Email = new EmailOperations(_db, this);
            EmailTemplate = new EmailTemplateOperations(_db, this);
            Equipment = new EquipmentOperation(_db, this);
            Event = new EventOperations(_db, this);
            Floor = new FloorOperations(_db, this);
            Fund = new FundOperations(_db, this);
            Giving = new GivingOperations(_db, this);
            Household = new HouseholdOperations(_db, this);
            IPBlacklist = new IPBlacklistOperations(_db, this);
            IPWhitelist = new IPWhitelistOperations(_db, this);
            LatLong = new LatLongOperations(_db, this);
            Leads = new LeadsOperations(_db, this);
            Log = new LogOperations(_db, this);
            Media = new MediaOperations(_db, this);
            MessageRequest = new MessageRequestOperations(_db, this);
            Module = new ModuleOperations(_db, this);
            Note = new NoteOperations(_db, this);
            Notification = new NotificationOperations(_db, this);
            OfflineGiving = new OfflineGivingOperations(_db, this);
            Payment = new PaymentOperations(_db, this);
            PaymentMethodAccount = new PaymentMethodAccountOperations(_db, this);
            Permission = new PermissionOperations(_db, this);
            Person = new PersonOperations(_db, this);
            PrayerRequest = new PrayerRequestOperations(_db, this);
            Report = new ReportOperations(_db, this);
            Role = new RoleOperations(_db, this);
            Room = new RoomOperations(_db, this);
            Salvation = new SalvationOperations(_db, this);
            ScheduledPayment = new ScheduledPaymentOperations(_db, this);
            Search = new SearchOperations(_db, this);
            ServeTeam = new ServeTeamOperations(_db, this);
            ServiceArea = new ServiceAreaOperations(_db, this);
            SmallGroup = new SmallGroupOperations(_db, this);
            SmallGroupCategoryType = new SmallGroupCategoryTypeOperations(_db, this);
            Subscription = new SubscriptionOperations(_db, this);
            Support = new SupportOperations(_db, this);
            TagCommunication = new TagCommunication(_db, this);
            Tags = new TagOperations(_db, this);
            Task = new TaskOperations(_db, this);
            User = new UserOperations(_db, this);
            UserMerchantAccount = new UserMerchantAccountOperations(_db, this);
            UserSetting = new UserSettingOperations(_db, this);
        }

        public AccountOperations Account { get; set; }
        public AttachmentOperations Attachment { get; set; }
        public AttendanceOperations Attendance { get; set; }
        public BaptismOperations Baptism { get; set; }
        public BuildingOperations Building { get; set; }
        public BlogPostOperations BlogPost { get; set; }
        public BlogCategoryOperations BlogCategory { get; set; }
        public BlogTagOperations BlogTag { get; set; }
        public CalendarOperations Calendar { get; set; }
        public CampusOperations Campus { get; set; }
        public ChurchEventOperations ChurchEvents { get; set; }
        public ChurchEventOperationsOLD ChurchEvent { get; set; }
        public ChurchEventTypeOperations ChurchEventType { get; set; }
        public ChurchMerchantAccountOperations ChurchMerchantAccount { get; set; }
        public ChurchOperations Church { get; set; }
        public CommunicationGroupOperations CommunicationGroup { get; set; }
        public DashboardTemplateOperations DashboardTemplate { get; set; }
        public DenominationOperations Denomination { get; set; }
        public EmailOperations Email { get; set; }
        public EmailTemplateOperations EmailTemplate { get; set; }
        public EquipmentOperation Equipment { get; set; }
        public EventOperations Event { get; set; }
        public FloorOperations Floor { get; set; }
        public FundOperations Fund { get; set; }
        public GivingOperations Giving { get; set; }
        public HouseholdOperations Household { get; set; }
        public IPBlacklistOperations IPBlacklist { get; set; }
        public IPWhitelistOperations IPWhitelist { get; set; }
        public LatLongOperations LatLong { get; set; }
        public LeadsOperations Leads { get; set; }
        public LogOperations Log { get; set; }
        public MediaOperations Media { get; set; }
        public MessageRequestOperations MessageRequest { get; set; }
        public ModuleOperations Module { get; set; }
        public NoteOperations Note { get; set; }
        public NotificationOperations Notification { get; set; }
        public OfflineGivingOperations OfflineGiving { get; set; }
        public PaymentOperations Payment { get; set; }
        public PaymentMethodAccountOperations PaymentMethodAccount { get; set; }
        public PermissionOperations Permission { get; set; }
        public PersonOperations Person { get; set; }
        public PrayerRequestOperations PrayerRequest { get; set; }
        public ReportOperations Report { get; set; }
        public RoleOperations Role { get; set; }
        public RoomOperations Room { get; set; }
        public SalvationOperations Salvation { get; set; }
        public ScheduledPaymentOperations ScheduledPayment { get; set; }
        public SearchOperations Search { get; set; }
        public ServeTeamOperations ServeTeam { get; set; }
        public ServiceAreaOperations ServiceArea { get; set; }
        public SmallGroupCategoryTypeOperations SmallGroupCategoryType { get; set; }
        public SmallGroupOperations SmallGroup { get; set; }
        public SubscriptionOperations Subscription { get; set; }
        public SupportOperations Support { get; set; }
        public TagCommunication TagCommunication { get; set; }
        public TagOperations Tags { get; set; }
        public TaskOperations Task { get; set; }
        public UserMerchantAccountOperations UserMerchantAccount { get; set; }
        public UserOperations User { get; set; }
        public UserSettingOperations UserSetting { get; set; }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Implement Dispose(bool) to provide a way to release both managed and unmanaged resources
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Release managed resources
                if (_db != null)
                {
                    _db.Dispose();
                    _db = null;
                }
            }

            // Release unmanaged resources
            if (handle?.IsInvalid == false)
            {
                handle.Dispose();
            }

            disposed = true;
        }

        // Finalizer (destructor) to release unmanaged resources in case Dispose is not called explicitly
        ~Work()
        {
            Dispose(false);
        }
    }
}