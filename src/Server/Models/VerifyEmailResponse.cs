namespace Vetrina.Server.Models
{
    public class VerifyEmailResponse
    {
        public VerifyEmailResponse()
        {
        }

        public VerifyEmailResponse(
            VerifyEmailResponseType type,
            string details = default)
        {
            Type = type;
            Details = details;
        }

        public VerifyEmailResponseType Type { get; }

        public string Details { get; }
    }
}