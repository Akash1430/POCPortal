using Models;

namespace Interfaces;

public interface IPermissionLogic
{
    Task<bool> HasPermissionAsync(string roleRefCode, string permissionRefCode);
    Task<ApiResponse<PermissionsModel>> GetCurrentUserPermissionsAsync(string roleRefCode);
}
