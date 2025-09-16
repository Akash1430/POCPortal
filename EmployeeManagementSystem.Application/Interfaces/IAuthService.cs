using EmployeeManagementSystem.Application.DTOs;

namespace EmployeeManagementSystem.Application.Interfaces
{
    public interface IAuthService
    {
        Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest);
        Task<ApiResponse<RefreshTokenResponseDto>> RefreshTokenAsync(string refreshToken);
        Task<ApiResponse<string>> LogoutAsync(string refreshToken);
        Task<ApiResponse<string>> RevokeTokenAsync(RevokeTokenRequestDto revokeTokenRequest);
        Task<ApiResponse<string>> RevokeAllUserTokensAsync(int userId, string reason = "Revoked by admin");
        string HashPassword(string password);
        bool VerifyPassword(string password, string hashedPassword);
    }
}
