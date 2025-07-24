namespace PraiseCMS.API.Models
{
    public class AccessTokenResponse
    {
        public string access_token { get; set; }
        public string access_token_created_timestamp_utc { get; set; }
        public string expires_in { get; set; }
    }
}