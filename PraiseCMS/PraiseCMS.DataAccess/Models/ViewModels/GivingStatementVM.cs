using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models.ViewModels
{
    public class GivingStatementVM
    {
        public GivingStatementVM()
        {
            Church = new Church();
            Statement = new List<GivingStatementModel>();
        }

        public ApplicationUser User { get; set; }
        public Church Church { get; set; }
        public int Year { get; set; }
        public List<GivingStatementModel> Statement { get; set; }
        public decimal Total { get; set; }
    }

    public class GivingStatementModel
    {
        public string Date { get; set; }
        public string Fund { get; set; }
        public string Method { get; set; }
        public string Amount { get; set; }
    }
}