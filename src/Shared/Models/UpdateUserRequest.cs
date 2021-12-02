using System.ComponentModel.DataAnnotations;

namespace Vetrina.Shared.Models
{
    public class UpdateUserRequest
    {
        public int UserId { get; set; }

        public string Title { get; set; }
      
        public string FirstName { get; set; }
        
        public string LastName { get; set; }

        [MinLength(2)]
        public string UserName { get; set; }

        [EnumDataType(typeof(RoleType))]
        public RoleType Role { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [MinLength(6)]
        public string Password { get; set; }

        [Compare(otherProperty: nameof(Password))]
        public string ConfirmPassword { get; set; }
    }
}