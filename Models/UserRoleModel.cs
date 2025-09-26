namespace Models;

public class UserRoleModel : BaseModel
{
    public string RoleName { get; set; } = string.Empty;
    public string RefCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
}