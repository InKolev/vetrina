namespace Vetrina.Server.Models
{
    public class ForgotPasswordResponse
    {
        public ForgotPasswordResponse()
        {
        }

        public ForgotPasswordResponse(
            ForgotPasswordResponseType type, 
                string details = default)
        {
            Type = type;
            Details = details;
        }

        public ForgotPasswordResponseType Type { get; set; }

        public string Details { get; set; }
    }
}