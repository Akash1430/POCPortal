namespace Models;

public class UserRoleWithModuleAccessesModel : UserRoleModel
{
    public List<ModuleAccessModel> ModuleAccesses { get; set; } = new List<ModuleAccessModel>();
}

public class UserRolesWithModuleAccessesModel
{
    public List<UserRoleWithModuleAccessesModel> UserRoles { get; set; } = new List<UserRoleWithModuleAccessesModel>();
}

public class UpdateUserRoleModuleAccessesRequestModel
{
    public List<int> ModuleAccessIds { get; set; } = new List<int>();
}

public class UpdateUserRoleModuleAccessesResponseModel
{
    public int UserRoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public List<string> ModuleAccessRefs { get; set; } = new List<string>();
}