using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PontoEletronico.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [MaxLength(20)]
    public string? EmployeeCode { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<TimeRecord> TimeRecords { get; set; } = new List<TimeRecord>();
}