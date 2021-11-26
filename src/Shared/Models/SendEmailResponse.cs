namespace Vetrina.Shared.Models
{
    public class SendEmailResponse
    {
        public SendEmailResponse()
        {
        }

        public SendEmailResponse(
            SendEmailResponseType type, 
            string details = null)
        {
            Type = type;
            Details = details;
        }

        public SendEmailResponseType Type { get; set; }

        public string Details { get; set; }
    }
}
