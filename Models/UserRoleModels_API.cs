namespace Models;

public class CreateUserRoleRequestModel : BaseModel
{
    public string RoleName { get; set; } = string.Empty;
    public string RefCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}