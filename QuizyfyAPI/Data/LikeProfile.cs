using AutoMapper;
using QuizyfyAPI.Contracts.Responses;

namespace QuizyfyAPI.Data
{
    public class LikeProfile : Profile
    {
        public LikeProfile()
        {
            CreateMap<Like, LikeResponse>().ReverseMap();
        }
    }
}
