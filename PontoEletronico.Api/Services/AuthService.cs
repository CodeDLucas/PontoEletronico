using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PontoEletronico.DTOs;
using PontoEletronico.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PontoEletronico.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IConfiguration configuration,
        ILogger<AuthService> logger)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<ApiResponseDto<AuthResponseDto>> LoginAsync(LoginRequestDto request)
    {
        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !user.IsActive)
            {
                return ApiResponseDto<AuthResponseDto>.ErrorResult("Email ou senha inválidos");
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                if (result.IsLockedOut)
                {
                    return ApiResponseDto<AuthResponseDto>.ErrorResult("Conta bloqueada devido a múltiplas tentativas de login incorretas");
                }

                return ApiResponseDto<AuthResponseDto>.ErrorResult("Email ou senha inválidos");
            }

            var token = await GenerateJwtToken(user);
            var userProfile = MapToUserProfile(user);

            var authResponse = new AuthResponseDto
            {
                Token = token.Token,
                Expiration = token.Expiration,
                User = userProfile
            };

            _logger.LogInformation("Login realizado com sucesso para o usuário {Email}", request.Email);

            return ApiResponseDto<AuthResponseDto>.SuccessResult(authResponse, "Login realizado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar login para o usuário {Email}", request.Email);
            return ApiResponseDto<AuthResponseDto>.ErrorResult("Erro interno do servidor");
        }
    }

    public async Task<ApiResponseDto<AuthResponseDto>> RegisterAsync(RegisterRequestDto request)
    {
        try
        {
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return ApiResponseDto<AuthResponseDto>.ErrorResult("Email já está em uso");
            }

            if (!string.IsNullOrEmpty(request.EmployeeCode))
            {
                var existingEmployeeCode = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.EmployeeCode == request.EmployeeCode);
                if (existingEmployeeCode != null)
                {
                    return ApiResponseDto<AuthResponseDto>.ErrorResult("Código do funcionário já está em uso");
                }
            }

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                FullName = request.FullName,
                EmployeeCode = string.IsNullOrWhiteSpace(request.EmployeeCode) ? null : request.EmployeeCode,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return ApiResponseDto<AuthResponseDto>.ErrorResult("Erro ao criar usuário", errors);
            }

            var token = await GenerateJwtToken(user);
            var userProfile = MapToUserProfile(user);

            var authResponse = new AuthResponseDto
            {
                Token = token.Token,
                Expiration = token.Expiration,
                User = userProfile
            };

            _logger.LogInformation("Usuário {Email} registrado com sucesso", request.Email);

            return ApiResponseDto<AuthResponseDto>.SuccessResult(authResponse, "Usuário registrado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao registrar usuário {Email}", request.Email);
            return ApiResponseDto<AuthResponseDto>.ErrorResult("Erro interno do servidor");
        }
    }

    public async Task<ApiResponseDto<bool>> LogoutAsync(string userId)
    {
        try
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return ApiResponseDto<bool>.ErrorResult("Usuário não encontrado");
            }

            await _signInManager.SignOutAsync();

            _logger.LogInformation("Logout realizado com sucesso para o usuário {UserId}", userId);

            return ApiResponseDto<bool>.SuccessResult(true, "Logout realizado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao realizar logout para o usuário {UserId}", userId);
            return ApiResponseDto<bool>.ErrorResult("Erro interno do servidor");
        }
    }

    public async Task<ApiResponseDto<AuthResponseDto>> RefreshTokenAsync(string token)
    {
        try
        {
            var principal = GetPrincipalFromExpiredToken(token);
            if (principal == null)
            {
                return ApiResponseDto<AuthResponseDto>.ErrorResult("Token inválido");
            }

            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return ApiResponseDto<AuthResponseDto>.ErrorResult("Token inválido");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null || !user.IsActive)
            {
                return ApiResponseDto<AuthResponseDto>.ErrorResult("Usuário não encontrado ou inativo");
            }

            var newToken = await GenerateJwtToken(user);
            var userProfile = MapToUserProfile(user);

            var authResponse = new AuthResponseDto
            {
                Token = newToken.Token,
                Expiration = newToken.Expiration,
                User = userProfile
            };

            return ApiResponseDto<AuthResponseDto>.SuccessResult(authResponse, "Token renovado com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao renovar token");
            return ApiResponseDto<AuthResponseDto>.ErrorResult("Erro interno do servidor");
        }
    }

    private async Task<(string Token, DateTime Expiration)> GenerateJwtToken(ApplicationUser user)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];
        var issuer = jwtSettings["Issuer"];
        var audience = jwtSettings["Audience"];
        var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"]!);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName!),
            new(ClaimTypes.Email, user.Email!),
            new("FullName", user.FullName),
            new("EmployeeCode", user.EmployeeCode ?? ""),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        var roles = await _userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiration = DateTime.UtcNow.AddMinutes(expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiration,
            signingCredentials: credentials
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
    }

    private ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"];

        var tokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false,
            ValidateIssuer = false,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey!)),
            ValidateLifetime = false
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

        if (securityToken is not JwtSecurityToken jwtSecurityToken ||
            !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new SecurityTokenException("Token inválido");
        }

        return principal;
    }

    private static UserProfileDto MapToUserProfile(ApplicationUser user)
    {
        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email!,
            EmployeeCode = user.EmployeeCode,
            CreatedAt = user.CreatedAt,
            IsActive = user.IsActive
        };
    }
}