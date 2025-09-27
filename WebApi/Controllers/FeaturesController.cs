using Microsoft.AspNetCore.Mvc;
using Interfaces;
using WebApi.Attributes;
using Dtos;
using Core.Feature;
using Core.ModuleAccess;
using Models.Constants;
using System.Security.Claims;


namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FeaturesController : ControllerBase
{
    private readonly IFeatureLogic _featureLogic;

    public FeaturesController(IFeatureLogic featureLogic)
    {
        _featureLogic = featureLogic;
    }

    /// <summary>
    /// Retrieves all roles and their associated permissions
    /// </summary>
    /// <remarks>
    /// Gets a comprehensive list of all roles in the system along with their assigned permissions.
    /// Permissions are organized in a hierarchical tree structure.
    /// Requires FEATURES_READ_ROLES permission.
    /// </remarks>
    /// <returns>List of all roles with their permissions</returns>
    [HttpGet("roles-with-permissions")]
    [RequirePermission(Permissions.FEATURES_READ_ROLES)]
    public async Task<ActionResult<ApiResponse<UserRolesWithModuleAccessesDto>>> GetAllRolesWithPermissions()
    {
        var response = await _featureLogic.GetAllRolesWithPermissionsAsync();
        if (response.Success && response.Data != null)
        {
            return Ok(ApiResponse<UserRolesWithModuleAccessesDto>.SuccessResult(response.Data.ToDto(), response.Message));
        }
        return BadRequest(ApiResponse<UserRolesWithModuleAccessesDto>.ErrorResult(response.Message));
    }

    /// <summary>
    /// Retrieves a specific role and its permissions
    /// </summary>
    /// <remarks>
    /// Gets detailed information about a specific role including all its assigned permissions.
    /// Permissions are organized in a hierarchical tree structure.
    /// Requires FEATURES_READ_ROLES permission.
    /// </remarks>
    /// <param name="roleId">The unique identifier of the role to retrieve</param>
    /// <returns>Role information with permissions</returns>
    [HttpGet("roles/{roleId}/permissions")]
    [RequirePermission(Permissions.FEATURES_READ_ROLES)]
    public async Task<ActionResult<ApiResponse<UserRoleWithModuleAccessesDto>>> GetRoleWithPermissions(int roleId)
    {
        var response = await _featureLogic.GetRoleWithPermissionsAsync(roleId);
        if (response.Success && response.Data != null)
        {
            return Ok(ApiResponse<UserRoleWithModuleAccessesDto>.SuccessResult(response.Data.ToDto(), response.Message));
        }
        return BadRequest(ApiResponse<UserRoleWithModuleAccessesDto>.ErrorResult(response.Message));
    }

    /// <summary>
    /// Retrieves all available permissions in the system
    /// </summary>
    /// <remarks>
    /// Gets a comprehensive list of all permissions available in the system.
    /// Permissions are organized in a hierarchical tree structure by module.
    /// Requires FEATURES_READ_PERMISSIONS permission.
    /// </remarks>
    /// <returns>List of all available permissions in hierarchical structure</returns>
    [HttpGet("permissions")]
    [RequirePermission(Permissions.FEATURES_READ_PERMISSIONS)]
    public async Task<ActionResult<ApiResponse<ModuleAccessesDto>>> GetAllPermissions()
    {
        var response = await _featureLogic.GetAllPermissionsAsync();
        if (response.Success && response.Data != null)
        {
            return Ok(ApiResponse<ModuleAccessesDto>.SuccessResult(response.Data.ToDto(), response.Message));
        }
        return BadRequest(ApiResponse<ModuleAccessesDto>.ErrorResult(response.Message));
    }

    /// <summary>
    /// Updates the permissions assigned to a specific role
    /// </summary>
    /// <remarks>
    /// Updates the list of permissions assigned to a specific role.
    /// This operation replaces all existing permissions for the role with the new list.
    /// Requires FEATURES_UPDATE_ROLE_PERMISSIONS permission.
    /// </remarks>
    /// <param name="roleId">The unique identifier of the role to update</param>
    /// <param name="request">List of permission IDs to assign to the role</param>
    /// <returns>Updated role permissions information</returns>
    [HttpPut("roles/{roleId}/permissions")]
    [RequirePermission(Permissions.FEATURES_UPDATE_ROLE_PERMISSIONS)]
    public async Task<ActionResult<ApiResponse<UpdateUserRoleModuleAccessesResponseDto>>> UpdateRolePermissions(int roleId, [FromBody] UpdateUserRoleModuleAccessesRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<UpdateUserRoleModuleAccessesResponseDto>.ErrorResult(errors));
        }

        var updatedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _featureLogic.UpdateRolePermissionsAsync(roleId, request.ToModel(), updatedBy);
        if (response.Success && response.Data != null)
        {
            return Ok(ApiResponse<UpdateUserRoleModuleAccessesResponseDto>.SuccessResult(response.Data.ToDto(), response.Message));
        }
        return BadRequest(ApiResponse<UpdateUserRoleModuleAccessesResponseDto>.ErrorResult(response.Message));
    }
}