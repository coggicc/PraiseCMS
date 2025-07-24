namespace PraiseCMS.API.Models
{
    public class LeadApiRequest
    {
        public string reseller_username { get; set; }
        public string reseller_password { get; set; }
        public int reseller_key { get; set; }
        public string correlation_id { get; set; }
        public string application_template_id { get; set; }
        public string terminal_template_ids { get; set; }
        public string ach_template_id { get; set; }
        public Company company { get; set; }
        public OwnerPrincipal owner_Principal { get; set; }
        public Bank bank { get; set; }
    }
}