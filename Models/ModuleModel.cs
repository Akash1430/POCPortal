namespace Models;

public class ModuleModel : BaseModel
{
    public string ModuleName { get; set; } = string.Empty;
    public int? ParentId { get; set; }
    public string RefCode { get; set; } = string.Empty;
    public bool IsVisible { get; set; }
    public string? LogoName { get; set; } = null;
    public string? RedirectPage { get; set; } = null;
    public int SortOrder { get; set; }
    public string? Description { get; set; } = null;
    public List<ModuleModel> SubModules { get; set; } = new List<ModuleModel>();
}