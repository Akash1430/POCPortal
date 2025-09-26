namespace Models;

public class ModulesModel
{
    public List<ModuleModel> Modules { get; set; } = new List<ModuleModel>();
}

public class ModuleWithModuleAccessesModel : ModuleModel
{
    public List<ModuleAccessModel> ModuleAccesses { get; set; } = new List<ModuleAccessModel>();
}

public class ModulesWithModuleAccessesModel
{
    public List<ModuleWithModuleAccessesModel> Modules { get; set; } = new List<ModuleWithModuleAccessesModel>();
}