using System.ComponentModel.DataAnnotations;

namespace Vetrina.Shared.Models
{
    public class ValidatePasswordResetTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}