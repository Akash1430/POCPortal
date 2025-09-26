using System.ComponentModel.DataAnnotations;

namespace Dtos;

public class UserDto : BaseDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleRefCode { get; set; } = string.Empty;
    public bool IsFrozen { get; set; }
    public DateTime? LastLoginUTC { get; set; }
}

public class CreateUserRequestDto
{
    [Required]
    [MaxLength(100)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string UserName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; set; } = string.Empty;

    public int? UserRoleId { get; set; }
}

// Response DTOs
public class UserResponseDto : BaseDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsFrozen { get; set; }
    public DateTime? LastLoginUTC { get; set; }
    public DateTime? PasswordChangedUTC { get; set; }
    public UserRoleDto? UserRole { get; set; }
}

public class UsersRequestDto
{
    public string? SearchTerm { get; set; }
    public string[]? RoleRefs { get; set; } = Array.Empty<string>();
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

public class UsersResponseDto
{
    public List<UserDto> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class FreezeUserRequestDto
{
    [Required]
    public bool IsFrozen { get; set; }
}

public class UpdateUserRequestDto
{
    [Required]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    public string LastName { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    public string? RoleRefCode { get; set; }
}

public class AdminChangePasswordRequestDto
{
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = string.Empty;
}