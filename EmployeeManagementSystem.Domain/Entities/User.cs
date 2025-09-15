using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Domain.Entities
{
    public class User : BaseEntity
    {
        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string UserName { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [MaxLength(255)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public int UserRoleId { get; set; }

        public bool IsFrozen { get; set; } = false;

        public DateTime? LastLoginUTC { get; set; }

        public DateTime? PasswordChangedUTC { get; set; }

        // Navigation properties
        public virtual UserRole UserRole { get; set; } = null!;
        public virtual ICollection<UserRoleAccess> UserRoleAccesses { get; set; } = new List<UserRoleAccess>();
        public virtual User? CreatedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }
        
        // Self-referencing navigation properties for created/updated by
        public virtual ICollection<User> CreatedUsers { get; set; } = new List<User>();
        public virtual ICollection<User> UpdatedUsers { get; set; } = new List<User>();
        public virtual ICollection<UserRole> CreatedUserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<UserRole> UpdatedUserRoles { get; set; } = new List<UserRole>();
        public virtual ICollection<Module> CreatedModules { get; set; } = new List<Module>();
        public virtual ICollection<Module> UpdatedModules { get; set; } = new List<Module>();
        public virtual ICollection<ModuleAccess> CreatedModuleAccesses { get; set; } = new List<ModuleAccess>();
        public virtual ICollection<ModuleAccess> UpdatedModuleAccesses { get; set; } = new List<ModuleAccess>();
        public virtual ICollection<UserRoleAccess> CreatedUserRoleAccesses { get; set; } = new List<UserRoleAccess>();
        public virtual ICollection<UserRoleAccess> UpdatedUserRoleAccesses { get; set; } = new List<UserRoleAccess>();
    }
}
