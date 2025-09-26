namespace Dtos;

public class ModuleDto : BaseDto
{
    public string ModuleName { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public string RefCode { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public string? LogoName { get; set; } = null;
    public string? RedirectPage { get; set; } = null;
    public int SortOrder { get; set; }
    public string? Description { get; set; } = null;
    public List<ModuleDto> SubModules { get; set; } = new List<ModuleDto>();
}

public class ModulesDto
{
    public List<ModuleDto> Modules { get; set; } = new List<ModuleDto>();
}

public class ModuleWithModuleAccessesDto : ModuleDto
{
    public List<ModuleAccessDto> ModuleAccesses { get; set; } = new List<ModuleAccessDto>();
}

public class ModulesWithModuleAccessesDto
{
    public List<ModuleWithModuleAccessesDto> Modules { get; set; } = new List<ModuleWithModuleAccessesDto>();
}