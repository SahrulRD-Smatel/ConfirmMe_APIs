using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace ConfirmMe.Attributes
{
    public class RoleAuthorizeAttribute : AuthorizeAttribute, IAuthorizationFilter
    {
        private readonly string _requiredRole;

        public RoleAuthorizeAttribute(string requiredRole)
        {
            _requiredRole = requiredRole;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (!user.Identity.IsAuthenticated)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var userRole = user.FindFirst("role")?.Value;

            if (userRole != _requiredRole && userRole != "Admin")
            {
                context.Result = new ForbidResult();
            }
        }
    }
}
