using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PontoEletronico.DTOs;
using PontoEletronico.Services;
using System.Security.Claims;

namespace PontoEletronico.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimeRecordController : ControllerBase
{
    private readonly ITimeRecordService _timeRecordService;
    private readonly ILogger<TimeRecordController> _logger;

    public TimeRecordController(ITimeRecordService timeRecordService, ILogger<TimeRecordController> logger)
    {
        _timeRecordService = timeRecordService;
        _logger = logger;
    }

    /// <summary>
    /// Cria uma nova marcação de ponto
    /// </summary>
    /// <param name="request">Dados da marcação</param>
    /// <returns>Marcação criada</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseDto<TimeRecordResponseDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponseDto<TimeRecordResponseDto>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseDto<TimeRecordResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> CreateTimeRecord([FromBody] TimeRecordCreateDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponseDto<TimeRecordResponseDto>.ErrorResult(errors));
        }

        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponseDto<TimeRecordResponseDto>.ErrorResult("Usuário não autenticado"));
        }

        var result = await _timeRecordService.CreateTimeRecordAsync(userId, request);

        if (!result.Success)
        {
            return BadRequest(result);
        }

        return CreatedAtAction(nameof(GetTimeRecordById), new { id = result.Data!.Id }, result);
    }

    /// <summary>
    /// Obtém uma marcação específica por ID
    /// </summary>
    /// <param name="id">ID da marcação</param>
    /// <returns>Dados da marcação</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseDto<TimeRecordResponseDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<TimeRecordResponseDto>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<TimeRecordResponseDto>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTimeRecordById(int id)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponseDto<TimeRecordResponseDto>.ErrorResult("Usuário não autenticado"));
        }

        var result = await _timeRecordService.GetTimeRecordByIdAsync(id, userId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Obtém as marcações do usuário com filtros
    /// </summary>
    /// <param name="startDate">Data de início</param>
    /// <param name="endDate">Data de fim</param>
    /// <param name="type">Tipo de marcação</param>
    /// <param name="page">Página</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <returns>Lista de marcações paginada</returns>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<TimeRecordListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<TimeRecordListDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTimeRecords(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] Models.TimeRecordType? type = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponseDto<PagedResponseDto<TimeRecordListDto>>.ErrorResult("Usuário não autenticado"));
        }

        var filter = new TimeRecordFilterDto
        {
            StartDate = startDate,
            EndDate = endDate,
            Type = type,
            Page = page,
            PageSize = pageSize
        };

        var result = await _timeRecordService.GetUserTimeRecordsAsync(userId, filter);

        return Ok(result);
    }

    /// <summary>
    /// Obtém resumo das marcações agrupadas por dia
    /// </summary>
    /// <param name="startDate">Data de início</param>
    /// <param name="endDate">Data de fim</param>
    /// <param name="page">Página</param>
    /// <param name="pageSize">Tamanho da página</param>
    /// <returns>Resumo das marcações por dia</returns>
    [HttpGet("summary")]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<TimeRecordSummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<PagedResponseDto<TimeRecordSummaryDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTimeRecordsSummary(
        [FromQuery] DateTime? startDate = null,
        [FromQuery] DateTime? endDate = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponseDto<PagedResponseDto<TimeRecordSummaryDto>>.ErrorResult("Usuário não autenticado"));
        }

        var filter = new TimeRecordFilterDto
        {
            StartDate = startDate,
            EndDate = endDate,
            Page = page,
            PageSize = pageSize
        };

        var result = await _timeRecordService.GetUserTimeRecordsSummaryAsync(userId, filter);

        return Ok(result);
    }

    /// <summary>
    /// Obtém as marcações do dia atual
    /// </summary>
    /// <returns>Marcações do dia</returns>
    [HttpGet("today")]
    [ProducesResponseType(typeof(ApiResponseDto<List<TimeRecordListDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<List<TimeRecordListDto>>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetTodayTimeRecords()
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponseDto<List<TimeRecordListDto>>.ErrorResult("Usuário não autenticado"));
        }

        var result = await _timeRecordService.GetTodayTimeRecordsAsync(userId);

        return Ok(result);
    }

    /// <summary>
    /// Remove uma marcação de ponto
    /// </summary>
    /// <param name="id">ID da marcação</param>
    /// <returns>Confirmação da remoção</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponseDto<bool>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> DeleteTimeRecord(int id)
    {
        var userId = GetCurrentUserId();
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponseDto<bool>.ErrorResult("Usuário não autenticado"));
        }

        var result = await _timeRecordService.DeleteTimeRecordAsync(id, userId);

        if (!result.Success)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    private string? GetCurrentUserId()
    {
        return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }
}