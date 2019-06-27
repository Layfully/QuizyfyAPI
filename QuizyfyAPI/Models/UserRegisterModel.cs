﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Models
{
    /// <summary>
    /// A user with username, password, role and full name. Used for creating user.
    /// </summary>
    public class UserRegisterModel
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
    }
}