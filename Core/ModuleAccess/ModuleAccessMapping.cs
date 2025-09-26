namespace Core.ModuleAccess;

using Models;
using DataAccess;
using Dtos;

public static class ModuleAccessMapping
{
    public static ModuleAccessModel ToModel(this ModuleAccess entity)
    {
        if (entity == null) return null!;
        return new ModuleAccessModel
        {
            Id = entity.Id,
            ModuleId = entity.ModuleId,
            ModuleName = entity.Module != null ? entity.Module.ModuleName : string.Empty,
            ModuleAccessName = entity.ModuleAccessName,
            ParentId = entity.ParentId,
            RefCode = entity.RefCode,
            Description = entity.Description ?? string.Empty,
            IsVisible = entity.IsVisible,
            HasPermission = false, // Default value; actual permission should be set based on user role
            SubModuleAccesses = entity.SubModuleAccesses != null
                ? entity.SubModuleAccesses.Select(sma => sma.ToModel()).ToList()
                : []
        };
    }

    public static IEnumerable<ModuleAccessModel> ToModel(this IEnumerable<ModuleAccess> entities)
    {
        return entities.Select(e => e.ToModel());
    }

    public static ModuleAccessModel ToModel(this ModuleAccessDto dto)
    {
        if (dto == null) return null!;
        return new ModuleAccessModel
        {
            Id = dto.Id,
            ModuleId = dto.ModuleId,
            ModuleName = dto.ModuleName,
            ModuleAccessName = dto.ModuleAccessName,
            ParentId = dto.ParentId,
            RefCode = dto.RefCode,
            Description = dto.Description,
            IsVisible = dto.IsVisible,
            HasPermission = dto.HasPermission,
            SubModuleAccesses = dto.SubModuleAccesses?.Select(sma => sma.ToModel()).ToList() ?? new List<ModuleAccessModel>()
        };
    }

    public static ModuleAccessDto ToDto(this ModuleAccessModel model)
    {
        if (model == null) return null!;
        return new ModuleAccessDto
        {
            Id = model.Id,
            ModuleId = model.ModuleId,
            ModuleName = model.ModuleName,
            ModuleAccessName = model.ModuleAccessName,
            ParentId = model.ParentId,
            RefCode = model.RefCode,
            Description = model.Description,
            IsVisible = model.IsVisible,
            HasPermission = model.HasPermission,
            SubModuleAccesses = model.SubModuleAccesses?.Select(sma => sma.ToDto()).ToList() ?? []
        };
    }

    public static ModuleAccessesDto ToDto(this ModuleAccessesModel model)
    {
        if (model == null) return null!;
        return new ModuleAccessesDto
        {
            ModuleAccessDtos = model.ModuleAccesses?.Select(ma => ma.ToDto()).ToList() ?? []
        };
    }
}