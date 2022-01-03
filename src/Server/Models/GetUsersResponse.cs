using System.Collections.Generic;

namespace Vetrina.Server.Models
{
    public class GetUsersResponse
    {
        public GetUsersResponse(
            GetUsersResponseType type,
            IEnumerable<UserDTO> users,
            string details = default)
        {
            Type = type;
            Users = users;
            Details = details;
        }

        public string Details { get; set; }

        public GetUsersResponseType Type { get; set; }

        public IEnumerable<UserDTO> Users { get; set; }
    }
}