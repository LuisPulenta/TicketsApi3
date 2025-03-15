using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TicketsApi.Common.Enums;
using TicketsApi.Web.Data;
using TicketsApi.Web.Data.Entities;
using TicketsApi.Web.Models;

namespace TicketsApi.Web.Helpers
{
    public class ConverterHelper : IConverterHelper
    {
        private readonly DataContext _context;
        private readonly UserHelper _userHelper;

        public ConverterHelper(DataContext context, UserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        //-------------------------------------------------------------------------------------------------
        public async Task<User> ToUserAsync(UserViewModel model)
        {
            User createUser = await _userHelper.GetUserByIdAsync(model.CreateUserId);
            User lastChangeUser = await _userHelper.GetUserByIdAsync(model.LastChangeUserId);

            Company company = await _context.Companies.FirstOrDefaultAsync(o => o.Id == model.CompanyId);

            UserType userType = model.UserTypeId == 0 ? UserType.AdminKP : model.UserTypeId == 1 ? UserType.Admin : UserType.User;

            return new User
            {
                Id=model.Id,
                FirstName = model.FirstName,
                LastName = model.LastName,
                UserType = userType,
                Email = model.Email,
                EmailConfirmed=model.EmailConfirm,
                UserName = model.Email,
                PhoneNumber = model.PhoneNumber,
                Company= company,
                CreateDate=model.CreateDate,
                CreateUserId= createUser.Id,
                CreateUserName=createUser.UserName,
                LastChangeDate=model.LastChangeDate,
                LastChangeUserId= lastChangeUser.Id,
                LastChangeUserName=lastChangeUser.UserName, 
                Active=model.Active,
            };
        }

        //-------------------------------------------------------------------------------------------------
        public UserViewModel ToUserViewModel(User user)
        {
            return new UserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserTypeId = (int) user.UserType,
                UserTypeName = user.UserType.ToString(),
                Email = user.Email,
                EmailConfirm=user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                CompanyId=user.Company.Id,
                CompanyName=user.Company.Name,
                CreateDate=user.CreateDate,
                CreateUserId=user.CreateUserId,
                CreateUserName=user.CreateUserName,
                LastChangeDate = user.LastChangeDate,
                LastChangeUserId = user.LastChangeUserId,
                LastChangeUserName = user.LastChangeUserName,
                Active=user.Active,
            };
        }

        //-------------------------------------------------------------------------------------------------

        public async Task<Company> ToCompanyAsync(CompanyViewModel model)
        {
            User createUser = await _userHelper.GetUserByIdAsync(model.CreateUserId);
            
            return new Company
            {
                Id = model.Id,
                Name = model.Name,
                CreateDate = model.CreateDate,
                CreateUserId = createUser.Id,
                CreateUserName = createUser.FullName,
                LastChangeDate=createUser.LastChangeDate,
                LastChangeUserId = createUser.Id,
                LastChangeName = createUser.FullName,
                Active = model.Active,
                Photo = model.Photo,
            };
        }

        //-------------------------------------------------------------------------------------------------
        public CompanyViewModel ToCompanyViewModel(Company company)
        {
            return new CompanyViewModel
            {
                Id = company.Id,
                Name = company.Name,
                CreateDate = company.CreateDate,
                CreateUserId = company.CreateUserId,
                CreateUserName = company.CreateUserName,
                LastChangeDate = company.LastChangeDate,
                LastChangeUserId = company.LastChangeUserId,
                LastChangeUserName = company.CreateUserName,
                Active = company.Active,
                Photo=company.Photo,
                Users = company.Users?.Select(user => new UserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserTypeId = (int)user.UserType,
                    UserTypeName = user.UserType.ToString(),
                    Email = user.Email,
                    EmailConfirm = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    CompanyId = user.Company.Id,
                    CompanyName = user.Company.Name,
                    CreateDate = user.CreateDate,
                    CreateUserId = user.CreateUserId,
                    CreateUserName = user.CreateUserName,
                    LastChangeDate = user.LastChangeDate,
                    LastChangeUserId = user.LastChangeUserId,
                    LastChangeUserName = user.LastChangeUserName,
                    Active = user.Active,
                }).ToList(),

            };
        }
        
        //-------------------------------------------------------------------------------------------------
        public List<UserViewModel> ToUserResponse(List<User> users)
        {
            List<UserViewModel> list = new List<UserViewModel>();
            foreach (User user in users)
            {
                list.Add(ToUserViewModel(user));
            }

            return list;
        }
    }
}
