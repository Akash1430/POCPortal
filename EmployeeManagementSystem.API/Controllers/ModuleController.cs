using Microsoft.AspNetCore.Mvc;
using EmployeeManagementSystem.Application.Interfaces;
using EmployeeManagementSystem.Application.Attributes;
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
    }
}