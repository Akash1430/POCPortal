using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Interfaces;
using Dtos;
using Models.Constants;
using WebApi.Attributes;
using Core.Auth;
using Core.User;

namespace WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthLogic _authLogic;

    public AuthController(IAuthLogic authLogic)
    {
        _authLogic = authLogic;
    }

    /// <summary>
    /// Authenticates a user and returns a JWT token
    /// </summary>
    /// <remarks>
    /// Authenticates a user with username and password, returns JWT access token and sets refresh token in HTTP-only cookie.
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

        var result = await _authLogic.LoginAsync(loginRequest.ToModel());
        
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

        return Ok(ApiResponse<LoginResponseDto>.SuccessResult(result.Data.ToDto(), result.Message));
    }

    /// <summary>
    /// Gets current user information from JWT token
    /// </summary>
    /// <remarks>
    /// Retrieves information about the currently authenticated user.
    /// </remarks>
    /// <returns>Current user information</returns>
    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<MeDto>>> GetCurrentUser()
    {
        var userName = User.FindFirstValue(ClaimTypes.Name);
        if (string.IsNullOrEmpty(userName))
        {
            return Unauthorized(ApiResponse<MeDto>.ErrorResult("User is not authenticated"));
        }

        var result = await _authLogic.GetCurrentUserAsync(userName);

        if (!result.Success || result.Data == null)
            return NotFound(result);

        return Ok(ApiResponse<MeDto>.SuccessResult(result.Data.ToMeDto(), result.Message));
    }

    /// <summary>
    /// Refreshes an access token using a valid refresh token from HTTP-only cookie
    /// </summary>
    /// <remarks>
    /// Generates a new access token using the refresh token stored in HTTP-only cookie. 
    /// Also updates the refresh token cookie with a new one.
    /// The refresh token must be present in the request cookies.
    /// </remarks>
    /// <returns>New access token</returns>
    [HttpPost("refresh-token")]
    public async Task<ActionResult<ApiResponse<RefreshTokenResponseDto>>> RefreshToken()
    {
        if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken) || string.IsNullOrEmpty(refreshToken))
        {
            return Unauthorized(ApiResponse<RefreshTokenResponseDto>.ErrorResult("Refresh token not found"));
        }

        var result = await _authLogic.RefreshTokenAsync(refreshToken);
        
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

        return Ok(ApiResponse<RefreshTokenResponseDto>.SuccessResult(result.Data.ToDto(), result.Message));
    }

    /// <summary>
    /// Logs out a user by revoking their refresh token and clearing the cookie
    /// </summary>
    /// <remarks>
    /// Logs out the user by revoking their refresh token from the database and clearing the HTTP-only cookie.
    /// This endpoint can be called even without authentication.
    /// </remarks>
    /// <returns>Logout confirmation</returns>
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse<string>>> Logout()
    {
        if (Request.Cookies.TryGetValue("refreshToken", out var refreshToken) && !string.IsNullOrEmpty(refreshToken))
        {
            await _authLogic.LogoutAsync(refreshToken);
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

        var result = await _authLogic.RevokeTokenAsync(revokeTokenRequest.ToModel());
        return Ok(result);
    }

    /// <summary>
    /// Revokes all refresh tokens for a specific user (Admin only)
    /// </summary>
    /// <remarks>
    /// Revokes all refresh tokens for a specific user. This is an admin-only operation.
    /// Requires authentication and appropriate permissions.
    /// </remarks>
    /// <param name="userId">User ID whose tokens should be revoked</param>
    /// <param name="reason">Reason for revocation</param>
    /// <returns>Revocation confirmation</returns>
    [HttpPost("revoke-all-tokens/{userId}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<string>>> RevokeAllUserTokens(int userId, [FromBody] string? reason = null)
    {
        var result = await _authLogic.RevokeAllUserTokensAsync(userId, reason ?? "Revoked by admin");
        return Ok(result);
    }

    /// <summary>
    /// Changes the current user's password
    /// </summary>
    /// <remarks>
    /// Changes the password for the currently authenticated user.
    /// Requires the USER_CHANGE_PASSWORD permission.
    /// All refresh tokens will be revoked after successful password change.
    /// </remarks>
    /// <param name="changePasswordRequest">Password change request with current and new password</param>
    /// <returns>Password change confirmation</returns>
    [HttpPost("change-password")]
    [Authorize]
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
        var result = await _authLogic.ChangePasswordAsync(userName, changePasswordRequest.ToModel());
        
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
    /// </remarks>
    /// <param name="userId">User ID whose password should be changed</param>
    /// <param name="changePasswordRequest">New password details</param>
    /// <returns>Password change confirmation</returns>
    [HttpPost("admin-change-password/{userId}")]
    [RequirePermission(Permissions.ADMIN_CHANGE_PASSWORD)]
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
        var result = await _authLogic.AdminChangePasswordAsync(userId, changePasswordRequest.ToModel(), adminUserName);
        
        return result.Success ? Ok(result) : BadRequest(result);
    }
}
