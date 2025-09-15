namespace EmployeeManagementSystem.Domain.Entities
{
    public class UserRoleAccess : BaseEntity
    {
        public int UserRoleId { get; set; }
        
        public int ModuleAccessId { get; set; }

        // Navigation properties
        public virtual UserRole UserRole { get; set; } = null!;
        public virtual ModuleAccess ModuleAccess { get; set; } = null!;
        public virtual User? CreatedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }
    }
}
