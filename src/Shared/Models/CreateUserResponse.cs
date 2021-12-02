namespace Vetrina.Shared.Models
{
    public class CreateUserResponse
    {
        public CreateUserResponse(
            CreateUserResponseType type,
            UserDTO user,
            string details = default)
        {
            Type = type;
            User = user;
            Details = details;
        }

        public CreateUserResponse(
            CreateUserResponseType type,
            string details = default)
        {
            Type = type;
            Details = details;
            User = null;
        }

        public string Details { get; set; }

        public CreateUserResponseType Type { get; set; }
       
        public UserDTO User { get; }
    }
}