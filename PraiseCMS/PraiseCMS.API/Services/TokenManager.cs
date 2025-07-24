using PraiseCMS.API.Models;
using System;

namespace PraiseCMS.API.Services
{
    public class TokenManager
    {
        private string accessToken;
        private DateTime expirationTime;

        public string GetAccessToken()
        {
            // Check if the token is not null and not expired
            if (!string.IsNullOrEmpty(accessToken) && expirationTime > DateTime.UtcNow)
            {
                return accessToken;
            }

            // If the token is expired or not obtained, return null or an empty string
            return null;
        }

        public void StoreAccessToken(AccessTokenResponse tokenResponse)
        {
            // Store the token and set the expiration time based on the expires_in value
            accessToken = tokenResponse?.access_token;

            // Parse expires_in to seconds and set the expiration time
            if (int.TryParse(tokenResponse?.expires_in, out int expiresIn))
            {
                expirationTime = DateTime.UtcNow.AddSeconds(expiresIn);
            }
            else
            {
                // If parsing fails, set a default expiration time or handle it accordingly
                expirationTime = DateTime.UtcNow.AddHours(1); // Default to 1 hour if parsing fails
            }
        }
    }
}