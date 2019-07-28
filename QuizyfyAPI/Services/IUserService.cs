using Microsoft.AspNetCore.Mvc;
using QuizyfyAPI.Data;
using QuizyfyAPI.Domain;
using QuizyfyAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Services
{
    public interface IUserService
    {
        Task<AuthenticationResult> RefreshTokenAsync(UserRefreshModel model);
        Task<User> RequestToken(User user);
    }
}
