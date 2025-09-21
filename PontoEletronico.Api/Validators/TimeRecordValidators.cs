using FluentValidation;
using PontoEletronico.DTOs;
using PontoEletronico.Models;

namespace PontoEletronico.Validators;

public class TimeRecordCreateValidator : AbstractValidator<TimeRecordCreateDto>
{
    public TimeRecordCreateValidator()
    {
        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de marcação inválido")
            .NotEqual(TimeRecordType.ClockIn).When(x => false) // Sempre válido, mas permite customização futura
            .WithMessage("Tipo de marcação é obrigatório");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");

        RuleFor(x => x.Timestamp)
            .Must(BeValidTimestamp).WithMessage("Data/hora da marcação deve estar entre 7 dias atrás e agora")
            .When(x => x.Timestamp.HasValue);
    }

    private static bool BeValidTimestamp(DateTime? timestamp)
    {
        if (!timestamp.HasValue) return true;

        var now = DateTime.Now;
        var sevenDaysAgo = now.AddDays(-7);

        return timestamp.Value >= sevenDaysAgo && timestamp.Value <= now.AddMinutes(5);
    }
}

public class TimeRecordFilterValidator : AbstractValidator<TimeRecordFilterDto>
{
    public TimeRecordFilterValidator()
    {
        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(x => x.EndDate)
            .WithMessage("Data de início deve ser anterior ou igual à data de fim")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("Data de fim deve ser posterior ou igual à data de início")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.StartDate)
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("Data de início não pode ser no futuro")
            .When(x => x.StartDate.HasValue);

        RuleFor(x => x.EndDate)
            .LessThanOrEqualTo(DateTime.Today)
            .WithMessage("Data de fim não pode ser no futuro")
            .When(x => x.EndDate.HasValue);

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Tipo de marcação inválido")
            .When(x => x.Type.HasValue);

        RuleFor(x => x.Page)
            .GreaterThan(0).WithMessage("Página deve ser maior que zero");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Tamanho da página deve ser maior que zero")
            .LessThanOrEqualTo(100).WithMessage("Tamanho da página deve ser no máximo 100");
    }
}