namespace QuizyfyAPI.Contracts.Requests;
/// <summary>
/// A user credentials. Used for authentication
/// </summary>
public class UserLoginRequest
{
    /// <summary>
    /// User name.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// User password.
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// Token for checking if request is made by bot or real person.
    /// </summary>
    public string RecaptchaToken { get; set; } = null!;
}
