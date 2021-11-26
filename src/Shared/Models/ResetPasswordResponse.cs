namespace Vetrina.Shared.Models
{
    public class ResetPasswordResponse
    {
        public ResetPasswordResponse()
        {
        }

        public ResetPasswordResponse(
            ResetPasswordResponseType type,
            string details = default)
        {
            Type = type;
            Details = details;
        }

        public ResetPasswordResponseType Type { get; set; }

        public string Details { get; set; }
    }
}