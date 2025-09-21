using System.ComponentModel.DataAnnotations;

namespace PontoEletronico.DTOs;

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public bool IsActive { get; set; }
}

public class UserUpdateDto
{
    [Required(ErrorMessage = "Nome completo é obrigatório")]
    [StringLength(100, ErrorMessage = "Nome completo deve ter no máximo 100 caracteres")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email é obrigatório")]
    [EmailAddress(ErrorMessage = "Email deve ter um formato válido")]
    public string Email { get; set; } = string.Empty;
}

public class ChangePasswordDto
{
    [Required(ErrorMessage = "Senha atual é obrigatória")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nova senha é obrigatória")]
    [MinLength(6, ErrorMessage = "Nova senha deve ter pelo menos 6 caracteres")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$",
        ErrorMessage = "Nova senha deve conter pelo menos uma letra minúscula, uma maiúscula e um número")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Confirmação de nova senha é obrigatória")]
    [Compare("NewPassword", ErrorMessage = "Nova senha e confirmação devem ser iguais")]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}

public class UserListDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string EmployeeCode { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}