﻿using QuizyfyAPI.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Models
{
    public class UserRefreshModel
    {
        public RefreshToken RefreshToken { get; set; }
        public string JwtToken { get; set; }
    }
}
