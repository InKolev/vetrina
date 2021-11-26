using System.ComponentModel.DataAnnotations;

namespace Vetrina.Shared.Models
{
    public class ForgotPasswordRequest
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        public string Origin { get; set; }
    }
}