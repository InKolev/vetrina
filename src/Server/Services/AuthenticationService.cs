using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Constants;
using Vetrina.Server.Domain;
using Vetrina.Server.Options;
using Vetrina.Server.Persistence;
using Vetrina.Shared;
using Vetrina.Shared.Models;
using BC = BCrypt.Net.BCrypt;

namespace Vetrina.Server.Services
{
    public class AuthenticationService : IAuthenticationService
    {
        private const string UserVerificationEmailSender = "extreme.scalability@gmail.com";

        private readonly VetrinaDbContext dbContext;
        private readonly IEmailService emailService;
        private readonly IMapper mapper;
        private readonly JwtOptions jwtOptions;
        private readonly ILogger<UsersService> logger;

        public AuthenticationService(
            VetrinaDbContext dbContext,
            IEmailService emailService,
            IMapper mapper,
            IOptions<JwtOptions> jwtOptions,
            ILogger<UsersService> logger)
        {
            this.dbContext = dbContext;
            this.emailService = emailService;
            this.mapper = mapper;
            this.jwtOptions = jwtOptions.Value;
            this.logger = logger;
        }

        public async Task<LoginResponse> LoginAsync(
            LoginRequest loginRequest,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var account =
                    await dbContext.Users.SingleOrDefaultAsync(
                        x => x.Email == loginRequest.Email,
                        cancellationToken);

                // We do not return a specific error message in the cases where the user is not verified,
                // because we want to guard against potential user enumeration attacks.
                if (account is not { IsEmailVerified: true }
                    || !BC.Verify(loginRequest.Password, account.PasswordHash))
                {
                    return new LoginResponse(
                        LoginResponseType.ValidationError,
                        AuthenticationServiceConstants.FailedToLoginInvalidCredentials);
                }

                // Authentication successful. Generate JWT and Refresh tokens.
                var jwtToken = GenerateAccessToken(account);
                var refreshToken = GenerateRefreshToken(loginRequest.CallerIpAddress);
                account.RefreshTokens.Add(refreshToken);

                RemoveExpiredRefreshTokensFromUser(account);

                dbContext.Users.Update(account);

                var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);
                if (affectedRows == 0)
                {
                    return new LoginResponse(
                        LoginResponseType.UnexpectedError,
                        AuthenticationServiceConstants.FailedToLoginNoRecordsAffected);
                }

                var response = mapper.Map<LoginResponse>(account);
                response.AccessToken = jwtToken;
                response.RefreshToken = refreshToken.Token;
                response.Type = LoginResponseType.Successful;

