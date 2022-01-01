using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Vetrina.Server.Abstractions;
using Vetrina.Server.Domain;
using Vetrina.Server.Persistence;
using Vetrina.Shared.Models;
using BC = BCrypt.Net.BCrypt;

namespace Vetrina.Server.Services
{
    public class UsersService : IUsersService
    {
        private readonly VetrinaDbContext dbContext;
        private readonly IMapper mapper;
        private readonly ILogger<UsersService> logger;

        public UsersService(
            VetrinaDbContext dbContext,
            IMapper mapper,
            ILogger<UsersService> logger)
        {
            this.dbContext = dbContext;
            this.mapper = mapper;
            this.logger = logger;
        }

        public async Task<GetUsersResponse> GetUsersAsync(
            GetUsersRequest getUsersRequest,
            CancellationToken cancellationToken = default)
        {
            var skip = (getUsersRequest.Page - 1) * getUsersRequest.Limit; // TODO: add hard lower boundary for the page.
            var take = getUsersRequest.Limit; // TODO: Add hard upper boundary for the limit. e.g. 100 entities.

            var users =
                await dbContext.Users
                    .Skip(skip)
                    .Take(take)
                    .ProjectTo<UserDTO>(mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

            return new GetUsersResponse(
                GetUsersResponseType.Successful,
                users);
        }

        public async Task<GetUserByIdResponse> GetUserByIdAsync(
            GetUserByIdRequest getUserByIdRequest,
            CancellationToken cancellationToken = default)
        {
            var user =
                await dbContext.Users
                    .ProjectTo<UserDTO>(mapper.ConfigurationProvider)
                    .FirstOrDefaultAsync(
                        x => x.Id == getUserByIdRequest.UserId,
                        cancellationToken);

            if (user == null)
            {
                return new GetUserByIdResponse(
                    GetUserByIdResponseType.NotFound,
                    $"User with the provided Id ({getUserByIdRequest.UserId}) does not exist.");
            }

            return new GetUserByIdResponse(
                GetUserByIdResponseType.Successful,
                user);
        }

        public async Task<CreateUserResponse> CreateUserAsync(
            CreateUserRequest createUserRequest,
            CancellationToken cancellationToken)
        {
            var accountWithThatEmailAlreadyExists =
                await dbContext.Users.AnyAsync(
                    a => a.Email == createUserRequest.Email,
                    cancellationToken);

            if (accountWithThatEmailAlreadyExists)
            {
                return new CreateUserResponse(
                    CreateUserResponseType.ValidationError,
                    $"Email '{createUserRequest.Email}' is already registered.");
            }

            // map forgotPasswordRequest to new account object
            var user = mapper.Map<User>(createUserRequest);
            user.Created = DateTime.UtcNow;
            user.Verified = DateTime.UtcNow;
            user.PasswordHash = BC.HashPassword(createUserRequest.Password);

            dbContext.Users.Add(user);
            
            var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);

            var userDto = mapper.Map<UserDTO>(user);

            return new CreateUserResponse(
                CreateUserResponseType.Successful,
                userDto);
        }

        public async Task<UpdateUserResponse> UpdateUserAsync(
            UpdateUserRequest updateUserRequest,
            CancellationToken cancellationToken)
        {
            var user =
                await dbContext.Users.FirstOrDefaultAsync(
                    x => x.Id == updateUserRequest.UserId,
                    cancellationToken);

            if (user == null)
            {
                return new UpdateUserResponse(
                    UpdateUserResponseType.NotFound,
                    $"User with the provided Id ({updateUserRequest.UserId}) was not found.");
            }

            if (user.Email != updateUserRequest.Email 
                && await dbContext.Users.AnyAsync(x => x.Email == updateUserRequest.Email, cancellationToken: cancellationToken))
            {
                return new UpdateUserResponse(
                    UpdateUserResponseType.ValidationError,
                    $"Email '{updateUserRequest.Email}' is already taken");
            }

            if (!string.IsNullOrEmpty(updateUserRequest.Password))
            {
                user.PasswordHash = BC.HashPassword(updateUserRequest.Password);
            }

            // Copy forgotPasswordRequest to account and save
            mapper.Map(updateUserRequest, user);
            
            user.Updated = DateTime.UtcNow;

            dbContext.Users.Update(user);
            var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);

            var userDto = mapper.Map<UserDTO>(user);

            return new UpdateUserResponse(
                UpdateUserResponseType.Successful,
                userDto);
        }

        public async Task<DeleteUserResponse> DeleteUserAsync(
            DeleteUserRequest deleteUserRequest,
            CancellationToken cancellationToken)
        {
            var user =
                await dbContext.Users.FirstOrDefaultAsync(
                    x => x.Id == deleteUserRequest.UserId,
                    cancellationToken);

            if (user == null)
            {
                return new DeleteUserResponse(
                    DeleteUserResponseType.NotFound,
                    $"User with the provided Id ({deleteUserRequest.UserId}) does not exist.");
            }

            dbContext.Users.Remove(user);

            var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                return new DeleteUserResponse(
                    DeleteUserResponseType.UnexpectedError,
                    "Zero database records were affected by the operation.");
            }

            return new DeleteUserResponse(
                DeleteUserResponseType.Successful);
        }
    }
}
