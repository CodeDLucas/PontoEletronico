using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PontoEletronico.Models;

public class ApplicationUser : IdentityUser
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [MaxLength(20)]
    public string EmployeeCode { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<TimeRecord> TimeRecords { get; set; } = new List<TimeRecord>();
}