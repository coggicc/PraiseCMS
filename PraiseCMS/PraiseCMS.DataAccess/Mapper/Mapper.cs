using PraiseCMS.DataAccess.DAL;
using PraiseCMS.DataAccess.Models;
using PraiseCMS.DataAccess.Models.ViewModels;
using PraiseCMS.Shared.Methods;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PraiseCMS.DataAccess.Mapper
{
    public static class Mapper
    {
        public static List<MyGivingVM> Map(List<Payment> payments)
        {
            var db = new ApplicationDbContext();
            var userIds = payments.Select(x => x.UserId).ToList();
            var users = db.Users.Where(x => userIds.Contains(x.Id)).ToList();

            return payments.Select(x => new MyGivingVM
            {
                Amount = x.Amount,
                CampusId = x.CampusId,
                CreatedDate = x.CreatedDate,
                FundId = x.FundId,
                PaymentMethod = x.PaymentMethod,
                PersonId = users.Where(q => q.Id.Equals(x.UserId)).Select(q => q.PersonId).FirstOrDefault()
            }).ToList();
        }

        public static List<MyGivingVM> Map(List<OfflineGiving> offlineGiving)
        {
            return offlineGiving.Select(x => new MyGivingVM
            {
                Amount = x.Amount,
                CampusId = x.CampusId,
                CreatedDate = x.DateReceived.IsNotNullOrEmpty() ? (DateTime)x.DateReceived : x.CreatedDate,
                FundId = x.FundId,
                OfflinePaymentMethod = x.OfflinePaymentMethod,
                CheckNumber = x.CheckNumber,
                PersonId = x.PersonId
            }).ToList();
        }

        public static List<ChurchPaymentsVM> MapPaymentsSummary(List<Payment> payments)
        {
            return payments.Select(x => new ChurchPaymentsVM
            {
                Amount = x.Amount,
                ProcessingFee = x.ProcessingFee,
                Campus = x.CampusId,
                CreatedDate = x.CreatedDate,
                Fund = x.FundId,
                TransactionId = x.TransactionId ?? string.Empty
            }).ToList();
        }
    }
}