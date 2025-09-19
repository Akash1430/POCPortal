using EmployeeManagementSystem.Application.DTOs;

namespace EmployeeManagementSystem.Application.Interfaces
{
    public interface IModuleService
    {
        Task<ApiResponse<ModulesResponseDto>> GetModulesAsync(string userRoleRefCode);
        Task<ApiResponse<GetModulesWithPermissionsResponseDto>> GetAllModulesWithPermissionsAsync();
    }
}