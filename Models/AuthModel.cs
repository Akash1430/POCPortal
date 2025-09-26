namespace Models;

public class LoginRequestModel
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseModel
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiration { get; set; }
    public UserModel User { get; set; } = null!;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiration { get; set; }
}

public class RegisterRequestModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string RoleRefCode { get; set; } = string.Empty;
}

public class RegisterResponseModel
{
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string RoleRefCode { get; set; } = string.Empty;
}

public class RefreshTokenResponseModel
{
    public string AccessToken { get; set; } = string.Empty;
    public DateTime AccessTokenExpiration { get; set; }
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime RefreshTokenExpiration { get; set; }
}

public class RevokeTokenRequestModel
{
    public string RefreshToken { get; set; } = string.Empty;
    public string? Reason { get; set; }
}

public class ChangePasswordRequestModel
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}