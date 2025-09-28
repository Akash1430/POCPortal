using Interfaces;
using Models;
using Core.ModuleAccess;
using DataAccess.Interfaces;
using Core.Module;
using Core.UserRole;
using Models.Constants;

namespace Core.Feature;

public class FeatureLogic : IFeatureLogic
{
    private readonly IUnitOfWork _unitOfWork;

    public FeatureLogic(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }


    // Helper methods
    private async Task<List<ModuleAccessModel>> GetPermissionsForRoleAsync(int roleId)
    {
        var roleAccesses = await _unitOfWork.UserRoleAccesses.FindAsync(ura => ura.UserRoleId == roleId);
        var permissionIds = roleAccesses.Select(ra => ra.ModuleAccessId).ToList();

        var allPermissions = await _unitOfWork.ModuleAccesses.FindAsync(ma => ma.IsVisible && permissionIds.Contains(ma.Id));
        var modules = await _unitOfWork.Modules.GetAllAsync();

        return BuildPermissionTree(allPermissions.ToModel(), modules.ToModel(), permissionIds);
    }

    private List<ModuleAccessModel> BuildPermissionTree(
        IEnumerable<ModuleAccessModel> permissions,
        IEnumerable<ModuleModel> modules,
        List<int>? userPermissionIds = null)
    {
        var permissionTree = new List<ModuleAccessModel>();
        var rootPermissions = permissions.Where(p => p.ParentId == null);

        foreach (var permission in rootPermissions)
        {
            var module = modules.FirstOrDefault(m => m.Id == permission.ModuleId);
            var permissionDto = new ModuleAccessModel
            {
                Id = permission.Id,
                ModuleId = permission.ModuleId,
                ModuleName = module?.ModuleName ?? "Unknown",
                ModuleAccessName = permission.ModuleAccessName,
                ParentId = permission.ParentId,
                RefCode = permission.RefCode,
                Description = permission.Description,
                IsVisible = permission.IsVisible,
                HasPermission = userPermissionIds?.Contains(permission.Id) ?? false,
                SubModuleAccesses = GetChildPermissions(permission.Id, permissions, modules, userPermissionIds)
            };

            permissionTree.Add(permissionDto);
        }

        return permissionTree;
    }

    private List<ModuleAccessModel> GetChildPermissions(
        int parentId,
        IEnumerable<ModuleAccessModel> permissions,
        IEnumerable<ModuleModel> modules,
        List<int>? userPermissionIds = null)
    {
        var childPermissions = new List<ModuleAccessModel>();
        var children = permissions.Where(p => p.ParentId == parentId);

        foreach (var child in children)
        {
            var module = modules.FirstOrDefault(m => m.Id == child.ModuleId);
            var childDto = new ModuleAccessModel
            {
                Id = child.Id,
                ModuleId = child.ModuleId,
                ModuleName = module?.ModuleName ?? "Unknown",
                ModuleAccessName = child.ModuleAccessName,
                ParentId = child.ParentId,
                RefCode = child.RefCode,
                Description = child.Description,
                IsVisible = child.IsVisible,
                HasPermission = userPermissionIds?.Contains(child.Id) ?? false,
                SubModuleAccesses = GetChildPermissions(child.Id, permissions, modules, userPermissionIds)
            };

            childPermissions.Add(childDto);
        }

        return childPermissions;
    }

