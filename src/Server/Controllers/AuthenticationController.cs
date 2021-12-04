using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Attributes;
using Vetrina.Server.Constants;
using Vetrina.Server.Controllers.Abstract;
using Vetrina.Shared.Constants;
using Vetrina.Shared.Models;

namespace Vetrina.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthenticationController : ApiControllerBase
    {
        private readonly IAuthenticationService authenticationService;

        public AuthenticationController(IAuthenticationService authenticationService)
        {
            this.authenticationService = authenticationService;
        }

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> LoginAsync(
            LoginRequest loginRequest,
            CancellationToken cancellationToken)
        {
            loginRequest.CallerIpAddress = GetCallerIpAddress();

            var loginResponse =
                await authenticationService.LoginAsync(
                    loginRequest,
                    cancellationToken);

            if (loginResponse.Type == LoginResponseType.Successful)
            {
                AddRefreshTokenToCookies(loginResponse.RefreshToken);
            }

            return loginResponse.Type switch
            {
                LoginResponseType.Successful => Ok(loginResponse),
                LoginResponseType.ValidationError => BadRequest(loginResponse),
                LoginResponseType.UnexpectedError => InternalServerError(),
                _ => InternalServerError()
            };
        }

        [CustomAuthorize]
        [HttpPost("refresh-token")]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(
            RefreshTokenRequest refreshTokenRequest,
            CancellationToken cancellationToken)
        {
            refreshTokenRequest.UserId = CurrentUser.Id;
            refreshTokenRequest.RefreshToken ??= GetRefreshTokenFromCookies();
            refreshTokenRequest.CallerIpAddress = GetCallerIpAddress();

            var refreshTokenResponse =
                await authenticationService.RefreshTokenAsync(
                    refreshTokenRequest,
                    cancellationToken);

            if (refreshTokenResponse.Type == RefreshTokenResponseType.Successful)
            {
                AddRefreshTokenToCookies(refreshTokenResponse.RefreshToken);
            }

            return refreshTokenResponse.Type switch
            {
                RefreshTokenResponseType.Successful => Ok(refreshTokenResponse),
                RefreshTokenResponseType.ValidationError => BadRequest(refreshTokenResponse),
                RefreshTokenResponseType.UnexpectedError => InternalServerError(),
                _ => InternalServerError()
            };
        }

        [CustomAuthorize]
        [HttpPost("revoke-token")]
        public async Task<ActionResult<RevokeTokenResponse>> RevokeToken(
            RevokeTokenRequest revokeTokenRequest,
            CancellationToken cancellationToken)
        {
            revokeTokenRequest.CallerIpAddress = GetCallerIpAddress();
            revokeTokenRequest.RefreshToken ??= GetRefreshTokenFromCookies();
            revokeTokenRequest.UserId = CurrentUser.Id;

            var revokeTokenResponse =
                await authenticationService.RevokeTokenAsync(
                    revokeTokenRequest,
                    cancellationToken);

            return revokeTokenResponse.Type switch
            {
                RevokeTokenResponseType.Successful => Ok(revokeTokenResponse),
                RevokeTokenResponseType.ValidationError => BadRequest(revokeTokenResponse),
                RevokeTokenResponseType.Forbidden => Forbidden(revokeTokenResponse),
                RevokeTokenResponseType.UnexpectedError => InternalServerError(),
                _ => InternalServerError()
            };
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterUserResponse>> Register(
            RegisterUserRequest registerUserRequest,
            CancellationToken cancellationToken)
        {
            registerUserRequest.Origin = GetOriginFromRequestHeaders();

            var registerAccountResponse =
                await authenticationService.RegisterUserAsync(
                    registerUserRequest,
                    cancellationToken);

            return registerAccountResponse.Type switch
            {
                RegisterUserResponseType.Successful => Ok(registerAccountResponse),
                RegisterUserResponseType.EmailAlreadyInUse => BadRequest(registerAccountResponse),
                RegisterUserResponseType.UnexpectedError => InternalServerError(),
                _ => InternalServerError()
            };
        }

        [HttpPost("verify-email")]
        public async Task<ActionResult<VerifyEmailResponse>> VerifyEmail(
            VerifyEmailRequest verifyEmailRequest,
            CancellationToken cancellationToken)
        {
            var verifyEmailResponse =
                await authenticationService.VerifyEmailAsync(
                    verifyEmailRequest,
                    cancellationToken);

            return verifyEmailResponse.Type switch
            {
                VerifyEmailResponseType.Successful => Ok(verifyEmailResponse),
                VerifyEmailResponseType.ValidationError => BadRequest(verifyEmailResponse),
                VerifyEmailResponseType.UnexpectedError => InternalServerError(),
                _ => InternalServerError()
            };
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ForgotPasswordResponse>> ForgotPassword(
            ForgotPasswordRequest forgotPasswordRequest,
            CancellationToken cancellationToken)
        {
            forgotPasswordRequest.Origin = GetOriginFromRequestHeaders();

            var forgotPasswordResponse =
                await authenticationService.ForgotPasswordAsync(
                    forgotPasswordRequest,
                    cancellationToken);

            return forgotPasswordResponse.Type switch
            {
                ForgotPasswordResponseType.Successful => Ok(forgotPasswordResponse),
                ForgotPasswordResponseType.UnexpectedError => InternalServerError(),
                _ => InternalServerError()
            };
        }

        [HttpPost("validate-reset-token")]
        public async Task<ActionResult<ValidatePasswordResetTokenResponse>> ValidateResetToken(
            ValidatePasswordResetTokenRequest validatePasswordResetTokenRequest,
            CancellationToken cancellationToken)
        {
            var validatePasswordResetTokenResponse =
                await authenticationService.ValidatePasswordResetTokenAsync(
                    validatePasswordResetTokenRequest,
                    cancellationToken);

            return validatePasswordResetTokenResponse.Type switch
            {
                ValidatePasswordResetTokenResponseType.Successful => Ok(validatePasswordResetTokenResponse),
                ValidatePasswordResetTokenResponseType.ValidationError => BadRequest(validatePasswordResetTokenResponse),
                ValidatePasswordResetTokenResponseType.UnexpectedFailure => InternalServerError(),
                _ => InternalServerError()
            };
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ResetPasswordResponse>> ResetPassword(
            ResetPasswordRequest resetPasswordRequest,
            CancellationToken cancellationToken)
        {
            var resetPasswordResponse =
                await authenticationService.ResetPasswordAsync(
                    resetPasswordRequest,
                    cancellationToken);

            return resetPasswordResponse.Type switch
            {
                ResetPasswordResponseType.Successful => Ok(resetPasswordResponse),
                ResetPasswordResponseType.ValidationError => BadRequest(resetPasswordResponse),
                ResetPasswordResponseType.UnexpectedFailure => InternalServerError(),
                _ => InternalServerError()
            };
        }

        private void AddRefreshTokenToCookies(string refreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = DateTime.UtcNow.AddHours(AuthenticationServiceConstants.RefreshTokenExpirationTimeInHours)
            };

            Response.Cookies.Append(
                SharedConstants.RefreshTokenCookieName,
                refreshToken,
                cookieOptions);
        }

        private string GetOriginFromRequestHeaders()
        {
            return Request.Headers[HeaderNames.Origin];
        }

        private string GetRefreshTokenFromCookies()
        {
            
            return Request.Cookies[SharedConstants.RefreshTokenCookieName];
        }
    }
}