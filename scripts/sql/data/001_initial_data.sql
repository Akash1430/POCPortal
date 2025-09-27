-- Initial Data Script
-- Generated: 2025-09-27 22:42:21
-- Description: Inserts initial data for the Employee Management System

USE EmployeeDB;

-- Disable foreign key constraints temporarily for initial setup
EXEC sp_msforeachtable "ALTER TABLE ? NOCHECK CONSTRAINT all";

DECLARE @SuperAdminRoleId INT;
DECLARE @SystemUserId INT;

-- Create System User Role first
IF NOT EXISTS (SELECT * FROM UserRoles WHERE RefCode = 'SYSADMIN')
BEGIN
    INSERT INTO UserRoles (RoleName, RefCode, Description, IsVisible, CreatedBy)
    VALUES ('System Administrator', 'SYSADMIN', 'System administrator with full access', 0, 1);
    SET @SuperAdminRoleId = SCOPE_IDENTITY();
    PRINT 'System Administrator role created with ID: ' + CAST(@SuperAdminRoleId AS NVARCHAR(10));
END
ELSE
BEGIN
    SELECT @SuperAdminRoleId = Id FROM UserRoles WHERE RefCode = 'SYSADMIN';
    PRINT 'System Administrator role already exists with ID: ' + CAST(@SuperAdminRoleId AS NVARCHAR(10));
END

-- Create System User
IF NOT EXISTS (SELECT * FROM Users WHERE UserName = 'sysadmin')
BEGIN
    INSERT INTO Users (FirstName, LastName, UserName, Password, Email, UserRoleId, CreatedBy)
    VALUES ('System', 'Administrator', 'sysadmin', 
            '$2y$12$A2GicVrRcQqrAI/YBXmuR.74G7Re.VxTKnyrJL4OSu5f.RUitO/JC',
            'sysadmin@company.com', @SuperAdminRoleId, 1);
    SET @SystemUserId = SCOPE_IDENTITY();
    PRINT 'System Administrator user created with ID: ' + CAST(@SystemUserId AS NVARCHAR(10));
END
ELSE
BEGIN
    SELECT @SystemUserId = Id FROM Users WHERE UserName = 'sysadmin';
    PRINT 'System Administrator user already exists with ID: ' + CAST(@SystemUserId AS NVARCHAR(10));
END

-- Re-enable all foreign key constraints
EXEC sp_msforeachtable "ALTER TABLE ? WITH CHECK CHECK CONSTRAINT all";

-- Insert additional user roles
INSERT INTO UserRoles (RoleName, RefCode, Description, IsVisible, CreatedBy)
SELECT * FROM (VALUES 
    ('User Administrator', 'USERADMIN', 'User administrator with user management access', 1, @SystemUserId),
    ('Employee', 'EMPLOYEE', 'Regular employee with basic access', 1, @SystemUserId),
    ('Manager', 'MANAGER', 'Manager with team management access', 1, @SystemUserId)
) AS NewRoles(RoleName, RefCode, Description, IsVisible, CreatedBy)
WHERE NOT EXISTS (SELECT * FROM UserRoles WHERE RefCode = NewRoles.RefCode);

-- Insert main modules
INSERT INTO Modules (ModuleName, ParentId, RefCode, IsVisible, LogoName, RedirectPage, SortOrder, Description, CreatedBy)
SELECT * FROM (VALUES 
    ('Manage Admin', NULL, 'MANAGE_ADMIN', 1, 'admin-icon.svg', 'admin', 1, 'Module for managing user administrators', @SystemUserId),
    ('Manage Feature', NULL, 'MANAGE_FEATURE', 1, 'feature-icon.svg', 'feature', 2, 'Module for managing user roles and permissions', @SystemUserId),
    ('Manage Employee', NULL, 'MANAGE_EMPLOYEE', 1, 'employee-icon.svg', 'employee', 3, 'Module for managing employees', @SystemUserId),
    ('Manage Manager', NULL, 'MANAGE_MANAGER', 1, 'manager-icon.svg', 'manager', 4, 'Module for managing managers', @SystemUserId)
) AS NewModules(ModuleName, ParentId, RefCode, IsVisible, LogoName, RedirectPage, SortOrder, Description, CreatedBy)
WHERE NOT EXISTS (SELECT * FROM Modules WHERE RefCode = NewModules.RefCode);

