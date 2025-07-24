using System.Collections.Generic;

namespace PraiseCMS.DataAccess.Models
{
    public class Source
    {
        public string type { get; set; }
        public string id { get; set; }
    }

    public class Metadata
    {
        public bool primary { get; set; }
        public bool verified { get; set; }
        public Source source { get; set; }
    }

    public class EmailAddress
    {
        public Metadata metadata { get; set; }
        public string value { get; set; }
    }

    public class PhoneNumber
    {
        public Metadata metadata { get; set; }
        public string value { get; set; }
        public string canonicalForm { get; set; }
        public string type { get; set; }
        public string formattedType { get; set; }
    }

    public class GooglePeopleApiModel
    {
        public string resourceName { get; set; }
        public string etag { get; set; }
        public IList<EmailAddress> emailAddresses { get; set; }
        public IList<PhoneNumber> phoneNumbers { get; set; }
    }

    public class GoogleTokenModel
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
        public string token_type { get; set; }
        public string id_token { get; set; }
    }

    public class GoogleProfileModel
    {
        public string id { get; set; }
        public string email { get; set; }
        public string verified_email { get; set; }
        public string name { get; set; }
        public string given_name { get; set; }
        public string family_name { get; set; }
        public string link { get; set; }
        public string picture { get; set; }
        public string gender { get; set; }
        public string locale { get; set; }
    }
}