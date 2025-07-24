using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Web.Controllers.Base;
using Rotativa;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace PraiseCMS.Web.Controllers
{
    public class StatementController : BaseController
    {
        // GET: Statement
        public ActionResult Preview(string t)
        {
            var splitted = WebUtility.UrlDecode(t).Decrypt().Split('-');

            string userId = splitted[0];
            int year = splitted[1].ToInt32();
            string churchId = splitted[2];

            return new PartialViewAsPdf("Statement", GetGivingStatement(churchId, userId, year));
        }

        private GivingStatementVM GetGivingStatement(string churchId, string userId, int year)
        {
            var payments = work.Payment.GetAllForYear(churchId, year, userId).OrderBy(x => x.CreatedDate).ToList();
            var funds = work.Fund.GetAll(churchId);
            var paymentMethod = payments.Select(x => x.PaymentMethod).ToList();
            var paymentMethodAccount = work.PaymentMethodAccount.GetAllByPaymentMethod(paymentMethod);

            var statementVM = new GivingStatementVM
            {
                User = work.User.Get(userId),
                Church = work.Church.Get(churchId),

                Total = payments.Select(x => x.Amount).Sum()
            };

            foreach (var payment in payments)
            {
                var column = new GivingStatementModel
                {
                    Date = payment.CreatedDate.ToShortDateString(),
                    Amount = payment.Amount.ToCurrencyString()
                };

                //Add Fund Name
                if (!string.IsNullOrEmpty(payment.FundId))
                {
                    column.Fund = funds.FirstOrDefault(x => x.Id.Equals(payment.FundId))?.Name ?? string.Empty;
                }

                //Add Payment method
                if (!string.IsNullOrEmpty(payment.PaymentMethod))
                {
                    var paymeyMethod = paymentMethodAccount.FirstOrDefault(x => x.AccountGUID == payment.PaymentMethod);

                    if (paymeyMethod.IsNotNull())
                    {
                        column.Method = paymeyMethod.AccountType;
                    }
                }

                statementVM.Statement.Add(column);
            }

            return statementVM;
        }
    }
}