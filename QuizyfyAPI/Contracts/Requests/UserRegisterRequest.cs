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

        /// <summary>
        /// Token used for checking if request was made by bot or real person.
        /// </summary>
        [Required]
        public string RecaptchaToken { get; set; }

        /// <summary>
        /// Email address on which we will send confirmation message.
        /// </summary>
        [EmailAddress]
        [Required]
        public string Email { get; set; }
    }
}
