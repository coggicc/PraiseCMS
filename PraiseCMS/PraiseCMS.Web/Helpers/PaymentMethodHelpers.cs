using PraiseCMS.API.Models;
using PraiseCMS.BusinessLayer;
using PraiseCMS.DataAccess.Helpers;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Session;
using PraiseCMS.Shared.Methods;
using PraiseCMS.Shared.Shared;
using PraiseCMS.Web.Controllers.Base;
using System;
using System.Threading.Tasks;

namespace PraiseCMS.Web.Helpers
{
    public static class PaymentMethodHelpers
    {
        public static async Task<PaymentMethodViewModel> GetEditCardViewModel(PaymentMethodAccount paymentMethodAccount, ApiCredentials apiCredentials, BaseController controller, Work work, NuveiHelper nuveiHelper)
        {
            var creditCard = new CreditCard();
            var cardResponse = await nuveiHelper.GetCardDetailsAsync(paymentMethodAccount?.AccountGUID, apiCredentials);
            bool cardsMatch = false;

            if (cardResponse?.result == "0" && cardResponse.paymentsafe_card != null)
            {
                cardsMatch = paymentMethodAccount?.AccountGUID == cardResponse.paymentsafe_card.card_key;
                creditCard.AccountGUID = paymentMethodAccount?.AccountGUID;
                creditCard.CardNumber = paymentMethodAccount?.PaymentMethodPreview;
                creditCard.ExpMonth = paymentMethodAccount?.ExpMonth;
                creditCard.ExpYear = paymentMethodAccount?.ExpYear;
                creditCard.CardType = paymentMethodAccount?.AccountType;
                creditCard.Nickname = paymentMethodAccount?.NickName;
            }

            if (!cardsMatch)
            {
                // Mark inactive in our tables since it is not found at merchant. We don't delete so we can use in giving history
                if (paymentMethodAccount != null)
                {
                    paymentMethodAccount.IsActive = false;
                    paymentMethodAccount.ModifiedDate = DateTime.Now;
                    paymentMethodAccount.ModifiedBy = SessionVariables.CurrentUser.User.Id;
                    work.PaymentMethodAccount.Update(paymentMethodAccount);
                }

                controller.CreateAlertMessage($"Your card ending in {paymentMethodAccount?.PaymentMethodPreview} could not be found and has been removed from this page. Please try adding this payment method again.", AlertMessageTypes.Failure, AlertMessageIcons.Failure);

                return null; // Or handle error case
            }

            return new PaymentMethodViewModel
            {
                User = SessionVariables.CurrentUser.User,
                ChurchId = SessionVariables.CurrentChurch.Id,
                PaymentMethod = DigitalPaymentMethods.Card,
                PaymentCard = new PaymentCard()
                {
                    CcName = creditCard?.NameOnCard,
                    CcNumber = creditCard?.CardNumber,
                    NickName = creditCard?.Nickname,
                    CcExpiry = paymentMethodAccount.IsNotNull() ? $"{paymentMethodAccount.ExpMonth}/{paymentMethodAccount.ExpYear}" : $"{creditCard.ExpMonth}/{creditCard.ExpYear}",
                    AccountGUID = paymentMethodAccount.IsNotNull() ? paymentMethodAccount.AccountGUID : creditCard.AccountGUID,
                    CcExpMonth = paymentMethodAccount.IsNotNull() ? paymentMethodAccount.ExpMonth : creditCard.ExpMonth,
                    CcExpYear = paymentMethodAccount.IsNotNull() ? paymentMethodAccount.ExpYear : creditCard.ExpYear,
                    CcType = creditCard.CardType
                }
            };
        }
    }
}