using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagementSystem.Application.Interfaces;
using EmployeeManagementSystem.Application.DTOs;
using System.Security.Claims;

namespace EmployeeManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PermissionController : ControllerBase
    {
        private readonly IPermissionService _permissionService;

        public PermissionController(IPermissionService permissionService)
        {
            _permissionService = permissionService;
        }

        /// <summary>
        /// Retrieves the current user's permissions
        /// </summary>
        /// <remarks>
        /// Gets a list of all permissions assigned to the currently authenticated user based on their role.
        /// Returns permission reference codes as an array of strings.
        /// Requires authentication.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "permissions": [
        ///         "USER_READ",
        ///         "ADMIN_READ",
        ///         "ADMIN_CREATE",
        ///         "ADMIN_UPDATE",
        ///         "ADMIN_DELETE",
        ///         "MODULE_READ",
        ///         "FEATURES_READ_ROLES",
        ///         "FEATURES_READ_PERMISSIONS",
        ///         "FEATURES_UPDATE_ROLE_PERMISSIONS"
        ///       ]
        ///     }
        /// 
        /// </remarks>
        /// <returns>List of current user's permissions</returns>
        [HttpGet("my-permissions")]
        [Authorize]
        public async Task<IActionResult> GetMyPermissions()
        {
            var roleRefCode = User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(roleRefCode))
            {
                return BadRequest("User role not found");
            }

            var response = await _permissionService.GetCurrentUserPermissionsAsync(roleRefCode);
            return Ok(response);
        }

        /// <summary>
        /// Checks if the current user has a specific permission
        /// </summary>
        /// <remarks>
        /// Verifies whether the currently authenticated user has a specific permission.
        /// Returns a boolean result indicating if the user has the requested permission.
        /// Requires authentication.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/permission/check
        ///     {
        ///       "permission": "ADMIN_CREATE"
        ///     }
        /// 
        /// Sample response (has permission):
        /// 
        ///     {
        ///       "success": true,
        ///       "data": {
        ///         "hasPermission": true,
        ///         "permission": "ADMIN_CREATE"
        ///       }
        ///     }
        /// 
        /// Sample response (no permission):
        /// 
        ///     {
        ///       "success": true,
        ///       "data": {
        ///         "hasPermission": false,
        ///         "permission": "ADMIN_DELETE"
        ///       }
        ///     }
        /// 
        /// </remarks>
        /// <param name="request">Permission check request containing the permission to verify</param>
        /// <returns>Permission check result</returns>
        [HttpPost("check")]
        [Authorize]
        public async Task<IActionResult> CheckPermission([FromBody] PermissionCheckRequestDto request)
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

            var hasPermission = await _permissionService.HasPermissionAsync(roleRefCode, request.Permission);

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
}
