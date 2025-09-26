using Microsoft.EntityFrameworkCore.Storage;
using DataAccess.Repository;
using DataAccess.Interfaces;

namespace DataAccess.UnitOfWork;

public class UnitOfWork : IUnitOfWork
{
    private readonly EmployeeManagementSystemDbContext _context;
    private IDbContextTransaction? _transaction;

    public UnitOfWork(EmployeeManagementSystemDbContext context)
    {
        _context = context;
        Users = new Repository<User>(_context);
        UserRoles = new Repository<UserRole>(_context);
        Modules = new Repository<Module>(_context);
        ModuleAccesses = new Repository<ModuleAccess>(_context);
        UserRoleAccesses = new Repository<UserRoleAccess>(_context);
        RefreshTokens = new Repository<RefreshToken>(_context);
    }

    public IRepository<User> Users { get; private set; }
    public IRepository<UserRole> UserRoles { get; private set; }
    public IRepository<Module> Modules { get; private set; }
    public IRepository<ModuleAccess> ModuleAccesses { get; private set; }
    public IRepository<UserRoleAccess> UserRoleAccesses { get; private set; }
    public IRepository<RefreshToken> RefreshTokens { get; private set; }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    public async Task BeginTransactionAsync()
    {
        _transaction = await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.CommitAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync()
    {
        if (_transaction != null)
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public void Dispose()
    {
        _transaction?.Dispose();
        _context.Dispose();
    }
}
