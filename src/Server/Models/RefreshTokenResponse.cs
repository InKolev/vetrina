using System.Text.Json.Serialization;

namespace Vetrina.Server.Models
{
    public class RefreshTokenResponse
    {
        public RefreshTokenResponse()
        {
        }

        public RefreshTokenResponse(
            RefreshTokenResponseType type, 
            string details)
        {
            Type = type;
            Details = details;
        }

        public string AccessToken { get; set; }

        /// <summary>
        /// Refresh token is returned in HTTP-Only cookie.
        /// </summary>
        [JsonIgnore] 
        public string RefreshToken { get; set; }

        public RefreshTokenResponseType Type { get; set; }

        public string Details { get; set; }
    }
}