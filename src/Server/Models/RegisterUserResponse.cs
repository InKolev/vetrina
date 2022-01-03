namespace Vetrina.Server.Models
{
    public class RegisterUserResponse
    {
        public RegisterUserResponse()
        {
        }

        public RegisterUserResponse(
            RegisterUserResponseType type, 
            string details = default)
        {
            Type = type;
            Details = details;
        }

        public RegisterUserResponseType Type { get; set; }

        public string Details { get; set; }
    }
}