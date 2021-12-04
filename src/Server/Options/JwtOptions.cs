using System;

namespace Vetrina.Server.Options
{
    public class JwtOptions
    {
        public string Issuer { get; set; }

        public string Audience { get; set; }

        public TimeSpan ExpirationTime { get; set; }

        /// <summary>
        /// The secret used for signing JWTs.
        /// </summary>
        public string Secret { get; set; }

        /// <summary>
        /// Refresh token time to live (in days).
        /// Inactive tokens are automatically deleted from the database after this time expires.
        /// </summary>
        public int RefreshTokenTimeToLive { get; set; }
    }
}
