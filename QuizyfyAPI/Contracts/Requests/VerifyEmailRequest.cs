﻿using System.ComponentModel.DataAnnotations;

namespace QuizyfyAPI.Contracts.Responses
{
    /// <summary>
    /// Request used to provide data for email verification.
    /// </summary>
    public class VerifyEmailRequest
    {
        /// <summary>
        /// Email verification token generated by guid.
        /// </summary>
        [Required]
        public string Token { get; set; }
    }
}
