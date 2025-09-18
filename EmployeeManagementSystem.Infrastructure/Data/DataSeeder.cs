using Microsoft.EntityFrameworkCore;
using EmployeeManagementSystem.Domain.Entities;
using EmployeeManagementSystem.Infrastructure.Context;

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
            var userAdminRole = await context.UserRoles
                .Where(r => r.RefCode == "USERADMIN")
                .FirstOrDefaultAsync();

            if (sysAdminRole != null && userAdminRole != null)
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
                    },
                    new User
                    {
                        FirstName = "Default",
                        LastName = "Admin",
                        UserName = "defaultadmin",
                        Password = HashPassword("DefaultAdmin@123!"),
                        Email = "defaultadmin@company.com",
                        UserRoleId = userAdminRole.Id,
                        IsFrozen = false,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = 1
                    },
                    new User
                    {
                        FirstName = "John",
                        LastName = "Doe",
                        UserName = "johndoe",
                        Password = HashPassword("JohnDoe@123!"),
                        Email = "johndoe@company.com",
                        UserRoleId = userAdminRole.Id,
                        IsFrozen = false,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = 1
                    },
                    new User
                    {
                        FirstName = "Jane",
                        LastName = "Smith",
                        UserName = "janesmith",
                        Password = HashPassword("JaneSmith@123!"),
                        Email = "janesmith@company.com",
                        UserRoleId = userAdminRole.Id,
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
                    new Module {
                        ModuleName = "Manage Admin",
                        ParentId = null,
                        RefCode = "MANAGE_ADMIN",
                        IsVisible = true,
                        LogoName = "manage-admin-icon.svg",
                        RedirectPage = "/manage-admin",
                        SortOrder = 1,
                        Description = "Module for managing user administrators and their permissions.",
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminUser.Id
                    },
                    new Module {
                        ModuleName = "Manage Feature",
                        ParentId = null,
                        RefCode = "MANAGE_FEATURE",
                        IsVisible = true,
                        LogoName = "features-icon.svg",
                        RedirectPage = "/manage-feature",
                        SortOrder = 2,
                        Description = "Module for managing user roles and permissions.",
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminUser.Id
                    },
                    new Module {
                        ModuleName = "Employee Management",
                        ParentId = null,
                        RefCode = "EMPLOYEE_MANAGEMENT",
                        IsVisible = true,
                        LogoName = "employee-management-icon.svg",
                        RedirectPage = "/employee-management",
                        SortOrder = 3,
                        Description = "Module for managing employee records and information.",
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminUser.Id
                    },
                    new Module {
                        ModuleName = "Manager Management",
                        ParentId = null,
                        RefCode = "MANAGER_MANAGEMENT",
                        IsVisible = true,
                        LogoName = "manager-management-icon.svg",
                        RedirectPage = "/manager-management",
                        SortOrder = 4,
                        Description = "Module for managing manager records and information.",
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
                .Where(m => m.RefCode == "MANAGE_ADMIN")
                .FirstOrDefaultAsync();

            var featuresModule = await context.Modules
                .Where(m => m.RefCode == "MANAGE_FEATURE")
                .FirstOrDefaultAsync();

            var employeeModule = await context.Modules
                .Where(m => m.RefCode == "EMPLOYEE_MANAGEMENT")
                .FirstOrDefaultAsync();

            var managerModule = await context.Modules
                .Where(m => m.RefCode == "MANAGER_MANAGEMENT")
                .FirstOrDefaultAsync();

            if (adminModule != null && featuresModule != null && employeeModule != null && managerModule != null)
            {
                var moduleAccesses = new List<ModuleAccess>
                {
                    // MANAGE_ADMIN module accesses
                    new ModuleAccess
                    {
                        ModuleId = adminModule.Id,
                        ModuleAccessName = "View Admin Dashboard",
                        RefCode = "ADMIN_READ",
                        Description = "View admin dashboard and overview",
                        IsVisible = false,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = adminModule.CreatedBy
                    },
                    new ModuleAccess
                    {
                        ModuleId = adminModule.Id,
                        ModuleAccessName = "Create Admin Users",
                        RefCode = "ADMIN_CREATE",
                        Description = "Create admin users",
                        IsVisible = false,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = adminModule.CreatedBy
                    },
                    new ModuleAccess
                    {
                        ModuleId = adminModule.Id,
                        ModuleAccessName = "Edit Admin Users",
                        RefCode = "ADMIN_UPDATE",
                        Description = "Edit existing admin users",
                        IsVisible = false,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = adminModule.CreatedBy
                    },

                    new ModuleAccess
                    {
                        ModuleId = adminModule.Id,
                        ModuleAccessName = "Delete Admin Users",
                        RefCode = "ADMIN_DELETE",
                        Description = "Delete existing admin users",
                        IsVisible = false,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = adminModule.CreatedBy
                    },

                    new ModuleAccess
                    {
                        ModuleId = adminModule.Id,
                        ModuleAccessName = "Change User Password",
                        RefCode = "ADMIN_CHANGE_PASSWORD",
                        Description = "Change password for other users",
                        IsVisible = false,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = adminModule.CreatedBy
                    },

                    // MANAGE_FEATURE module accesses
                    new ModuleAccess
                    {
                        ModuleId = featuresModule.Id,
                        ModuleAccessName = "Read Roles & Permissions",
                        RefCode = "FEATURES_READ_ROLES",
                        Description = "View roles and their assigned permissions",
                        IsVisible = false,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = featuresModule.CreatedBy
                    },
                    new ModuleAccess
                    {
                        ModuleId = featuresModule.Id,
                        ModuleAccessName = "Read All Permissions",
                        RefCode = "FEATURES_READ_PERMISSIONS",
                        Description = "View all available permissions",
                        IsVisible = false,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = featuresModule.CreatedBy
                    },
                    new ModuleAccess
                    {
                        ModuleId = featuresModule.Id,
                        ModuleAccessName = "Manage Role Permissions",
                        RefCode = "FEATURES_UPDATE_ROLE_PERMISSIONS",
                        Description = "Update permissions assigned to roles",
                        IsVisible = false,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = featuresModule.CreatedBy
                    },

                    // EMPLOYEE_MANAGEMENT module accesses
                    new ModuleAccess
                    {
                        ModuleId = employeeModule.Id,
                        ModuleAccessName = "View Employee Records",
                        RefCode = "EMPLOYEE_READ",
                        Description = "View employee records and basic profile information",
                        IsVisible = true,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = employeeModule.CreatedBy
                    },
                    new ModuleAccess
                    {
                        ModuleId = employeeModule.Id,
                        ModuleAccessName = "Create Employee",
                        RefCode = "EMPLOYEE_CREATE",
                        Description = "Create new employee records",
                        IsVisible = true,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = employeeModule.CreatedBy
                    },
                    new ModuleAccess
                    {
                        ModuleId = employeeModule.Id,
                        ModuleAccessName = "Edit Employee",
                        RefCode = "EMPLOYEE_UPDATE",
                        Description = "Edit existing employee records",
                        IsVisible = true,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = employeeModule.CreatedBy
                    },
                    new ModuleAccess
                    {
                        ModuleId = employeeModule.Id,
                        ModuleAccessName = "Delete Employee",
                        RefCode = "EMPLOYEE_DELETE",
                        Description = "Delete employee records",
                        IsVisible = true,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = employeeModule.CreatedBy
                    },

                    // MANAGER_MANAGEMENT module accesses
                    new ModuleAccess
                    {
                        ModuleId = managerModule.Id,
                        ModuleAccessName = "View Managers",
                        RefCode = "MANAGER_READ",
                        Description = "View manager records and team assignments",
                        IsVisible = true,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = managerModule.CreatedBy
                    },
                    new ModuleAccess
                    {
                        ModuleId = managerModule.Id,
                        ModuleAccessName = "Create Manager",
                        RefCode = "MANAGER_CREATE",
                        Description = "Add new manager records",
                        IsVisible = true,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = managerModule.CreatedBy
                    },
                    new ModuleAccess
                    {
                        ModuleId = managerModule.Id,
                        ModuleAccessName = "Edit Manager",
                        RefCode = "MANAGER_UPDATE",
                        Description = "Edit existing manager records and assignments",
                        IsVisible = true,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = managerModule.CreatedBy
                    },
                    new ModuleAccess
                    {
                        ModuleId = managerModule.Id,
                        ModuleAccessName = "Delete Manager",
                        RefCode = "MANAGER_DELETE",
                        Description = "Remove manager records",
                        IsVisible = true,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = managerModule.CreatedBy
                    },

                    // General user permissions (applies to all authenticated users)
                    new ModuleAccess
                    {
                        ModuleId = featuresModule.Id,
                        ModuleAccessName = "View Modules",
                        RefCode = "MODULE_READ",
                        Description = "View allowed modules and navigation",
                        IsVisible = false,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = featuresModule.CreatedBy
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

            var userRoleAccesses = new List<UserRoleAccess> { };
            if (sysAdminRole != null)
            {
                // Assign all MANAGE_ADMIN module accesses to SYSADMIN role
                var adminViewAccess = await context.ModuleAccesses
                .Where(ma => ma.RefCode == "ADMIN_READ")
                .FirstOrDefaultAsync();

                if (adminViewAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = sysAdminRole.Id,
                        ModuleAccessId = adminViewAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminRole.CreatedBy
                    });
                }

                var adminCreateAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "ADMIN_CREATE")
                    .FirstOrDefaultAsync();

                if (adminCreateAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = sysAdminRole.Id,
                        ModuleAccessId = adminCreateAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminRole.CreatedBy
                    });
                }

                var adminEditAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "ADMIN_UPDATE")
                    .FirstOrDefaultAsync();

                if (adminEditAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = sysAdminRole.Id,
                        ModuleAccessId = adminEditAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminRole.CreatedBy
                    });
                }

                var adminDeleteAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "ADMIN_DELETE")
                    .FirstOrDefaultAsync();
                if (adminDeleteAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = sysAdminRole.Id,
                        ModuleAccessId = adminDeleteAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminRole.CreatedBy
                    });
                }

                var adminChangePasswordAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "ADMIN_CHANGE_PASSWORD")
                    .FirstOrDefaultAsync();
                if (adminChangePasswordAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = sysAdminRole.Id,
                        ModuleAccessId = adminChangePasswordAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminRole.CreatedBy
                    });
                }

                var featuresReadRolesAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "FEATURES_READ_ROLES")
                    .FirstOrDefaultAsync();
                if (featuresReadRolesAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = sysAdminRole.Id,
                        ModuleAccessId = featuresReadRolesAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminRole.CreatedBy
                    });
                }

                var featuresReadPermissionsAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "FEATURES_READ_PERMISSIONS")
                    .FirstOrDefaultAsync();
                if (featuresReadPermissionsAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = sysAdminRole.Id,
                        ModuleAccessId = featuresReadPermissionsAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminRole.CreatedBy
                    });
                }

                var featureUpdateRolePermissionsAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "FEATURES_UPDATE_ROLE_PERMISSIONS")
                    .FirstOrDefaultAsync();
                if (featureUpdateRolePermissionsAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = sysAdminRole.Id,
                        ModuleAccessId = featureUpdateRolePermissionsAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminRole.CreatedBy
                    });
                }

                var moduleReadAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "MODULE_READ")
                    .FirstOrDefaultAsync();
                if (moduleReadAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = sysAdminRole.Id,
                        ModuleAccessId = moduleReadAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = sysAdminRole.CreatedBy
                    });
                }
            }

            // Add permissions for USERADMIN role
            var userAdminRole = await context.UserRoles
                .Where(r => r.RefCode == "USERADMIN")
                .FirstOrDefaultAsync();

            if (userAdminRole != null)
            {
                var adminChangePasswordAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "ADMIN_CHANGE_PASSWORD")
                    .FirstOrDefaultAsync();
                if (adminChangePasswordAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = userAdminRole.Id,
                        ModuleAccessId = adminChangePasswordAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = userAdminRole.CreatedBy
                    });
                }

                var employeeReadAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "EMPLOYEE_READ")
                    .FirstOrDefaultAsync();
                if (employeeReadAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = userAdminRole.Id,
                        ModuleAccessId = employeeReadAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = userAdminRole.CreatedBy
                    });
                }

                var employeeCreateAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "EMPLOYEE_CREATE")
                    .FirstOrDefaultAsync();
                if (employeeCreateAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = userAdminRole.Id,
                        ModuleAccessId = employeeCreateAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = userAdminRole.CreatedBy
                    });
                }

                var employeeUpdateAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "EMPLOYEE_UPDATE")
                    .FirstOrDefaultAsync();
                if (employeeUpdateAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = userAdminRole.Id,
                        ModuleAccessId = employeeUpdateAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = userAdminRole.CreatedBy
                    });
                }

                var employeeDeleteAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "EMPLOYEE_DELETE")
                    .FirstOrDefaultAsync();
                if (employeeDeleteAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = userAdminRole.Id,
                        ModuleAccessId = employeeDeleteAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = userAdminRole.CreatedBy
                    });
                }

                var managerReadAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "MANAGER_READ")
                    .FirstOrDefaultAsync();
                if (managerReadAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = userAdminRole.Id,
                        ModuleAccessId = managerReadAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = userAdminRole.CreatedBy
                    });
                }

                var managerCreateAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "MANAGER_CREATE")
                    .FirstOrDefaultAsync();
                if (managerCreateAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = userAdminRole.Id,
                        ModuleAccessId = managerCreateAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = userAdminRole.CreatedBy
                    });
                }

                var managerUpdateAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "MANAGER_UPDATE")
                    .FirstOrDefaultAsync();
                if (managerUpdateAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = userAdminRole.Id,
                        ModuleAccessId = managerUpdateAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = userAdminRole.CreatedBy
                    });
                }

                var managerDeleteAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "MANAGER_DELETE")
                    .FirstOrDefaultAsync();
                if (managerDeleteAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = userAdminRole.Id,
                        ModuleAccessId = managerDeleteAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = userAdminRole.CreatedBy
                    });
                }

                var moduleReadAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "MODULE_READ")
                    .FirstOrDefaultAsync();
                if (moduleReadAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = userAdminRole.Id,
                        ModuleAccessId = moduleReadAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = userAdminRole.CreatedBy
                    });
                }
            }

            // Add basic permissions for other roles (MANAGER, EMPLOYEE, HR)
            var allRoles = await context.UserRoles
                .Where(r => r.RefCode == "MANAGER" || r.RefCode == "EMPLOYEE" || r.RefCode == "HR")
                .ToListAsync();

            foreach (var role in allRoles)
            {

                var moduleReadAccess = await context.ModuleAccesses
                    .Where(ma => ma.RefCode == "MODULE_READ")
                    .FirstOrDefaultAsync();
                if (moduleReadAccess != null)
                {
                    userRoleAccesses.Add(new UserRoleAccess
                    {
                        UserRoleId = role.Id,
                        ModuleAccessId = moduleReadAccess.Id,
                        DateCreatedUTC = DateTime.UtcNow,
                        CreatedBy = role.CreatedBy
                    });
                }
            }

            if (userRoleAccesses.Count != 0)
            {
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