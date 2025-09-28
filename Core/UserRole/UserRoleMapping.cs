using Models;
using Dtos;

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

    public static UserRoleDto ToDto(this UserRoleModel model)
    {
        if (model == null) return null!;

        return new UserRoleDto
        {
            Id = model.Id,
            RoleName = model.RoleName,
            RefCode = model.RefCode,
            Description = model.Description ?? string.Empty,
            IsVisible = model.IsVisible,
            DateCreatedUTC = model.DateCreatedUTC,
            LatestDateUpdatedUTC = model.LatestDateUpdatedUTC,
            CreatedBy = model.CreatedBy,
            LatestUpdatedBy = model.LatestUpdatedBy
        };
    }
}