using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Interfaces;
using Core.Permission;
using Dtos;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PermissionController : ControllerBase
{
    private readonly IPermissionLogic _permissionLogic;

    public PermissionController(IPermissionLogic permissionLogic)
    {
        _permissionLogic = permissionLogic;
    }

    /// <summary>
    /// Retrieves the current user's permissions
    /// </summary>
    /// <remarks>
    /// Gets a list of all permissions assigned to the currently authenticated user based on their role.
    /// Returns permission reference codes as an array of strings.
    /// Requires authentication.
    /// </remarks>
    /// <returns>List of current user's permissions</returns>
    [HttpGet("my-permissions")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PermissionsDto>>> GetMyPermissions()
    {
        var roleRefCode = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(roleRefCode))
        {
            return BadRequest(ApiResponse<PermissionsDto>.ErrorResult("User role not found"));
        }

        var response = await _permissionLogic.GetCurrentUserPermissionsAsync(roleRefCode);
        if (response.Success && response.Data != null)
        {
            return Ok(ApiResponse<PermissionsDto>.SuccessResult(response.Data.ToDto(), response.Message));
        }
        return BadRequest(ApiResponse<PermissionsDto>.ErrorResult(response.Message));
    }

    /// <summary>
    /// Checks if the current user has a specific permission
    /// </summary>
    /// <remarks>
    /// Verifies whether the currently authenticated user has a specific permission.
    /// Returns a boolean result indicating if the user has the requested permission.
    /// Requires authentication.
    /// </remarks>
    /// <param name="request">Permission check request containing the permission to verify</param>
    /// <returns>Permission check result</returns>
    [HttpPost("check")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<PermissionCheckResponseDto>>> CheckPermission([FromBody] PermissionCheckRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<PermissionCheckResponseDto>.ErrorResult(errors));
        }

        var roleRefCode = User.FindFirstValue(ClaimTypes.Role);
        if (string.IsNullOrEmpty(roleRefCode))
        {
            return BadRequest("User role not found");
        }

        var hasPermission = await _permissionLogic.HasPermissionAsync(roleRefCode, request.Permission);

        return Ok(new ApiResponse<PermissionCheckResponseDto>
        {
            Success = true,
            Data = new PermissionCheckResponseDto
            {
                HasPermission = hasPermission,
                Permission = request.Permission
            }
        });
    }
}
