namespace EmployeeManagementSystem.Domain.Entities
{
    public abstract class BaseEntity
    {
        public int Id { get; set; }
        public DateTime DateCreatedUTC { get; set; } = DateTime.UtcNow;
        public DateTime LatestDateUpdatedUTC { get; set; } = DateTime.UtcNow;
        public int CreatedBy { get; set; }
        public int? LatestUpdatedBy { get; set; }
    }

}