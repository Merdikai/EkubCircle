using AutoMapper;
using EkubCircle.Application.Commands.Auth;
using EkubCircle.Application.DTOs.Auth;
using EkubCircle.Application.DTOs.Circles;
using EkubCircle.Application.DTOs.Payments;
using EkubCircle.Domain.Entities;

namespace EkubCircle.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<RegisterRequestDto, RegisterUserCommand>();
        CreateMap<LoginRequestDto, LoginUserCommand>();

        CreateMap<User, UserDto>();
        CreateMap<Circle, CircleDto>()
            .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.FullName : string.Empty))
            .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.Members.Count));

        CreateMap<CircleMember, CircleMemberDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : string.Empty))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : string.Empty));

        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Member != null && src.Member.User != null ? src.Member.User.FullName : string.Empty))
            .ForMember(dest => dest.RoundNumber, opt => opt.MapFrom(src => src.Round != null ? src.Round.RoundNumber : 0));
    }
}
