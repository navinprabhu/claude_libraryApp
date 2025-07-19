using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Claims;

namespace LibraryApp.BookService.Infrastructure.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class AuthorizeRolesAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        public AuthorizeRolesAttribute(params string[] roles)
        {
            _roles = roles ?? throw new ArgumentNullException(nameof(roles));
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            if (!context.HttpContext.User.Identity?.IsAuthenticated ?? true)
            {
                context.Result = new UnauthorizedObjectResult(new { error = "Authentication required" });
                return;
            }

            var userRole = context.HttpContext.User.FindFirst(ClaimTypes.Role)?.Value;
            
            if (string.IsNullOrEmpty(userRole) || !_roles.Contains(userRole))
            {
                context.Result = new ForbidResult();
                return;
            }
        }
    }

    public static class UserRoles
    {
        public const string Admin = "Admin";
        public const string Member = "Member";
        
        public static readonly string[] AllRoles = { Admin, Member };
    }
}