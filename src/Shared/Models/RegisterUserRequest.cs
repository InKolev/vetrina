using System.ComponentModel.DataAnnotations;
using Vetrina.Shared.Constants;

namespace Vetrina.Shared.Models
{
    public class RegisterUserRequest
    {
        public string Origin { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Range(typeof(bool), "true", "true")]
        public bool AcceptTerms { get; set; }

        public RegisterUserRequest MaskSensitiveData()
        {
            return new RegisterUserRequest
            {
                Email = this.Email,
                Title = this.Title,
                Origin = this.Origin,
                LastName = this.LastName,
                FirstName = this.FirstName,
                AcceptTerms = this.AcceptTerms,
                Password = LoggingConstants.MaskedValue,
                ConfirmPassword = LoggingConstants.MaskedValue,
            };
        }
    }
}