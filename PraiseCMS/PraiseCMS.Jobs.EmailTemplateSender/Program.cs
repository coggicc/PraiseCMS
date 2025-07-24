namespace PraiseCMS.Job.EmailTemplateSender
{
    public static class Program
    {
        static void Main(string[] args)
        {
            //var db = new ApplicationDbContext();
            //var churches = db.Churches.ToList();
            //var emailTemplates = db.EmailTemplates.ToList();

            //foreach (var church in churches)
            //{
            //    var emailTemplates = emailTemplateProvider.GetAllEmailTemplatesFor(EmailTemplateTypes.Company, company.Id).OrderBy(x => x.Display);

            //    //Handle warranty letter
            //    foreach (var emailTemplate in emailTemplates)
            //    {
            //        try
            //        {
            //            var emails = new List<EmailSD>();

            //            //Handle warranty letter
            //            if (!string.IsNullOrEmpty(emailTemplate.Category) && emailTemplate.Category.Equals(EmailTemplateCategories.WarrantyLetter))
            //            {
            //                //Grab customers that closed 11 months ago
            //                var closingDate = DateTime.Now.AddMonths(-11).ToShortDateString();
            //                var units = unitProvider.GetUnitsByClosingDate(company.Id, closingDate);
            //                var lots = lotProvider.GetAllForInList(units.Select(x => x.LotId), "id");
            //                var subdivisions = subdivisionProvider.GetAllForInList(lots.Select(x => x.SubdivisionId), "id");
            //                var customers = customerProvider.GetAllForInList(units.Select(x => x.CustomerId), "id");
            //                foreach (var unit in units)
            //                {
            //                    var lot = !string.IsNullOrEmpty(unit.LotId) ? lots.FirstOrDefault(x => x.Id.Equals(unit.LotId)) : null;
            //                    var subdivision = !string.IsNullOrEmpty(lot.SubdivisionId) ? subdivisions.FirstOrDefault(x => x.Id.Equals(lot.SubdivisionId)) : null;
            //                    var customer = !string.IsNullOrEmpty(unit.CustomerId) ? customers.FirstOrDefault(x => x.Id.Equals(unit.CustomerId)) : null;
            //                    if (lot != null && subdivision != null && customer != null && !string.IsNullOrEmpty(customer.Email))
            //                    {
            //                        var email = new EmailSD();

            //                        email.Id = Utilities.GenerateUniqueId();
            //                        email.Type = "Email Template";
            //                        email.TypeId = emailTemplate.Id;
            //                        email.DateCreated = DateTime.Now;
            //                        //email.To = customer.Email;
            //                        email.To = "cale@riipl.com";
            //                        email.Subject = emailTemplate.Subject;
            //                        email.Message = emailTemplate.Body.Replace("{current-date}", DateTime.Now.ToShortDateString())
            //                            .Replace("{customer-name}", !string.IsNullOrEmpty(customer.Name) ? customer.Name : "")
            //                            .Replace("{customer-address1}", !string.IsNullOrEmpty(customer.PhysicalAddress1) ? customer.PhysicalAddress1 : "")
            //                            .Replace("{customer-city}", !string.IsNullOrEmpty(customer.PhysicalCity) ? customer.PhysicalCity + ", " : "")
            //                            .Replace("{customer-state}", !string.IsNullOrEmpty(customer.PhysicalState) ? customer.PhysicalState : "")
            //                            .Replace("{customer-zip}", !string.IsNullOrEmpty(customer.PhysicalZip) ? customer.PhysicalZip : "")
            //                            .Replace("{company-name}", !string.IsNullOrEmpty(company.Name) ? company.Name : "")
            //                            .Replace("{company-address1}", !string.IsNullOrEmpty(company.PhysicalAddress1) ? company.PhysicalAddress1 : "")
            //                            .Replace("{company-address2}", !string.IsNullOrEmpty(company.PhysicalAddress2) ? ", " + company.PhysicalAddress2 : "")
            //                            .Replace("{company-city}", !string.IsNullOrEmpty(company.PhysicalCity) ? company.PhysicalCity + ", " : "")
            //                            .Replace("{company-state}", !string.IsNullOrEmpty(company.PhysicalState) ? company.PhysicalState : "")
            //                            .Replace("{company-zip}", !string.IsNullOrEmpty(company.PhysicalZip) ? company.PhysicalZip : "")
            //                            .Replace("{company-phone}", !string.IsNullOrEmpty(company.Phone) ? company.Phone : "")
            //                            .Replace("{company-fax}", !string.IsNullOrEmpty(company.Fax) ? company.Fax : "")
            //                            .Replace("{company-zip}", !string.IsNullOrEmpty(company.PhysicalZip) ? company.PhysicalZip : "")
            //                            .Replace("{company-warranty-email}", !string.IsNullOrEmpty(company.WarrantyEmail) ? company.WarrantyEmail : "")
            //                            .Replace("{subdivision-name}", !string.IsNullOrEmpty(subdivision.Display) ? subdivision.Display : "")
            //                            .Replace("{lot-number}", !string.IsNullOrEmpty(lot.Display) ? lot.Display : "");

            //                        emails.Add(email);
            //                    }
            //                }
            //            }
            //            else if (!string.IsNullOrEmpty(emailTemplate.Category) && emailTemplate.Category.Equals(EmailTemplateCategories.WelcomeLetter))
            //            {

            //            }

            //            foreach (var email in emails)
            //            {
            //                try
            //                {
            //                    Emailer.SendEmail(email, null, null, domain, company, false);
            //                }
            //                catch
            //                {
            //                    //Ignore for now
            //                }
            //            }
            //        }
            //        catch
            //        {
            //            //Ignore for now
            //        }
            //    }

            //}
        }
    }
}
