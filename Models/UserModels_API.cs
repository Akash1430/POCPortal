namespace Models;

public class UsersRequestModel
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
     public string[]? RoleRefs { get; set; } = Array.Empty<string>();
    public string? SortBy { get; set; } = "DateCreatedUTC"; // Default sort by DateCreatedUTC
    public bool IsSortAscending { get; set; } = false; // Default to descending
}

public class UsersResponseModel
{
    public List<UserModel> Users { get; set; } = new();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
}

public class UpdateUserRequestModel
{
    public string UserName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? RoleRefCode { get; set; }
}

public class FreezeUserRequestModel
{
    public bool IsFrozen { get; set; }
}

public class AdminChangePasswordRequestModel
{
    public string NewPassword { get; set; } = string.Empty;
}