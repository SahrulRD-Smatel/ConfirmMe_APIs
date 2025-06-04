using ConfirmMe.Middleware;

namespace ConfirmMe.Extensions
{
    public static class RoleAuthorizationExtensions
    {
        public static IApplicationBuilder UseRoleAuthorization(this IApplicationBuilder builder, string role)
        {
            return builder.UseMiddleware<RoleAuthorizationMiddleware>(role);
        }
    }
}
