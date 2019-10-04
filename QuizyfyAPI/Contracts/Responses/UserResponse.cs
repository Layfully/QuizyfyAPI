using QuizyfyAPI.Data;
using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Responses
{
    /// <summary>
    /// A user with username, password, role, token and full name.
    /// </summary>
    public class UserResponse
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
        /// JWT Token used for authentication
        /// </summary>
        public string JwtToken { get; set; }

        /// <summary>
        /// Token used for refreshing JWT token.
        /// </summary>
        public RefreshToken RefreshToken { get; set; }
    }
}
