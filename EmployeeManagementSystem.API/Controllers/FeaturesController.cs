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

        /// <summary>
        /// Retrieves all roles and their associated permissions
        /// </summary>
        /// <remarks>
        /// Gets a comprehensive list of all roles in the system along with their assigned permissions.
        /// Permissions are organized in a hierarchical tree structure.
        /// Requires FEATURES_READ_ROLES permission.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "Roles retrieved successfully",
        ///       "data": {
        ///         "roles": [
        ///           {
        ///             "id": 1,
        ///             "roleName": "System Administrator",
        ///             "refCode": "SYSADMIN",
        ///             "description": "Full system access",
        ///             "isVisible": true,
        ///             "permissions": [
        ///               {
        ///                 "id": 1,
        ///                 "moduleId": 1,
        ///                 "moduleName": "User Management",
        ///                 "permissionName": "Read Users",
        ///                 "refCode": "USER_READ",
        ///                 "hasPermission": true,
        ///                 "subPermissions": []
        ///               }
        ///             ],
        ///             "dateCreatedUTC": "2024-01-01T00:00:00Z"
        ///           }
        ///         ]
        ///       }
        ///     }
        /// 
        /// </remarks>
        /// <returns>List of all roles with their permissions</returns>
        [HttpGet("roles-with-permissions")]
        [RequirePermission("FEATURES_READ_ROLES")]
        public async Task<IActionResult> GetAllRolesWithPermissions()
        {
            var response = await _featureService.GetAllRolesWithPermissionsAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Retrieves a specific role and its permissions
        /// </summary>
        /// <remarks>
        /// Gets detailed information about a specific role including all its assigned permissions.
        /// Permissions are organized in a hierarchical tree structure.
        /// Requires FEATURES_READ_ROLES permission.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "Role retrieved successfully",
        ///       "data": {
        ///         "id": 1,
        ///         "roleName": "System Administrator",
        ///         "refCode": "SYSADMIN",
        ///         "description": "Full system access",
        ///         "isVisible": true,
        ///         "permissions": [
        ///           {
        ///             "id": 1,
        ///             "moduleId": 1,
        ///             "moduleName": "User Management",
        ///             "permissionName": "Read Users",
        ///             "refCode": "USER_READ",
        ///             "hasPermission": true,
        ///             "subPermissions": []
        ///           }
        ///         ],
        ///         "dateCreatedUTC": "2024-01-01T00:00:00Z"
        ///       }
        ///     }
        /// 
        /// </remarks>
        /// <param name="roleId">The unique identifier of the role to retrieve</param>
        /// <returns>Role information with permissions</returns>
        [HttpGet("roles/{roleId}/permissions")]
        [RequirePermission("FEATURES_READ_ROLES")]
        public async Task<IActionResult> GetRoleWithPermissions(int roleId)
        {
            var response = await _featureService.GetRoleWithPermissionsAsync(roleId);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Retrieves all available permissions in the system
        /// </summary>
        /// <remarks>
        /// Gets a comprehensive list of all permissions available in the system.
        /// Permissions are organized in a hierarchical tree structure by module.
        /// Requires FEATURES_READ_PERMISSIONS permission.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "Permissions retrieved successfully",
        ///       "data": {
        ///         "permissions": [
        ///           {
        ///             "id": 1,
        ///             "moduleId": 1,
        ///             "moduleName": "User Management",
        ///             "permissionName": "Read Users",
        ///             "parentId": null,
        ///             "refCode": "USER_READ",
        ///             "description": "View user information",
        ///             "isVisible": true,
        ///             "hasPermission": false,
        ///             "subPermissions": [
        ///               {
        ///                 "id": 2,
        ///                 "moduleId": 1,
        ///                 "moduleName": "User Management",
        ///                 "permissionName": "Read Admin Users",
        ///                 "parentId": 1,
        ///                 "refCode": "ADMIN_READ",
        ///                 "hasPermission": false,
        ///                 "subPermissions": []
        ///               }
        ///             ]
        ///           }
        ///         ]
        ///       }
        ///     }
        /// 
        /// </remarks>
        /// <returns>List of all available permissions in hierarchical structure</returns>
        [HttpGet("permissions")]
        [RequirePermission("FEATURES_READ_PERMISSIONS")]
        public async Task<IActionResult> GetAllPermissions()
        {
            var response = await _featureService.GetAllPermissionsAsync();
            return response.Success ? Ok(response) : BadRequest(response);
        }

        /// <summary>
        /// Updates the permissions assigned to a specific role
        /// </summary>
        /// <remarks>
        /// Updates the list of permissions assigned to a specific role.
        /// This operation replaces all existing permissions for the role with the new list.
        /// Requires FEATURES_UPDATE_ROLE_PERMISSIONS permission.
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/features/roles/2/permissions
        ///     {
        ///       "permissionIds": [1, 2, 5, 8, 12]
        ///     }
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "Role permissions updated successfully",
        ///       "data": {
        ///         "roleId": 2,
        ///         "roleName": "User Admin",
        ///         "updatedPermissions": [
        ///           "USER_READ",
        ///           "ADMIN_READ",
        ///           "ADMIN_CREATE",
        ///           "ADMIN_UPDATE",
        ///           "MODULE_READ"
        ///         ]
        ///       }
        ///     }
        /// 
        /// </remarks>
        /// <param name="roleId">The unique identifier of the role to update</param>
        /// <param name="request">List of permission IDs to assign to the role</param>
        /// <returns>Updated role permissions information</returns>
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

            var updatedBy = User.Identity!.Name!;
            var response = await _featureService.UpdateRolePermissionsAsync(roleId, request, updatedBy);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}