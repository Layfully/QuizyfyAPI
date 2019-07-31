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
        Task<ObjectResult<UserModel>> Login(UserLoginModel model);
        Task<BasicResult> Register(UserRegisterModel model);
        Task<ObjectResult<UserModel>> Update(int userId, UserRegisterModel model);
        Task<DetailedResult> Delete(int userId);
        Task<ObjectResult<UserModel>> RefreshTokenAsync(UserRefreshModel model);
        Task<User> RequestToken(User user);
    }
}
