using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Vetrina.Server.Constants;
using Vetrina.Server.Options;
using Vetrina.Server.Persistence;

namespace Vetrina.Server.Middlewares
{
    public class JwtMiddleware
    {
        private readonly RequestDelegate next;
        private readonly JwtOptions jwtOptions;

        public JwtMiddleware(
            RequestDelegate next, 
            IOptions<JwtOptions> jwtOptions)
        {
            this.next = next;
            this.jwtOptions = jwtOptions.Value;
        }

        public async Task Invoke(
            HttpContext context, 
            VetrinaDbContext dbContext)
        {
            var token = context.Request.Headers[HeaderNames.Authorization]
                .FirstOrDefault()?
                .Split(" ")
                .Last();

            if (token != null)
            {
                await AttachUserToContext(context, dbContext, token);
            }

            await next(context);
        }

        private async Task AttachUserToContext(HttpContext context, VetrinaDbContext dataContext, string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var secretKey = Encoding.ASCII.GetBytes(jwtOptions.Secret);

                var tokenValidationParameters =
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(secretKey),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };

                var claimsPrincipal = tokenHandler.ValidateToken(token, tokenValidationParameters , out var validatedToken);
                var jwtToken = (JwtSecurityToken)validatedToken;
                var accountId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);

                // attach account to context on successful jwt validation
                context.Items[AuthenticationControllerConstants.CurrentUser] = await dataContext.Users.FindAsync(accountId);
            }
            catch 
            {
                // Do nothing if JWT validation fails
                // User is not attached to the context so request won't have access to secured routes
            }
        }
    }
}