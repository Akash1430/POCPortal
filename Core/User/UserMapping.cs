using Models;
using Dtos;

namespace Core.User;

public static class UserMapping
{
    public static UserModel ToModel(this DataAccess.User entity)
    {
        if (entity == null) return null!;

        return new UserModel
        {
            Id = entity.Id,
            FirstName = entity.FirstName,
            LastName = entity.LastName,
            UserName = entity.UserName,
            Email = entity.Email,
            Password = entity.Password,
            IsFrozen = entity.IsFrozen,
            LastLoginUTC = entity.LastLoginUTC,
            PasswordChangedUTC = entity.PasswordChangedUTC,
            DateCreatedUTC = entity.DateCreatedUTC,
            LatestDateUpdatedUTC = entity.LatestDateUpdatedUTC,
            CreatedBy = entity.CreatedBy,
            LatestUpdatedBy = entity.LatestUpdatedBy
        };
    }

    public static UserDto ToDto(this UserModel model)
    {
        if (model == null) return null!;

        return new UserDto
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            UserName = model.UserName,
            Email = model.Email,
            RoleRefCode = model.RoleRefCode,
            IsFrozen = model.IsFrozen,
            LastLoginUTC = model.LastLoginUTC,
            DateCreatedUTC = model.DateCreatedUTC,
            LatestDateUpdatedUTC = model.LatestDateUpdatedUTC,
            CreatedBy = model.CreatedBy,
            LatestUpdatedBy = model.LatestUpdatedBy
        };
    }

    public static MeDto ToMeDto(this UserModel model)
    {
        if (model == null) return null!;

        return new MeDto
        {
            Id = model.Id,
            FirstName = model.FirstName,
            LastName = model.LastName,
            UserName = model.UserName,
            Email = model.Email,
            Role = model.RoleRefCode,
            PasswordChangedUTC = model.PasswordChangedUTC,
            LastLoginUTC = model.LastLoginUTC
        };
    }

    public static UsersResponseDto ToDto(this UsersResponseModel model)
    {
        if (model == null) return null!;

        return new UsersResponseDto
        {
            Users = model.Users?.Select(u => u.ToDto()).ToList() ?? [],
            PageNumber = model.PageNumber,
            PageSize = model.PageSize,
            TotalCount = model.TotalCount
        };
    }   

    public static UsersRequestModel ToModel(this UsersRequestDto dto)
    {
        if (dto == null) return null!;

        return new UsersRequestModel
        {
            PageNumber = dto.PageNumber,
            PageSize = dto.PageSize,
            SearchTerm = dto.SearchTerm,
            RoleRefs = dto.RoleRefs ?? [],
        };
    }
    
    public static FreezeUserRequestModel ToModel(this FreezeUserRequestDto dto)
    {
        if (dto == null) return null!;

        return new FreezeUserRequestModel
        {
            IsFrozen = dto.IsFrozen
        };
    }

    public static UpdateUserRequestModel ToModel(this UpdateUserRequestDto dto)
    {
        if (dto == null) return null!;

        return new UpdateUserRequestModel
        {
            UserName = dto.UserName,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            RoleRefCode = dto.RoleRefCode
        };
    }


    public static AdminChangePasswordRequestModel ToModel(this AdminChangePasswordRequestDto dto)
    {
        if (dto == null) return null!;

        return new AdminChangePasswordRequestModel
        {
            NewPassword = dto.NewPassword
        };
    }
}