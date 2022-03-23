using AutoMapper;
using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;

namespace QuizyfyAPI.Data;
public class QuestionProfile : Profile
{
    public QuestionProfile()
    {
        CreateMap<Question, QuestionResponse>()
            .ForMember(question => question.ImageUrl, opt => opt.MapFrom(src => src.Image.ImageUrl))
            .ReverseMap()
            .ForMember(question => question.QuizId, opt => opt.Ignore())
            .ForMember(question => question.Choices, opt => opt.Ignore());

        CreateMap<Question, QuestionCreateRequest>()
            .ReverseMap()
            .ForMember(question => question.QuizId, opt => opt.Ignore())
            .ForMember(question => question.Choices, opt => opt.Ignore());
    }
}
