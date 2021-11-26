namespace Vetrina.Server.Options
{
    public class JwtOptions
    {
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
