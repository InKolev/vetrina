using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Attributes;
using Vetrina.Server.Controllers.Abstract;
using Vetrina.Shared;
using Vetrina.Shared.Models;

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
        [HttpGet]
        public ActionResult<IEnumerable<UserDTO>> GetAll()
        {
            var accounts = usersService.GetAllUsers();

            return Ok(accounts);
        }

        [CustomAuthorize]
        [HttpGet("{id:int}")]
        public ActionResult<UserDTO> GetById(int id)
        {
            // users can get their own account and admins can get any account
            if (id != CurrentUser.Id && CurrentUser.Role != RoleType.SystemAdmin)
                return Unauthorized(new { message = "Unauthorized" });

            var account = usersService.GetUserById(id);
            return Ok(account);
        }

        [CustomAuthorize(RoleType.SystemAdmin)]
        [HttpPost]
        public async Task<ActionResult<UserDTO>> Create(
            CreateUserRequest model,
            CancellationToken cancellationToken)
        {
            var account = await usersService.CreateAsync(model, cancellationToken);
            return Ok(account);
        }

        [CustomAuthorize]
        [HttpPut("{id:int}")]
        public async Task<ActionResult<UserDTO>> Update(
            int id,
            UpdateUserRequest model,
            CancellationToken cancellationToken)
        {
            // users can update their own account and admins can update any account
            if (id != CurrentUser.Id && CurrentUser.Role != RoleType.SystemAdmin)
                return Unauthorized(new { message = "Unauthorized" });

            // Only admins can update role.
            if (CurrentUser.Role != RoleType.SystemAdmin)
            {
                //model.Role = null;
            }

            var account = await usersService.UpdateAsync(id, model, cancellationToken);
            return Ok(account);
        }

        [Authorize]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(
            int id,
            CancellationToken cancellationToken)
        {
            // users can delete their own account and admins can delete any account
            if (id != CurrentUser.Id && CurrentUser.Role != RoleType.SystemAdmin)
            {
                return Unauthorized(new { message = "Unauthorized" });
            }

            await usersService.DeleteUserByIdAsync(id, cancellationToken);

            return Ok(new { message = "CurrentUser deleted successfully" });
        }

        // helper methods

    }
}
