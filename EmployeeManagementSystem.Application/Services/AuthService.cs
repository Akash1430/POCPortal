using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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

                var token = GenerateJwtToken(user, userRole);
                var expiration = DateTime.UtcNow.AddHours(24);

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

                var response = new LoginResponseDto
                {
                    Token = token,
                    Expiration = expiration,
                    User = userDto
                };

                return ApiResponse<LoginResponseDto>.SuccessResult(response, "Login successful");
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

        private string GenerateJwtToken(User user, UserRole userRole)
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
                Expires = DateTime.UtcNow.AddHours(24),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
