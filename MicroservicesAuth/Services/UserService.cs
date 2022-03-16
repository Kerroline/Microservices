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
        Task<IEnumerable<CustomUserModel>> GetAllAsync();
        Task<UserWithRoles> GetByIdAsync(string id);
        Task<CustomUserModel> CreateAsync(CustomUserModel user);
        Task<CustomUserModel> EditAsync(string id);
        Task<bool> DeleteAsync(string id);

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

        public async Task<IEnumerable<CustomUserModel>> GetAllAsync()
        {
            throw new NotImplementedException();
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

        public async Task<CustomUserModel> CreateAsync(CustomUserModel user)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> DeleteAsync(string id)
        {
            throw new NotImplementedException();
        }

        public async Task<CustomUserModel> EditAsync(string id)
        {
            throw new NotImplementedException();
        }

        private async Task<IEnumerable<string>> GetUserRolesAsync(CustomUserModel user)
        {
            return await _userManager.GetRolesAsync(user);
        }

    }
}
