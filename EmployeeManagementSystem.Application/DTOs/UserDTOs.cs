using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EmployeeManagementSystem.Application.DTOs
{
    public class LoginRequestDto
    {
        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiration { get; set; }
        public UserDto User { get; set; } = null!;
        
        // Properties for cookie handling only - not exposed in API response
        [JsonIgnore]
        public string RefreshToken { get; set; } = string.Empty;
        [JsonIgnore]
        public DateTime RefreshTokenExpiration { get; set; }
    }

    public class UserDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
        public string RoleRefCode { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public bool IsFrozen { get; set; }
        public DateTime? LastLoginUTC { get; set; }
    }

    public class RefreshTokenResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public DateTime AccessTokenExpiration { get; set; }
        
        // Properties for cookie handling only - not exposed in API response
        [JsonIgnore]
        public string RefreshToken { get; set; } = string.Empty;
        [JsonIgnore]
        public DateTime RefreshTokenExpiration { get; set; }
    }

    public class RevokeTokenRequestDto
    {
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
        public string? Reason { get; set; }
    }

}
