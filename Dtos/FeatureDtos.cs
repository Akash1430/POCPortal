namespace Dtos;

public class UserRoleWithModuleAccessesDto : UserRoleDto
{
    public List<ModuleAccessDto> ModuleAccesses { get; set; } = new List<ModuleAccessDto>();
}

public class UserRolesWithModuleAccessesDto
{
    public List<UserRoleWithModuleAccessesDto> UserRoles { get; set; } = new List<UserRoleWithModuleAccessesDto>();
}



public class ModuleAccessesDto
{
    public List<ModuleAccessDto> ModuleAccessDtos { get; set; } = new List<ModuleAccessDto>();
}


public class UpdateUserRoleModuleAccessesRequestDto
{
    public List<int> ModuleAccessIds { get; set; } = new List<int>();
}

public class UpdateUserRoleModuleAccessesResponseDto
{
    public int UserRoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public List<string> ModuleAccessRefs { get; set; } = new List<string>();
}
