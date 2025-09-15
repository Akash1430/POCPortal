using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Domain.Entities
{
    public class UserRole : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string RoleName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string RefCode { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsVisible { get; set; } = true;

        // Navigation properties
        public virtual ICollection<User> Users { get; set; } = new List<User>();
        public virtual ICollection<UserRoleAccess> UserRoleAccesses { get; set; } = new List<UserRoleAccess>();
        public virtual User? CreatedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }
    }
}
