using System.Threading;
using System.Threading.Tasks;
using Vetrina.Shared;
using Vetrina.Shared.Models;

namespace Vetrina.Server.Abstractions
{
    public interface IAuthenticationService
    {
        Task<LoginResponse> LoginAsync(
            LoginRequest loginRequest,
            CancellationToken cancellationToken = default);

        Task<RegisterUserResponse> RegisterUserAsync(
            RegisterUserRequest registerUserRequest,
            CancellationToken cancellationToken = default);

        Task<RefreshTokenResponse> RefreshTokenAsync(
            RefreshTokenRequest refreshTokenRequest,
            CancellationToken cancellationToken = default);

        Task<RevokeTokenResponse> RevokeTokenAsync(
            RevokeTokenRequest revokeTokenRequest,
            CancellationToken cancellationToken = default);

        Task<VerifyEmailResponse> VerifyEmailAsync(
            VerifyEmailRequest verifyEmailRequest,
            CancellationToken cancellationToken = default);

        Task<ForgotPasswordResponse> ForgotPasswordAsync(
            ForgotPasswordRequest forgotPasswordRequest,
            CancellationToken cancellationToken = default);

        Task<ResetPasswordResponse> ResetPasswordAsync(
            ResetPasswordRequest resetPasswordRequest,
            CancellationToken cancellationToken = default);

        Task<ValidatePasswordResetTokenResponse> ValidatePasswordResetTokenAsync(
            ValidatePasswordResetTokenRequest validatePasswordResetTokenRequest,
            CancellationToken cancellationToken = default);
    }
}