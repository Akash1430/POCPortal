namespace Models;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public List<string> Errors { get; set; } = new List<string>();

    public static ApiResponse<T> SuccessResult(T data, string message = "Operation successful")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResult(string message, List<string>? errors = null)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>()
        };
    }

    public static ApiResponse<T> ErrorResult(List<string> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = "Validation failed",
            Errors = errors
        };
    }
}

public class PermissionsModel
{
    public List<string> Permissions { get; set; } = new List<string>();
}

public class PermissionCheckRequestModel
{
    public string Permission { get; set; } = string.Empty;
}

public class PermissionCheckResponseModel
{
    public bool HasPermission { get; set; }
    public string Permission { get; set; } = string.Empty;
}
