using System.ComponentModel.DataAnnotations;

namespace Dtos;

public class UserRoleAccessDto : BaseDto
{
    public int UserRoleId { get; set; }
    public int ModuleAccessId { get; set; }
    public string? UserRoleName { get; set; }
    public string? ModuleAccessName { get; set; }
}

public class CreateUserRoleAccessDto
{
    [Required]
    public int UserRoleId { get; set; }

    [Required]
    public int ModuleAccessId { get; set; }
}

public class UpdateUserRoleAccessDto
{
    [Required]
    public int UserRoleId { get; set; }

    [Required]
    public int ModuleAccessId { get; set; }
}