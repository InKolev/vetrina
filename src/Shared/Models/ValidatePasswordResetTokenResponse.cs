namespace Vetrina.Shared.Models
{
    public class ValidatePasswordResetTokenResponse
    {
        public ValidatePasswordResetTokenResponse()
        {
        }

        public ValidatePasswordResetTokenResponse(
            ValidatePasswordResetTokenResponseType type, 
            string details = default)
        {
            Type = type;
            Details = details;
        }

        public ValidatePasswordResetTokenResponseType Type { get; set; }

        public string Details { get; set; }
    }
}