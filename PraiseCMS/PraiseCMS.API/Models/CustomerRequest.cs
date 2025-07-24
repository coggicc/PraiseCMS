namespace PraiseCMS.API.Models
{
    public class CustomerRequest
    {
        public string customer_type { get; set; }
        public string customer_id { get; set; }
        public string customer_name { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string street1 { get; set; }
        public string street2 { get; set; }
        public string city { get; set; }
        public string state_id { get; set; }
        public string zip_code { get; set; }
        public string email { get; set; }
        public string status { get; set; }

        // Constructor to ensure all required fields are initialized
        public CustomerRequest(string customer_type, string customer_id, string customer_name, string first_name, string last_name, string street1, string street2, string city, string state_id, string zip_code, string email, string status)
        {
            this.customer_type = customer_type;
            this.customer_id = customer_id;
            this.customer_name = customer_name;
            this.first_name = first_name;
            this.last_name = last_name;
            this.street1 = street1;
            this.street2 = street2;
            this.city = city;
            this.state_id = state_id;
            this.zip_code = zip_code;
            this.email = email;
            this.status = status;
        }
    }
}