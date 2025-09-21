using PontoEletronico.DTOs;

namespace PontoEletronico.Services;

public interface IUserService
{
    Task<ApiResponseDto<UserProfileDto>> GetUserProfileAsync(string userId);
    Task<ApiResponseDto<UserProfileDto>> UpdateUserProfileAsync(string userId, UserUpdateDto request);
    Task<ApiResponseDto<bool>> ChangePasswordAsync(string userId, ChangePasswordDto request);
    Task<ApiResponseDto<bool>> DeactivateUserAsync(string userId);
    Task<ApiResponseDto<PagedResponseDto<UserListDto>>> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null);
}