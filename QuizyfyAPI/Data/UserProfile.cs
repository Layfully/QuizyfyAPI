using AutoMapper;
using QuizyfyAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<RefreshToken, RefreshToken>().ForMember(m => m.User, opt => opt.Ignore());
            CreateMap<User, UserModel>().ReverseMap();
            CreateMap<User, UserRegisterModel>().ReverseMap();
            CreateMap<UserModel, UserRegisterModel>().ReverseMap();
            CreateMap<User, UserLoginModel>().ReverseMap();
            CreateMap<UserModel, UserLoginModel>().ReverseMap();
        }
    }
}
