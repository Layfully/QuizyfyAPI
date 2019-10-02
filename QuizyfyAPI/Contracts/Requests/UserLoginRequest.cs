using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests
{
    /// <summary>
    /// A user credentials. Used for authentication
    /// </summary>
    public class UserLoginRequest
    {
        /// <summary>
        /// User name.
        /// </summary>
        [Required]
        public string Username { get; set; }
        /// <summary>
        /// User password.
        /// </summary>
        [Required]
        public string Password { get; set; }
        /// <summary>
        /// Token for checking if request is made by bot or real person.
        /// </summary>
        [Required]
        public string RecaptchaToken { get; set; }
    }
}