                return response;
            }
            catch (Exception exception)
            {
                LogUnexpectedFailure(loginRequest.MaskSensitiveData(), exception);

                return new LoginResponse(
                    LoginResponseType.UnexpectedError,
                    AuthenticationServiceConstants.FailedToLoginUnexpectedError);
            }
        }

        public async Task<RegisterUserResponse> RegisterUserAsync(
            RegisterUserRequest registerUserRequest,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var emailAlreadyUsed =
                    await dbContext.Users.AnyAsync(
                        x => x.Email == registerUserRequest.Email,
                        cancellationToken);

                if (emailAlreadyUsed)
                {
                    // It's advisable that we do not return the error message directly to the caller when an email is already in use
                    // because we want to guard against potential user enumeration attacks.
                    // That's why an email notifying about the situation is sent to the user that initially used the provided email address.
                    await SendUserAlreadyRegisteredEmail(
                        registerUserRequest.Email,
                        registerUserRequest.Origin,
                        cancellationToken);

                    logger.LogWarning(
                        $"{AuthenticationServiceConstants.FailedToRegisterEmailAlreadyInUse}({registerUserRequest.Email})");

                    return new RegisterUserResponse(
                        RegisterUserResponseType.Successful,
                        AuthenticationServiceConstants.RegistrationCompletedSuccessfully);
                }

                var user = mapper.Map<User>(registerUserRequest);

                // All accounts registered through this service are defaulted to User role.
                // Admin accounts are seeded through separate internal services and tools.
                user.Role = RoleType.User;
                user.Created = DateTime.UtcNow;
                user.VerificationToken = CreateRandomTokenString();
                user.PasswordHash = BC.HashPassword(registerUserRequest.Password);

                await dbContext.Users.AddAsync(user, cancellationToken);

                var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);
                if (affectedRows == 0)
                {
                    return new RegisterUserResponse(
                        RegisterUserResponseType.UnexpectedError,
                        AuthenticationServiceConstants.FailedToRegisterNoRecordsAffected);
                }

                await SendUserVerificationEmail(
                    registerUserRequest.Email,
                    registerUserRequest.Origin,
                    user.VerificationToken,
                    cancellationToken);

                return new RegisterUserResponse(
                    RegisterUserResponseType.Successful,
                    AuthenticationServiceConstants.RegistrationCompletedSuccessfully);
            }
            catch (Exception exception)
            {
                LogUnexpectedFailure(registerUserRequest.MaskSensitiveData(), exception);

                return new RegisterUserResponse(
                    RegisterUserResponseType.UnexpectedError,
                    AuthenticationServiceConstants.FailedToRegisterUnexpectedError);
            }
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync(
            RefreshTokenRequest refreshTokenRequest,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user =
                    await dbContext.Users.SingleOrDefaultAsync(
                        x =>
                            x.Id == refreshTokenRequest.UserId &&
                            x.RefreshTokens.Any(r => r.Token == refreshTokenRequest.RefreshToken),
                        cancellationToken);

                if (user == null)
                {
                    return new RefreshTokenResponse(
                        RefreshTokenResponseType.ValidationError,
                        AuthenticationServiceConstants.InvalidRefreshToken);
                }

                var refreshToken = user.RefreshTokens.Single(x => x.Token == refreshTokenRequest.RefreshToken);
                if (!refreshToken.IsActive)
                {
                    return new RefreshTokenResponse(
                        RefreshTokenResponseType.ValidationError,
                        AuthenticationServiceConstants.InvalidRefreshToken);
                }

                // Replace the old refresh token with a new one and save it to the database.
                var newRefreshToken = GenerateRefreshToken(refreshTokenRequest.CallerIpAddress);
                refreshToken.Revoked = DateTime.UtcNow;
                refreshToken.RevokedByIp = refreshTokenRequest.CallerIpAddress;
                refreshToken.ReplacedByToken = newRefreshToken.Token;

                user.RefreshTokens.Add(newRefreshToken);

                RemoveExpiredRefreshTokensFromUser(user);

                dbContext.Users.Update(user);
                var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);
                if (affectedRows == 0)
                {
                    return new RefreshTokenResponse(
                        RefreshTokenResponseType.UnexpectedError,
                        AuthenticationServiceConstants.FailedToRefreshTokenNoRecordsAffected);
                }

                // Generate new JWT access token.
                var accessToken = GenerateAccessToken(user);

                return new RefreshTokenResponse
                {
                    Type = RefreshTokenResponseType.Successful,
                    AccessToken = accessToken,
                    RefreshToken = newRefreshToken.Token
                };
            }
            catch (Exception exception)
            {
                LogUnexpectedFailure(refreshTokenRequest.MaskSensitiveData(), exception);

                return new RefreshTokenResponse(
                    RefreshTokenResponseType.UnexpectedError,
                    AuthenticationServiceConstants.FailedToRefreshTokenUnexpectedError);
            }
        }

        public async Task<RevokeTokenResponse> RevokeTokenAsync(
            RevokeTokenRequest revokeTokenRequest,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var loggedInUser =
                    await dbContext.Users.SingleOrDefaultAsync(
                        x => x.Id == revokeTokenRequest.UserId,
                        cancellationToken);

                // Users can revoke their own tokens and system admins can revoke any tokens.
                if (!UserOwnsRefreshToken(loggedInUser, revokeTokenRequest.RefreshToken)
                    && !UserIsSystemAdmin(loggedInUser))
                {
                    return new RevokeTokenResponse(
                        RevokeTokenResponseType.Forbidden,
                        AuthenticationServiceConstants.FailedToRevokeTokenForbidden);
                }

                if (string.IsNullOrWhiteSpace(revokeTokenRequest.RefreshToken))
                {
                    return new RevokeTokenResponse(
                        RevokeTokenResponseType.ValidationError,
                        AuthenticationServiceConstants.InvalidRefreshToken);
                }

                var userToRevokeTokenFrom =
                    await dbContext.Users.SingleOrDefaultAsync(
                        a => a.RefreshTokens.Any(r => r.Token == revokeTokenRequest.RefreshToken),
                        cancellationToken);

                if (userToRevokeTokenFrom == null)
                {
                    return new RevokeTokenResponse(
                        RevokeTokenResponseType.ValidationError,
                        AuthenticationServiceConstants.InvalidRefreshToken);
                }

                var refreshToken = userToRevokeTokenFrom.RefreshTokens.Single(x => x.Token == revokeTokenRequest.RefreshToken);
                if (!refreshToken.IsActive)
                {
                    return new RevokeTokenResponse(
                        RevokeTokenResponseType.ValidationError,
                        AuthenticationServiceConstants.InvalidRefreshToken);
                }

                // Revoke token and save
                refreshToken.Revoked = DateTime.UtcNow;
                refreshToken.RevokedByIp = revokeTokenRequest.CallerIpAddress;

                dbContext.Users.Update(userToRevokeTokenFrom);

                var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);
                if (affectedRows == 0)
                {
                    return new RevokeTokenResponse(
                        RevokeTokenResponseType.UnexpectedError,
                        AuthenticationServiceConstants.FailedToRevokeTokenNoRecordsAffected);
                }

                return new RevokeTokenResponse(
                    RevokeTokenResponseType.Successful);
            }
            catch (Exception exception)
            {
                LogUnexpectedFailure(revokeTokenRequest.MaskSensitiveData(), exception);

                return new RevokeTokenResponse(
                    RevokeTokenResponseType.UnexpectedError,
                    AuthenticationServiceConstants.FailedToRevokeTokenUnexpectedError);
            }
        }

        public async Task<VerifyEmailResponse> VerifyEmailAsync(
            VerifyEmailRequest verifyEmailRequest,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var account =
                    await dbContext.Users.SingleOrDefaultAsync(
                        x => x.VerificationToken == verifyEmailRequest.VerificationToken,
                        cancellationToken);

                if (account == null)
                {
                    return new VerifyEmailResponse(
                        VerifyEmailResponseType.ValidationError,
                        AuthenticationServiceConstants.FailedToVerifyEmailInvalidVerificationToken);
                }

                account.Verified = DateTime.UtcNow;
                account.VerificationToken = null;

                dbContext.Users.Update(account);

                var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);
                if (affectedRows == 0)
                {
                    return new VerifyEmailResponse(
                        VerifyEmailResponseType.UnexpectedError,
                        AuthenticationServiceConstants.FailedToVerifyEmailNoRecordsAffected);
                }

                return new VerifyEmailResponse(
                    VerifyEmailResponseType.Successful);
            }
            catch (Exception exception)
            {
                LogUnexpectedFailure(verifyEmailRequest, exception);

                return new VerifyEmailResponse(
                    VerifyEmailResponseType.UnexpectedError,
                    AuthenticationServiceConstants.FailedToVerifyEmailUnexpectedError);
            }
        }

        public async Task<ForgotPasswordResponse> ForgotPasswordAsync(
            ForgotPasswordRequest forgotPasswordRequest,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var account =
                    await dbContext.Users.SingleOrDefaultAsync(
                        a => a.Email == forgotPasswordRequest.Email,
                        cancellationToken);

                if (account == null)
                {
                    // Return successful response if the user is not found, in order to prevent email enumeration attack.
                    return new ForgotPasswordResponse(
                        ForgotPasswordResponseType.Successful,
                        AuthenticationServiceConstants.ForgotPasswordCompletedSuccessfully);
                }

                account.PasswordResetToken = CreateRandomTokenString();
                account.PasswordResetTokenExpires = DateTime.UtcNow.AddHours(24);

                dbContext.Users.Update(account);

                var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);
                if (affectedRows == 0)
                {
                    return new ForgotPasswordResponse(
                        ForgotPasswordResponseType.UnexpectedError,
                        AuthenticationServiceConstants.ForgotPasswordFailedNoRecordsAffected);
                }

                await SendPasswordResetEmail(account, forgotPasswordRequest.Origin, cancellationToken);

                return new ForgotPasswordResponse(
                    ForgotPasswordResponseType.Successful,
                    AuthenticationServiceConstants.ForgotPasswordCompletedSuccessfully);
            }
            catch (Exception exception)
            {
                LogUnexpectedFailure(forgotPasswordRequest, exception);

                return new ForgotPasswordResponse(
                    ForgotPasswordResponseType.UnexpectedError,
                    AuthenticationServiceConstants.ForgotPasswordFailedUnexpectedError);
            }
        }

        public async Task<ResetPasswordResponse> ResetPasswordAsync(
            ResetPasswordRequest resetPasswordRequest,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var user =
                    await dbContext.Users.SingleOrDefaultAsync(x =>
                            x.PasswordResetToken == resetPasswordRequest.Token &&
                            x.PasswordResetTokenExpires > DateTime.UtcNow,
                        cancellationToken);

                if (user == null)
                {
                    return new ResetPasswordResponse(
                        ResetPasswordResponseType.ValidationError,
                        AuthenticationServiceConstants.InvalidPasswordResetToken);
                }

                // Update password and remove reset token.
                user.PasswordHash = BC.HashPassword(resetPasswordRequest.Password);
                user.PasswordReset = DateTime.UtcNow;
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpires = null;

                dbContext.Users.Update(user);

                var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);
                if (affectedRows == 0)
                {
                    return new ResetPasswordResponse(
                        ResetPasswordResponseType.UnexpectedFailure,
                        AuthenticationServiceConstants.FailedToResetPasswordNoRecordsAffected);
                }

                return new ResetPasswordResponse(
                    ResetPasswordResponseType.Successful,
                    AuthenticationServiceConstants.ResetPasswordCompletedSuccessfully);
            }
            catch (Exception exception)
            {
                LogUnexpectedFailure(resetPasswordRequest, exception);

                return new ResetPasswordResponse(
                    ResetPasswordResponseType.UnexpectedFailure,
                    AuthenticationServiceConstants.FailedToResetPasswordUnexpectedError);
            }
        }

        public async Task<ValidatePasswordResetTokenResponse> ValidatePasswordResetTokenAsync(
            ValidatePasswordResetTokenRequest validatePasswordResetTokenRequest,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var account =
                    await dbContext.Users.SingleOrDefaultAsync(x =>
                            x.PasswordResetToken == validatePasswordResetTokenRequest.Token &&
                            x.PasswordResetTokenExpires >= DateTime.UtcNow,
                        cancellationToken);

                if (account == null)
                {
                    return new ValidatePasswordResetTokenResponse(
                        ValidatePasswordResetTokenResponseType.ValidationError,
                        AuthenticationServiceConstants.InvalidPasswordResetToken);
                }

                return new ValidatePasswordResetTokenResponse(
                    ValidatePasswordResetTokenResponseType.Successful);
            }
            catch (Exception exception)
            {
                LogUnexpectedFailure(validatePasswordResetTokenRequest, exception);

                return new ValidatePasswordResetTokenResponse(
                    ValidatePasswordResetTokenResponseType.UnexpectedFailure,
                    AuthenticationServiceConstants.FailedToValidateResetPasswordTokenUnexpectedError);
            }
        }

        private string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(jwtOptions.Secret);
            var tokenDescriptor =
                new SecurityTokenDescriptor
                {
                    Expires = DateTime.UtcNow.Add(jwtOptions.ExpirationTime),
                    Subject =
                        new ClaimsIdentity(
                            new List<Claim>
                            {
                                new Claim(ClaimTypes.Email, user.Email),
                                new Claim(ClaimTypes.Role, user.Role.ToString()),
                                new Claim("name", $"{user.FirstName} {user.LastName}"),
                                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            }),
                    SigningCredentials =
                        new SigningCredentials(
                            new SymmetricSecurityKey(key),
                            SecurityAlgorithms.HmacSha256Signature),
                    Issuer = jwtOptions.Issuer,
                    Audience = jwtOptions.Audience,
                    IssuedAt = DateTime.UtcNow,
                };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private static RefreshToken GenerateRefreshToken(string callerIpAddress)
        {
            return new RefreshToken
            {
                Token = CreateRandomTokenString(),
                Expires = DateTime.UtcNow.AddHours(AuthenticationServiceConstants.RefreshTokenExpirationTimeInHours),
                Created = DateTime.UtcNow,
                CreatedByIp = callerIpAddress
            };
        }

        private void RemoveExpiredRefreshTokensFromUser(User user)
        {
            user.RefreshTokens.RemoveAll(x =>
                !x.IsActive &&
                x.Created.AddDays(jwtOptions.RefreshTokenTimeToLive) <= DateTime.UtcNow);
        }

        private static string CreateRandomTokenString()
        {
            var randomBytes = new byte[40];
            using var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            rngCryptoServiceProvider.GetBytes(randomBytes);

            // Convert random bytes to hex string
            return BitConverter.ToString(randomBytes).Replace("-", "");
        }

        private async Task SendUserVerificationEmail(
            string email,
            string origin,
            string verificationToken,
            CancellationToken cancellationToken)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var verifyUrl = $"{origin}/verify-email/{verificationToken}";
                message = $@"<p>Please click the below link to verify your email address:</p>
                             <p><a href=""{verifyUrl}"">{verifyUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to verify your email address with the <code>/accounts/verify-email</code> api route:</p>
                             <p><code>{verificationToken}</code></p>";
            }

            var sendEmailRequest =
                new SendEmailRequest(
                    UserVerificationEmailSender,
                    email,
                    "Sign-up Verification - Verify Email",
                    $@"<h4>Verify Email</h4>
                         <p>Thanks for registering!</p>
                         {message}");

            await emailService.SendEmailAsync(sendEmailRequest, cancellationToken);
        }

        private async Task SendUserAlreadyRegisteredEmail(
            string email,
            string origin,
            CancellationToken cancellationToken)
        {
            string message;

            if (!string.IsNullOrEmpty(origin))
            {
                message = $@"<p>If you don't know your password please visit the <a href=""{origin}/user/forgot-password"">forgot password</a> page.</p>";
            }
            else
            {
                message = "<p>If you don't know your password you can reset it via the <code>/accounts/forgot-password</code> api route.</p>";
            }

            var sendEmailRequest =
                new SendEmailRequest(
                    UserVerificationEmailSender,
                    email,
                    "Sign-up Verification - Email Already Registered",
                    $@"<h4>Email Already Registered</h4>
                         <p>Your email <strong>{email}</strong> is already registered.</p>
                         {message}");

            await emailService.SendEmailAsync(sendEmailRequest, cancellationToken);
        }

        private async Task SendPasswordResetEmail(
            User user,
            string origin,
            CancellationToken cancellationToken)
        {
            string message;
            if (!string.IsNullOrEmpty(origin))
            {
                var resetUrl = $"{origin}/user/reset-password?token={user.PasswordResetToken}";
                message = $@"<p>Please click the below link to reset your password, the link will be valid for 1 day:</p>
                             <p><a href=""{resetUrl}"">{resetUrl}</a></p>";
            }
            else
            {
                message = $@"<p>Please use the below token to reset your password with the <code>/accounts/reset-password</code> api route:</p>
                             <p><code>{user.PasswordResetToken}</code></p>";
            }

            var sendEmailRequest =
                new SendEmailRequest(
                    UserVerificationEmailSender,
                    user.Email,
                    "Sign-up Verification - Reset password",
                    $@"<h4>Reset Password Email</h4>
                         {message}");

            await emailService.SendEmailAsync(sendEmailRequest, cancellationToken);
        }

        private void LogUnexpectedFailure<T>(
            T request,
            Exception exception,
            [CallerMemberName] string methodName = default)
            where T : class
        {
            var errorMessage =
                $"{methodName} failed. " +
                $"Reason: {exception.Message}. " +
                $"Request: {JsonSerializer.Serialize(request)}";

            logger.LogError(exception, errorMessage);
        }

        // TODO: Move those User-specific methods to the domain model or to an extension method.
        private static bool UserOwnsRefreshToken(User user, string refreshToken)
        {
            return user.RefreshTokens?.Find(x => x.Token == refreshToken) != null;
        }

        private static bool UserIsSystemAdmin(User user)
        {
            return user.Role == RoleType.SystemAdmin;
        }
    }
}