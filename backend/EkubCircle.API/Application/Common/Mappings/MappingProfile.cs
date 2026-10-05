using AutoMapper;
using EkubCircle.API.Application.Commands.Auth;
using EkubCircle.API.Application.Commands.Circles;
using EkubCircle.API.Application.Commands.Payments;
using EkubCircle.API.DTOs.Auth;
using EkubCircle.API.DTOs.Circles;
using EkubCircle.API.DTOs.Payments;
using EkubCircle.API.DTOs.Rounds;
using EkubCircle.API.Models;

namespace EkubCircle.API.Application.Common.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Auth DTO mappings
        CreateMap<RegisterRequestDto, RegisterUserCommand>();
        CreateMap<LoginRequestDto, LoginUserCommand>();
        CreateMap<User, UserDto>();

        // Circle DTO mappings
        CreateMap<CreateCircleRequestDto, CreateCircleCommand>();
        CreateMap<AddMemberRequestDto, AddMemberCommand>();
        CreateMap<Circle, CircleDto>()
            .ForMember(dest => dest.CreatedByUserName, opt => opt.MapFrom(src => src.CreatedByUser != null ? src.CreatedByUser.FullName : "Unknown"))
            .ForMember(dest => dest.MemberCount, opt => opt.MapFrom(src => src.Members.Count));
        CreateMap<CircleMember, CircleMemberDto>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.User != null ? src.User.FullName : "Unknown"))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.User != null ? src.User.Email : "Unknown"));

        // Payment DTO mappings
        CreateMap<RecordPaymentRequestDto, RecordPaymentCommand>();
        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.MemberName, opt => opt.MapFrom(src => src.Member != null && src.Member.User != null ? src.Member.User.FullName : "Unknown"))
            .ForMember(dest => dest.RoundNumber, opt => opt.MapFrom(src => src.Round != null ? src.Round.RoundNumber : 0));

        // Round DTO mappings
        CreateMap<Round, RoundSummaryDto>()
            .ForMember(dest => dest.ReceiverName, opt => opt.MapFrom(src => src.ReceiverMember != null && src.ReceiverMember.User != null ? src.ReceiverMember.User.FullName : "Unknown"));
    }
}
