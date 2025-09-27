namespace Models.Constants;


public static class Permissions
{
    // Admin permissions
    public const string ADMIN_CREATE = "ADMIN_CREATE";
    public const string ADMIN_READ = "ADMIN_READ";
    public const string ADMIN_UPDATE = "ADMIN_UPDATE";
    public const string ADMIN_DELETE = "ADMIN_DELETE";
    public const string ADMIN_CHANGE_PASSWORD = "ADMIN_CHANGE_PASSWORD";

    // Feature permissions
    public const string FEATURES_READ_ROLES = "FEATURES_READ_ROLES";
    public const string FEATURES_READ_PERMISSIONS = "FEATURES_READ_PERMISSIONS";
    public const string FEATURES_UPDATE_ROLE_PERMISSIONS = "FEATURES_UPDATE_ROLE_PERMISSIONS";

    // Employee permissions
    public const string EMPLOYEE_READ = "EMPLOYEE_READ";
    public const string EMPLOYEE_CREATE = "EMPLOYEE_CREATE";
    public const string EMPLOYEE_UPDATE = "EMPLOYEE_UPDATE";
    public const string EMPLOYEE_DELETE = "EMPLOYEE_DELETE";

    // Manager permissions
    public const string MANAGER_READ = "MANAGER_READ";
    public const string MANAGER_CREATE = "MANAGER_CREATE";
    public const string MANAGER_UPDATE = "MANAGER_UPDATE";
    public const string MANAGER_DELETE = "MANAGER_DELETE";
}