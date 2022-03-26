using AutoMapper;
using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;

namespace QuizyfyAPI.Data;
public class QuizProfile : Profile
{
    public QuizProfile()
    {
        CreateMap<Quiz, QuizResponse>().ForMember(quiz => quiz.ImageUrl, opt => opt.MapFrom(src => src.Image.ImageUrl)).ReverseMap();
        CreateMap<Quiz, QuizCreateRequest>().ReverseMap();
    }
}
