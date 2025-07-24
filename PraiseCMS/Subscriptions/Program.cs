using PraiseCMS.BusinessLayer;
using System.Threading.Tasks;

namespace PraiseCMS.Job.Subscriptions
{
    public static class Program
    {
        public static Work Work { get; set; }

        public static async Task Main(string[] args)
        {
            await Work.Subscription.RenewSubscriptions();
            Work.Subscription.SendReminderEmail();
        }
    }
}