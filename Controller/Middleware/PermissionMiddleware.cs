using System.Security.Claims;
using Domain.Interface;

namespace Controller.Middleware
{
    public class PermissionMiddleware
    {
        private readonly RequestDelegate _next;

        public PermissionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext context,
            IPermissionService permissionService)
        {
            var endpoint = context.GetEndpoint();

            var permissionAttribute = endpoint?
                .Metadata
                .GetMetadata<PermissionAttribute>();

            // Endpoint requires no custom permission
            if (permissionAttribute == null)
            {
                await _next(context);
                return;
            }

            // Get UserId from JWT
            var userId = context.User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
            {
                context.Response.StatusCode = 401;
                return;
            }

            // Get user's permissions
            var permissions = await permissionService
                .GetPermissionsAsync(userId);

            // Check permission using bitwise
            var required = permissionAttribute.Permission;

            if ((permissions & required) != required)
            {
                context.Response.StatusCode = 403;
                return;
            }

            await _next(context);
        }
    }
}
