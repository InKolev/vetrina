namespace Vetrina.Server.Models
{
    public class UpdateUserResponse
    {
        public UpdateUserResponse(
            UpdateUserResponseType type,
            UserDTO user,
            string details = default)
        {
            Type = type;
            User = user;
            Details = details;
        }

        public UpdateUserResponse(
            UpdateUserResponseType type,
            string details = default)
        {
            Type = type;
            Details = details;
        }

        public string Details { get; set; }

        public UpdateUserResponseType Type { get; set; }
        public UserDTO User { get; }
    }
}