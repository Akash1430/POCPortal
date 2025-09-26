namespace Core.Permission;

using Models;
using Dtos;

public static class PermissionMapping
{
    public static PermissionsDto ToDto(this PermissionsModel model)
    {
        if (model == null) return null!;
        return new PermissionsDto
        {
            Permissions = model.Permissions ?? new List<string>()
        };
    }
}