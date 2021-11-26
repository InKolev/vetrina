using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Vetrina.Server.Domain;
using Vetrina.Shared;

namespace Vetrina.Server.Attributes
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class CustomAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly IList<RoleType> roles;

        public CustomAuthorizeAttribute(params RoleType[] roles)
        {
            this.roles = roles ?? new RoleType[] { };
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var account = (User)context.HttpContext.Items["User"];
            if (account == null || (roles.Any() && !roles.Contains(account.Role)))
            {
                // not logged in or role not authorized
                context.Result = new JsonResult(new { message = "Unauthorized" }) { StatusCode = StatusCodes.Status401Unauthorized };
            }
        }
    }
}