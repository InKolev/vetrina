using System.ComponentModel.DataAnnotations;
using Vetrina.Shared.Constants;

namespace Vetrina.Shared.Models
{
    public class LoginRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        public string CallerIpAddress { get; set; }

        public LoginRequest MaskSensitiveData()
        {
            return new LoginRequest
            {
                Email = this.Email,
                Password = LoggingConstants.MaskedValue,
                CallerIpAddress = LoggingConstants.MaskedValue
            };
        }
    }
}