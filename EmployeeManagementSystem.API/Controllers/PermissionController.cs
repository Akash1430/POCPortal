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
