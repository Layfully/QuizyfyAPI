using AutoMapper;
using QuizyfyAPI.Contracts.Responses;

namespace QuizyfyAPI.Data;
public class ImageProfile : Profile
{
    public ImageProfile()
    {
        CreateMap<Image, ImageResponse>().ReverseMap();
    }
}
