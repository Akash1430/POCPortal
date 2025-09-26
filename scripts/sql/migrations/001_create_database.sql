-- Migration 001: Create Database
-- Created: 2025-09-24
-- Description: Creates the EmployeeDB database and initial schema

-- Create database if it doesn't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'EmployeeDB')
BEGIN
    CREATE DATABASE EmployeeDB;
    PRINT 'Database EmployeeDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database EmployeeDB already exists.';
END
GO

-- Use the database
USE EmployeeDB;

-- Create version tracking table
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='DbVersions' AND xtype='U')
BEGIN
    CREATE TABLE DbVersions (
        Id INT IDENTITY(1,1) PRIMARY KEY,
        Version NVARCHAR(50) NOT NULL,
        Description NVARCHAR(500),
        AppliedDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        AppliedBy NVARCHAR(100) NOT NULL DEFAULT SYSTEM_USER
    );
    PRINT 'DbVersions table created successfully.';
END

-- Record this migration
IF NOT EXISTS (SELECT * FROM DbVersions WHERE Version = '001')
BEGIN
    INSERT INTO DbVersions (Version, Description) 
    VALUES ('001', 'Initial database creation and version tracking');
END