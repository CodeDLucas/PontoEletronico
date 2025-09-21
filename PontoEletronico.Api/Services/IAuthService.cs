using PontoEletronico.DTOs;

namespace PontoEletronico.Services;

public interface IAuthService
{
    Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto request);
    Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto request);
    Task<ApiResponseDto<bool>> LogoutAsync(string userId);
    Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(string token);
}