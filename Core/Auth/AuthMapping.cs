
using Dtos;
using Models;
using Core.User;

namespace Core.Auth;

public static class AuthMapping
{
    public static LoginRequestModel ToModel(this LoginRequestDto dto)
    {
        if (dto == null) return null!;
        return new LoginRequestModel
        {
            UserName = dto.UserName,
            Password = dto.Password
        };
    }

    public static LoginResponseDto ToDto(this LoginResponseModel model)
    {
        if (model == null) return null!;
        return new LoginResponseDto
        {
            AccessToken = model.AccessToken,
            AccessTokenExpiration = model.AccessTokenExpiration,
            User = model.User != null ? model.User.ToDto() : null!
        };
    }

    public static RegisterRequestModel ToModel(this RegisterRequestDto dto)
    {
        if (dto == null) return null!;
        return new RegisterRequestModel
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            UserName = dto.UserName,
            Email = dto.Email,
            Password = dto.Password,
            RoleRefCode = dto.RoleRefCode
        };
    }

    public static RegisterResponseDto ToDto(this RegisterResponseModel model)
    {
        if (model == null) return null!;
        return new RegisterResponseDto
        {
            UserId = model.UserId,
            UserName = model.UserName,
            Email = model.Email,
            RoleRefCode = model.RoleRefCode
        };
    }

    public static RefreshTokenResponseDto ToDto(this RefreshTokenResponseModel model)
    {
        if (model == null) return null!;
        return new RefreshTokenResponseDto
        {
            AccessToken = model.AccessToken,
            AccessTokenExpiration = model.AccessTokenExpiration
        };
    }

    public static RevokeTokenRequestModel ToModel(this RevokeTokenRequestDto dto)
    {
        if (dto == null) return null!;
        return new RevokeTokenRequestModel
        {
            RefreshToken = dto.RefreshToken,
            Reason = dto.Reason
        };
    }

    public static ChangePasswordRequestModel ToModel(this ChangePasswordRequestDto dto)
    {
        if (dto == null) return null!;
        return new ChangePasswordRequestModel
        {
            CurrentPassword = dto.CurrentPassword,
            NewPassword = dto.NewPassword
        };
    }
}

