using System.ComponentModel.DataAnnotations;

namespace Dtos;

public class UserRoleDto : BaseDto
{
    public string RoleName { get; set; } = string.Empty;
    public string RefCode { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsVisible { get; set; }
}

public class CreateUserRoleRequestDto
{
    [Required]
    [MaxLength(100)]
    public string RoleName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RefCode { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsVisible { get; set; } = true;
}

public class UpdateUserRoleRequestDto
{
    [Required]
    [MaxLength(100)]
    public string RoleName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string RefCode { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    public bool IsVisible { get; set; } = true;
}