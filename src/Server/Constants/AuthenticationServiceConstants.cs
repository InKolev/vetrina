namespace Vetrina.Server.Constants
{
    public class AuthenticationServiceConstants
    {
        public const string FailedToLoginInvalidCredentials =
            "Login failed due to invalid credentials.";

        public const string FailedToLoginNoRecordsAffected =
            "Login failed due to zero database records affected.";

        public const string FailedToLoginUnexpectedError =
            "Login failed due to unexpected error.";

        public const string FailedToRevokeTokenForbidden =
            "Non-admin users can only attempt to revoke their own refresh tokens.";

        public const string FailedToRevokeTokenNoRecordsAffected = 
            "Failed to revoke token due to zero database records affected.";
        
        public const string FailedToRevokeTokenUnexpectedError = 
            "Failed to revoke token due to unexpected error.";

        public const string FailedToRefreshTokenNoRecordsAffected =
            "Failed to refresh token due to zero database records affected.";

        public const string FailedToRefreshTokenUnexpectedError = 
            "Failed to refresh token due to unexpected error.";

        public const string FailedToRegisterEmailAlreadyInUse = 
            "Failed to register due to account with the provided email already exists.";

        public const string FailedToRegisterNoRecordsAffected = 
            "Failed to register account due to zero database records affected.";

        public const string FailedToRegisterUnexpectedError = 
            "Failed to register account due to unexpected error.";
        
        public const string RegistrationCompletedSuccessfully =
            "Registration was completed successfully. Check your email for verification instructions.";

        public const string FailedToVerifyEmailInvalidVerificationToken = 
            "Failed to verify email due to invalid verification token.";
       
        public const string FailedToVerifyEmailNoRecordsAffected =
            "Failed to verify email due to zero database records affected.";
        
        public const string FailedToVerifyEmailUnexpectedError =
            "Failed to verify email due to unexpected error.";

        public const string ForgotPasswordFailedNoRecordsAffected = 
            "Forgot password failed due to zero database records affected.";
        
        public const string ForgotPasswordFailedUnexpectedError = 
            "Forgot password failed due to unexpected error.";

        public const string ForgotPasswordCompletedSuccessfully =
            "Password reset process initiated successfully. Check your email for password reset instructions.";

        public const string FailedToResetPasswordNoRecordsAffected = 
            "Reset password failed due to zero database records affected.";
        
        public const string FailedToResetPasswordUnexpectedError = 
            "Failed to reset password due to unexpected error.";

        public const string ResetPasswordCompletedSuccessfully =
            "Password reset completed successfully. You can now login.";

        public const string FailedToValidateResetPasswordTokenUnexpectedError =
            "Failed to reset password token due to unexpected error.";

        public const string InvalidRefreshToken = 
            "The provided refresh token is invalid.";
        
        public const string InvalidPasswordResetToken = 
            "The provided password reset token is invalid.";
        
        public const int RefreshTokenExpirationTimeInHours = 30 * 24; // 30 days
    }
}
