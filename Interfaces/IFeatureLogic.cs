using Models;

namespace Interfaces;

public interface IFeatureLogic
{
    // Role and Permission Display
    Task<ApiResponse<UserRolesWithModuleAccessesModel>> GetAllRolesWithPermissionsAsync();
    Task<ApiResponse<UserRoleWithModuleAccessesModel>> GetRoleWithPermissionsAsync(int roleId);
    Task<ApiResponse<ModuleAccessesModel>> GetAllPermissionsAsync();

    // Role Permission Management
    Task<ApiResponse<UpdateUserRoleModuleAccessesResponseModel>> UpdateRolePermissionsAsync(int roleId, UpdateUserRoleModuleAccessesRequestModel request, int updatedBy);

    // Role Management
    Task<ApiResponse<UserRoleModel>> CreateUserRoleAsync(CreateUserRoleRequestModel request, int createdBy);
    Task<ApiResponse<string>> DeleteRoleAsync(int roleId);
}
