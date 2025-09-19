using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagementSystem.Application.Interfaces;
using EmployeeManagementSystem.Application.Attributes;
using EmployeeManagementSystem.Application.DTOs;
using System.Security.Claims;

namespace EmployeeManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ModuleController : ControllerBase
    {
        private readonly IModuleService _moduleService;

        public ModuleController(IModuleService moduleService)
        {
            _moduleService = moduleService;
        }

        /// <summary>
        /// Retrieves modules accessible to the current user
        /// </summary>
        /// <remarks>
        /// Gets a list of modules that the current user has access to based on their role.
        /// Modules are returned in a hierarchical structure with parent and child modules.
        /// Requires MODULE_READ permission.
        /// 
        /// Sample response:
        /// 
        ///     [
        ///       {
        ///         "id": 1,
        ///         "moduleName": "User Management",
        ///         "parentId": null,
        ///         "refCode": "USER_MGMT",
        ///         "isVisible": true,
        ///         "logoName": "users-icon.svg",
        ///         "redirectPage": "/users",
        ///         "sortOrder": 1,
        ///         "description": "Manage system users and permissions",
        ///         "dateCreatedUTC": "2024-01-01T00:00:00Z",
        ///         "createdBy": 1,
        ///         "subModules": [
        ///           {
        ///             "id": 2,
        ///             "moduleName": "Admin Users",
        ///             "parentId": 1,
        ///             "refCode": "ADMIN_USERS",
        ///             "isVisible": true,
        ///             "subModules": []
        ///           }
        ///         ]
        ///       }
        ///     ]
        /// 
        /// </remarks>
        /// <returns>List of accessible modules in hierarchical structure</returns>
        [HttpGet("modules")]
        [RequirePermission("MODULE_READ")]
        public async Task<IActionResult> GetModules()
        {
            var response = await _moduleService.GetModulesAsync(User.FindFirstValue(ClaimTypes.Role)!);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Data);
        }

        /// <summary>
        /// Retrieves all modules with their associated permissions (Admin only)
        /// </summary>
        /// <remarks>
        /// Gets a comprehensive list of all modules in the system along with their associated permissions.
        /// This endpoint is restricted to users with the SYSADMIN role only.
        /// Returns detailed permission trees for each module.
        /// 
        /// Sample response:
        /// 
        ///     [
        ///       {
        ///         "id": 1,
        ///         "moduleName": "User Management",
        ///         "refCode": "USER_MGMT",
        ///         "permissions": [
        ///           {
        ///             "id": 1,
        ///             "permissionName": "Read Users",
        ///             "parentId": null,
        ///             "refCode": "USER_READ",
        ///             "description": "View user information",
        ///             "subPermissions": [
        ///               {
        ///                 "id": 2,
        ///                 "permissionName": "Read Admin Users",
        ///                 "parentId": 1,
        ///                 "refCode": "ADMIN_READ",
        ///                 "description": "View admin user details",
        ///                 "subPermissions": []
        ///               }
        ///             ]
        ///           }
        ///         ]
        ///       }
        ///     ]
        /// 
        /// </remarks>
        /// <returns>List of all modules with their permission trees</returns>
        [HttpGet("modules-with-permissions")]
        [Authorize(Roles = "SYSADMIN")]
        public async Task<IActionResult> GetAllModulesWithPermissions()
        {
            var response = await _moduleService.GetAllModulesWithPermissionsAsync();
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Data);
        }   
    }
}