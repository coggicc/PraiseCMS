namespace PraiseCMS.API.Models
{
    public class Company
    {
        public string company_legal_name { get; set; }
        public string company_dba_name { get; set; }
        public string company_ownership_type { get; set; }
        public string company_federal_tax_id { get; set; }
        public LegalAddress legal_address { get; set; }
    }
}