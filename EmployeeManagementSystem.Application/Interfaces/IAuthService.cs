using EmployeeManagementSystem.Application.DTOs;

namespace EmployeeManagementSystem.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest);
        Task<ApiResponse<RegisterResponseDto>> RegisterAsync(string userName, RegisterRequestDto registerRequest);
        Task<ApiResponse<RefreshTokenResponseDto>> RefreshTokenAsync(string refreshToken);
        Task<ApiResponse<string>> LogoutAsync(string refreshToken);
        Task<ApiResponse<string>> RevokeTokenAsync(RevokeTokenRequestDto revokeTokenRequest);
        Task<ApiResponse<string>> RevokeAllUserTokensAsync(int userId, string reason = "Revoked by admin");
        Task<ApiResponse<string>> DeleteUserAsync(int userId);
        Task<ApiResponse<string>> DeleteUserAdminAsync(int userId);
        Task<ApiResponse<UserDto>> UpdateUserAsync(int userId, UpdateUserRequestDto updateRequest, string updatedBy);
        Task<ApiResponse<UserDto>> FreezeUnfreezeUserAsync(int userId, FreezeUserRequestDto freezeRequest, string updatedBy);
        Task<ApiResponse<string>> ChangePasswordAsync(string userName, ChangePasswordRequestDto changePasswordRequest);
        Task<ApiResponse<string>> AdminChangePasswordAsync(int userId, AdminChangePasswordRequestDto changePasswordRequest, string changedBy);
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
        Task<ApiResponse<GetUsersResponseDto>> GetAllUsersAsync(GetUsersRequestDto request);
        Task<ApiResponse<UserDto>> GetUserByIdAsync(int userId);
    }
}
