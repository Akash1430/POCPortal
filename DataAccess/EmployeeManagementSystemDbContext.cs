using Microsoft.EntityFrameworkCore;

namespace DataAccess;

public class EmployeeManagementSystemDbContext : DbContext
{
    public EmployeeManagementSystemDbContext(DbContextOptions<EmployeeManagementSystemDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<ModuleAccess> ModuleAccesses { get; set; }
    public DbSet<UserRoleAccess> UserRoleAccesses { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.FirstName).IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            entity.Property(e => e.UserName).IsRequired().HasMaxLength(50).HasColumnType("nvarchar(50)");
            entity.Property(e => e.Password).IsRequired().HasMaxLength(255).HasColumnType("nvarchar(255)");
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255).HasColumnType("nvarchar(255)");
            entity.Property(e => e.UserRoleId).IsRequired();
            entity.Property(e => e.IsFrozen).IsRequired().HasColumnType("bit").HasDefaultValue(false);
            entity.Property(e => e.LastLoginUTC).HasColumnType("datetime2");
            entity.Property(e => e.PasswordChangedUTC).HasColumnType("datetime2");
            entity.Property(e => e.DateCreatedUTC).IsRequired().HasColumnType("datetime2").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.LatestDateUpdatedUTC).HasColumnType("datetime2");

            entity.HasIndex(e => e.UserName).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasOne(d => d.UserRole)
                .WithMany(p => p.Users)
                .HasForeignKey(d => d.UserRoleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.CreatedByUser)
                .WithMany(p => p.CreatedUsers)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UpdatedByUser)
                .WithMany(p => p.UpdatedUsers)
                .HasForeignKey(d => d.LatestUpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            entity.Property(e => e.RefCode).IsRequired().HasMaxLength(50).HasColumnType("nvarchar(50)");
            entity.Property(e => e.Description).HasMaxLength(500).HasColumnType("nvarchar(500)");
            entity.Property(e => e.IsVisible).IsRequired().HasColumnType("bit").HasDefaultValue(true);
            entity.Property(e => e.DateCreatedUTC).IsRequired().HasColumnType("datetime2").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.LatestDateUpdatedUTC).HasColumnType("datetime2");

            entity.HasIndex(e => e.RefCode).IsUnique();

            entity.HasOne(d => d.CreatedByUser)
                .WithMany(p => p.CreatedUserRoles)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UpdatedByUser)
                .WithMany(p => p.UpdatedUserRoles)
                .HasForeignKey(d => d.LatestUpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Module>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ModuleName).IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            entity.Property(e => e.ParentId);
            entity.Property(e => e.RefCode).IsRequired().HasMaxLength(50).HasColumnType("nvarchar(50)");
            entity.Property(e => e.IsVisible).IsRequired().HasColumnType("bit").HasDefaultValue(true);
            entity.Property(e => e.LogoName).HasMaxLength(100).HasColumnType("nvarchar(100)");
            entity.Property(e => e.RedirectPage).HasMaxLength(200).HasColumnType("nvarchar(200)");
            entity.Property(e => e.SortOrder).IsRequired().HasDefaultValue(0);
            entity.Property(e => e.Description).HasMaxLength(500).HasColumnType("nvarchar(500)");
            entity.Property(e => e.DateCreatedUTC).IsRequired().HasColumnType("datetime2").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.LatestDateUpdatedUTC).HasColumnType("datetime2");

            entity.HasIndex(e => e.RefCode).IsUnique();

            entity.HasOne(d => d.ParentModule)
                .WithMany(p => p.SubModules)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.CreatedByUser)
                .WithMany(p => p.CreatedModules)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UpdatedByUser)
                .WithMany(p => p.UpdatedModules)
                .HasForeignKey(d => d.LatestUpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ModuleAccess>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.ModuleId).IsRequired();
            entity.Property(e => e.ParentId);
            entity.Property(e => e.ModuleAccessName).IsRequired().HasMaxLength(100).HasColumnType("nvarchar(100)");
            entity.Property(e => e.RefCode).IsRequired().HasMaxLength(50).HasColumnType("nvarchar(50)");
            entity.Property(e => e.Description).HasMaxLength(500).HasColumnType("nvarchar(500)");
            entity.Property(e => e.IsVisible).IsRequired().HasColumnType("bit").HasDefaultValue(true);
            entity.Property(e => e.DateCreatedUTC).IsRequired().HasColumnType("datetime2").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.LatestDateUpdatedUTC).HasColumnType("datetime2");

            entity.HasIndex(e => e.RefCode).IsUnique();

            entity.HasOne(d => d.Module)
                .WithMany(p => p.ModuleAccesses)
                .HasForeignKey(d => d.ModuleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.ParentAccess)
                .WithMany(p => p.SubModuleAccesses)
                .HasForeignKey(d => d.ParentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.CreatedByUser)
                .WithMany(p => p.CreatedModuleAccesses)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UpdatedByUser)
                .WithMany(p => p.UpdatedModuleAccesses)
                .HasForeignKey(d => d.LatestUpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<UserRoleAccess>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UserRoleId).IsRequired();
            entity.Property(e => e.ModuleAccessId).IsRequired();
            entity.Property(e => e.DateCreatedUTC).IsRequired().HasColumnType("datetime2").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.LatestDateUpdatedUTC).HasColumnType("datetime2");

            entity.HasOne(d => d.UserRole)
                .WithMany(p => p.UserRoleAccesses)
                .HasForeignKey(d => d.UserRoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.ModuleAccess)
                .WithMany(p => p.UserRoleAccesses)
                .HasForeignKey(d => d.ModuleAccessId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.CreatedByUser)
                .WithMany(p => p.CreatedUserRoleAccesses)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UpdatedByUser)
                .WithMany(p => p.UpdatedUserRoleAccesses)
                .HasForeignKey(d => d.LatestUpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500).HasColumnType("nvarchar(500)");
            entity.Property(e => e.UserId).IsRequired();
            entity.Property(e => e.ExpiryDateUTC).IsRequired().HasColumnType("datetime2");
            entity.Property(e => e.IsRevoked).IsRequired().HasColumnType("bit").HasDefaultValue(false);
            entity.Property(e => e.RevokedDateUTC).HasColumnType("datetime2");
            entity.Property(e => e.RevokedByIp).HasMaxLength(100).HasColumnType("nvarchar(100)");
            entity.Property(e => e.ReplacedByToken).HasMaxLength(500).HasColumnType("nvarchar(500)");
            entity.Property(e => e.ReasonRevoked).HasMaxLength(200).HasColumnType("nvarchar(200)");
            entity.Property(e => e.DateCreatedUTC).IsRequired().HasColumnType("datetime2").HasDefaultValueSql("GETUTCDATE()");
            entity.Property(e => e.LatestDateUpdatedUTC).HasColumnType("datetime2");

            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.UserId);

            entity.HasOne(d => d.User)
                .WithMany(p => p.RefreshTokens)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(d => d.CreatedByUser)
                .WithMany(p => p.CreatedRefreshTokens)
                .HasForeignKey(d => d.CreatedBy)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(d => d.UpdatedByUser)
                .WithMany(p => p.UpdatedRefreshTokens)
                .HasForeignKey(d => d.LatestUpdatedBy)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
