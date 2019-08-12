using AutoMapper;
using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;

namespace QuizyfyAPI.Data
{
    public class QuizProfile : Profile
    {
        public QuizProfile()
        {
            CreateMap<Quiz, QuizResponse>().ReverseMap();

            CreateMap<Quiz, QuizCreateRequest>().ReverseMap();
        }
    }
}
