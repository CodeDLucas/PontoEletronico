using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PontoEletronico.DTOs;
using PontoEletronico.Services;
using System.Security.Claims;

namespace PontoEletronico.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Realiza login do usuário
    /// </summary>
    /// <param name="request">Dados de login</param>
    /// <returns>Token de autenticação e dados do usuário</returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponseDto<AuthResponseDto>.ErrorResult(errors));
        }

        var result = await _authService.LoginAsync(request);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Registra um novo usuário
    /// </summary>
    /// <param name="request">Dados de registro</param>
    /// <returns>Token de autenticação e dados do usuário criado</returns>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponseDto<AuthResponseDto>.ErrorResult(errors));
        }

        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(Register), result);
    }

    /// <summary>
    /// Realiza logout do usuário
    /// </summary>
    /// <returns>Confirmação de logout</returns>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponseDto<bool>.ErrorResult("Usuário não autenticado"));
        }

        var result = await _authService.LogoutAsync(userId);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Renova o token de autenticação
    /// </summary>
    /// <param name="request">Token atual</param>
    /// <returns>Novo token de autenticação</returns>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<AuthResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponseDto<AuthResponseDto>.ErrorResult(errors));
        }

        var result = await _authService.RefreshTokenAsync(request.Token);

        if (!result.Success)
        {
            return Unauthorized(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Verifica se o token é válido
    /// </summary>
    /// <returns>Status da autenticação</returns>
    [HttpGet("verify")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status401Unauthorized)]
    public IActionResult VerifyToken()
    {
        return Ok(ApiResponseDto<bool>.SuccessResult(true, "Token válido"));
    }
}