using Models;

namespace Interfaces;

public interface IModuleLogic
{
    Task<ApiResponse<ModulesModel>> GetModulesAsync(string userRoleRefCode);
    Task<ApiResponse<ModulesWithModuleAccessesModel>> GetAllModulesWithPermissionsAsync();
}