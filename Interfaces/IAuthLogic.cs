using Models;

namespace Interfaces;

public interface IAuthLogic
{
    Task<ApiResponse<LoginResponseModel>> LoginAsync(LoginRequestModel loginRequest);
    Task<ApiResponse<RegisterResponseModel>> RegisterAsync(string userName, RegisterRequestModel registerRequest);
    Task<ApiResponse<UserModel>> GetCurrentUserAsync(string userName);
    Task<ApiResponse<RefreshTokenResponseModel>> RefreshTokenAsync(string refreshToken);
    Task<ApiResponse<string>> LogoutAsync(string refreshToken);
    Task<ApiResponse<string>> RevokeTokenAsync(RevokeTokenRequestModel revokeTokenRequest);
    Task<ApiResponse<string>> RevokeAllUserTokensAsync(int userId, string reason = "Revoked by admin");
    Task<ApiResponse<string>> DeleteUserAsync(int userId);
    Task<ApiResponse<string>> DeleteUserAdminAsync(int userId);
    Task<ApiResponse<UserModel>> UpdateUserAsync(int userId, UpdateUserRequestModel updateRequest, string updatedBy);
    Task<ApiResponse<UserModel>> FreezeUnfreezeUserAsync(int userId, FreezeUserRequestModel freezeRequest, string updatedBy);
    Task<ApiResponse<string>> ChangePasswordAsync(string userName, ChangePasswordRequestModel changePasswordRequest);
    Task<ApiResponse<string>> AdminChangePasswordAsync(int userId, AdminChangePasswordRequestModel changePasswordRequest, string changedBy);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
    Task<ApiResponse<UsersResponseModel>> GetAllUsersAsync(UsersRequestModel request);
    Task<ApiResponse<UserModel>> GetUserByIdAsync(int userId);
}
