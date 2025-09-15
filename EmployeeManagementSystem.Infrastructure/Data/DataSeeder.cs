using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Domain.Entities;
using EmployeeManagementSystem.Infrastructure.Context;
using System.Security.Cryptography;
using System.Text;

namespace EmployeeManagementSystem.Infrastructure.Data
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(EmployeeManagementSystemDbContext context)
        {
            await context.Database.EnsureCreatedAsync();

            if (await context.UserRoles.AnyAsync())
            {
                return;
            }

            await SeedUserRolesAsync(context);
            await SeedUsersAsync(context);
            await SeedModulesAsync(context);
            await SeedModuleAccessAsync(context);
            await SeedUserRoleAccessAsync(context);
        }

        private static async Task SeedUserRolesAsync(EmployeeManagementSystemDbContext context)
        {
            var userRoles = new List<UserRole>
            {
                new UserRole
                {
                    RoleName = "System Administrator",
                    RefCode = "SYSADMIN",
                    Description = "System administrator with full system access. Can manage User Admins and system configuration. Hidden from regular role selection.",
                    IsVisible = false,
                    DateCreatedUTC = DateTime.UtcNow,
                    CreatedBy = 1
                },
                new UserRole
                {
                    RoleName = "User Administrator",
                    RefCode = "USERADMIN",
                    Description = "User administrator who can manage employees and users. Can access assigned business modules.",
                    IsVisible = true,
                    DateCreatedUTC = DateTime.UtcNow,
                    CreatedBy = 1
                },
                new UserRole
                {
                    RoleName = "Manager",
                    RefCode = "MANAGER",
                    Description = "Department manager with team management capabilities and reporting access.",
                    IsVisible = true,
                    DateCreatedUTC = DateTime.UtcNow,
                    CreatedBy = 1
                },
                new UserRole
                {
                    RoleName = "Employee",
                    RefCode = "EMPLOYEE",
                    Description = "Regular employee with limited access to own profile and basic employee information.",
                    IsVisible = true,
                    DateCreatedUTC = DateTime.UtcNow,
                    CreatedBy = 1
                },
                new UserRole
                {
                    RoleName = "Human Resources",
                    RefCode = "HR",
                    Description = "HR personnel with employee management and reporting capabilities.",
                    IsVisible = true,
                    DateCreatedUTC = DateTime.UtcNow,
                    CreatedBy = 1
                }
            };

            // Temporarily disable foreign key constraint checking (SQL Server syntax)
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE [UserRoles] NOCHECK CONSTRAINT ALL");
            
            await context.UserRoles.AddRangeAsync(userRoles);
            await context.SaveChangesAsync();
            
            // Re-enable foreign key constraint checking
            await context.Database.ExecuteSqlRawAsync("ALTER TABLE [UserRoles] CHECK CONSTRAINT ALL");
        }

        private static async Task SeedUsersAsync(EmployeeManagementSystemDbContext context)
        {
            var sysAdminRole = await context.UserRoles
                .Where(r => r.RefCode == "SYSADMIN")
                .FirstOrDefaultAsync();

            if (sysAdminRole != null)
            {
                var users = new List<User>
                {
                    new User
                    {
                        FirstName = "System",
                        LastName = "Administrator",
                        UserName = "sysadmin",
                        Password = HashPassword("SysAdmin@123!"),
                        Email = "sysadmin@company.com",
                        UserRoleId = sysAdminRole.Id,
                        IsFrozen = false,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = 1
                    }
                };

                await context.Users.AddRangeAsync(users);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedModulesAsync(EmployeeManagementSystemDbContext context)
        {
            var sysAdminUser = await context.Users
                .Where(u => u.UserName == "sysadmin")
                .FirstOrDefaultAsync();

            if (sysAdminUser != null)
            {
                var modules = new List<Module>
                {
                    new Module
                    {
                        ModuleName = "Admin",
                        ParentId = null,
                        RefCode = "ADMIN",
                        IsVisible = false,
                        LogoName = "admin-icon.svg",
                        RedirectPage = "/admin",
                        SortOrder = 1,
                        Description = "System administration module for managing users, roles, and system settings.",
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminUser.Id
                    },
                    new Module
                    {
                        ModuleName = "Features",
                        ParentId = null,
                        RefCode = "FEATURES",
                        IsVisible = false,
                        LogoName = "features-icon.svg",
                        RedirectPage = "/features",
                        SortOrder = 2,
                        Description = "System features and configuration module.",
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminUser.Id
                    },
                    new Module
                    {
                        ModuleName = "Employee",
                        ParentId = null,
                        RefCode = "EMPLOYEE",
                        IsVisible = true,
                        LogoName = "employee-icon.svg",
                        RedirectPage = "/employee",
                        SortOrder = 10,
                        Description = "Employee management module for handling employee records and profiles.",
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminUser.Id
                    }
                };

                await context.Modules.AddRangeAsync(modules);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedModuleAccessAsync(EmployeeManagementSystemDbContext context)
        {
            var adminModule = await context.Modules
                .Where(m => m.RefCode == "ADMIN")
                .FirstOrDefaultAsync();

            if (adminModule != null)
            {
                var moduleAccesses = new List<ModuleAccess>
                {
                    new ModuleAccess
                    {
                        ModuleId = adminModule.Id,
                        ModuleAccessName = "View Admin Dashboard",
                        RefCode = "ADMIN_VIEW",
                        Description = "View admin dashboard and overview",
                        IsVisible = false,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = adminModule.CreatedBy
                    },
                    new ModuleAccess
                    {
                        ModuleId = adminModule.Id,
                        ModuleAccessName = "Manage System Settings",
                        RefCode = "ADMIN_SETTINGS",
                        Description = "Manage system-wide settings and configuration",
                        IsVisible = false,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = adminModule.CreatedBy
                    }
                };

                await context.ModuleAccesses.AddRangeAsync(moduleAccesses);
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedUserRoleAccessAsync(EmployeeManagementSystemDbContext context)
        {
            var sysAdminRole = await context.UserRoles
                .Where(r => r.RefCode == "SYSADMIN")
                .FirstOrDefaultAsync();

            var adminViewAccess = await context.ModuleAccesses
                .Where(ma => ma.RefCode == "ADMIN_VIEW")
                .FirstOrDefaultAsync();

            var adminSettingsAccess = await context.ModuleAccesses
                .Where(ma => ma.RefCode == "ADMIN_SETTINGS")
                .FirstOrDefaultAsync();

            if (sysAdminRole != null && adminViewAccess != null && adminSettingsAccess != null)
            {
                var userRoleAccesses = new List<UserRoleAccess>
                {
                    new UserRoleAccess
                    {
                        UserRoleId = sysAdminRole.Id,
                        ModuleAccessId = adminViewAccess.Id
                    },
                    new UserRoleAccess
                    {
                        UserRoleId = sysAdminRole.Id,
                        ModuleAccessId = adminSettingsAccess.Id
                    }
                };

                await context.UserRoleAccesses.AddRangeAsync(userRoleAccesses);
                await context.SaveChangesAsync();
            }
        }

        private static string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
    }
}