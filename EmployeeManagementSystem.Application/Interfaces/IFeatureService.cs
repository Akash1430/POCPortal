using EmployeeManagementSystem.Application.DTOs;

namespace EmployeeManagementSystem.Application.Interfaces
{
    public interface IFeatureService
    {
        // Role and Permission Display
        Task<ApiResponse<GetRolesWithPermissionsResponseDto>> GetAllRolesWithPermissionsAsync();
        Task<ApiResponse<RoleWithPermissionsDto>> GetRoleWithPermissionsAsync(int roleId);
        Task<ApiResponse<GetAllPermissionsResponseDto>> GetAllPermissionsAsync();
        
        // Role Permission Management
        Task<ApiResponse<UpdateRolePermissionsResponseDto>> UpdateRolePermissionsAsync(int roleId, UpdateRolePermissionsRequestDto request, string updatedBy);
    }
}
