using Microsoft.EntityFrameworkCore;
using PontoEletronico.Data;
using PontoEletronico.DTOs;
using PontoEletronico.Models;

namespace PontoEletronico.Services;

public class TimeRecordService : ITimeRecordService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<TimeRecordService> _logger;

    public TimeRecordService(ApplicationDbContext context, ILogger<TimeRecordService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ApiResponseDto<TimeRecordResponseDto>> CreateTimeRecordAsync(string userId, TimeRecordCreateDto request)
    {
        try
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null || !user.IsActive)
            {
                return ApiResponseDto<TimeRecordResponseDto>.ErrorResult("Usuário não encontrado ou inativo");
            }

            var timestamp = request.Timestamp ?? DateTime.UtcNow;

            var validationResult = await ValidateTimeRecord(userId, request.Type, timestamp);
            if (!validationResult.IsValid)
            {
                return ApiResponseDto<TimeRecordResponseDto>.ErrorResult(validationResult.ErrorMessage!);
            }

            var timeRecord = new TimeRecord
            {
                UserId = userId,
                Timestamp = timestamp,
                Type = request.Type,
                Description = request.Description,
                CreatedAt = DateTime.UtcNow
            };

            _context.TimeRecords.Add(timeRecord);
            await _context.SaveChangesAsync();

            var response = MapToTimeRecordResponse(timeRecord);

            _logger.LogInformation("Marcação de ponto criada com sucesso. UserId: {UserId}, Type: {Type}, Timestamp: {Timestamp}",
                userId, request.Type, timestamp);

            return ApiResponseDto<TimeRecordResponseDto>.SuccessResult(response, "Marcação de ponto registrada com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar marcação de ponto para o usuário {UserId}", userId);
            return ApiResponseDto<TimeRecordResponseDto>.ErrorResult("Erro interno do servidor");
        }
    }

    public async Task<ApiResponseDto<List<TimeRecordListDto>>> GetUserTimeRecordsAsync(string userId, TimeRecordFilterDto filter)
    {
        try
        {
            var query = _context.TimeRecords
                .Where(tr => tr.UserId == userId)
                .AsQueryable();

            if (filter.StartDate.HasValue)
            {
                query = query.Where(tr => tr.Timestamp.Date >= filter.StartDate.Value.Date);
            }

            if (filter.EndDate.HasValue)
            {
                query = query.Where(tr => tr.Timestamp.Date <= filter.EndDate.Value.Date);
            }

            if (filter.Type.HasValue)
            {
                query = query.Where(tr => tr.Type == filter.Type.Value);
            }

            var timeRecords = await query
                .OrderByDescending(tr => tr.Timestamp)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(tr => new TimeRecordListDto
                {
                    Id = tr.Id,
                    Timestamp = tr.Timestamp,
                    Type = tr.Type,
                    TypeDescription = GetTypeDescription(tr.Type),
                    Description = tr.Description
                })
                .ToListAsync();

            return ApiResponseDto<List<TimeRecordListDto>>.SuccessResult(timeRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar marcações de ponto para o usuário {UserId}", userId);
            return ApiResponseDto<List<TimeRecordListDto>>.ErrorResult("Erro interno do servidor");
        }
    }

    public async Task<ApiResponseDto<PagedResponseDto<TimeRecordSummaryDto>>> GetUserTimeRecordsSummaryAsync(string userId, TimeRecordFilterDto filter)
    {
        try
        {
            var startDate = filter.StartDate?.Date ?? DateTime.UtcNow.Date.AddDays(-30);
            var endDate = filter.EndDate?.Date ?? DateTime.UtcNow.Date;

            var timeRecords = await _context.TimeRecords
                .Where(tr => tr.UserId == userId &&
                           tr.Timestamp.Date >= startDate &&
                           tr.Timestamp.Date <= endDate)
                .OrderByDescending(tr => tr.Timestamp)
                .ToListAsync();

            var groupedByDate = timeRecords
                .GroupBy(tr => tr.Timestamp.Date)
                .Select(g => new TimeRecordSummaryDto
                {
                    Date = g.Key,
                    Records = g.Select(tr => new TimeRecordListDto
                    {
                        Id = tr.Id,
                        Timestamp = tr.Timestamp,
                        Type = tr.Type,
                        TypeDescription = GetTypeDescription(tr.Type),
                        Description = tr.Description
                    }).OrderBy(tr => tr.Timestamp).ToList(),
                    TotalWorkedTime = CalculateWorkedTime(g.ToList()),
                    IsComplete = IsWorkDayComplete(g.ToList())
                })
                .OrderByDescending(s => s.Date)
                .ToList();

            var totalCount = groupedByDate.Count;
            var pagedData = groupedByDate
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .ToList();

            var pagedResponse = new PagedResponseDto<TimeRecordSummaryDto>(pagedData, totalCount, filter.Page, filter.PageSize);

            return ApiResponseDto<PagedResponseDto<TimeRecordSummaryDto>>.SuccessResult(pagedResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar resumo de marcações para o usuário {UserId}", userId);
            return ApiResponseDto<PagedResponseDto<TimeRecordSummaryDto>>.ErrorResult("Erro interno do servidor");
        }
    }

    public async Task<ApiResponseDto<TimeRecordResponseDto>> GetTimeRecordByIdAsync(int id, string userId)
    {
        try
        {
            var timeRecord = await _context.TimeRecords
                .FirstOrDefaultAsync(tr => tr.Id == id && tr.UserId == userId);

            if (timeRecord == null)
            {
                return ApiResponseDto<TimeRecordResponseDto>.ErrorResult("Marcação de ponto não encontrada");
            }

            var response = MapToTimeRecordResponse(timeRecord);

            return ApiResponseDto<TimeRecordResponseDto>.SuccessResult(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar marcação de ponto {Id} para o usuário {UserId}", id, userId);
            return ApiResponseDto<TimeRecordResponseDto>.ErrorResult("Erro interno do servidor");
        }
    }

    public async Task<ApiResponseDto<bool>> DeleteTimeRecordAsync(int id, string userId)
    {
        try
        {
            var timeRecord = await _context.TimeRecords
                .FirstOrDefaultAsync(tr => tr.Id == id && tr.UserId == userId);

            if (timeRecord == null)
            {
                return ApiResponseDto<bool>.ErrorResult("Marcação de ponto não encontrada");
            }

            _context.TimeRecords.Remove(timeRecord);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Marcação de ponto {Id} removida com sucesso para o usuário {UserId}", id, userId);

            return ApiResponseDto<bool>.SuccessResult(true, "Marcação de ponto removida com sucesso");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao remover marcação de ponto {Id} para o usuário {UserId}", id, userId);
            return ApiResponseDto<bool>.ErrorResult("Erro interno do servidor");
        }
    }

    public async Task<ApiResponseDto<List<TimeRecordListDto>>> GetTodayTimeRecordsAsync(string userId)
    {
        try
        {
            // Use UTC para obter o dia atual, mas compare apenas a data
            var utcNow = DateTime.UtcNow;
            var startOfDay = new DateTime(utcNow.Year, utcNow.Month, utcNow.Day, 0, 0, 0, DateTimeKind.Utc);
            var endOfDay = startOfDay.AddDays(1);

            var timeRecords = await _context.TimeRecords
                .Where(tr => tr.UserId == userId &&
                           tr.Timestamp >= startOfDay &&
                           tr.Timestamp < endOfDay)
                .OrderBy(tr => tr.Timestamp)
                .Select(tr => new TimeRecordListDto
                {
                    Id = tr.Id,
                    Timestamp = tr.Timestamp,
                    Type = tr.Type,
                    TypeDescription = GetTypeDescription(tr.Type),
                    Description = tr.Description
                })
                .ToListAsync();

            return ApiResponseDto<List<TimeRecordListDto>>.SuccessResult(timeRecords);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao buscar marcações de hoje para o usuário {UserId}", userId);
            return ApiResponseDto<List<TimeRecordListDto>>.ErrorResult("Erro interno do servidor");
        }
    }

    private async Task<(bool IsValid, string? ErrorMessage)> ValidateTimeRecord(string userId, TimeRecordType type, DateTime timestamp)
    {
        var today = timestamp.Date;
        var todayRecords = await _context.TimeRecords
            .Where(tr => tr.UserId == userId && tr.Timestamp.Date == today)
            .OrderBy(tr => tr.Timestamp)
            .ToListAsync();

        var lastRecord = todayRecords.LastOrDefault();

        switch (type)
        {
            case TimeRecordType.ClockIn:
                if (lastRecord?.Type == TimeRecordType.ClockIn)
                {
                    return (false, "Não é possível registrar entrada. Última marcação já foi uma entrada.");
                }
                break;

            case TimeRecordType.ClockOut:
                if (lastRecord == null || lastRecord.Type != TimeRecordType.ClockIn)
                {
                    return (false, "Não é possível registrar saída. É necessário registrar entrada primeiro.");
                }
                break;

            case TimeRecordType.BreakStart:
                if (lastRecord?.Type != TimeRecordType.ClockIn)
                {
                    return (false, "Não é possível iniciar pausa. É necessário estar trabalhando (ter registrado entrada).");
                }
                break;

            case TimeRecordType.BreakEnd:
                if (lastRecord?.Type != TimeRecordType.BreakStart)
                {
                    return (false, "Não é possível finalizar pausa. É necessário ter iniciado uma pausa primeiro.");
                }
                break;
        }

        var duplicateRecord = todayRecords
            .FirstOrDefault(tr => Math.Abs((tr.Timestamp - timestamp).TotalMinutes) < 1);

        if (duplicateRecord != null)
        {
            return (false, "Não é possível registrar marcação muito próxima a uma marcação existente.");
        }

        return (true, null);
    }

    private static TimeSpan? CalculateWorkedTime(List<TimeRecord> records)
    {
        if (records.Count < 2) return null;

        var orderedRecords = records.OrderBy(r => r.Timestamp).ToList();
        var totalTime = TimeSpan.Zero;
        DateTime? workStart = null;

        foreach (var record in orderedRecords)
        {
            switch (record.Type)
            {
                case TimeRecordType.ClockIn:
                    workStart = record.Timestamp;
                    break;

                case TimeRecordType.ClockOut:
                    if (workStart.HasValue)
                    {
                        totalTime = totalTime.Add(record.Timestamp - workStart.Value);
                        workStart = null;
                    }
                    break;

                case TimeRecordType.BreakStart:
                    if (workStart.HasValue)
                    {
                        totalTime = totalTime.Add(record.Timestamp - workStart.Value);
                        workStart = null;
                    }
                    break;

                case TimeRecordType.BreakEnd:
                    workStart = record.Timestamp;
                    break;
            }
        }

        return totalTime;
    }

    private static bool IsWorkDayComplete(List<TimeRecord> records)
    {
        if (records.Count == 0) return false;

        var lastRecord = records.OrderBy(r => r.Timestamp).Last();
        return lastRecord.Type == TimeRecordType.ClockOut;
    }

    private static string GetTypeDescription(TimeRecordType type)
    {
        return type switch
        {
            TimeRecordType.ClockIn => "Entrada",
            TimeRecordType.ClockOut => "Saída",
            TimeRecordType.BreakStart => "Início do Intervalo",
            TimeRecordType.BreakEnd => "Fim do Intervalo",
            _ => "Desconhecido"
        };
    }

    private static TimeRecordResponseDto MapToTimeRecordResponse(TimeRecord timeRecord)
    {
        return new TimeRecordResponseDto
        {
            Id = timeRecord.Id,
            Timestamp = timeRecord.Timestamp,
            Type = timeRecord.Type,
            TypeDescription = GetTypeDescription(timeRecord.Type),
            Description = timeRecord.Description,
            CreatedAt = timeRecord.CreatedAt
        };
    }
}