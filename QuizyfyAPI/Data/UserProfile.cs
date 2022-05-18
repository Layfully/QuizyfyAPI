using AutoMapper;
using QuizyfyAPI.Contracts.Requests;
using QuizyfyAPI.Contracts.Responses;

namespace QuizyfyAPI.Data;
public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<RefreshToken, RefreshToken>().ForMember(m => m.User, opt => opt.Ignore());
        CreateMap<User, UserResponse>().ReverseMap();
        CreateMap<User, UserRegisterRequest>().ReverseMap();
        CreateMap<UserResponse, UserRegisterRequest>().ReverseMap();
        CreateMap<User, UserLoginRequest>().ReverseMap();
        CreateMap<UserResponse, UserLoginRequest>().ReverseMap();

        CreateMap<UserUpdateRequest, User>().ForAllMembers(opt => opt.Condition((src, dest, sourceMember) => sourceMember != null));
    }
}
