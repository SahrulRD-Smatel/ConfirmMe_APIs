namespace ConfirmMe.Middleware
{
    public class RoleAuthorizationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly string _requiredRole;

        public RoleAuthorizationMiddleware(RequestDelegate next, string requiredRole)
        {
            _next = next;
            _requiredRole = requiredRole;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var user = context.User;
            if (user.Identity != null && user.Identity.IsAuthenticated)
            {
                var userRole = user.FindFirst("role")?.Value;

                if (userRole == _requiredRole || userRole == "Manager") // Manager always allowed
                {
                    await _next(context);
                    return;
                }

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsync("Forbidden: You do not have the required role.");
                return;
            }

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Please login.");
        }
    }
}
