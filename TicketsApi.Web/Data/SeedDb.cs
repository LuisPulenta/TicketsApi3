using System.Threading.Tasks;
using TicketsApi.Web.Data.Entities;
using TicketsApi.Web.Helpers;
using TicketsApi.Common.Enums;
using System.Linq;
using System;
using Microsoft.EntityFrameworkCore;

namespace TicketsApi.Web.Data
{
    public class SeedDb
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public SeedDb(DataContext context, IUserHelper userHelper)
        {
            _context = context;
            _userHelper = userHelper;
        }

        public async Task SeedAsync()
        {
            await _context.Database.EnsureCreatedAsync();
            await CheckRolesAsycn();
            await CheckUserAsync("Luis", "Núñez", "luisalbertonu@gmail.com", "351 681 4963", UserType.AdminKP, null);

            await CheckCompaniesAsync();

            Company keypress = await _context.Companies.FirstOrDefaultAsync(o => o.Id ==1);
            Company fleet = await _context.Companies.FirstOrDefaultAsync(o => o.Id == 2);
            Company rowing = await _context.Companies.FirstOrDefaultAsync(o => o.Id == 3);

            //await CheckUserAsync("Pablo", "Lacuadri", "pablo@yopmail.com", "351 111 2222", UserType.AdminKP, keypress);

            //await CheckUserAsync("Darío", "Nolose", "dario@yopmail.com", "11 4444 5555", UserType.Admin, fleet);
            //await CheckUserAsync("Gonzalo", "Prieto", "gonzalo@yopmail.com", "11 2222 3333", UserType.Admin, rowing);
            
            //await CheckUserAsync("Lionel", "Messi", "messi@yopmail.com", "311 322 4620", UserType.User, fleet);
            //await CheckUserAsync("Diego", "Maradona", "maradona@yopmail.com", "311 322 4620", UserType.User, fleet);
            //await CheckUserAsync("Mario", "Kempes", "kempes@yopmail.com", "311 322 4620", UserType.User, rowing);
            //await CheckUserAsync("Gabriel", "Batistuta", "batistuta@yopmail.com", "311 322 4620", UserType.User, rowing);
            //await CheckUserAsync("Daniel", "Passarella", "passarella@yopmail.com", "311 322 4620", UserType.User, rowing);
            
            await CheckCompaniesAsync();
        }

        //--------------------------------------------------------------------------------------------
        private async Task CheckRolesAsycn()
        {
            await _userHelper.CheckRoleAsync(UserType.AdminKP.ToString());
            await _userHelper.CheckRoleAsync(UserType.Admin.ToString());
            await _userHelper.CheckRoleAsync(UserType.User.ToString());
        }

        //--------------------------------------------------------------------------------------------
        private async Task CheckUserAsync(string firstName, string lastName, string email, string phoneNumber, UserType userType, Company? company)
        {
            DateTime ahora = DateTime.Now;

            User user = await _userHelper.GetUserAsync(email);
            
            if (user == null)
            {
                user = new User
                {
                    Email = email,
                    FirstName = firstName,
                    LastName = lastName,
                    PhoneNumber = phoneNumber,
                    UserName = email,
                    UserType = userType,
                    Company = company,
                    CreateDate = ahora,
                    CreateUserId = "",
                    CreateUserName="",
                    LastChangeDate = ahora,
                    LastChangeUserId = "",
                    LastChangeUserName="",
                    Active = true,
                };

                await _userHelper.AddUserAsync(user, "123456");
                await _userHelper.AddUserToRoleAsync(user, userType.ToString());

                string token = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
                await _userHelper.ConfirmEmailAsync(user, token);
            }
        }

        //--------------------------------------------------------------------------------------------
        private async Task CheckCompaniesAsync()
        {
            if (!_context.Companies.Any())

            {
                DateTime ahora = DateTime.Now;
                User user = await _userHelper.GetUserAsync("luisalbertonu@gmail.com");

                _context.Companies.Add(new Company { 
                    Name = "Keypress", 
                    CreateDate = ahora, 
                    CreateUserId = user.Id, 
                    CreateUserName=user.FullName,
                    LastChangeDate=ahora,
                    LastChangeUserId=user.Id,
                    LastChangeName=user.FullName,
                    Active = true, 
                    Photo = "~/images/Logos/logokp.png", });
                
                _context.Companies.Add(new Company { 
                    Name = "Fleet",
                    CreateDate = ahora,
                    CreateUserId = user.Id,
                    CreateUserName = user.FullName,
                    LastChangeDate = ahora,
                    LastChangeUserId = user.Id,
                    LastChangeName = user.FullName,
                    Active = true, Photo = "~/images/Logos/logofleet.png", });

                _context.Companies.Add(new Company { 
                    Name = "Rowing",
                    CreateDate = ahora,
                    CreateUserId = user.Id,
                    CreateUserName = user.FullName,
                    LastChangeDate = ahora,
                    LastChangeUserId = user.Id,
                    LastChangeName = user.FullName,
                    Active = true, Photo = "~/images/Logos/logorowing.png" });
                await _context.SaveChangesAsync();
            }
        }
    }
}