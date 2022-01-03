using Vetrina.Shared.Constants;

namespace Vetrina.Server.Models
{
    public class RefreshTokenRequest
    {
        public int UserId { get; set; }

        public string RefreshToken { get; set; }

        public string CallerIpAddress { get; set; }

        public RefreshTokenRequest MaskSensitiveData()
        {
            return new RefreshTokenRequest
            {
                UserId = this.UserId,
                RefreshToken = LoggingConstants.MaskedValue,
                CallerIpAddress = LoggingConstants.MaskedValue
            };
        }
    }
}