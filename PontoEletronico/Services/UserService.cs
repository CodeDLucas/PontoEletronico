using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PontoEletronico.DTOs;
using PontoEletronico.Models;

namespace PontoEletronico.Services;

public class UserService : IUserService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserService> _logger;

    public UserService(UserManager<ApplicationUser> userManager, ILogger<UserService> logger)
    {
        _userManager = userManager;
        _logger = logger;
    }

    public async Task<ApiResponseDto<UserProfileDto>> GetUserProfileAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponseDto<UserProfileDto>.ErrorResult("Usuário não encontrado");
            }

            var userProfile = new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                EmployeeCode = user.EmployeeCode,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };

            return ApiResponseDto<UserProfileDto>.SuccessResult(userProfile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar perfil do usuário {UserId}", userId);
            return ApiResponseDto<UserProfileDto>.ErrorResult("Erro interno do servidor");
        }
    }

    public async Task<ApiResponseDto<UserProfileDto>> UpdateUserProfileAsync(string userId, UserUpdateDto request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponseDto<UserProfileDto>.ErrorResult("Usuário não encontrado");
            }

            if (user.Email != request.Email)
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Email);
                if (existingUser != null && existingUser.Id != userId)
                {
                    return ApiResponseDto<UserProfileDto>.ErrorResult("Email já está em uso por outro usuário");
                }

                user.Email = request.Email;
                user.UserName = request.Email;
                user.NormalizedEmail = request.Email.ToUpperInvariant();
                user.NormalizedUserName = request.Email.ToUpperInvariant();
            }

            user.FullName = request.FullName;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponseDto<UserProfileDto>.ErrorResult("Erro ao atualizar usuário", errors);
            }

            var userProfile = new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email!,
                EmployeeCode = user.EmployeeCode,
                CreatedAt = user.CreatedAt,
                IsActive = user.IsActive
            };

            _logger.LogInformation("Perfil do usuário {UserId} atualizado com sucesso", userId);

            return ApiResponseDto<UserProfileDto>.SuccessResult(userProfile, "Perfil atualizado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar perfil do usuário {UserId}", userId);
            return ApiResponseDto<UserProfileDto>.ErrorResult("Erro interno do servidor");
        }
    }

    public async Task<ApiResponseDto<bool>> ChangePasswordAsync(string userId, ChangePasswordDto request)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponseDto<bool>.ErrorResult("Usuário não encontrado");
            }

            var result = await _userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponseDto<bool>.ErrorResult("Erro ao alterar senha", errors);
            }

            _logger.LogInformation("Senha do usuário {UserId} alterada com sucesso", userId);

            return ApiResponseDto<bool>.SuccessResult(true, "Senha alterada com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao alterar senha do usuário {UserId}", userId);
            return ApiResponseDto<bool>.ErrorResult("Erro interno do servidor");
        }
    }

    public async Task<ApiResponseDto<bool>> DeactivateUserAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponseDto<bool>.ErrorResult("Usuário não encontrado");
            }

            user.IsActive = false;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponseDto<bool>.ErrorResult("Erro ao desativar usuário", errors);
            }

            _logger.LogInformation("Usuário {UserId} desativado com sucesso", userId);

            return ApiResponseDto<bool>.SuccessResult(true, "Usuário desativado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desativar usuário {UserId}", userId);
            return ApiResponseDto<bool>.ErrorResult("Erro interno do servidor");
        }
    }

    public async Task<ApiResponseDto<PagedResponseDto<UserListDto>>> GetUsersAsync(int page = 1, int pageSize = 10, string? search = null)
    {
        try
        {
            var query = _userManager.Users.AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(u => u.FullName.Contains(search) ||
                                       u.Email!.Contains(search) ||
                                       u.EmployeeCode.Contains(search));
            }

            var totalCount = await query.CountAsync();

            var users = await query
                .OrderBy(u => u.FullName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(u => new UserListDto
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email!,
                    EmployeeCode = u.EmployeeCode,
                    IsActive = u.IsActive,
                    CreatedAt = u.CreatedAt
                })
                .ToListAsync();

            var pagedResponse = new PagedResponseDto<UserListDto>(users, totalCount, page, pageSize);

            return ApiResponseDto<PagedResponseDto<UserListDto>>.SuccessResult(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar lista de usuários");
            return ApiResponseDto<PagedResponseDto<UserListDto>>.ErrorResult("Erro interno do servidor");
        }
    }
}