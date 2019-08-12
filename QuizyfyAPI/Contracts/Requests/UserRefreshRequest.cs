namespace QuizyfyAPI.Contracts.Requests
{
    public class UserRefreshRequest
    {
        public string RefreshToken { get; set; }
        public string JwtToken { get; set; }
    }
}
