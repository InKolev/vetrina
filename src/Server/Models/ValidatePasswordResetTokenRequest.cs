using System.ComponentModel.DataAnnotations;

namespace Vetrina.Server.Models
{
    public class ValidatePasswordResetTokenRequest
    {
        [Required]
        public string Token { get; set; }
    }
}