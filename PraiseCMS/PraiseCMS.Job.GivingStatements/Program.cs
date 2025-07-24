using PraiseCMS.BusinessLayer;

namespace PraiseCMS.Job.GivingStatements
{
    public static class Program
    {
        public static Work Work { get; set; }

        public static void Main(string[] args)
        {
            Work.Giving.SendAnnualGivingStatements();
        }
    }
}