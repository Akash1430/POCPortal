using Microsoft.AspNetCore.Mvc;
using Interfaces;
using WebApi.Attributes;
using Dtos;
using Core.Auth;
using Core.User;
using Models.Constants;
using System.Security.Claims;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserAdminController : ControllerBase
{

    private readonly IAuthLogic _authLogic;

    public UserAdminController(IAuthLogic authLogic)
    {
        _authLogic = authLogic;
    }

    /// <summary>
    /// Retrieves a specific user by their ID
    /// </summary>
    /// <remarks>
    /// Gets detailed information about a specific user by their unique identifier.
    /// Requires ADMIN_READ permission.
    /// </remarks>
    /// <param name="userId">The unique identifier of the user to retrieve</param>
    /// <returns>User information</returns>
    [HttpGet("users/{userId}")]
    [RequirePermission(Permissions.ADMIN_READ)]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetAdminById(int userId)
    {
        var response = await _authLogic.GetUserByIdAsync(userId);
        if (response.Success && response.Data != null)
        {
            return Ok(ApiResponse<UserDto>.SuccessResult(response.Data.ToDto(), response.Message));
        }

        return BadRequest(ApiResponse<UserDto>.ErrorResult(response.Message));
    }

    /// <summary>
    /// Retrieves a paginated list of all admin users
    /// </summary>
    /// <remarks>
    /// Gets a paginated list of all users with admin roles, with optional search and filtering.
    /// Requires ADMIN_READ permission.
    /// </remarks>
    /// <param name="request">Pagination and search parameters</param>
    /// <returns>Paginated list of admin users</returns>
    [HttpGet("users")]
    [RequirePermission(Permissions.ADMIN_READ)]
    public async Task<ActionResult<ApiResponse<UsersResponseDto>>> GetAllAdmins([FromQuery] UsersRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<UsersResponseDto>.ErrorResult(errors));
        }
        request.RoleRefs = [UserRoles.USERADMIN];
        var response = await _authLogic.GetAllUsersAsync(request.ToModel());

        if (response.Success && response.Data != null)
        {
            return Ok(ApiResponse<UsersResponseDto>.SuccessResult(response.Data.ToDto(), response.Message));
        }

        return BadRequest(ApiResponse<UsersResponseDto>.ErrorResult(response.Message));
    }

    /// <summary>
    /// Creates a new admin user
    /// </summary>
    /// <remarks>
    /// Creates a new user with admin role (USERADMIN).
    /// Requires ADMIN_CREATE permission.
    /// </remarks>
    /// <param name="userDto">User creation details</param>
    /// <returns>Created user information</returns>
    [HttpPost("users")]
    [RequirePermission(Permissions.ADMIN_CREATE)]
    public async Task<ActionResult<ApiResponse<RegisterResponseDto>>> CreateAdmin([FromBody] RegisterRequestDto userDto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<RegisterResponseDto>.ErrorResult(errors));
        }

        userDto.RoleRefCode = UserRoles.USERADMIN;
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _authLogic.RegisterAsync(userId, userDto.ToModel());
        if (response.Success && response.Data != null)
        {
            return Ok(ApiResponse<RegisterResponseDto>.SuccessResult(response.Data.ToDto(), response.Message));
        }

        return BadRequest(ApiResponse<RegisterResponseDto>.ErrorResult(response.Message));
    }

    /// <summary>
    /// Deletes an admin user account
    /// </summary>
    /// <remarks>
    /// Permanently deletes an admin user account from the system.
    /// Requires ADMIN_DELETE permission.
    /// </remarks>
    /// <param name="userId">The unique identifier of the user to delete</param>
    /// <returns>Deletion confirmation</returns>
    [HttpDelete("users/{userId}")]
    [RequirePermission(Permissions.ADMIN_DELETE)]
    public async Task<ActionResult<ApiResponse<string>>> DeleteUserAdmin(int userId)
    {
        var response = await _authLogic.DeleteUserAdminAsync(userId);
        if (!response.Success)
        {
            return BadRequest(ApiResponse<string>.ErrorResult(response.Message));
        }

        return Ok(ApiResponse<string>.SuccessResult("User deleted successfully", "User deleted successfully"));
    }

    /// <summary>
    /// Updates an existing admin user's information
    /// </summary>
    /// <remarks>
    /// Updates the profile information of an existing admin user.
    /// Requires ADMIN_UPDATE permission.
    /// </remarks>
    /// <param name="userId">The unique identifier of the user to update</param>
    /// <param name="updateRequest">Updated user information</param>
    /// <returns>Updated user information</returns>
    [HttpPut("users/{userId}")]
    [RequirePermission(Permissions.ADMIN_UPDATE)]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateAdmin(int userId, [FromBody] UpdateUserRequestDto updateRequest)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<UserDto>.ErrorResult(errors));
        }

        var updatedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _authLogic.UpdateUserAsync(userId, updateRequest.ToModel(), updatedBy);
        if (response.Success && response.Data != null)
        {
            return Ok(ApiResponse<UserDto>.SuccessResult(response.Data.ToDto(), "User updated successfully"));
        }
        return BadRequest(ApiResponse<UserDto>.ErrorResult(response.Message));
    }

    /// <summary>
    /// Freezes or unfreezes an admin user account
    /// </summary>
    /// <remarks>
    /// Changes the frozen status of an admin user account.
    /// Frozen users cannot log in to the system.
    /// Requires ADMIN_UPDATE permission.
    /// </remarks>
    /// <param name="userId">The unique identifier of the user to freeze/unfreeze</param>
    /// <param name="freezeRequest">Freeze status request</param>
    /// <returns>Updated user information</returns>
    [HttpPatch("users/{userId}/freeze")]
    [RequirePermission(Permissions.ADMIN_UPDATE)]
    public async Task<ActionResult<ApiResponse<UserDto>>> FreezeUnfreezeAdmin(int userId, [FromBody] FreezeUserRequestDto freezeRequest)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(ApiResponse<UserDto>.ErrorResult(errors));
        }

        var updatedBy = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var response = await _authLogic.FreezeUnfreezeUserAsync(userId, freezeRequest.ToModel(), updatedBy);
        if (response.Success && response.Data != null)
        {
            return Ok(ApiResponse<UserDto>.SuccessResult(response.Data.ToDto(), "User freeze status updated successfully"));
        }
        return BadRequest(ApiResponse<UserDto>.ErrorResult(response.Message));
    }


}