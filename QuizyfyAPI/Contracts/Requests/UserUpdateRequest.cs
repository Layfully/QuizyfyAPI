namespace QuizyfyAPI.Contracts.Requests
{
    /// <summary>
    /// A user with username, password, role and full name. Used for creating user.
    /// </summary>
    public class UserUpdateRequest
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
        public string Username { get; set; }
        /// <summary>
        /// User password.
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// User role. (Can be either admin or user)
        /// </summary>
        public string Role { get; set; }
    }
}
