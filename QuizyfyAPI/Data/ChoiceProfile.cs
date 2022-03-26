using AutoMapper;
using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;

namespace QuizyfyAPI.Data;
public class ChoiceProfile : Profile
{
    public ChoiceProfile()
    {
        CreateMap<Choice, ChoiceCreateRequest>().ReverseMap();

        CreateMap<Choice, ChoiceResponse>().ReverseMap();
    }
}
