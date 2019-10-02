namespace QuizyfyAPI.Contracts.Requests
{
    /// <summary>
    /// Request used for refreshing user JWT token.
    /// </summary>
    public class UserRefreshRequest
    {
        /// <summary>
        /// Refresh token which lasts for long time.
        /// </summary>
        public string RefreshToken { get; set; }
        /// <summary>
        /// Already expired previous JWT token which needs to be refreshed.
        /// </summary>
        public string JwtToken { get; set; }
    }
}
