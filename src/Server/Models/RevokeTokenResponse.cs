namespace Vetrina.Server.Models
{
    public class RevokeTokenResponse
    {
        public RevokeTokenResponse()
        {
        }

        public RevokeTokenResponse(
            RevokeTokenResponseType type,
            string details = default)
        {
            Type = type;
            Details = details;
        }

        public RevokeTokenResponseType Type { get; set; }

        public string Details { get; set; }
    }
}