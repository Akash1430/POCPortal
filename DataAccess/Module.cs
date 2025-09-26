using System.ComponentModel.DataAnnotations;

namespace DataAccess;

public class Module : BaseEntity
{
    [Required]
    [MaxLength(100)]
    public string ModuleName { get; set; } = string.Empty;

    public int? ParentId { get; set; }

    [Required]
    [MaxLength(50)]
    public string RefCode { get; set; } = string.Empty;

    public bool IsVisible { get; set; } = true;

    [MaxLength(100)]
    public string? LogoName { get; set; }

    [MaxLength(200)]
    public string? RedirectPage { get; set; }

    public int SortOrder { get; set; } = 0;

    [MaxLength(500)]
    public string? Description { get; set; }

    // Navigation properties
    public virtual Module? ParentModule { get; set; }
    public virtual ICollection<Module> SubModules { get; set; } = new List<Module>();
    public virtual ICollection<ModuleAccess> ModuleAccesses { get; set; } = new List<ModuleAccess>();
    public virtual User? CreatedByUser { get; set; }
    public virtual User? UpdatedByUser { get; set; }
}
