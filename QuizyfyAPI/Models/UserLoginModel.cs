using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Models
{
    /// <summary>
    /// A user credentials. Used for authentication
    /// </summary>
    public class UserLoginModel
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
    }
}
