using EkubCircle.API.DTOs.Circles;

namespace EkubCircle.API.Services;

public interface ICircleService
{
    Task<CircleDetailDto> CreateCircleAsync(int userId, CreateCircleRequestDto request);
    Task<List<CircleDto>> GetUserCirclesAsync(int userId, string? status = null);
    Task<CircleDetailDto> GetCircleDetailsAsync(int circleId, int userId);
    Task<CircleMemberDto> AddMemberAsync(int circleId, int organizerUserId, AddMemberRequestDto request);
    Task RemoveMemberAsync(int circleId, int organizerUserId, int memberId);
    Task<CircleDetailDto> StartCircleAsync(int circleId, int organizerUserId);
}
