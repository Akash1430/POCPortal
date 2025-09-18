namespace EmployeeManagementSystem.Application.DTOs
{
    public class ModulesResponseDto
    {
        public List<ModuleResponseDto> Modules { get; set; } = new List<ModuleResponseDto>();
    }
    
    public class ModuleResponseDto
    {
        public int Id { get; set; }
        public string ModuleName { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string RefCode { get; set; } = string.Empty;
        public bool IsVisible { get; set; }
        public string? LogoName { get; set; } = null;
        public string? RedirectPage { get; set; } = null;
        public int SortOrder { get; set; }
        public string? Description { get; set; } = null;
        public DateTime DateCreatedUTC { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? LatestDateUpdatedUTC { get; set; }
        public int? LatestUpdatedBy { get; set; }
        public List<ModuleResponseDto> SubModules { get; set; } = new List<ModuleResponseDto>();
    }
}
