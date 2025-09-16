using EmployeeManagementSystem.Domain.Entities;

namespace EmployeeManagementSystem.Application.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<User> Users { get; }
        IRepository<UserRole> UserRoles { get; }
        IRepository<Module> Modules { get; }
        IRepository<ModuleAccess> ModuleAccesses { get; }
        IRepository<UserRoleAccess> UserRoleAccesses { get; }
        IRepository<RefreshToken> RefreshTokens { get; }
        
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
