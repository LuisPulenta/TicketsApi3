using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TicketsApi.Web.Data;
using TicketsApi.Web.Data.Entities;
using TicketsApi.Web.Models;
using System;
using TicketsApi.Common.Enums;

namespace TicketsApi.Web.Helpers
{
    public class UserHelper : IUserHelper
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly DataContext _context;
        private readonly SignInManager<User> _signInManager;

        public UserHelper(UserManager<User> userManager, RoleManager<IdentityRole> roleManager, DataContext context, SignInManager<User> signInManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _signInManager = signInManager;
        }

        //------------------------------------------------------------------------------
        public async Task<IdentityResult> AddUserAsync(User user, string password)
        {
            return await _userManager.CreateAsync(user, password);
        }

        //------------------------------------------------------------------------------
        public async Task AddUserToRoleAsync(User user, string roleName)
        {
            await _userManager.AddToRoleAsync(user, roleName);
        }

        //------------------------------------------------------------------------------
        public async Task CheckRoleAsync(string roleName)
        {
            bool roleExists = await _roleManager.RoleExistsAsync(roleName);
            if (!roleExists)
            {
                await _roleManager.CreateAsync(new IdentityRole { Name = roleName });
            }
        }

        //------------------------------------------------------------------------------
        public async Task<User> GetUserAsync(string email)
        {
            return await _context.Users
                .Include(x=>x.Company)
                .FirstOrDefaultAsync(x => x.Email == email);
        }

        //------------------------------------------------------------------------------
        public async Task<bool> IsUserInRoleAsync(User user, string roleName)
        {
            return await _userManager.IsInRoleAsync(user, roleName);
        }

        //------------------------------------------------------------------------------
        public async Task<SignInResult> LoginAsync(LoginViewModel model)
        {
            return await _signInManager.PasswordSignInAsync(model.Username, model.Password, model.RememberMe, false);
        }

        //------------------------------------------------------------------------------
        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }

        //------------------------------------------------------------------------------
        public async Task<User> GetUserAsync(Guid id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id.ToString());
        }

        //------------------------------------------------------------------------------
        public async Task<IdentityResult> UpdateUserAsync(User user)
        {
            User currentUser = await GetUserAsync(user.Email);
            currentUser.LastName = user.LastName;
            currentUser.FirstName = user.FirstName;
            currentUser.PhoneNumber = user.PhoneNumber;
            currentUser.Company = user.Company;
            return await _userManager.UpdateAsync(currentUser);
        }

        //------------------------------------------------------------------------------
        public async Task<IdentityResult> DeleteUserAsync(User user)
        {
            return await _userManager.DeleteAsync(user);
        }

        //------------------------------------------------------------------------------
        public async Task<User> AddUserAsync(AddUserViewModel model, string imageId, UserType userType)
        {
            Company company = await _context.Companies
                .FirstOrDefaultAsync(x => x.Id == model.CompanyId);

            User user = new User
            {
                Email = model.Username,
                FirstName = model.FirstName,
                LastName = model.LastName,
                PhoneNumber = model.PhoneNumber,
                UserName = model.Username,
                UserType = userType,
                Company = company,
            };

            IdentityResult result = await _userManager.CreateAsync(user, model.Password);
            if (result != IdentityResult.Success)
            {
                return null;
            }

            User newUser = await GetUserAsync(model.Username);
            await AddUserToRoleAsync(newUser, user.UserType.ToString());
            return newUser;
        }

        //------------------------------------------------------------------------------
        public async Task<IdentityResult> ChangePasswordAsync(User user, string oldPassword, string newPassword)
        {
            return await _userManager.ChangePasswordAsync(user, oldPassword, newPassword);
        }

        //------------------------------------------------------------------------------
        public async Task<IdentityResult> ConfirmEmailAsync(User user, string token)
        {
            return await _userManager.ConfirmEmailAsync(user, token);
        }

        //------------------------------------------------------------------------------
        public async Task<string> GenerateEmailConfirmationTokenAsync(User user)
        {
            return await _userManager.GenerateEmailConfirmationTokenAsync(user);
        }

        //------------------------------------------------------------------------------
        public async Task<string> GeneratePasswordResetTokenAsync(User user)
        {
            return await _userManager.GeneratePasswordResetTokenAsync(user);
        }

        //------------------------------------------------------------------------------
        public async Task<IdentityResult> ResetPasswordAsync(User user, string token, string password)
        {
            return await _userManager.ResetPasswordAsync(user, token, password);
        }

        //------------------------------------------------------------------------------
        public async Task<User> GetUserByIdAsync(string id)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Id == id.ToString());
        }

        //------------------------------------------------------------------------------
        public async Task<SignInResult> ValidatePasswordAsync(User user, string password)
        {
            return await _signInManager.CheckPasswordSignInAsync(user, password, false);
        }
    }
}