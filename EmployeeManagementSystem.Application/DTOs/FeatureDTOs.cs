namespace EmployeeManagementSystem.Application.DTOs
{
    public class RoleWithPermissionsDto
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RefCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsVisible { get; set; }
        public List<PermissionTreeDto> Permissions { get; set; } = new List<PermissionTreeDto>();
        public DateTime DateCreatedUTC { get; set; }
    }

    public class GetRolesWithPermissionsResponseDto
    {
        public List<RoleWithPermissionsDto> Roles { get; set; } = new List<RoleWithPermissionsDto>();
    }

    public class PermissionTreeDto
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string RefCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsVisible { get; set; }
        public bool HasPermission { get; set; } = false;
        public List<PermissionTreeDto> SubPermissions { get; set; } = new List<PermissionTreeDto>();
    }

    public class GetAllPermissionsResponseDto
    {
        public List<PermissionTreeDto> Permissions { get; set; } = new List<PermissionTreeDto>();
    }


    public class UpdateRolePermissionsRequestDto
    {
        public List<int> PermissionIds { get; set; } = new List<int>();
    }

    public class UpdateRolePermissionsResponseDto
    {
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public List<string> UpdatedPermissions { get; set; } = new List<string>();
    }

    public class RoleResponseDto
    {
        public int Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string RefCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsVisible { get; set; }
        public DateTime DateCreatedUTC { get; set; }
        public DateTime? LatestDateUpdatedUTC { get; set; }
    }


    public class PermissionResponseDto
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public string PermissionName { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public string RefCode { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsVisible { get; set; }
        public DateTime DateCreatedUTC { get; set; }
        public DateTime? LatestDateUpdatedUTC { get; set; }
    }
}
