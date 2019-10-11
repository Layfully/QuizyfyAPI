﻿using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Requests
{
    /// <summary>
    /// Request used to provide data for email verification.
    /// </summary>
    public class RecoverPasswordRequest
    {
        /// <summary>
        /// Password Recovery token generated by guid.
        /// </summary>
        [Required]
        public string Token { get; set; }

        [Required]
        public string Password { get; set; }
    }
}
