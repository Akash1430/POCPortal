using EmployeeManagementSystem.Application.DTOs;
using EmployeeManagementSystem.Application.Interfaces;
using EmployeeManagementSystem.Domain.Entities;

namespace EmployeeManagementSystem.Application.Services
{
    public class FeatureService : IFeatureService
    {
        private readonly IUnitOfWork _unitOfWork;

        public FeatureService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<ApiResponse<GetRolesWithPermissionsResponseDto>> GetAllRolesWithPermissionsAsync()
        {
            try
            {
                var roles = await _unitOfWork.UserRoles.FindAsync(ur => ur.IsVisible);
                var rolePermissions = new List<RoleWithPermissionsDto>();

                foreach (var role in roles)
                {
                    var permissionsForRole = await GetPermissionsForRoleAsync(role.Id);
                    
                    rolePermissions.Add(new RoleWithPermissionsDto
                    {
                        Id = role.Id,
                        RoleName = role.RoleName,
                        RefCode = role.RefCode,
                        Description = role.Description,
                        IsVisible = role.IsVisible,
                        DateCreatedUTC = role.DateCreatedUTC,
                        Permissions = permissionsForRole
                    });
                }

                return ApiResponse<GetRolesWithPermissionsResponseDto>.SuccessResult(
                    new GetRolesWithPermissionsResponseDto { Roles = rolePermissions }
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<GetRolesWithPermissionsResponseDto>.ErrorResult(
                    $"Error retrieving roles with permissions: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<RoleWithPermissionsDto>> GetRoleWithPermissionsAsync(int roleId)
        {
            try
            {
                var role = await _unitOfWork.UserRoles.GetByIdAsync(roleId);
                if (role == null)
                {
                    return ApiResponse<RoleWithPermissionsDto>.ErrorResult("Role not found");
                }

                var permissions = await GetPermissionsForRoleAsync(roleId);

                var roleWithPermissions = new RoleWithPermissionsDto
                {
                    Id = role.Id,
                    RoleName = role.RoleName,
                    RefCode = role.RefCode,
                    Description = role.Description,
                    IsVisible = role.IsVisible,
                    DateCreatedUTC = role.DateCreatedUTC,
                    Permissions = permissions
                };

                return ApiResponse<RoleWithPermissionsDto>.SuccessResult(roleWithPermissions);
            }
            catch (Exception ex)
            {
                return ApiResponse<RoleWithPermissionsDto>.ErrorResult(
                    $"Error retrieving role with permissions: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<GetAllPermissionsResponseDto>> GetAllPermissionsAsync()
        {
            try
            {
                var allPermissions = await _unitOfWork.ModuleAccesses.FindAsync(ma => ma.IsVisible);
                var modules = await _unitOfWork.Modules.GetAllAsync();

                var permissionTree = BuildPermissionTree(allPermissions, modules);

                return ApiResponse<GetAllPermissionsResponseDto>.SuccessResult(
                    new GetAllPermissionsResponseDto { Permissions = permissionTree }
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<GetAllPermissionsResponseDto>.ErrorResult(
                    $"Error retrieving permissions: {ex.Message}"
                );
            }
        }

        public async Task<ApiResponse<UpdateRolePermissionsResponseDto>> UpdateRolePermissionsAsync(
            int roleId, UpdateRolePermissionsRequestDto request, string updatedBy)
        {
            try
            {
                var role = await _unitOfWork.UserRoles.GetByIdAsync(roleId);
                if (role == null)
                {
                    return ApiResponse<UpdateRolePermissionsResponseDto>.ErrorResult("Role not found");
                }

                if (role.RefCode == "SYSADMIN")
                {
                    return ApiResponse<UpdateRolePermissionsResponseDto>.ErrorResult(
                        "Cannot modify permissions for SYSADMIN role"
                    );
                }

                // Get current role accesses
                var currentAccesses = await _unitOfWork.UserRoleAccesses.FindAsync(ura => ura.UserRoleId == roleId);
                
                // Remove existing permissions
                await _unitOfWork.UserRoleAccesses.DeleteRangeAsync(currentAccesses);

                var user = await _unitOfWork.Users.FindFirstAsync(u => u.UserName == updatedBy);
                var userId = user != null ? user.Id : 1; // Default to system
                // Add new permissions
                var newAccesses = request.PermissionIds.Select(permissionId => new UserRoleAccess
                {
                    UserRoleId = roleId,
                    ModuleAccessId = permissionId,
                    DateCreatedUTC = DateTime.UtcNow,
                    CreatedBy = userId
                });

                await _unitOfWork.UserRoleAccesses.AddRangeAsync(newAccesses);
                await _unitOfWork.SaveChangesAsync();

                // Get updated permission names
                var updatedPermissions = await _unitOfWork.ModuleAccesses.FindAsync(ma => 
                    request.PermissionIds.Contains(ma.Id));

                return ApiResponse<UpdateRolePermissionsResponseDto>.SuccessResult(
                    new UpdateRolePermissionsResponseDto
                    {
                        RoleId = roleId,
                        RoleName = role.RoleName,
                        UpdatedPermissions = updatedPermissions.Select(p => p.RefCode).ToList()
                    },
                    "Role permissions updated successfully"
                );
            }
            catch (Exception ex)
            {
                return ApiResponse<UpdateRolePermissionsResponseDto>.ErrorResult(
                    $"Error updating role permissions: {ex.Message}"
                );
            }
        }

  
        // Helper methods
        private async Task<List<PermissionTreeDto>> GetPermissionsForRoleAsync(int roleId)
        {
            var roleAccesses = await _unitOfWork.UserRoleAccesses.FindAsync(ura => ura.UserRoleId == roleId);
            var permissionIds = roleAccesses.Select(ra => ra.ModuleAccessId).ToList();
            
            var allPermissions = await _unitOfWork.ModuleAccesses.FindAsync(ma => ma.IsVisible && permissionIds.Contains(ma.Id));
            var modules = await _unitOfWork.Modules.GetAllAsync();

            return BuildPermissionTree(allPermissions, modules, permissionIds);
        }

        private List<PermissionTreeDto> BuildPermissionTree(
            IEnumerable<ModuleAccess> permissions, 
            IEnumerable<Module> modules, 
            List<int>? userPermissionIds = null)
        {
            var permissionTree = new List<PermissionTreeDto>();
            var rootPermissions = permissions.Where(p => p.ParentId == null);

            foreach (var permission in rootPermissions)
            {
                var module = modules.FirstOrDefault(m => m.Id == permission.ModuleId);
                var permissionDto = new PermissionTreeDto
                {
                    Id = permission.Id,
                    ModuleId = permission.ModuleId,
                    ModuleName = module?.ModuleName ?? "Unknown",
                    PermissionName = permission.ModuleAccessName,
                    ParentId = permission.ParentId,
                    RefCode = permission.RefCode,
                    Description = permission.Description,
                    IsVisible = permission.IsVisible,
                    HasPermission = userPermissionIds?.Contains(permission.Id) ?? false,
                    SubPermissions = GetChildPermissions(permission.Id, permissions, modules, userPermissionIds)
                };

                permissionTree.Add(permissionDto);
            }

            return permissionTree;
        }

        private List<PermissionTreeDto> GetChildPermissions(
            int parentId, 
            IEnumerable<ModuleAccess> permissions, 
            IEnumerable<Module> modules,
            List<int>? userPermissionIds = null)
        {
            var childPermissions = new List<PermissionTreeDto>();
            var children = permissions.Where(p => p.ParentId == parentId);

            foreach (var child in children)
            {
                var module = modules.FirstOrDefault(m => m.Id == child.ModuleId);
                var childDto = new PermissionTreeDto
                {
                    Id = child.Id,
                    ModuleId = child.ModuleId,
                    ModuleName = module?.ModuleName ?? "Unknown",
                    PermissionName = child.ModuleAccessName,
                    ParentId = child.ParentId,
                    RefCode = child.RefCode,
                    Description = child.Description,
                    IsVisible = child.IsVisible,
                    HasPermission = userPermissionIds?.Contains(child.Id) ?? false,
                    SubPermissions = GetChildPermissions(child.Id, permissions, modules, userPermissionIds)
                };

                childPermissions.Add(childDto);
            }

            return childPermissions;
        }

    }
}
