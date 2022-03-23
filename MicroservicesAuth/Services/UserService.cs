using Microsoft.AspNetCore.Identity;
using MSAuth.DAL;
using MSAuth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSAuth.Services
{
    public interface IUserService
    {
        Task<IEnumerable<UserWithRoles>> GetAllAsync();
        Task<UserWithRoles> GetByIdAsync(string id);
        Task<CustomUserModel> FindUserAsync(string username, string email);
        Task<CustomUserModel> CreateAsync(CreateCustomUserRequest user);
    }
    public class UserService : IUserService
    {
        private readonly MSAuthContext _authContext;

        private readonly UserManager<CustomUserModel> _userManager;

        public UserService(
            MSAuthContext context,
            UserManager<CustomUserModel> userManager)
        {
            _authContext = context;
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserWithRoles>> GetAllAsync()
        {
            List<UserWithRoles> userRolesList = new();

            var users = _userManager.Users.ToList();

            foreach (var user in users)
            {
                var userRoles = await GetUserRolesAsync(user);
                userRolesList.Add(new UserWithRoles(user, userRoles));
            }


            return userRolesList;
        }

        public async Task<UserWithRoles> GetByIdAsync(string id)
        {
            var currentUser = await _userManager.FindByIdAsync(id);

            if(currentUser is not null)
            {
                var userRoles = await GetUserRolesAsync(currentUser);
                return new UserWithRoles(currentUser, userRoles);
            }
            return null;
        }

        public async Task<CustomUserModel> FindUserAsync(string username, string email)
        {
            var candidate = await _userManager.FindByNameAsync(username) ??
                            await _userManager.FindByEmailAsync(email);
            return candidate;
        }

        public async Task<CustomUserModel> CreateAsync(CreateCustomUserRequest userRequest)
        {
            var candidate = await _userManager.FindByNameAsync(userRequest.Username) ??
                            await _userManager.FindByEmailAsync(userRequest.Email);

            if(candidate is null)
            {
                var newUser = new CustomUserModel
                {

                    UserName = userRequest.Username,
                    Email = userRequest.Email,

                };
                await _userManager.CreateAsync(newUser, userRequest.Password);
                return newUser;
            }
            return null;
        }


        private async Task<IEnumerable<string>> GetUserRolesAsync(CustomUserModel user)
        {
            return await _userManager.GetRolesAsync(user);
        }

    }
}
