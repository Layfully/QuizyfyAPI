using AutoMapper;
using QuizyfyAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public class QuizProfile : Profile
    {
        public QuizProfile()
        {
            CreateMap<Quiz, QuizModel>().ReverseMap();

            CreateMap<Quiz, QuizCreateModel>().ReverseMap();
        }
    }
}
