using AutoMapper;
using QuizyfyAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace QuizyfyAPI.Data
{
    public class QuestionProfile : Profile
    {
        public QuestionProfile()
        {
            CreateMap<Question, QuestionModel>()
                .ReverseMap()
                .ForMember(question => question.QuizId, opt => opt.Ignore())
                .ForMember(question => question.Choices, opt => opt.Ignore());
        }
    }
}
