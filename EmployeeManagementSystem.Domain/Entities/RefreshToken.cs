using System.ComponentModel.DataAnnotations;

namespace EmployeeManagementSystem.Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        [Required]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        [Required]
        public int UserId { get; set; }

        [Required]
        public DateTime ExpiryDateUTC { get; set; }

        public bool IsRevoked { get; set; } = false;

        public DateTime? RevokedDateUTC { get; set; }

        [MaxLength(100)]
        public string? RevokedByIp { get; set; }

        [MaxLength(500)]
        public string? ReplacedByToken { get; set; }

        [MaxLength(200)]
        public string? ReasonRevoked { get; set; }

        // Navigation properties
        public virtual User User { get; set; } = null!;
        public virtual User? CreatedByUser { get; set; }
        public virtual User? UpdatedByUser { get; set; }

        // Helper properties
        public bool IsExpired => DateTime.UtcNow >= ExpiryDateUTC;
        public bool IsActive => !IsRevoked && !IsExpired;
    }
}