    public async Task<ApiResponse<UserRolesWithModuleAccessesModel>> GetAllRolesWithPermissionsAsync()
    {
        try
        {
            var roles = await _unitOfWork.UserRoles.FindAsync(ur => ur.IsVisible);
            var rolePermissions = new List<UserRoleWithModuleAccessesModel>();

            foreach (var role in roles)
            {
                var permissionsForRole = await GetPermissionsForRoleAsync(role.Id);

                rolePermissions.Add(new UserRoleWithModuleAccessesModel
                {
                    Id = role.Id,
                    RoleName = role.RoleName,
                    RefCode = role.RefCode,
                    Description = role.Description ?? string.Empty,
                    IsVisible = role.IsVisible,
                    DateCreatedUTC = role.DateCreatedUTC,
                    ModuleAccesses = permissionsForRole
                });
            }

            return ApiResponse<UserRolesWithModuleAccessesModel>.SuccessResult(
                new UserRolesWithModuleAccessesModel { UserRoles = rolePermissions }
            );
        }
        catch (Exception ex)
        {
            return ApiResponse<UserRolesWithModuleAccessesModel>.ErrorResult(
                $"Error retrieving roles with permissions: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<UserRoleWithModuleAccessesModel>> GetRoleWithPermissionsAsync(int roleId)
    {
        try
        {
            var role = await _unitOfWork.UserRoles.GetByIdAsync(roleId);
            if (role == null)
            {
                return ApiResponse<UserRoleWithModuleAccessesModel>.ErrorResult("Role not found");
            }

            var permissions = await GetPermissionsForRoleAsync(roleId);

            var roleWithPermissions = new UserRoleWithModuleAccessesModel
            {
                Id = role.Id,
                RoleName = role.RoleName,
                RefCode = role.RefCode,
                Description = role.Description ?? string.Empty,
                IsVisible = role.IsVisible,
                DateCreatedUTC = role.DateCreatedUTC,
                ModuleAccesses = permissions
            };

            return ApiResponse<UserRoleWithModuleAccessesModel>.SuccessResult(roleWithPermissions);
        }
        catch (Exception ex)
        {
            return ApiResponse<UserRoleWithModuleAccessesModel>.ErrorResult(
                $"Error retrieving role with permissions: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<ModuleAccessesModel>> GetAllPermissionsAsync()
    {
        try
        {
            var allPermissions = await _unitOfWork.ModuleAccesses.FindAsync(ma => ma.IsVisible);
            var modules = await _unitOfWork.Modules.GetAllAsync();

            var permissionTree = BuildPermissionTree(allPermissions.ToModel(), modules.ToModel());

            return ApiResponse<ModuleAccessesModel>.SuccessResult(
                new ModuleAccessesModel { ModuleAccesses = permissionTree }
            );
        }
        catch (Exception ex)
        {
            return ApiResponse<ModuleAccessesModel>.ErrorResult(
                $"Error retrieving permissions: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<UpdateUserRoleModuleAccessesResponseModel>> UpdateRolePermissionsAsync(int roleId, UpdateUserRoleModuleAccessesRequestModel request, int updatedBy)
    {
        try
        {
            var role = await _unitOfWork.UserRoles.GetByIdAsync(roleId);
            if (role == null)
            {
                return ApiResponse<UpdateUserRoleModuleAccessesResponseModel>.ErrorResult("Role not found");
            }

            if (role.RefCode == UserRoles.SYSADMIN)
            {
                return ApiResponse<UpdateUserRoleModuleAccessesResponseModel>.ErrorResult(
                    "Cannot modify permissions for SYSADMIN role"
                );
            }

            // Get current role accesses
            var currentAccesses = await _unitOfWork.UserRoleAccesses.FindAsync(ura => ura.UserRoleId == roleId);

            // Remove existing permissions
            await _unitOfWork.UserRoleAccesses.DeleteRangeAsync(currentAccesses);


            var newAccesses = request.ModuleAccessIds.Select(permissionId => new DataAccess.UserRoleAccess
            {
                UserRoleId = roleId,
                ModuleAccessId = permissionId,
                DateCreatedUTC = DateTime.UtcNow,
                CreatedBy = updatedBy
            });

            await _unitOfWork.UserRoleAccesses.AddRangeAsync(newAccesses);
            await _unitOfWork.SaveChangesAsync();

            // Get updated permission names
            var updatedPermissions = await _unitOfWork.ModuleAccesses.FindAsync(ma =>
                request.ModuleAccessIds.Contains(ma.Id));

            return ApiResponse<UpdateUserRoleModuleAccessesResponseModel>.SuccessResult(
                new UpdateUserRoleModuleAccessesResponseModel
                {
                    UserRoleId = roleId,
                    RoleName = role.RoleName,
                    ModuleAccessRefs = updatedPermissions.Select(p => p.RefCode).ToList()
                },
                "Role permissions updated successfully"
            );
        }
        catch (Exception ex)
        {
            return ApiResponse<UpdateUserRoleModuleAccessesResponseModel>.ErrorResult(
                $"Error updating role permissions: {ex.Message}"
            );
        }
    }

    public async Task<ApiResponse<UserRoleModel>> CreateUserRoleAsync(CreateUserRoleRequestModel request, int createdBy)
    {
        try
        {
            // Check for duplicate role name or ref code
            var existingRole = await _unitOfWork.UserRoles.FindAsync(r =>
                r.RoleName == request.RoleName || r.RefCode == request.RefCode);

            if (existingRole.Any())
            {
                return ApiResponse<UserRoleModel>.ErrorResult("Role name or reference code already exists.");
            }

            var newRole = new DataAccess.UserRole
            {
                RoleName = request.RoleName,
                RefCode = request.RefCode,
                Description = request.Description,
                IsVisible = true,
                DateCreatedUTC = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            await _unitOfWork.UserRoles.AddAsync(newRole);
            await _unitOfWork.SaveChangesAsync();

            var createdRole = await _unitOfWork.UserRoles.GetByIdAsync(newRole.Id);

            return ApiResponse<UserRoleModel>.SuccessResult(createdRole!.ToModel(), "Role created successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<UserRoleModel>.ErrorResult($"Error creating role: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> DeleteRoleAsync(int roleId)
    {
        try
        {
            var role = await _unitOfWork.UserRoles.GetByIdAsync(roleId);
            if (role == null)
            {
                return ApiResponse<string>.ErrorResult("Role not found");
            }

            if (role.RefCode == UserRoles.SYSADMIN || role.RefCode == UserRoles.USERADMIN)
            {
                return ApiResponse<string>.ErrorResult("Cannot delete SYSADMIN or USERADMIN role");
            }

            // Remove associated accesses
            var accesses = await _unitOfWork.UserRoleAccesses.FindAsync(ura => ura.UserRoleId == roleId);
            await _unitOfWork.UserRoleAccesses.DeleteRangeAsync(accesses);

            // Remove the role itself
            await _unitOfWork.UserRoles.DeleteAsync(role);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<string>.SuccessResult("Role deleted successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResult($"Error deleting role: {ex.Message}");
        }
    }
}
