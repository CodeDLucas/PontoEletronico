using FluentValidation;
using PontoEletronico.DTOs;

namespace PontoEletronico.Validators;

public class UserUpdateValidator : AbstractValidator<UserUpdateDto>
{
    public UserUpdateValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Nome completo é obrigatório")
            .MaximumLength(100).WithMessage("Nome completo deve ter no máximo 100 caracteres")
            .Matches(@"^[a-zA-ZÀ-ÿ\s]+$").WithMessage("Nome completo deve conter apenas letras e espaços");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório")
            .EmailAddress().WithMessage("Email deve ter um formato válido")
            .MaximumLength(256).WithMessage("Email deve ter no máximo 256 caracteres");
    }
}

public class ChangePasswordValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordValidator()
    {
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Senha atual é obrigatória");

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithMessage("Nova senha é obrigatória")
            .MinimumLength(6).WithMessage("Nova senha deve ter pelo menos 6 caracteres")
            .Matches(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$")
            .WithMessage("Nova senha deve conter pelo menos uma letra minúscula, uma maiúscula e um número")
            .NotEqual(x => x.CurrentPassword).WithMessage("Nova senha deve ser diferente da senha atual");

        RuleFor(x => x.ConfirmNewPassword)
            .NotEmpty().WithMessage("Confirmação de nova senha é obrigatória")
            .Equal(x => x.NewPassword).WithMessage("Nova senha e confirmação devem ser iguais");
    }
}