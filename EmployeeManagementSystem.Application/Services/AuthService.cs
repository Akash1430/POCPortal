using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using EmployeeManagementSystem.Application.Interfaces;
using EmployeeManagementSystem.Application.DTOs;
using EmployeeManagementSystem.Domain.Entities;

namespace EmployeeManagementSystem.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto loginRequest)
        {
            try
            {
                var user = await _unitOfWork.Users.FindFirstAsync(u =>
                    u.UserName == loginRequest.UserName);

                if (user == null || !VerifyPassword(loginRequest.Password, user.Password))
                {
                    return ApiResponse<LoginResponseDto>.ErrorResult("Invalid username or password");
                }

                if (user.IsFrozen)
                {
                    return ApiResponse<LoginResponseDto>.ErrorResult("User account is frozen");
                }

                user.LastLoginUTC = DateTime.UtcNow;
                await _unitOfWork.Users.UpdateAsync(user);
                await _unitOfWork.SaveChangesAsync();

                var userRole = await _unitOfWork.UserRoles.GetByIdAsync(user.UserRoleId);
                if (userRole == null)
                {
                    return ApiResponse<LoginResponseDto>.ErrorResult("User role not found");
                }

                var expiration = DateTime.UtcNow.AddSeconds(5); // Token valid for 15 minutes
                var accessToken = GenerateJwtToken(user, userRole, expiration);

                // Generate refresh token
                var refreshToken = await GenerateRefreshTokenAsync(user.Id);

                var userDto = new UserDto
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserName = user.UserName,
                    Email = user.Email,
                    RoleName = userRole.RoleName,
                    RoleRefCode = userRole.RefCode,
                    IsFrozen = user.IsFrozen,
                    LastLoginUTC = user.LastLoginUTC
                };

                var loginResponse = new LoginResponseDto
                {
                    AccessToken = accessToken,
                    AccessTokenExpiration = expiration,
                    RefreshToken = refreshToken.Token,
                    RefreshTokenExpiration = refreshToken.ExpiryDateUTC,
                    User = userDto
                };

                return ApiResponse<LoginResponseDto>.SuccessResult(loginResponse, "Login successful");
            }
            catch (Exception ex)
            {
                return ApiResponse<LoginResponseDto>.ErrorResult($"Login failed: {ex.Message}");
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

        private string GenerateJwtToken(User user, UserRole userRole, DateTime expiration)
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
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<ApiResponse<RefreshTokenResponseDto>> RefreshTokenAsync(string refreshTokenString)
        {
            try
            {
                var refreshToken = await _unitOfWork.RefreshTokens.FindFirstAsync(rt =>
                    rt.Token == refreshTokenString);

                if (refreshToken == null || !refreshToken.IsActive)
                {
                    return ApiResponse<RefreshTokenResponseDto>.ErrorResult("Invalid or expired refresh token");
                }

                var user = await _unitOfWork.Users.GetByIdAsync(refreshToken.UserId);
                if (user == null || user.IsFrozen)
                {
                    return ApiResponse<RefreshTokenResponseDto>.ErrorResult("User not found or account is frozen");
                }

                var userRole = await _unitOfWork.UserRoles.GetByIdAsync(user.UserRoleId);
                if (userRole == null)
                {
                    return ApiResponse<RefreshTokenResponseDto>.ErrorResult("User role not found");
                }

                // Revoke the old refresh token
                refreshToken.IsRevoked = true;
                refreshToken.RevokedDateUTC = DateTime.UtcNow;
                refreshToken.ReasonRevoked = "Replaced by new token";
                await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken);

                // Generate new tokens
                var accessTokenExpiration = DateTime.UtcNow.AddSeconds(15 * 60); // Token valid for 15 minutes
                var newAccessToken = GenerateJwtToken(user, userRole, accessTokenExpiration);
                var newRefreshToken = await GenerateRefreshTokenAsync(user.Id);

                // Replace the old token with the new one
                refreshToken.ReplacedByToken = newRefreshToken.Token;
                await _unitOfWork.RefreshTokens.UpdateAsync(refreshToken);
                await _unitOfWork.SaveChangesAsync();

                var refreshResponse = new RefreshTokenResponseDto
                {
                    AccessToken = newAccessToken,
                    AccessTokenExpiration = accessTokenExpiration,
                    RefreshToken = newRefreshToken.Token,
                    RefreshTokenExpiration = newRefreshToken.ExpiryDateUTC
                };

                return ApiResponse<RefreshTokenResponseDto>.SuccessResult(refreshResponse, "Token refreshed successfully");
            }
            catch (Exception ex)
            {
                return ApiResponse<RefreshTokenResponseDto>.ErrorResult($"Token refresh failed: {ex.Message}");
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

        public async Task<ApiResponse<string>> RevokeTokenAsync(RevokeTokenRequestDto revokeTokenRequest)
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

        private async Task<RefreshToken> GenerateRefreshTokenAsync(int userId)
        {
            var refreshToken = new RefreshToken
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
    }
}
