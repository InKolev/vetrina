using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Vetrina.Shared.Models;

namespace Vetrina.Server.Abstractions
{
    public interface IUsersService
    {
        Task<IEnumerable<UserDTO>> GetAllUsers(
            CancellationToken cancellationToken = default);

        Task<UserDTO> GetUserById(
            int userId,
            CancellationToken cancellationToken = default);

        Task<UserDTO> CreateAsync(
            CreateUserRequest model,
            CancellationToken cancellationToken = default);

        Task<UserDTO> UpdateAsync(
            int userId,
            UpdateUserRequest model,
            CancellationToken cancellationToken = default);

        Task DeleteUserByIdAsync(
            int userId,
            CancellationToken cancellationToken = default);
    }
}