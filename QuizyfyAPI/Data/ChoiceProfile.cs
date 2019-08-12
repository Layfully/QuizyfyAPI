using AutoMapper;
using QuizyfyAPI.Contracts.Responses;

namespace QuizyfyAPI.Data
{
    public class ChoiceProfile : Profile
    {
        public ChoiceProfile()
        {
            CreateMap<Choice, ChoiceResponse>().ReverseMap();
        }
    }
}
