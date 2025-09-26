using Interfaces;
using Models;
using DataAccess.Interfaces;

namespace Core.Permission;

public class PermissionLogic : IPermissionLogic
{
    private readonly IUnitOfWork _unitOfWork;

    public PermissionLogic(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> HasPermissionAsync(string roleRefCode, string permissionRefCode)
    {
        try
        {
            var userRole = await _unitOfWork.UserRoles.FindFirstAsync(ur => ur.RefCode == roleRefCode);
            if (userRole == null) return false;

            var userRoleAccesses = await _unitOfWork.UserRoleAccesses.FindAsync(ura => ura.UserRoleId == userRole.Id);
            var moduleAccessIds = userRoleAccesses.Select(ura => ura.ModuleAccessId).ToList();

            var hasPermission = await _unitOfWork.ModuleAccesses.ExistsAsync(ma => 
                moduleAccessIds.Contains(ma.Id) && ma.RefCode == permissionRefCode);

            return hasPermission;
        }
        catch
        {
            return false;
        }
    }

    public async Task<ApiResponse<PermissionsModel>> GetCurrentUserPermissionsAsync(string roleRefCode)
    {
        try
        {
            var userRole = await _unitOfWork.UserRoles.FindFirstAsync(ur => ur.RefCode == roleRefCode);
            if (userRole == null) return new ApiResponse<PermissionsModel>
            {
                Success = false,
                Message = "Role not found"
            };

            var userRoleAccesses = await _unitOfWork.UserRoleAccesses.FindAsync(ura => ura.UserRoleId == userRole.Id);
            var moduleAccessIds = userRoleAccesses.Select(ura => ura.ModuleAccessId).ToList();

            var moduleAccesses = await _unitOfWork.ModuleAccesses.FindAsync(ma => moduleAccessIds.Contains(ma.Id));
        
            
            return new ApiResponse<PermissionsModel>
            {
                Success = true,
                Message = "Permissions retrieved successfully",
                Data = new PermissionsModel
                {
                    Permissions = [.. moduleAccesses.Select(ma => ma.RefCode)]
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<PermissionsModel>
            {
                Success = false,
                Message = $"Error retrieving permissions: {ex.Message}"
            };
        }
    }
}
