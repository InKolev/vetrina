using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
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

        public async Task<IEnumerable<UserDTO>> GetAllUsers(
            CancellationToken cancellationToken = default)
        {
            var users = await dbContext.Users.ToListAsync(cancellationToken);

            return mapper.Map<List<UserDTO>>(users);
        }

        public async Task<UserDTO> GetUserById(
            int userId,
            CancellationToken cancellationToken = default)
        {
            var user =
                await dbContext.Users.FirstOrDefaultAsync(
                    x => x.Id == userId,
                    cancellationToken);

            if (user == null)
            {
                throw new Exception($"User with the provided Id({userId}) does not exist.");
            }

            return mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> CreateAsync(
            CreateUserRequest model,
            CancellationToken cancellationToken)
        {
            var accountWithThatEmailsAlreadyExists =
                await dbContext.Users.AnyAsync(
                    a => a.Email == model.Email,
                    cancellationToken);

            if (accountWithThatEmailsAlreadyExists)
            {
                throw new Exception($"Email '{model.Email}' is already registered");
            }

            // map forgotPasswordRequest to new account object
            var user = mapper.Map<User>(model);
            user.Created = DateTime.UtcNow;
            user.Verified = DateTime.UtcNow;

            // hash password
            user.PasswordHash = BC.HashPassword(model.Password);

            // save account
            dbContext.Users.Add(user);
            var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);

            return mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> UpdateAsync(
            int userId,
            UpdateUserRequest model,
            CancellationToken cancellationToken)
        {
            var account =
                await dbContext.Users.FirstOrDefaultAsync(
                    x => x.Id == userId,
                    cancellationToken);

            if (account == null)
            {

            }

            // validate
            if (account.Email != model.Email && dbContext.Users.Any(x => x.Email == model.Email))
            {
                throw new Exception($"Email '{model.Email}' is already taken");
            }

            // hash password if it was entered
            if (!string.IsNullOrEmpty(model.Password))
            {
                account.PasswordHash = BC.HashPassword(model.Password);
            }

            // copy forgotPasswordRequest to account and save
            mapper.Map(model, account);
            account.Updated = DateTime.UtcNow;
            dbContext.Users.Update(account);
            var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);

            return mapper.Map<UserDTO>(account);
        }

        public async Task DeleteUserByIdAsync(
            int userId,
            CancellationToken cancellationToken)
        {
            var user =
                await dbContext.Users.FirstOrDefaultAsync(
                    x => x.Id == userId,
                    cancellationToken);

            if (user == null)
            {
                throw new Exception($"User with the provided Id({userId}) does not exist.");
            }

            dbContext.Users.Remove(user);
            var affectedRows = await dbContext.SaveChangesAsync(cancellationToken);
            if (affectedRows == 0)
            {
                // return error
            }

            // return ok deleted
        }
    }
}
