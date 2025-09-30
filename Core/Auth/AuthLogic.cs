using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Interfaces;
using DataAccess.Interfaces;
using Models;
using Core.User;
using Core.UserRole;
using Models.Constants;

namespace Core.Auth;

public class AuthLogic : IAuthLogic
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthLogic(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<ApiResponse<LoginResponseModel>> LoginAsync(LoginRequestModel loginRequest)
    {
        try
        {
            var user = await _unitOfWork.Users.FindFirstAsync(u =>
                u.UserName == loginRequest.UserName);

            if (user == null || !VerifyPassword(loginRequest.Password, user.Password))
            {
                return ApiResponse<LoginResponseModel>.ErrorResult("Invalid username or password");
            }

            if (user.IsFrozen)
            {
                return ApiResponse<LoginResponseModel>.ErrorResult("User account is frozen");
            }

            user.LastLoginUTC = DateTime.UtcNow;
            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            var userRole = await _unitOfWork.UserRoles.GetByIdAsync(user.UserRoleId);
            if (userRole == null)
            {
                return ApiResponse<LoginResponseModel>.ErrorResult("User role not found");
            }

            var expiration = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "15"));
            var accessToken = GenerateJwtToken(user.ToModel(), userRole.ToModel(), expiration);

            // Generate refresh token
            var refreshToken = await GenerateRefreshTokenAsync(user.Id);

            user.UserRoleId = userRole.Id;

            var loginResponse = new LoginResponseModel
            {
                AccessToken = accessToken,
                AccessTokenExpiration = expiration,
                RefreshToken = refreshToken.Token,
                RefreshTokenExpiration = refreshToken.ExpiryDateUTC,
                User = user.ToModel()
            };

            return ApiResponse<LoginResponseModel>.SuccessResult(loginResponse, "Login successful");
        }
        catch (Exception ex)
        {
            return ApiResponse<LoginResponseModel>.ErrorResult($"Login failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<RegisterResponseModel>> RegisterAsync(int userId, RegisterRequestModel registerRequest)
    {
        try
        {

            // Business rule: Cannot register SYSADMIN users
            if (string.Equals(registerRequest.RoleRefCode, UserRoles.SYSADMIN, StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<RegisterResponseModel>.ErrorResult("Cannot register a user with SYSADMIN role");
            }

            var createdBy = await _unitOfWork.Users.FindFirstAsync(u => u.Id == userId);
            if (createdBy == null)
            {
                return ApiResponse<RegisterResponseModel>.ErrorResult("Creator user not found");
            }

            var createdByUserRole = await _unitOfWork.UserRoles.GetByIdAsync(createdBy.UserRoleId);
            if (createdByUserRole == null)
            {
                return ApiResponse<RegisterResponseModel>.ErrorResult("Creator role not found");
            }

            // Business rule: Only SYSADMIN can create USERADMIN users
            if (string.Equals(registerRequest.RoleRefCode, UserRoles.USERADMIN, StringComparison.OrdinalIgnoreCase)
                && !string.Equals(createdByUserRole.RefCode, UserRoles.SYSADMIN, StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<RegisterResponseModel>.ErrorResult("Only SYSADMIN users can create USERADMIN users");
            }


            var existingUser = await _unitOfWork.Users.FindFirstAsync(u =>
                u.UserName == registerRequest.UserName || u.Email == registerRequest.Email);

            if (existingUser != null)
            {
                return ApiResponse<RegisterResponseModel>.ErrorResult("Username or email already exists");
            }

            var userRole = await _unitOfWork.UserRoles.FindFirstAsync(ur => ur.RefCode == registerRequest.RoleRefCode);
            if (userRole == null)
            {
                return ApiResponse<RegisterResponseModel>.ErrorResult("Invalid role reference code");
            }

            var newUser = new DataAccess.User
            {
                FirstName = registerRequest.FirstName,
                LastName = registerRequest.LastName,
                UserName = registerRequest.UserName,
                Email = registerRequest.Email,
                Password = HashPassword(registerRequest.Password),
                UserRoleId = userRole.Id,
                IsFrozen = false,
                CreatedBy = createdBy.Id,
                DateCreatedUTC = DateTime.UtcNow
            };

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();

            var registerResponse = new RegisterResponseModel
            {
                UserId = newUser.Id,
                UserName = newUser.UserName,
                Email = newUser.Email,
                RoleRefCode = userRole.RefCode
            };

            return ApiResponse<RegisterResponseModel>.SuccessResult(registerResponse, "Registration successful");
        }
        catch (Exception ex)
        {
            return ApiResponse<RegisterResponseModel>.ErrorResult($"Registration failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UserModel>> GetCurrentUserAsync(int userId)
    {
        try
        {
            var user = await _unitOfWork.Users.FindFirstAsync(u => u.Id == userId);
            if (user == null)
            {
                return ApiResponse<UserModel>.ErrorResult("User not found");
            }

            var userRole = await _unitOfWork.UserRoles.GetByIdAsync(user.UserRoleId);
            if (userRole == null)
            {
                return ApiResponse<UserModel>.ErrorResult("User role not found");
            }

            var userModel = new UserModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                RoleRefCode = userRole.RefCode,
                IsFrozen = user.IsFrozen,
                LastLoginUTC = user.LastLoginUTC
            };

            return ApiResponse<UserModel>.SuccessResult(userModel, "User retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<UserModel>.ErrorResult($"Failed to retrieve user: {ex.Message}");
        }
    }

    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }

    private string GenerateJwtToken(UserModel user, UserRoleModel userRole, DateTime expiration)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"] ?? "default-secret-key-that-should-be-changed-in-production");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.UserName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, userRole.RefCode),
            new("FirstName", user.FirstName),
            new("LastName", user.LastName),
            new("RoleName", userRole.RoleName)
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expiration,
            Audience = _configuration["Jwt:Audience"],
            Issuer = _configuration["Jwt:Issuer"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public async Task<ApiResponse<RefreshTokenResponseModel>> RefreshTokenAsync(string refreshTokenString)
    {
        try
        {
            var refreshToken = await _unitOfWork.RefreshTokens.FindFirstAsync(rt =>
                rt.Token == refreshTokenString);

            if (refreshToken == null || !refreshToken.IsActive)
            {
                return ApiResponse<RefreshTokenResponseModel>.ErrorResult("Invalid or expired refresh token");
            }

            var user = await _unitOfWork.Users.GetByIdAsync(refreshToken.UserId);
            if (user == null || user.IsFrozen)
            {
                return ApiResponse<RefreshTokenResponseModel>.ErrorResult("User not found or account is frozen");
            }

            var userRole = await _unitOfWork.UserRoles.GetByIdAsync(user.UserRoleId);
            if (userRole == null)
            {
                return ApiResponse<RefreshTokenResponseModel>.ErrorResult("User role not found");
            }

            // Revoke the old refresh token
            refreshToken.IsRevoked = true;
            refreshToken.RevokedDateUTC = DateTime.UtcNow;
            refreshToken.ReasonRevoked = "Replaced by new token";
            await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken);

            // Generate new tokens
            var accessTokenExpiration = DateTime.UtcNow.AddMinutes(int.Parse(_configuration["Jwt:ExpireMinutes"] ?? "15"));
            var newAccessToken = GenerateJwtToken(user.ToModel(), userRole.ToModel(), accessTokenExpiration);
            var newRefreshToken = await GenerateRefreshTokenAsync(user.Id);

            // Replace the old token with the new one
            refreshToken.ReplacedByToken = newRefreshToken.Token;
            await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            var refreshResponse = new RefreshTokenResponseModel
            {
                AccessToken = newAccessToken,
                AccessTokenExpiration = accessTokenExpiration,
                RefreshToken = newRefreshToken.Token,
                RefreshTokenExpiration = newRefreshToken.ExpiryDateUTC
            };

            return ApiResponse<RefreshTokenResponseModel>.SuccessResult(refreshResponse, "Token refreshed successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<RefreshTokenResponseModel>.ErrorResult($"Token refresh failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> LogoutAsync(string refreshTokenString)
    {
        try
        {
            var refreshToken = await _unitOfWork.RefreshTokens.FindFirstAsync(rt =>
                rt.Token == refreshTokenString);

            if (refreshToken != null && refreshToken.IsActive)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedDateUTC = DateTime.UtcNow;
                refreshToken.ReasonRevoked = "Logged out by user";
                await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken);
                await _unitOfWork.SaveChangesAsync();
            }

            return ApiResponse<string>.SuccessResult("Logged out successfully", "User logged out successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResult($"Logout failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> RevokeTokenAsync(RevokeTokenRequestModel revokeTokenRequest)
    {
        try
        {
            var refreshToken = await _unitOfWork.RefreshTokens.FindFirstAsync(rt =>
                rt.Token == revokeTokenRequest.RefreshToken);

            if (refreshToken == null)
            {
                return ApiResponse<string>.ErrorResult("Token not found");
            }

            if (!refreshToken.IsActive)
            {
                return ApiResponse<string>.ErrorResult("Token is already inactive");
            }

            refreshToken.IsRevoked = true;
            refreshToken.RevokedDateUTC = DateTime.UtcNow;
            refreshToken.ReasonRevoked = revokeTokenRequest.Reason ?? "Revoked by request";
            await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<string>.SuccessResult("Token revoked successfully", "Token has been revoked");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResult($"Token revocation failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> RevokeAllUserTokensAsync(int userId, string reason = "Revoked by admin")
    {
        try
        {
            var activeTokens = await _unitOfWork.RefreshTokens.FindAsync(rt =>
                rt.UserId == userId && rt.IsActive);

            foreach (var token in activeTokens)
            {
                token.IsRevoked = true;
                token.RevokedDateUTC = DateTime.UtcNow;
                token.ReasonRevoked = reason;
                await _unitOfWork.RefreshTokens.UpdateAsync(token);
            }

            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<string>.SuccessResult($"Revoked {activeTokens.Count()} tokens", "All user tokens have been revoked");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResult($"Token revocation failed: {ex.Message}");
        }
    }

    private async Task<DataAccess.RefreshToken> GenerateRefreshTokenAsync(int userId)
    {
        var refreshToken = new DataAccess.RefreshToken
        {
            Token = GenerateRefreshTokenString(),
            UserId = userId,
            ExpiryDateUTC = DateTime.UtcNow.AddDays(7), // Refresh token valid for 7 days
            CreatedBy = userId
        };

        await _unitOfWork.RefreshTokens.AddAsync(refreshToken);
        await _unitOfWork.SaveChangesAsync();

        return refreshToken;
    }

    private static string GenerateRefreshTokenString()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }

    public async Task<ApiResponse<string>> DeleteUserAdminAsync(int userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<string>.ErrorResult("User not found");
            }

            var userRole = await _unitOfWork.UserRoles.GetByIdAsync(user.UserRoleId);
            if (userRole != null && string.Equals(userRole.RefCode, UserRoles.SYSADMIN, StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<string>.ErrorResult("Cannot delete a user with SYSADMIN role");
            }

            if (userRole != null && !string.Equals(userRole.RefCode, UserRoles.USERADMIN, StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<string>.ErrorResult("Can only delete USERADMIN users via this method");
            }

            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<string>.SuccessResult("User deleted successfully", "User has been deleted");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResult($"User deletion failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> DeleteUserAsync(int userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<string>.ErrorResult("User not found");
            }

            var userRole = await _unitOfWork.UserRoles.GetByIdAsync(user.UserRoleId);
            if (userRole != null && string.Equals(userRole.RefCode, UserRoles.SYSADMIN, StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<string>.ErrorResult("Cannot delete a user with SYSADMIN role");
            }

            await _unitOfWork.Users.DeleteAsync(user);
            await _unitOfWork.SaveChangesAsync();

            return ApiResponse<string>.SuccessResult("User deleted successfully", "User has been deleted");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResult($"User deletion failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UsersResponseModel>> GetAllUsersAsync(UsersRequestModel request)
    {
        try
        {
            if (request.RoleRefs == null || request.RoleRefs.Length == 0)
            {
                return new ApiResponse<UsersResponseModel>
                {
                    Success = false,
                    Message = "RoleRefs cannot be empty."
                };
            }

            var userRoles = await _unitOfWork.UserRoles.FindAsync(ur => request.RoleRefs.Contains(ur.RefCode));
            if (userRoles == null || !userRoles.Any())
            {
                return new ApiResponse<UsersResponseModel>
                {
                    Success = false,
                    Message = "No valid user roles found for the provided RoleRefs."
                };
            }

            var roleIds = userRoles.Select(ur => ur.Id).ToList();
            var searchTerm = request.SearchTerm?.Trim();
            var searchIsEmpty = string.IsNullOrWhiteSpace(searchTerm);
            var searchLower = searchTerm?.ToLower() ?? string.Empty;

            var users = await _unitOfWork.Users.FindAsync(u =>
                roleIds.Contains(u.UserRoleId) &&
                (searchIsEmpty ||
                 (
                    (u.FirstName + " " + u.LastName).ToLower().Contains(searchLower) ||
                    u.UserName.ToLower().Contains(searchLower) ||
                    u.Email.ToLower().Contains(searchLower)
                 )
                )
            );

            if (users == null || !users.Any())
            {
                return new ApiResponse<UsersResponseModel>
                {
                    Success = true,
                    Data = new UsersResponseModel
                    {
                        Users = new List<UserModel>()
                    }
                };
            }

            var userModels = users
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(u => new UserModel
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    LastName = u.LastName,
                    UserName = u.UserName,
                    Email = u.Email,
                    RoleRefCode = userRoles.First((ur) => ur.Id == u.UserRoleId).RefCode,
                    IsFrozen = u.IsFrozen,
                    LastLoginUTC = u.LastLoginUTC
                })
                .ToList();

            return new ApiResponse<UsersResponseModel>
            {
                Success = true,
                Data = new UsersResponseModel
                {
                    Users = userModels,
                    TotalCount = users.Count(),
                    PageNumber = request.PageNumber,
                    PageSize = request.PageSize
                }
            };
        }
        catch (Exception ex)
        {
            return new ApiResponse<UsersResponseModel>
            {
                Success = false,
                Message = $"An error occurred while retrieving users: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<UserModel>> UpdateUserAsync(int userId, UpdateUserRequestModel updateRequest, int updatedBy)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserModel>.ErrorResult("User not found");
            }
            var currentUserRole = await _unitOfWork.UserRoles.GetByIdAsync(user.UserRoleId);
            if (string.Equals(currentUserRole!.RefCode, UserRoles.SYSADMIN) && user.Id != updatedBy)
            {
                return ApiResponse<UserModel>.ErrorResult("Cannot update SYSADMIN user");
            }

            // Check if username is already taken by another user
            var existingUserWithUsername = await _unitOfWork.Users.FindFirstAsync(u =>
                u.UserName == updateRequest.UserName && u.Id != userId);
            if (existingUserWithUsername != null)
            {
                return ApiResponse<UserModel>.ErrorResult("Username is already in use by another user");
            }

            // Check if email is already taken by another user
            var existingUserWithEmail = await _unitOfWork.Users.FindFirstAsync(u =>
                u.Email == updateRequest.Email && u.Id != userId);
            if (existingUserWithEmail != null)
            {
                return ApiResponse<UserModel>.ErrorResult("Email address is already in use by another user");
            }

            // Update user fields
            user.UserName = updateRequest.UserName;
            user.FirstName = updateRequest.FirstName;
            user.LastName = updateRequest.LastName;
            user.Email = updateRequest.Email;
            user.LatestDateUpdatedUTC = DateTime.UtcNow;
            user.LatestUpdatedBy = updatedBy;

            // Update role if provided and user is not SYSADMIN
            if (!string.IsNullOrEmpty(updateRequest.RoleRefCode))
            {
                if (currentUserRole?.RefCode == UserRoles.SYSADMIN)
                {
                    return ApiResponse<UserModel>.ErrorResult("Cannot change role of SYSADMIN user");
                }

                var newRole = await _unitOfWork.UserRoles.FindFirstAsync(r => r.RefCode == updateRequest.RoleRefCode);
                if (newRole == null)
                {
                    return ApiResponse<UserModel>.ErrorResult("Invalid role specified");
                }

                if (newRole.RefCode == UserRoles.SYSADMIN)
                {
                    return ApiResponse<UserModel>.ErrorResult("Cannot assign SYSADMIN role");
                }

                user.UserRoleId = newRole.Id;
            }

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Get updated user with role info
            var updatedUserRole = await _unitOfWork.UserRoles.GetByIdAsync(user.UserRoleId);

            var userModel = new UserModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                RoleRefCode = updatedUserRole?.RefCode ?? string.Empty,
                IsFrozen = user.IsFrozen,
                LastLoginUTC = user.LastLoginUTC
            };

            return ApiResponse<UserModel>.SuccessResult(userModel, "User updated successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<UserModel>.ErrorResult($"An error occurred while updating user: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UserModel>> FreezeUnfreezeUserAsync(int userId, FreezeUserRequestModel freezeRequest, int updatedBy)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserModel>.ErrorResult("User not found");
            }

            // Prevent freezing SYSADMIN users
            var userRole = await _unitOfWork.UserRoles.GetByIdAsync(user.UserRoleId);
            if (userRole?.RefCode == UserRoles.SYSADMIN)
            {
                return ApiResponse<UserModel>.ErrorResult("Cannot freeze/unfreeze SYSADMIN users");
            }

            user.IsFrozen = freezeRequest.IsFrozen;
            user.LatestDateUpdatedUTC = DateTime.UtcNow;
            user.LatestUpdatedBy = updatedBy;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // If freezing user, revoke all their tokens
            if (freezeRequest.IsFrozen)
            {
                await RevokeAllUserTokensAsync(userId, $"User account frozen by {updatedBy}");
            }

            var userModel = new UserModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                RoleRefCode = userRole?.RefCode ?? string.Empty,
                IsFrozen = user.IsFrozen,
                LastLoginUTC = user.LastLoginUTC
            };

            var message = freezeRequest.IsFrozen ? "User frozen successfully" : "User unfrozen successfully";
            return ApiResponse<UserModel>.SuccessResult(userModel, message);
        }
        catch (Exception ex)
        {
            return ApiResponse<UserModel>.ErrorResult($"An error occurred while updating user status: {ex.Message}");
        }
    }

    public async Task<ApiResponse<UserModel>> GetUserByIdAsync(int userId)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<UserModel>.ErrorResult("User not found");
            }

            var userRole = await _unitOfWork.UserRoles.GetByIdAsync(user.UserRoleId);

            if (userRole == null)
            {
                return ApiResponse<UserModel>.ErrorResult("User role not found");
            }

            if (string.Equals(userRole.RefCode, UserRoles.SYSADMIN, StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<UserModel>.ErrorResult("Access to SYSADMIN user details is restricted");
            }

            var userModel = new UserModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                RoleRefCode = userRole?.RefCode ?? string.Empty,
                IsFrozen = user.IsFrozen,
                LastLoginUTC = user.LastLoginUTC
            };

            return ApiResponse<UserModel>.SuccessResult(userModel, "User retrieved successfully");
        }
        catch (Exception ex)
        {
            return ApiResponse<UserModel>.ErrorResult($"Failed to retrieve user: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> ChangePasswordAsync(int userId, ChangePasswordRequestModel changePasswordRequest)
    {
        try
        {
            var user = await _unitOfWork.Users.FindFirstAsync(u => u.Id == userId);
            if (user == null)
            {
                return ApiResponse<string>.ErrorResult("User not found");
            }

            if (user.IsFrozen)
            {
                return ApiResponse<string>.ErrorResult("Account is frozen. Password cannot be changed");
            }

            // Verify current password
            if (!VerifyPassword(changePasswordRequest.CurrentPassword, user.Password))
            {
                return ApiResponse<string>.ErrorResult("Current password is incorrect");
            }

            // Hash and update new password
            user.Password = HashPassword(changePasswordRequest.NewPassword);
            user.LatestUpdatedBy = user.Id; // Self-update
            user.PasswordChangedUTC = DateTime.UtcNow;
            user.LatestDateUpdatedUTC = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Revoke all existing refresh tokens for security
            await RevokeAllUserTokensAsync(user.Id, "Password changed - security measure");

            return ApiResponse<string>.SuccessResult("Password changed successfully", "Your password has been updated. Please log in again.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResult($"Password change failed: {ex.Message}");
        }
    }

    public async Task<ApiResponse<string>> AdminChangePasswordAsync(int userId, AdminChangePasswordRequestModel changePasswordRequest, int changedBy)
    {
        try
        {
            var user = await _unitOfWork.Users.GetByIdAsync(userId);
            if (user == null)
            {
                return ApiResponse<string>.ErrorResult("User not found");
            }

            var userRole = await _unitOfWork.UserRoles.GetByIdAsync(user.UserRoleId);
            if (userRole != null && string.Equals(userRole.RefCode, UserRoles.SYSADMIN, StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<string>.ErrorResult("Cannot change password for SYSADMIN users");
            }

            // Get the admin user making the change
            var adminUser = await _unitOfWork.Users.FindFirstAsync(u => u.Id == changedBy);
            if (adminUser == null)
            {
                return ApiResponse<string>.ErrorResult("Admin user not found");
            }

            var adminRole = await _unitOfWork.UserRoles.GetByIdAsync(adminUser.UserRoleId);
            if (adminRole == null)
            {
                return ApiResponse<string>.ErrorResult("Admin role not found");
            }

            // Business rule: Only SYSADMIN and USERADMIN can change passwords
            if (!string.Equals(adminRole.RefCode, UserRoles.SYSADMIN, StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(adminRole.RefCode, UserRoles.USERADMIN, StringComparison.OrdinalIgnoreCase))
            {
                return ApiResponse<string>.ErrorResult("Insufficient privileges to change user passwords");
            }

            // Hash and update new password
            user.Password = HashPassword(changePasswordRequest.NewPassword);
            user.LatestUpdatedBy = adminUser.Id;
            user.PasswordChangedUTC = null;
            user.LatestDateUpdatedUTC = DateTime.UtcNow;

            await _unitOfWork.Users.UpdateAsync(user);
            await _unitOfWork.SaveChangesAsync();

            // Revoke all existing refresh tokens for security
            await RevokeAllUserTokensAsync(user.Id, $"Password changed by admin: {changedBy}");

            return ApiResponse<string>.SuccessResult("Password changed successfully", $"Password for user {user.UserName} has been updated by admin.");
        }
        catch (Exception ex)
        {
            return ApiResponse<string>.ErrorResult($"Admin password change failed: {ex.Message}");
        }
    }
}