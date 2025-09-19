using Microsoft.AspNetCore.Mvc;
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

        /// <summary>
        /// Retrieves a specific user by their ID
        /// </summary>
        /// <remarks>
        /// Gets detailed information about a specific user by their unique identifier.
        /// Requires ADMIN_READ permission.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "User retrieved successfully",
        ///       "data": {
        ///         "id": 5,
        ///         "firstName": "John",
        ///         "lastName": "Doe",
        ///         "userName": "john.doe",
        ///         "email": "john.doe@company.com",
        ///         "roleRefCode": "USERADMIN",
        ///         "isFrozen": false,
        ///         "lastLoginUTC": "2024-09-18T10:30:00Z"
        ///       }
        ///     }
        /// 
        /// </remarks>
        /// <param name="userId">The unique identifier of the user to retrieve</param>
        /// <returns>User information</returns>
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

        /// <summary>
        /// Retrieves a paginated list of all admin users
        /// </summary>
        /// <remarks>
        /// Gets a paginated list of all users with admin roles, with optional search and filtering.
        /// Requires ADMIN_READ permission.
        /// 
        /// Sample request:
        /// 
        ///     GET /api/useradmin/users?searchTerm=john&amp;pageNumber=1&amp;pageSize=10
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "Users retrieved successfully",
        ///       "data": {
        ///         "users": [
        ///           {
        ///             "id": 5,
        ///             "firstName": "John",
        ///             "lastName": "Doe",
        ///             "userName": "john.doe",
        ///             "email": "john.doe@company.com",
        ///             "roleRefCode": "USERADMIN",
        ///             "isFrozen": false,
        ///             "lastLoginUTC": "2024-09-18T10:30:00Z"
        ///           }
        ///         ],
        ///         "totalCount": 1,
        ///         "pageNumber": 1,
        ///         "pageSize": 10
        ///       }
        ///     }
        /// 
        /// </remarks>
        /// <param name="request">Pagination and search parameters</param>
        /// <returns>Paginated list of admin users</returns>
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

        /// <summary>
        /// Creates a new admin user
        /// </summary>
        /// <remarks>
        /// Creates a new user with admin role (USERADMIN).
        /// Requires ADMIN_CREATE permission.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/useradmin/users
        ///     {
        ///       "firstName": "Alice",
        ///       "lastName": "Johnson",
        ///       "userName": "alice.johnson",
        ///       "email": "alice.johnson@company.com",
        ///       "password": "TempPass123!"
        ///     }
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "User created successfully",
        ///       "data": {
        ///         "userId": 7,
        ///         "userName": "alice.johnson",
        ///         "email": "alice.johnson@company.com",
        ///         "roleRefCode": "USERADMIN"
        ///       }
        ///     }
        /// 
        /// </remarks>
        /// <param name="userDto">User creation details</param>
        /// <returns>Created user information</returns>
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

        /// <summary>
        /// Deletes an admin user account
        /// </summary>
        /// <remarks>
        /// Permanently deletes an admin user account from the system.
        /// Requires ADMIN_DELETE permission.
        /// This action cannot be undone.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "User deleted successfully",
        ///       "data": "User deleted successfully"
        ///     }
        /// 
        /// </remarks>
        /// <param name="userId">The unique identifier of the user to delete</param>
        /// <returns>Deletion confirmation</returns>
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

        /// <summary>
        /// Updates an existing admin user's information
        /// </summary>
        /// <remarks>
        /// Updates the profile information of an existing admin user.
        /// Requires ADMIN_UPDATE permission.
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/useradmin/users/7
        ///     {
        ///       "firstName": "Alice",
        ///       "lastName": "Johnson-Smith",
        ///       "email": "alice.johnson-smith@company.com"
        ///     }
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "User updated successfully",
        ///       "data": {
        ///         "id": 7,
        ///         "firstName": "Alice",
        ///         "lastName": "Johnson-Smith",
        ///         "userName": "alice.johnson",
        ///         "email": "alice.johnson-smith@company.com",
        ///         "roleRefCode": "USERADMIN",
        ///         "isFrozen": false,
        ///         "lastLoginUTC": null
        ///       }
        ///     }
        /// 
        /// </remarks>
        /// <param name="userId">The unique identifier of the user to update</param>
        /// <param name="updateRequest">Updated user information</param>
        /// <returns>Updated user information</returns>
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

        /// <summary>
        /// Freezes or unfreezes an admin user account
        /// </summary>
        /// <remarks>
        /// Changes the frozen status of an admin user account.
        /// Frozen users cannot log in to the system.
        /// Requires ADMIN_UPDATE permission.
        /// 
        /// Sample request to freeze a user:
        /// 
        ///     PATCH /api/useradmin/users/7/freeze
        ///     {
        ///       "isFrozen": true
        ///     }
        /// 
        /// Sample request to unfreeze a user:
        /// 
        ///     PATCH /api/useradmin/users/7/freeze
        ///     {
        ///       "isFrozen": false
        ///     }
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "User frozen successfully",
        ///       "data": {
        ///         "id": 7,
        ///         "firstName": "Alice",
        ///         "lastName": "Johnson-Smith",
        ///         "userName": "alice.johnson",
        ///         "email": "alice.johnson-smith@company.com",
        ///         "roleRefCode": "USERADMIN",
        ///         "isFrozen": true,
        ///         "lastLoginUTC": null
        ///       }
        ///     }
        /// 
        /// </remarks>
        /// <param name="userId">The unique identifier of the user to freeze/unfreeze</param>
        /// <param name="freezeRequest">Freeze status request</param>
        /// <returns>Updated user information</returns>
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