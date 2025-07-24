using Microsoft.AspNet.Identity;
using PraiseCMS.API.Models;
using PraiseCMS.Shared.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Constants = PraiseCMS.Shared.Shared.Constants;

namespace PraiseCMS.API.Helpers
{
    public static class Responses
    {
        #region Nuvei API Responses
        public static string GetApiTransactionResponse(string resultCode)
        {
            return resultCode == Constants.ApiTransactionSuccessCode || resultCode == APIStatuses.Success
                ? APIStatuses.Success
                : APIStatuses.Error;
        }

        public static string HandleApiTransactionFailure(ResultModel response)
        {
            List<string> errorMessages = new List<string>();

            if (!string.IsNullOrEmpty(response?.result_message))
            {
                errorMessages.Add(response.result_message);
            }

            if (!string.IsNullOrEmpty(response?.result_description))
            {
                errorMessages.Add(response.result_description);
            }

            if (!string.IsNullOrEmpty(response?.result_message_1))
            {
                errorMessages.Add(response.result_message_1);
            }

            if (!string.IsNullOrEmpty(response?.result_message_2))
            {
                errorMessages.Add(response.result_message_2);
            }

            // Concatenate error messages with a separator
            return string.Join(" | ", errorMessages);
        }
        #endregion
    }
}