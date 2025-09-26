namespace Dtos;

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

public class PagedResult<T>
{
    public List<T> Data { get; set; } = new List<T>();
    public int TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNextPage => PageNumber < TotalPages;
    public bool HasPreviousPage => PageNumber > 1;
}

public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool IsDescending { get; set; } = false;
}

public class PermissionsDto
{
    public List<string> Permissions { get; set; } = new List<string>();
}

public class PermissionCheckRequestDto
{
    public string Permission { get; set; } = string.Empty;
}

public class PermissionCheckResponseDto
{
    public bool HasPermission { get; set; }
    public string Permission { get; set; } = string.Empty;
}
