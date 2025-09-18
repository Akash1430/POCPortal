using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using EmployeeManagementSystem.Application.Interfaces;
using System.Security.Claims;

namespace EmployeeManagementSystem.Application.Attributes
{
    public class RequirePermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permission;

        public RequirePermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var roleRefCode = user.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(roleRefCode))
            {
                context.Result = new ForbidResult();
                return;
            }

            var permissionService = context.HttpContext.RequestServices
                .GetService<IPermissionService>();

            if (permissionService == null)
            {
                context.Result = new StatusCodeResult(500);
                return;
            }

            var hasPermission = await permissionService.HasPermissionAsync(roleRefCode, _permission);
            if (!hasPermission)
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }
}
