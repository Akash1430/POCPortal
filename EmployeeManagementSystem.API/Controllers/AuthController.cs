using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using EmployeeManagementSystem.Application.Interfaces;
using EmployeeManagementSystem.Application.DTOs;
using EmployeeManagementSystem.Application.Attributes;
using System.Security.Claims;

namespace EmployeeManagement.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <remarks>
        /// Authenticates a user with username and password, returns JWT access token and sets refresh token in HTTP-only cookie.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/auth/login
        ///     {
        ///        "userName": "admin",
        ///        "password": "Admin123!"
        ///     }
        /// 
        /// </remarks>
        /// <param name="loginRequest">User login credentials</param>
        /// <returns>JWT token and user information</returns>
        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login([FromBody] LoginRequestDto loginRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<LoginResponseDto>.ErrorResult(errors));
            }

            var result = await _authService.LoginAsync(loginRequest);
            
            if (!result.Success)
                return Unauthorized(result);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.Data!.RefreshTokenExpiration
            };
            Response.Cookies.Append("refreshToken", result.Data.RefreshToken, cookieOptions);

            return Ok(ApiResponse<LoginResponseDto>.SuccessResult(result.Data, result.Message));
        }

        /// <summary>
        /// Gets current user information from JWT token
        /// </summary>
        /// <remarks>
        /// Retrieves information about the currently authenticated user based on JWT token claims.
        /// Requires a valid Bearer token in the Authorization header.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "User information retrieved successfully",
        ///       "data": {
        ///         "id": 1,
        ///         "userName": "admin",
        ///         "email": "admin@company.com",
        ///         "role": "SYSADMIN",
        ///         "firstName": "Admin",
        ///         "lastName": "User",
        ///         "roleName": "System Administrator"
        ///       }
        ///     }
        /// 
        /// </remarks>
        /// <returns>Current user information</returns>
        [HttpGet("me")]
        [Authorize]
        public ActionResult<ApiResponse<object>> GetCurrentUser()
        {
            var userInfo = new
            {
                Id = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!),
                UserName = User.FindFirstValue(ClaimTypes.Name),
                Email = User.FindFirstValue(ClaimTypes.Email),
                Role = User.FindFirstValue(ClaimTypes.Role),
                FirstName = User.FindFirstValue("FirstName"),
                LastName = User.FindFirstValue("LastName"),
                RoleName = User.FindFirstValue("RoleName")
            };

            return Ok(ApiResponse<object>.SuccessResult(userInfo, "User information retrieved successfully"));
        }

        /// <summary>
        /// Refreshes an access token using a valid refresh token from HTTP-only cookie
        /// </summary>
        /// <remarks>
        /// Generates a new access token using the refresh token stored in HTTP-only cookie. 
        /// Also updates the refresh token cookie with a new one.
        /// The refresh token must be present in the request cookies.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "Token refreshed successfully",
        ///       "data": {
        ///         "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
        ///         "accessTokenExpiration": "2024-09-19T16:30:00Z"
        ///       }
        ///     }
        /// 
        /// </remarks>
        /// <returns>New access token</returns>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<ApiResponse<RefreshTokenResponseDto>>> RefreshToken()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
            {
                return Unauthorized(ApiResponse<RefreshTokenResponseDto>.ErrorResult("Refresh token not found"));
            }

            var result = await _authService.RefreshTokenAsync(refreshToken);
            
            if (!result.Success)
                return Unauthorized(result);

            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = result.Data!.RefreshTokenExpiration
            };
            Response.Cookies.Append("refreshToken", result.Data.RefreshToken, cookieOptions);

            return Ok(ApiResponse<RefreshTokenResponseDto>.SuccessResult(result.Data, result.Message));
        }

        /// <summary>
        /// Logs out a user by revoking their refresh token and clearing the cookie
        /// </summary>
        /// <remarks>
        /// Logs out the user by revoking their refresh token from the database and clearing the HTTP-only cookie.
        /// This endpoint can be called even without authentication.
        /// 
        /// Sample response:
        /// 
        ///     {
        ///       "success": true,
        ///       "message": "User logged out successfully",
        ///       "data": "Logged out successfully"
        ///     }
        /// 
        /// </remarks>
        /// <returns>Logout confirmation</returns>
        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<string>>> Logout()
        {
            if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken) && !string.IsNullOrEmpty(refreshToken))
            {
                await _authService.LogoutAsync(refreshToken);
            }

            Response.Cookies.Delete("refreshToken");

            return Ok(ApiResponse<string>.SuccessResult("Logged out successfully", "User logged out successfully"));
        }

        /// <summary>
        /// Revokes a specific refresh token
        /// </summary>
        /// <remarks>
        /// Revokes a specific refresh token provided in the request body.
        /// Requires authentication.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/auth/revoke-token
        ///     {
        ///       "refreshToken": "abc123def456...",
        ///       "reason": "Security concern"
        ///     }
        /// 
        /// </remarks>
        /// <param name="revokeTokenRequest">Token revocation request</param>
        /// <returns>Revocation confirmation</returns>
        [HttpPost("revoke-token")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> RevokeToken([FromBody] RevokeTokenRequestDto revokeTokenRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<string>.ErrorResult(errors));
            }

            var result = await _authService.RevokeTokenAsync(revokeTokenRequest);
            return Ok(result);
        }

        /// <summary>
        /// Revokes all refresh tokens for a specific user (Admin only)
        /// </summary>
        /// <remarks>
        /// Revokes all refresh tokens for a specific user. This is an admin-only operation.
        /// Requires authentication and appropriate permissions.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/auth/revoke-all-tokens/5
        ///     "Suspicious activity detected"
        /// 
        /// </remarks>
        /// <param name="userId">User ID whose tokens should be revoked</param>
        /// <param name="reason">Reason for revocation</param>
        /// <returns>Revocation confirmation</returns>
        [HttpPost("revoke-all-tokens/{userId}")]
        [Authorize]
        public async Task<ActionResult<ApiResponse<string>>> RevokeAllUserTokens(int userId, [FromBody] string? reason = null)
        {
            var result = await _authService.RevokeAllUserTokensAsync(userId, reason ?? "Revoked by admin");
            return Ok(result);
        }

        /// <summary>
        /// Changes the current user's password
        /// </summary>
        /// <remarks>
        /// Changes the password for the currently authenticated user.
        /// Requires the USER_CHANGE_PASSWORD permission.
        /// All refresh tokens will be revoked after successful password change.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/auth/change-password
        ///     {
        ///       "currentPassword": "OldPass123!",
        ///       "newPassword": "NewPass456!",
        ///       "confirmPassword": "NewPass456!"
        ///     }
        /// 
        /// </remarks>
        /// <param name="changePasswordRequest">Password change request with current and new password</param>
        /// <returns>Password change confirmation</returns>
        [HttpPost("change-password")]
        [RequirePermission("USER_CHANGE_PASSWORD")]
        public async Task<ActionResult<ApiResponse<string>>> ChangePassword([FromBody] ChangePasswordRequestDto changePasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<string>.ErrorResult(errors));
            }

            var userName = User.Identity!.Name!;
            var result = await _authService.ChangePasswordAsync(userName, changePasswordRequest);
            
            if (!result.Success)
                return BadRequest(result);

            Response.Cookies.Delete("refreshToken");
            
            return Ok(result);
        }

        /// <summary>
        /// Admin endpoint to change another user's password
        /// </summary>
        /// <remarks>
        /// Allows administrators to change another user's password.
        /// Requires the ADMIN_CHANGE_PASSWORD permission.
        /// The target user's refresh tokens will be revoked after successful password change.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/auth/admin-change-password/5
        ///     {
        ///       "newPassword": "NewPass789!",
        ///       "confirmPassword": "NewPass789!"
        ///     }
        /// 
        /// </remarks>
        /// <param name="userId">User ID whose password should be changed</param>
        /// <param name="changePasswordRequest">New password details</param>
        /// <returns>Password change confirmation</returns>
        [HttpPost("admin-change-password/{userId}")]
        [RequirePermission("ADMIN_CHANGE_PASSWORD")]
        public async Task<ActionResult<ApiResponse<string>>> AdminChangePassword(int userId, [FromBody] AdminChangePasswordRequestDto changePasswordRequest)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(ApiResponse<string>.ErrorResult(errors));
            }

            var adminUserName = User.Identity!.Name!;
            var result = await _authService.AdminChangePasswordAsync(userId, changePasswordRequest, adminUserName);
            
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}
