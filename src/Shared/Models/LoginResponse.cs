using System;
using System.Text.Json.Serialization;

namespace Vetrina.Shared.Models
{
    public class LoginResponse
    {
        public LoginResponse()
        {
        }

        public LoginResponse(
            LoginResponseType type, 
            string details = null)
        {
            Type = type;
            Details = details;
        }

        public int Id { get; set; }
        
        public string Title { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }
        
        public DateTime Created { get; set; }
        
        public DateTime? Updated { get; set; }
        
        public bool IsVerified { get; set; }
        
        public string AccessToken { get; set; }

        /// <summary>
        /// Refresh token is returned in HTTP-Only cookie.
        /// </summary>
        [JsonIgnore]
        public string RefreshToken { get; set; }

        public LoginResponseType Type { get; set; }

        public string Details { get; set; }
    }
}