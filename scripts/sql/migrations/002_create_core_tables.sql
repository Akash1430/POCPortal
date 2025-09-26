-- Migration 002: Create Core Tables
-- Created: 2025-09-24
-- Description: Creates the core tables for the Employee Management System

USE EmployeeDB;

-- Create UserRoles table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserRoles' AND xtype='U')
BEGIN
    CREATE TABLE UserRoles (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        RoleName NVARCHAR(100) NOT NULL,
        RefCode NVARCHAR(50) NOT NULL UNIQUE,
        Description NVARCHAR(500) NULL,
        IsVisible BIT NOT NULL DEFAULT 1,
        DateCreatedUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LatestDateUpdatedUTC DATETIME2 NULL,
        CreatedBy INT NOT NULL,
        LatestUpdatedBy INT NULL
    );
    PRINT 'UserRoles table created successfully.';
END

-- Create Users table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Users' AND xtype='U')
BEGIN
    CREATE TABLE Users (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        FirstName NVARCHAR(100) NOT NULL,
        LastName NVARCHAR(100) NOT NULL,
        UserName NVARCHAR(50) NOT NULL UNIQUE,
        Password NVARCHAR(255) NOT NULL,
        Email NVARCHAR(255) NOT NULL UNIQUE,
        UserRoleId INT NOT NULL,
        IsFrozen BIT NOT NULL DEFAULT 0,
        LastLoginUTC DATETIME2 NULL,
        PasswordChangedUTC DATETIME2 NULL,
        DateCreatedUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LatestDateUpdatedUTC DATETIME2 NULL,
        CreatedBy INT NOT NULL,
        LatestUpdatedBy INT NULL,
        
        CONSTRAINT FK_Users_UserRoles FOREIGN KEY (UserRoleId) REFERENCES UserRoles(Id),
        CONSTRAINT FK_Users_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
        CONSTRAINT FK_Users_UpdatedBy FOREIGN KEY (LatestUpdatedBy) REFERENCES Users(Id)
    );
    PRINT 'Users table created successfully.';
END

-- Add foreign key constraints for UserRoles after Users table exists
IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserRoles_CreatedBy')
BEGIN
    ALTER TABLE UserRoles ADD CONSTRAINT FK_UserRoles_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id);
END

IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_UserRoles_UpdatedBy')
BEGIN
    ALTER TABLE UserRoles ADD CONSTRAINT FK_UserRoles_UpdatedBy FOREIGN KEY (LatestUpdatedBy) REFERENCES Users(Id);
END

-- Create Modules table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Modules' AND xtype='U')
BEGIN
    CREATE TABLE Modules (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ModuleName NVARCHAR(100) NOT NULL,
        ParentId INT NULL,
        RefCode NVARCHAR(50) NOT NULL UNIQUE,
        IsVisible BIT NOT NULL DEFAULT 1,
        LogoName NVARCHAR(100) NULL,
        RedirectPage NVARCHAR(200) NULL,
        SortOrder INT NOT NULL DEFAULT 0,
        Description NVARCHAR(500) NULL,
        DateCreatedUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LatestDateUpdatedUTC DATETIME2 NULL,
        CreatedBy INT NOT NULL,
        LatestUpdatedBy INT NULL,
        
        CONSTRAINT FK_Modules_Parent FOREIGN KEY (ParentId) REFERENCES Modules(Id),
        CONSTRAINT FK_Modules_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
        CONSTRAINT FK_Modules_UpdatedBy FOREIGN KEY (LatestUpdatedBy) REFERENCES Users(Id)
    );
    PRINT 'Modules table created successfully.';
END

-- Create ModuleAccesses table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ModuleAccesses' AND xtype='U')
BEGIN
    CREATE TABLE ModuleAccesses (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        ModuleId INT NOT NULL,
        ModuleAccessName NVARCHAR(100) NOT NULL,
        ParentId INT NULL,
        RefCode NVARCHAR(50) NOT NULL UNIQUE,
        Description NVARCHAR(500) NULL,
        IsVisible BIT NOT NULL DEFAULT 1,
        DateCreatedUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LatestDateUpdatedUTC DATETIME2 NULL,
        CreatedBy INT NOT NULL,
        LatestUpdatedBy INT NULL,
        
        CONSTRAINT FK_ModuleAccesses_Module FOREIGN KEY (ModuleId) REFERENCES Modules(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ModuleAccesses_Parent FOREIGN KEY (ParentId) REFERENCES ModuleAccesses(Id),
        CONSTRAINT FK_ModuleAccesses_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
        CONSTRAINT FK_ModuleAccesses_UpdatedBy FOREIGN KEY (LatestUpdatedBy) REFERENCES Users(Id)
    );
    PRINT 'ModuleAccesses table created successfully.';
END

-- Create UserRoleAccesses table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='UserRoleAccesses' AND xtype='U')
BEGIN
    CREATE TABLE UserRoleAccesses (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        UserRoleId INT NOT NULL,
        ModuleAccessId INT NOT NULL,
        DateCreatedUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LatestDateUpdatedUTC DATETIME2 NULL,
        CreatedBy INT NOT NULL,
        LatestUpdatedBy INT NULL,
        
        CONSTRAINT FK_UserRoleAccesses_UserRole FOREIGN KEY (UserRoleId) REFERENCES UserRoles(Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserRoleAccesses_ModuleAccess FOREIGN KEY (ModuleAccessId) REFERENCES ModuleAccesses(Id) ON DELETE CASCADE,
        CONSTRAINT FK_UserRoleAccesses_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
        CONSTRAINT FK_UserRoleAccesses_UpdatedBy FOREIGN KEY (LatestUpdatedBy) REFERENCES Users(Id)
    );
    PRINT 'UserRoleAccesses table created successfully.';
END

-- Create RefreshTokens table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='RefreshTokens' AND xtype='U')
BEGIN
    CREATE TABLE RefreshTokens (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Token NVARCHAR(500) NOT NULL UNIQUE,
        UserId INT NOT NULL,
        ExpiryDateUTC DATETIME2 NOT NULL,
        IsRevoked BIT NOT NULL DEFAULT 0,
        RevokedDateUTC DATETIME2 NULL,
        RevokedByIp NVARCHAR(100) NULL,
        ReplacedByToken NVARCHAR(500) NULL,
        ReasonRevoked NVARCHAR(200) NULL,
        DateCreatedUTC DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LatestDateUpdatedUTC DATETIME2 NULL,
        CreatedBy INT NOT NULL,
        LatestUpdatedBy INT NULL,
        
        CONSTRAINT FK_RefreshTokens_User FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE,
        CONSTRAINT FK_RefreshTokens_CreatedBy FOREIGN KEY (CreatedBy) REFERENCES Users(Id),
        CONSTRAINT FK_RefreshTokens_UpdatedBy FOREIGN KEY (LatestUpdatedBy) REFERENCES Users(Id)
    );

    CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
    PRINT 'RefreshTokens table created successfully.';
END

-- Record this migration
IF NOT EXISTS (SELECT * FROM DbVersions WHERE Version = '002')
BEGIN
    INSERT INTO DbVersions (Version, Description) 
    VALUES ('002', 'Created core tables: UserRoles, Users, Modules, ModuleAccesses, UserRoleAccesses, RefreshTokens');
END

PRINT 'Migration 002 completed successfully.';