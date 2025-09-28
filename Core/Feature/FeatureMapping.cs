using Models;
using Dtos;
using Core.ModuleAccess;

namespace Core.Feature;

public static class FeatureMapping
{

    public static UserRoleWithModuleAccessesDto ToDto(this UserRoleWithModuleAccessesModel model)
    {
        if (model == null) return null!;
        return new UserRoleWithModuleAccessesDto
        {
            ModuleAccesses = model.ModuleAccesses?.Select(ma => ma.ToDto()).ToList() ?? new List<ModuleAccessDto>(),
            Id = model.Id,
            RoleName = model.RoleName,
            RefCode = model.RefCode,
            IsVisible = model.IsVisible,
            Description = model.Description ?? string.Empty,
            DateCreatedUTC = model.DateCreatedUTC,
            CreatedBy = model.CreatedBy,
            LatestDateUpdatedUTC = model.LatestDateUpdatedUTC,
            LatestUpdatedBy = model.LatestUpdatedBy
        };
    }

    public static UserRolesWithModuleAccessesDto ToDto(this UserRolesWithModuleAccessesModel model)
    {
        if (model == null) return null!;
        return new UserRolesWithModuleAccessesDto
        {
            UserRoles = model.UserRoles?.Select(ur => ur.ToDto()).ToList() ?? new List<UserRoleWithModuleAccessesDto>()
        };
    }

    public static UpdateUserRoleModuleAccessesRequestModel ToModel(this UpdateUserRoleModuleAccessesRequestDto dto)
    {
        if (dto == null) return null!;
        return new UpdateUserRoleModuleAccessesRequestModel
        {
            ModuleAccessIds = dto.ModuleAccessIds ?? []
        };
    }

    public static UpdateUserRoleModuleAccessesResponseDto ToDto(this UpdateUserRoleModuleAccessesResponseModel model)
    {
        if (model == null) return null!;
        return new UpdateUserRoleModuleAccessesResponseDto
        {
            UserRoleId = model.UserRoleId,
            RoleName = model.RoleName,
            ModuleAccessRefs = model.ModuleAccessRefs ?? []
        };
    }

    public static CreateUserRoleRequestModel ToModel(this CreateUserRoleRequestDto dto)
    {
        if (dto == null) return null!;
        return new CreateUserRoleRequestModel
        {
            RoleName = dto.RoleName,
            RefCode = dto.RefCode,
            Description = dto.Description ?? string.Empty
        };
    }
}

