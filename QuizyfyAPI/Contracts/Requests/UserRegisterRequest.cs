using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests
{
    /// <summary>
    /// A user with username, password, role and full name. Used for creating user.
    /// </summary>
    public class UserRegisterRequest
    {
        /// <summary>
        /// First name of the user owner.
        /// </summary>
        public string FirstName { get; set; }
        /// <summary>
        /// Last name of the user owner.
        /// </summary>
        public string LastName { get; set; }
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
        /// User role. (Can be either admin or user)
        /// </summary>
        [Required]
        public string Role { get; set; }
        [Required]
        public string RecaptchaToken { get; set; }
    }
}
