using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Attributes;
using Vetrina.Server.Controllers.Abstract;
using Vetrina.Server.Models;
using Vetrina.Shared;

namespace Vetrina.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ApiControllerBase
    {
        private readonly IUsersService usersService;
        private readonly ILogger<UsersController> logger;

        public UsersController(
            IUsersService usersService,
            ILogger<UsersController> logger)
        {
            this.usersService = usersService;
            this.logger = logger;
        }

        [CustomAuthorize(RoleType.SystemAdmin)]
        [HttpGet("list")]
        public async Task<ActionResult<GetUsersResponse>> GetUsersAsync(
            GetUsersRequest request,
            CancellationToken cancellationToken)
        {
            var response = await usersService.GetUsersAsync(request, cancellationToken);

            return response.Type switch
            {
                GetUsersResponseType.Successful => Ok(response),
                GetUsersResponseType.ValidationError => BadRequest(response),
                GetUsersResponseType.UnexpectedFailure => InternalServerError(),
                _ => InternalServerError()
            };
        }

        [CustomAuthorize]
        [HttpGet]
        public async Task<ActionResult<GetUserByIdResponse>> GetUserByIdAsync(
            GetUserByIdRequest request,
            CancellationToken cancellationToken)
        {
            // users can get their own account and admins can get any account
            if (request.UserId != CurrentUser.Id && CurrentUser.Role != RoleType.SystemAdmin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var response = await usersService.GetUserByIdAsync(request, cancellationToken);

            return response.Type switch
            {
                GetUserByIdResponseType.Successful => Ok(response),
                GetUserByIdResponseType.ValidationError => BadRequest(response),
                GetUserByIdResponseType.NotFound => NotFound(response),
                GetUserByIdResponseType.UnexpectedFailure => InternalServerError(),
                _ => InternalServerError()
            };
        }

        [CustomAuthorize(RoleType.SystemAdmin)]
        [HttpPost]
        public async Task<ActionResult<CreateUserResponse>> CreateUserAsync(
            CreateUserRequest request,
            CancellationToken cancellationToken)
        {
            var response = await usersService.CreateUserAsync(request, cancellationToken);

            return response.Type switch
            {
                CreateUserResponseType.Successful => Ok(response),
                CreateUserResponseType.ValidationError => BadRequest(response),
                CreateUserResponseType.UnexpectedFailure => InternalServerError(),
                _ => InternalServerError()
            };
        }

        [CustomAuthorize]
        [HttpPut]
        public async Task<ActionResult<UpdateUserResponse>> UpdateUserAsync(
            UpdateUserRequest request,
            CancellationToken cancellationToken)
        {
            // Users can update their own account and admins can update any account
            if (request.UserId != CurrentUser.Id && CurrentUser.Role != RoleType.SystemAdmin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            if (CurrentUser.Role != RoleType.SystemAdmin)
            {
                // Keep the role the same. Only admins are allowed to update user roles.
                request.Role = CurrentUser.Role;
            }

            var response = await usersService.UpdateUserAsync(request, cancellationToken);

            return response.Type switch
            {
                UpdateUserResponseType.Successful => Ok(response),
                UpdateUserResponseType.ValidationError => BadRequest(response),
                UpdateUserResponseType.NotFound => NotFound(response),
                UpdateUserResponseType.UnexpectedFailure => InternalServerError(),
                _ => InternalServerError()
            };
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<ActionResult<DeleteUserResponse>> DeleteUserAsync(
            DeleteUserRequest request,
            CancellationToken cancellationToken)
        {
            // users can delete their own account and admins can delete any account
            if (request.UserId != CurrentUser.Id && CurrentUser.Role != RoleType.SystemAdmin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            var response = await usersService.DeleteUserAsync(request, cancellationToken);
         
            return response.Type switch
            {
                DeleteUserResponseType.Successful => Ok(response),
                DeleteUserResponseType.ValidationError => BadRequest(response),
                DeleteUserResponseType.NotFound => NotFound(response),
                DeleteUserResponseType.UnexpectedError => InternalServerError(),
                _ => InternalServerError()
            };
        }
    }
}
