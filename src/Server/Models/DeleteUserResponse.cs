namespace Vetrina.Server.Models
{
    public class DeleteUserResponse
    {
        public DeleteUserResponse(
            DeleteUserResponseType type, 
            string details = default)
        {
            Type = type;
            Details = details;
        }

        public DeleteUserResponseType Type { get; set; }

        public string Details { get; set; }
    }
}
