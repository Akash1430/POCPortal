using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Interfaces;
using WebApi.Attributes;
using Core.ModuleAccess;
using Core.Module;
using Core.Feature;
using Models.Constants;
using Dtos;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModuleController : ControllerBase
{
    private readonly IModuleLogic _moduleLogic;

    public ModuleController(IModuleLogic moduleLogic)
    {
        _moduleLogic = moduleLogic;
    }

    /// <summary>
    /// Retrieves modules accessible to the current user
    /// </summary>
    /// <remarks>
    /// Gets a list of modules that the current user has access to based on their role.
    /// Modules are returned in a hierarchical structure with parent and child modules.
    /// Requires MODULE_READ permission.
    /// </remarks>
    /// <returns>List of accessible modules in hierarchical structure</returns>
    [HttpGet("modules")]
    [RequirePermission(Permissions.MODULE_READ)]
    public async Task<ActionResult<ApiResponse<ModulesDto>>> GetModules()
    {
        var response = await _moduleLogic.GetModulesAsync(User.FindFirstValue(ClaimTypes.Role)!);
        if (response.Success && response.Data != null)
        {
            return Ok(ApiResponse<ModulesDto>.SuccessResult(response.Data.ToDto(), response.Message));
        }
        return BadRequest(ApiResponse<ModulesDto>.ErrorResult(response.Message));
    }

    /// <summary>
    /// Retrieves all modules with their associated permissions (Admin only)
    /// </summary>
    /// <remarks>
    /// Gets a comprehensive list of all modules in the system along with their associated permissions.
    /// This endpoint is restricted to users with the SYSADMIN role only.
    /// Returns detailed permission trees for each module.
    /// </remarks>
    /// <returns>List of all modules with their permission trees</returns>
    [HttpGet("modules-with-permissions")]
    [Authorize(Roles = "SYSADMIN")]
    public async Task<ActionResult<ApiResponse<ModulesWithModuleAccessesDto>>> GetAllModulesWithPermissions()
    {
        var response = await _moduleLogic.GetAllModulesWithPermissionsAsync();
        if (response.Success && response.Data != null)
        {
            return Ok(ApiResponse<ModulesWithModuleAccessesDto>.SuccessResult(response.Data.ToDto(), response.Message));
        }
        return BadRequest(ApiResponse<ModulesWithModuleAccessesDto>.ErrorResult(response.Message));
    }
}