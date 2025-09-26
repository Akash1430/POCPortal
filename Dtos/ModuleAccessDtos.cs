namespace Dtos;

public class ModuleAccessDto : BaseDto
{
    public int ModuleId { get; set; }
    public string ModuleName { get; set; } = string.Empty;
    public string ModuleAccessName { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public string RefCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public bool HasPermission { get; set; }
    public List<ModuleAccessDto> SubModuleAccesses { get; set; } = new List<ModuleAccessDto>();
}
