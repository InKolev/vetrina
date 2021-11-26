using System;
using System.Collections.Generic;
using Vetrina.Shared;

namespace Vetrina.Server.Domain
{
    public class User
    {
        public int Id { get; set; }
        
        public string Title { get; set; }
        
        public string FirstName { get; set; }
        
        public string LastName { get; set; }
        
        public string UserName { get; set; }
        
        public string Email { get; set; }
        
        public string PasswordHash { get; set; }
        
        public bool AcceptTerms { get; set; }
        
        public RoleType Role { get; set; }
        
        public string VerificationToken { get; set; }
        
        public DateTime? Verified { get; set; }
        
        public bool IsEmailVerified => Verified.HasValue || PasswordReset.HasValue;
        
        public string PasswordResetToken { get; set; }
        
        public DateTime? PasswordResetTokenExpires { get; set; }
        
        public DateTime? PasswordReset { get; set; }
        
        public DateTime Created { get; set; }
        
        public DateTime? Updated { get; set; }
        
        public List<RefreshToken> RefreshTokens { get; set; }
    }
}