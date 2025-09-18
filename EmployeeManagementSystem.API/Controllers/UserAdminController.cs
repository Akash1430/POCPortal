using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagementSystem.Application.Interfaces;
using EmployeeManagementSystem.Application.DTOs;
using EmployeeManagementSystem.Application.Attributes;

namespace EmployeeManagementSystem.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserAdminController : ControllerBase
    {

        private readonly IAuthService _authService;

        public UserAdminController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet("users/{userId}")]
        [RequirePermission("ADMIN_READ")]
        public async Task<IActionResult> GetAdminById(int userId)
        {
            var response = await _authService.GetUserByIdAsync(userId);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }

            return Ok(response);
        }

        [HttpGet("users")]
        [RequirePermission("ADMIN_READ")]
        public async Task<IActionResult> GetAllAdmins([FromQuery] GetUsersRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<GetUsersResponseDto>.ErrorResult(errors));
            }
            request.RoleRefs = new[] { "USERADMIN" };
            var response = await _authService.GetAllUsersAsync(request);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }

            return Ok(response);
        }

        [HttpPost("users")]
        [RequirePermission("ADMIN_CREATE")]
        public async Task<IActionResult> CreateAdmin([FromBody] RegisterRequestDto userDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<object>.ErrorResult(errors));
            }

            userDto.RoleRefCode = "USERADMIN";
            var response = await _authService.RegisterAsync(User.Identity!.Name!, new RegisterRequestDto
            {
                UserName = userDto.UserName,
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                Password = userDto.Password,
                RoleRefCode = userDto.RoleRefCode
            });
            if (!response.Success || response.Data == null)
            {
                return BadRequest(response.Message);
            }

            return CreatedAtAction(nameof(GetAllAdmins), new { id = response.Data.UserId }, response);
        }

        [HttpDelete("users/{userId}")]
        [RequirePermission("ADMIN_DELETE")]
        public async Task<IActionResult> DeleteUserAdmin(int userId)
        {
            var response = await _authService.DeleteUserAdminAsync(userId);
            if (!response.Success)
            {
                return BadRequest(response.Message);
            }

            return Ok(response);
        }

        [HttpPut("users/{userId}")]
        [RequirePermission("ADMIN_UPDATE")]
        public async Task<IActionResult> UpdateAdmin(int userId, [FromBody] UpdateUserRequestDto updateRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<UserDto>.ErrorResult(errors));
            }

            var response = await _authService.UpdateUserAsync(userId, updateRequest, User.Identity!.Name!);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [HttpPatch("users/{userId}/freeze")]
        [RequirePermission("ADMIN_UPDATE")]
        public async Task<IActionResult> FreezeUnfreezeAdmin(int userId, [FromBody] FreezeUserRequestDto freezeRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<UserDto>.ErrorResult(errors));
            }

            var response = await _authService.FreezeUnfreezeUserAsync(userId, freezeRequest, User.Identity!.Name!);
            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        
    }
}