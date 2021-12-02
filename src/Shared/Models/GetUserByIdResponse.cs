namespace Vetrina.Shared.Models
{
    public class GetUserByIdResponse
    {
        public GetUserByIdResponse(
            GetUserByIdResponseType type,
            string details = default)
        {
            Type = type;
            Details = details;
        }

        public GetUserByIdResponse(
            GetUserByIdResponseType type,
            UserDTO user,
            string details = default)
        {
            Type = type;
            User = user;
            Details = details;
        }

        public GetUserByIdResponseType Type { get; set; }

        public string Details { get; set; }

        public UserDTO User { get; set; }
    }
}
