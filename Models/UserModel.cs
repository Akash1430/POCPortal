namespace Models;

public class UserModel : BaseModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsFrozen { get; set; } = false;
    public string Email { get; set; } = string.Empty;
    public int UserRoleId { get; set; }
    public string RoleRefCode { get; set; } = string.Empty;
    public DateTime? LastLoginUTC { get; set; }

    public DateTime? PasswordChangedUTC { get; set; }
}