-- Insert module accesses
INSERT INTO ModuleAccesses (ModuleId, ModuleAccessName, ParentId, RefCode, Description, IsVisible, CreatedBy)
SELECT M.Id, AccessData.AccessName, NULL, AccessData.RefCode, AccessData.Description, 1, @SystemUserId
FROM (VALUES 
    ('MANAGE_ADMIN', 'Read Admin', 'ADMIN_READ', 'Access to view admin users'),
    ('MANAGE_ADMIN', 'Create Admin', 'ADMIN_CREATE', 'Access to create new admin users'),
    ('MANAGE_ADMIN', 'Edit Admin', 'ADMIN_UPDATE', 'Access to update admin user details'),
    ('MANAGE_ADMIN', 'Delete Admin', 'ADMIN_DELETE', 'Access to delete admin users'),
    ('MANAGE_ADMIN', 'Change User Password', 'ADMIN_CHANGE_PASSWORD', 'Access to change user passwords'),

    ('MANAGE_FEATURE', 'Read Roles & Permissions', 'FEATURES_READ_ROLES', 'View roles and their assigned permissions'),
    ('MANAGE_FEATURE', 'Read All Permissions', 'FEATURES_READ_PERMISSIONS', 'View all available permissions'),
    ('MANAGE_FEATURE', 'Manage Role Permissions', 'FEATURES_UPDATE_ROLE_PERMISSIONS', 'Update permissions assigned to roles'),
    
    ('MANAGE_EMPLOYEE', 'View Employee Records', 'EMPLOYEE_READ', 'View employee records and basic profile information'),
    ('MANAGE_EMPLOYEE', 'Create Employee', 'EMPLOYEE_CREATE', 'Create new employee records'),
    ('MANAGE_EMPLOYEE', 'Edit Employee', 'EMPLOYEE_UPDATE', 'Edit existing employee records'),
    ('MANAGE_EMPLOYEE', 'Delete Employee', 'EMPLOYEE_DELETE', 'Delete employee records'),

    ('MANAGE_MANAGER', 'View Managers', 'MANAGER_READ', 'View manager records and team assignments'),
    ('MANAGE_MANAGER', 'Create Manager', 'MANAGER_CREATE', 'Add new manager records'),
    ('MANAGE_MANAGER', 'Edit Manager', 'MANAGER_UPDATE', 'Edit existing manager records and assignments'),
    ('MANAGE_MANAGER', 'Delete Manager', 'MANAGER_DELETE', 'Remove manager records')
) AS AccessData(ModuleRef, AccessName, RefCode, Description)
INNER JOIN Modules M ON M.RefCode = AccessData.ModuleRef
WHERE NOT EXISTS (SELECT * FROM ModuleAccesses WHERE RefCode = AccessData.RefCode);

-- Grant only necessary access to System Administrator role (MANAGE_ADMIN and MANAGE_FEATURE modules)
DECLARE @SystemAdminRoleId INT;
SELECT @SystemAdminRoleId = Id FROM UserRoles WHERE RefCode = 'SYSADMIN';

INSERT INTO UserRoleAccesses (UserRoleId, ModuleAccessId, CreatedBy)
SELECT @SystemAdminRoleId, MA.Id, @SystemUserId
FROM ModuleAccesses MA
WHERE NOT EXISTS (
    SELECT * FROM UserRoleAccesses 
    WHERE UserRoleId = @SystemAdminRoleId AND ModuleAccessId = MA.Id
)
AND MA.RefCode IN ('ADMIN_READ', 'ADMIN_CREATE', 'ADMIN_UPDATE', 'ADMIN_DELETE', 'ADMIN_CHANGE_PASSWORD',
                   'FEATURES_READ_ROLES', 'FEATURES_READ_PERMISSIONS', 'FEATURES_UPDATE_ROLE_PERMISSIONS');

-- Grant basic access to user administrator role
DECLARE @UserAdminRoleId INT;
SELECT @UserAdminRoleId = Id FROM UserRoles WHERE RefCode = 'USERADMIN';

INSERT INTO UserRoleAccesses (UserRoleId, ModuleAccessId, CreatedBy)
SELECT @UserAdminRoleId, MA.Id, @SystemUserId
FROM ModuleAccesses MA
WHERE NOT EXISTS (
    SELECT * FROM UserRoleAccesses 
    WHERE UserRoleId = @UserAdminRoleId AND ModuleAccessId = MA.Id
)
AND MA.RefCode IN ('ADMIN_CHANGE_PASSWORD', 'EMPLOYEE_READ', 'EMPLOYEE_CREATE', 'EMPLOYEE_UPDATE', 
                   'EMPLOYEE_DELETE', 'MANAGER_READ', 'MANAGER_CREATE', 'MANAGER_UPDATE', 'MANAGER_DELETE');

PRINT 'Initial data setup completed successfully.';
