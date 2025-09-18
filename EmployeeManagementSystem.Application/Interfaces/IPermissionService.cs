using EmployeeManagementSystem.Application.DTOs;

namespace EmployeeManagementSystem.Application.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(string roleRefCode, string permissionRefCode);
        Task<ApiResponse<UserPermissionsResponseDto>> GetCurrentUserPermissionsAsync(string roleRefCode);
    }
}
