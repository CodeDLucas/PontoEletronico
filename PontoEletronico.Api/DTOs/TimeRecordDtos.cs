using PontoEletronico.Models;
using System.ComponentModel.DataAnnotations;

namespace PontoEletronico.DTOs;

public class TimeRecordCreateDto
{
    [Required(ErrorMessage = "Tipo de marcação é obrigatório")]
    public TimeRecordType Type { get; set; }

    [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
    public string? Description { get; set; }

    public DateTime? Timestamp { get; set; }
}

public class TimeRecordResponseDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public TimeRecordType Type { get; set; }
    public string TypeDescription { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class TimeRecordListDto
{
    public int Id { get; set; }
    public DateTime Timestamp { get; set; }
    public TimeRecordType Type { get; set; }
    public string TypeDescription { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class TimeRecordFilterDto
{
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public TimeRecordType? Type { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class TimeRecordSummaryDto
{
    public DateTime Date { get; set; }
    public List<TimeRecordListDto> Records { get; set; } = new();
    public TimeSpan? TotalWorkedTime { get; set; }
    public bool IsComplete { get; set; }
}