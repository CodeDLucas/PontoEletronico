using FluentValidation;
using PontoEletronico.DTOs;

namespace PontoEletronico.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter um formato válido");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter pelo menos 6 caracteres");
    }
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nome completo é obrigatório")
            .MaximumLength(100).WithMessage("Nome completo deve ter no máximo 100 caracteres")
            .Matches(@"^[a-zA-ZÀ-ÿ\s]+$").WithMessage("Nome completo deve conter apenas letras e espaços");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter um formato válido")
            .MaximumLength(256).WithMessage("Email deve ter no máximo 256 caracteres");

        RuleFor(x => x.EmployeeCode)
            .NotEmpty().WithMessage("Código do funcionário é obrigatório")
            .MaximumLength(20).WithMessage("Código do funcionário deve ter no máximo 20 caracteres")
            .Matches(@"^[a-zA-Z0-9]+$").WithMessage("Código do funcionário deve conter apenas letras e números");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(6).WithMessage("Senha deve ter pelo menos 6 caracteres")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")
            .WithMessage("Senha deve conter pelo menos uma letra minúscula, uma maiúscula e um número");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirmação de senha é obrigatória")
            .Equal(x => x.Password).WithMessage("Senha e confirmação de senha devem ser iguais");
    }
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token é obrigatório");
    }
}