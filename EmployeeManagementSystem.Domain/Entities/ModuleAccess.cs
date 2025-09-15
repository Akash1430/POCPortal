using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Domain.Entities
{
    public class ModuleAccess : BaseEntity
    {
        [Required]
        public int ModuleId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ModuleAccessName { get; set; } = string.Empty;

        public int? ParentId { get; set; }

        [Required]
        [MaxLength(50)]
        public string RefCode { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsVisible { get; set; } = true;

        // Navigation properties
        public virtual Module Module { get; set; } = null!;
        public virtual ModuleAccess? ParentAccess { get; set; }
        public virtual ICollection<ModuleAccess> SubAccesses { get; set; } = new List<ModuleAccess>();
        public virtual ICollection<UserRoleAccess> UserRoleAccesses { get; set; } = new List<UserRoleAccess>();
        public virtual User? CreatedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }
    }
}
