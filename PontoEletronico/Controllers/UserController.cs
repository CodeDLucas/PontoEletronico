using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PontoEletronico.DTOs;
using PontoEletronico.Services;
using System.Security.Claims;

namespace PontoEletronico.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Obtém o perfil do usuário atual
    /// </summary>
    /// <returns>Dados do perfil do usuário</returns>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponseDto<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<UserProfileDto>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ApiResponseDto<UserProfileDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponseDto<UserProfileDto>.ErrorResult("Usuário não autenticado"));
        }

        var result = await _userService.GetUserProfileAsync(userId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Atualiza o perfil do usuário atual
    /// </summary>
    /// <param name="request">Dados para atualização</param>
    /// <returns>Perfil atualizado</returns>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponseDto<UserProfileDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<UserProfileDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<UserProfileDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile([FromBody] UserUpdateDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponseDto<UserProfileDto>.ErrorResult(errors));
        }

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponseDto<UserProfileDto>.ErrorResult("Usuário não autenticado"));
        }

        var result = await _userService.UpdateUserProfileAsync(userId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Altera a senha do usuário atual
    /// </summary>
    /// <param name="request">Dados para alteração de senha</param>
    /// <returns>Confirmação da alteração</returns>
    [HttpPost("change-password")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponseDto<bool>.ErrorResult(errors));
        }

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponseDto<bool>.ErrorResult("Usuário não autenticado"));
        }

        var result = await _userService.ChangePasswordAsync(userId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Desativa a conta do usuário atual
    /// </summary>
    /// <returns>Confirmação da desativação</returns>
    [HttpPost("deactivate")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeactivateAccount()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponseDto<bool>.ErrorResult("Usuário não autenticado"));
        }

        var result = await _userService.DeactivateUserAsync(userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Lista todos os usuários (apenas para administradores)
    /// </summary>
    /// <param name="page">Página</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <param name="search">Termo de busca</param>
    /// <returns>Lista paginada de usuários</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<UserListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<UserListDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null)
    {
        var result = await _userService.GetUsersAsync(page, pageSize, search);

        return Ok(result);
    }

    /// <summary>
    /// Obtém informações básicas do usuário atual
    /// </summary>
    /// <returns>Informações do usuário logado</returns>
    [HttpGet("me")]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<object>), StatusCodes.Status401Unauthorized)]
    public IActionResult GetCurrentUserInfo()
    {
        var userId = GetCurrentUserId();
        var userName = User.FindFirst(ClaimTypes.Name)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var fullName = User.FindFirst("FullName")?.Value;
        var employeeCode = User.FindFirst("EmployeeCode")?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponseDto<object>.ErrorResult("Usuário não autenticado"));
        }

        var userInfo = new
        {
            Id = userId,
            UserName = userName,
            Email = email,
            FullName = fullName,
            EmployeeCode = employeeCode
        };

        return Ok(ApiResponseDto<object>.SuccessResult(userInfo, "Informações do usuário obtidas com sucesso"));
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}