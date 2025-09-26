using Models;

namespace Core.UserRole;

public static class UserRoleMapping
{
    public static UserRoleModel ToModel(this DataAccess.UserRole entity)
    {
        if (entity == null) return null!;

        return new UserRoleModel
        {
            Id = entity.Id,
            RoleName = entity.RoleName,
            RefCode = entity.RefCode,
            Description = entity.Description ?? string.Empty,
            IsVisible = entity.IsVisible,
            DateCreatedUTC = entity.DateCreatedUTC,
            LatestDateUpdatedUTC = entity.LatestDateUpdatedUTC,
            CreatedBy = entity.CreatedBy,
            LatestUpdatedBy = entity.LatestUpdatedBy
        };
    }
}