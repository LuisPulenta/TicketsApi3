using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using TicketsApi.Web.Data;
using TicketsApi.Web.Data.Entities;
using System.Linq;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using System.IO;
using TicketsApi.Common.Helpers;
using TicketsApi.Web.Models.Request;
using Org.BouncyCastle.Asn1.Ocsp;
using TicketsApi.Web.Helpers;
using TicketsApi.Web.Models;
using System.Numerics;

namespace TicketsApi.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CompaniesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IFilesHelper _filesHelper;
        private readonly IUserHelper _userHelper;

        public CompaniesController(IUserHelper userHelper,DataContext context, IFilesHelper filesHelper )
        {
            _context = context;
            _filesHelper = filesHelper;
            _userHelper = userHelper;
        }

        //-----------------------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetCompanies()
        {
            List<Company> companies = await _context.Companies
              .Include(x => x.Users)
              .OrderBy(x => x.Name)
              .ToListAsync();

            List<CompanyViewModel> list = new List<CompanyViewModel>();

            foreach (Company company in companies)
            {
                CompanyViewModel companyViewModel = new CompanyViewModel
                {
                    Id = company.Id,
                    Name = company.Name,
                    CreateDate = company.CreateDate,
                    CreateUserId = company.CreateUserId,
                    CreateUserName = company.CreateUserName,
                    LastChangeDate = company.LastChangeDate,
                    LastChangeUserId = company.LastChangeUserId,
                    LastChangeUserName = company.LastChangeName,
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

                list.Add(companyViewModel);
            }
            return Ok(list);
        }

        //-----------------------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<CompanyViewModel>> GetCompany(int id)
        {

            Company company = await _context.Companies
                .Include(u => u.Users)
                .FirstOrDefaultAsync(p => p.Id == id);


            if (company == null)
            {
                return NotFound();
            }

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
                Photo = company.Photo,
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

        //-----------------------------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCompany(int id, CompanyRequest companyRequest)
        {
            if (id != companyRequest.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Company oldCompany = await _context.Companies.FirstOrDefaultAsync(o => o.Id == companyRequest.Id);

            //Foto
            string imageUrl = string.Empty;
            if (companyRequest.ImageArray != null && companyRequest.ImageArray.Length > 0)
            {
                imageUrl = string.Empty;
                var stream = new MemoryStream(companyRequest.ImageArray);
                var guid = Guid.NewGuid().ToString();
                var file = $"{guid}.jpg";
                var folder = "wwwroot\\images\\Logos";
                var fullPath = $"~/images/Logos/{file}";
                var response = _filesHelper.UploadPhoto(stream, folder, file);

                if (response)
                {
                    imageUrl = fullPath;
                    oldCompany!.Photo = imageUrl;
                }
            }

            DateTime ahora = DateTime.Now;
            User lastChangeUser = await _userHelper.GetUserByIdAsync(companyRequest.LastChangeUserId);

            oldCompany!.Active = companyRequest.Active;
            oldCompany!.Name = companyRequest.Name;
            oldCompany!.LastChangeDate=ahora;
            oldCompany!.LastChangeUserId=lastChangeUser.Id;
            oldCompany.LastChangeName = lastChangeUser.FullName;

            _context.Update(oldCompany);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException!.Message.Contains("duplicada"))
                {
                    return BadRequest("Ya existe una empresa con el mismo nombre.");
                }
                else
                {
                    return BadRequest(dbUpdateException.InnerException.Message);
                }
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }

            return Ok(oldCompany);
        }
        
        //-----------------------------------------------------------------------------------
        [HttpPost]
        public async Task<ActionResult<Company>> PostCompany(CompanyRequest companyRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DateTime ahora = DateTime.Now;
            User createUser = await _userHelper.GetUserByIdAsync(companyRequest.CreateUserId);
            User lastChangeUser = await _userHelper.GetUserByIdAsync(companyRequest.LastChangeUserId);

            Company newCompany = new Company
            {
                Id = 0,
                Name = companyRequest.Name,
                Active = true,
                CreateUserId = createUser.Id,
                CreateUserName=createUser.UserName,
                CreateDate = ahora,
                LastChangeDate=ahora,
                LastChangeUserId=lastChangeUser.Id,
                LastChangeName=lastChangeUser.FullName,
                Photo = null
            };


            //Foto


            if (companyRequest.ImageArray != null) {
                var stream = new MemoryStream(companyRequest.ImageArray);
                var guid = Guid.NewGuid().ToString();
                var file = $"{guid}.jpg";
                var folder = "wwwroot\\images\\Logos";
                var fullPath = $"~/images/Logos/{file}";
                var response = _filesHelper.UploadPhoto(stream, folder, file);

                if (response)
                {
                    newCompany.Photo = fullPath;
                }
            }
            _context.Companies.Add(newCompany);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(newCompany);
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException.Message.Contains("duplicada"))
                {
                    return BadRequest("Ya existe esta Empresa.");
                }
                else
                {
                    return BadRequest(dbUpdateException.InnerException.Message);
                }
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }
        //-----------------------------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCompany(int id)
        {
            Company company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }

            _context.Companies.Remove(company);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        //-----------------------------------------------------------------------------------
        [HttpGet("combo")]
        public async Task<ActionResult> GetCombo()
        {
            List<Company> companies = await _context.Companies
             .OrderBy(x => x.Name)
             .Where(c => c.Active)
             .ToListAsync();

            List<CompanyViewModel> list = new List<CompanyViewModel>();

            foreach (Company company in companies)
            {
                CompanyViewModel companyViewModel = new CompanyViewModel
                {
                    Id = company.Id,
                    Name = company.Name,
                    CreateDate = company.CreateDate,
                    CreateUserId = company.CreateUserId,
                    CreateUserName = company.CreateUserName,
                    LastChangeDate = company.LastChangeDate,
                    LastChangeUserId = company.LastChangeUserId,
                    LastChangeUserName = company.LastChangeName,
                    Active = company.Active,
                    Photo = company.Photo,
                    Users = {},
                };

                list.Add(companyViewModel);
            }
            return Ok(list);
        }
    }
}
