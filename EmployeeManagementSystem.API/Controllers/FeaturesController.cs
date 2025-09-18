using Microsoft.AspNetCore.Mvc;
using EmployeeManagementSystem.Application.Interfaces;
using EmployeeManagementSystem.Application.Attributes;
using EmployeeManagementSystem.Application.DTOs;

namespace EmployeeManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FeaturesController : ControllerBase
    {
        private readonly IFeatureService _featureService;

        public FeaturesController(IFeatureService featureService)
        {
            _featureService = featureService;
        }

        [HttpGet("roles-with-permissions")]
        [RequirePermission("FEATURES_READ_ROLES")]
        public async Task<IActionResult> GetAllRolesWithPermissions()
        {
            var response = await _featureService.GetAllRolesWithPermissionsAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("roles/{roleId}/permissions")]
        [RequirePermission("FEATURES_READ_ROLES")]
        public async Task<IActionResult> GetRoleWithPermissions(int roleId)
        {
            var response = await _featureService.GetRoleWithPermissionsAsync(roleId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("permissions")]
        [RequirePermission("FEATURES_READ_PERMISSIONS")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var response = await _featureService.GetAllPermissionsAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("roles/{roleId}/permissions")]
        [RequirePermission("FEATURES_UPDATE_ROLE_PERMISSIONS")]
        public async Task<IActionResult> UpdateRolePermissions(int roleId, [FromBody] UpdateRolePermissionsRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<UpdateRolePermissionsResponseDto>.ErrorResult(errors));
            }

            request.RoleId = roleId;
            var updatedBy = User.Identity!.Name!;
            var response = await _featureService.UpdateRolePermissionsAsync(roleId, request, updatedBy);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}