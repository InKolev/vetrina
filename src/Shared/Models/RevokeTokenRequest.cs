using Vetrina.Shared.Constants;

namespace Vetrina.Shared.Models
{
    public class RevokeTokenRequest
    {
        public int UserId { get; set; }

        public string RefreshToken { get; set; }

        public string CallerIpAddress { get; set; }

        public RevokeTokenRequest MaskSensitiveData()
        {
            return new RevokeTokenRequest
            {
                UserId = this.UserId,
                RefreshToken = LoggingConstants.MaskedValue,
                CallerIpAddress = LoggingConstants.MaskedValue
            };
        }
    }
}