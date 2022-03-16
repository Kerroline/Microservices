using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MSAuth.DAL;
using MSAuth.Helpers;
using MSAuth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MSAuth.Services
{
    public interface IAuthService
    {
        Task<UserWithRoles> VerifyingUserAsync(string username, string password);
    }

    public class AuthService : IAuthService
    {
        private readonly MSAuthContext _authContext;
        private readonly AppSettings _appSettings;
        private readonly UserManager<CustomUserModel> _userManager;

        public AuthService(
            MSAuthContext context,
            IOptions<AppSettings> appSettings,
            UserManager<CustomUserModel> userManager)
        {
            _authContext = context;
            _appSettings = appSettings.Value;
            _userManager = userManager;
        }

        public async Task<UserWithRoles> VerifyingUserAsync(string username, string password)
        {
            var currentUser = await _userManager.FindByNameAsync(username);

            bool userExist = currentUser is not null;

            if (userExist && ComparePasswordHash(currentUser, password))
            {
                var userRoles = await GetUserRolesAsync(currentUser);

                return new UserWithRoles(currentUser, userRoles);
            }
            else
            {
                return null;
            }
        }

        private bool ComparePasswordHash(CustomUserModel user, string password)
        {
            var verifiedPass = _userManager.PasswordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            return (verifiedPass == PasswordVerificationResult.Success);
        }

        private async Task<IEnumerable<string>> GetUserRolesAsync(CustomUserModel user)
        {
            return await _userManager.GetRolesAsync(user);
        }
    }
}
