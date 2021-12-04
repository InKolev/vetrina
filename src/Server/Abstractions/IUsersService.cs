using System.Threading;
using System.Threading.Tasks;
using Vetrina.Shared.Models;

namespace Vetrina.Server.Abstractions
{
    public interface IUsersService
    {
        Task<GetUsersResponse> GetUsersAsync(
            GetUsersRequest getUsersRequest,
            CancellationToken cancellationToken = default);

        Task<GetUserByIdResponse> GetUserByIdAsync(
            GetUserByIdRequest getUserByIdRequest,
            CancellationToken cancellationToken = default);

        Task<CreateUserResponse> CreateUserAsync(
            CreateUserRequest createUserRequest,
            CancellationToken cancellationToken = default);

        Task<UpdateUserResponse> UpdateUserAsync(
            UpdateUserRequest updateUserRequest,
            CancellationToken cancellationToken = default);

        Task<DeleteUserResponse> DeleteUserAsync(
            DeleteUserRequest deleteUserRequest,
            CancellationToken cancellationToken = default);
    }
}