namespace Core.Module;

using Models;
using DataAccess;
using Dtos;
using Core.ModuleAccess;

public static class ModuleMapping
{
    public static ModuleModel ToModel(this Module module)
    {
        if (module == null) return null!;
        return new ModuleModel
        {
            Id = module.Id,
            ModuleName = module.ModuleName,
            ParentId = module.ParentId,
            RefCode = module.RefCode,
            IsVisible = module.IsVisible,
            LogoName = module.LogoName,
            RedirectPage = module.RedirectPage,
            SortOrder = module.SortOrder,
            Description = module.Description ?? string.Empty,
            DateCreatedUTC = module.DateCreatedUTC,
            CreatedBy = module.CreatedBy,
            LatestDateUpdatedUTC = module.LatestDateUpdatedUTC,
            LatestUpdatedBy = module.LatestUpdatedBy
        };
    }

    public static IEnumerable<ModuleModel> ToModel(this IEnumerable<Module> modules)
    {
        return modules.Select(m => m.ToModel());
    }

    public static ModuleDto ToDto(this ModuleModel model)
    {
        if (model == null) return null!;
        return new ModuleDto
        {
            Id = model.Id,
            ModuleName = model.ModuleName,
            ParentId = model.ParentId,
            RefCode = model.RefCode,
            IsVisible = model.IsVisible,
            LogoName = model.LogoName,
            RedirectPage = model.RedirectPage,
            SortOrder = model.SortOrder,
            Description = model.Description,
            DateCreatedUTC = model.DateCreatedUTC,
            CreatedBy = model.CreatedBy,
            LatestDateUpdatedUTC = model.LatestDateUpdatedUTC,
            LatestUpdatedBy = model.LatestUpdatedBy
        };
    }

    public static ModuleModel ToModel(this ModuleDto dto)
    {
        if (dto == null) return null!;
        return new ModuleModel
        {
            Id = dto.Id,
            ModuleName = dto.ModuleName,
            ParentId = dto.ParentId,
            RefCode = dto.RefCode,
            IsVisible = dto.IsVisible,
            LogoName = dto.LogoName,
            RedirectPage = dto.RedirectPage,
            SortOrder = dto.SortOrder,
            Description = dto.Description ?? string.Empty,
            DateCreatedUTC = dto.DateCreatedUTC,
            CreatedBy = dto.CreatedBy,
            LatestDateUpdatedUTC = dto.LatestDateUpdatedUTC,
            LatestUpdatedBy = dto.LatestUpdatedBy
        };
    }


    public static ModulesDto ToDto(this ModulesModel model)
    {
        if (model == null) return null!;
        return new ModulesDto
        {
            Modules = model.Modules?.Select(m => m.ToDto()).ToList() ?? []
        };
    }

    public static ModulesWithModuleAccessesDto ToDto(this ModulesWithModuleAccessesModel model)
    {
        if (model == null) return null!;
        return new ModulesWithModuleAccessesDto
        {
            Modules = model.Modules?.Select(m => new ModuleWithModuleAccessesDto
            {
                Id = m.Id,
                ModuleName = m.ModuleName,
                ParentId = m.ParentId,
                RefCode = m.RefCode,
                IsVisible = m.IsVisible,
                LogoName = m.LogoName,
                RedirectPage = m.RedirectPage,
                SortOrder = m.SortOrder,
                Description = m.Description,
                DateCreatedUTC = m.DateCreatedUTC,
                CreatedBy = m.CreatedBy,
                LatestDateUpdatedUTC = m.LatestDateUpdatedUTC,
                LatestUpdatedBy = m.LatestUpdatedBy,
                ModuleAccesses = m.ModuleAccesses?.Select(ma => ma.ToDto()).ToList() ?? []
            }).ToList() ?? []
        };
    }

}