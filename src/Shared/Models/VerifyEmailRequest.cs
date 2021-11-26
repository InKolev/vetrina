using System.ComponentModel.DataAnnotations;

namespace Vetrina.Shared.Models
{
    public class VerifyEmailRequest
    {
        [Required]
        public string VerificationToken { get; set; }
    }
}