using System.ComponentModel.DataAnnotations;

namespace Vetrina.Server.Models
{
    public class VerifyEmailRequest
    {
        [Required]
        public string VerificationToken { get; set; }
    }
}