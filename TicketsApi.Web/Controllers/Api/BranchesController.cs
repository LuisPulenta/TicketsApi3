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
using TicketsApi.Web.Models.Request;
using TicketsApi.Web.Helpers;
using TicketsApi.Web.Models;

namespace TicketsApi.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class BranchesController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;

        public BranchesController(IUserHelper userHelper, DataContext context)
        {
            _context = context;
            _userHelper = userHelper;
        }

        //-----------------------------------------------------------------------------------
        [HttpGet]
        [Route("GetBranches/{companyId}")]
        public async Task<ActionResult<IEnumerable<Branch>>> GetBranches(int companyId)
        {
            List<Branch> branches = await _context.Branches
              .Include(x => x.Users)
              .Where(x => x.Company.Id == companyId)
              .OrderBy(x => x.Name)
              .ToListAsync();

            List<BranchViewModel> list = new List<BranchViewModel>();

            foreach (Branch branch in branches)
            {
                BranchViewModel branchViewModel = new BranchViewModel
                {
                    Id = branch.Id,
                    Name = branch.Name,
                    CreateDate = branch.CreateDate,
                    CreateUserId = branch.CreateUserId,
                    CreateUserName = branch.CreateUserName,
                    LastChangeDate = branch.LastChangeDate,
                    LastChangeUserId = branch.LastChangeUserId,
                    LastChangeUserName = branch.LastChangeName,
                    Active = branch.Active,

                    Users = branch.Users?.Select(user => new UserViewModel
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
                    }
                    ).ToList(),
                };

                list.Add(branchViewModel);
            }
            return Ok(list);
        }

        //-----------------------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<BranchViewModel>> GetBranch(int id)
        {
            Branch branch = await _context.Branches
                .Include(u => u.Users)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (branch == null)
            {
                return NotFound();
            }

            return new BranchViewModel
            {
                Id = branch.Id,
                Name = branch.Name,
                CreateDate = branch.CreateDate,
                CreateUserId = branch.CreateUserId,
                CreateUserName = branch.CreateUserName,
                LastChangeDate = branch.LastChangeDate,
                LastChangeUserId = branch.LastChangeUserId,
                LastChangeUserName = branch.CreateUserName,
                Active = branch.Active,
                Users = branch.Users?.Select(user => new UserViewModel
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
        public async Task<IActionResult> PutBranch(int id, BranchRequest branchRequest)
        {
            if (id != branchRequest.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Branch oldBranch = await _context.Branches
                .Include(c=>c.Company)
                .FirstOrDefaultAsync(o => o.Id == branchRequest.Id);

            DateTime ahora = DateTime.Now;
            User lastChangeUser = await _userHelper.GetUserByIdAsync(branchRequest.LastChangeUserId);

            oldBranch!.Active = branchRequest.Active;
            oldBranch!.Name = branchRequest.Name;
            oldBranch!.LastChangeDate = ahora;
            oldBranch!.LastChangeUserId = lastChangeUser.Id;
            oldBranch.LastChangeName = lastChangeUser.FullName;



            _context.Update(oldBranch);
            try
            {
                await _context.SaveChangesAsync();
                BranchViewModel branchViewModel = new BranchViewModel
                {
                    Id = oldBranch.Id,
                    Name = oldBranch.Name,
                    CreateDate = oldBranch.CreateDate,
                    CreateUserId = oldBranch.CreateUserId,
                    CreateUserName = oldBranch.CreateUserName,
                    LastChangeDate = oldBranch.LastChangeDate,
                    LastChangeUserId = oldBranch.LastChangeUserId,
                    LastChangeUserName = oldBranch.LastChangeName,
                    Active = oldBranch.Active,
                    CompanyId = oldBranch.Company.Id,
                    CompanyName = oldBranch.Company.Name,
                };

                return Ok(branchViewModel);
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException!.Message.Contains("duplicada"))
                {
                    return BadRequest("Ya existe una sucursal con el mismo nombre.");
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
        [HttpPost]
        public async Task<ActionResult<Branch>> PostBranch(BranchRequest branchRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DateTime ahora = DateTime.Now;
            User createUser = await _userHelper.GetUserByIdAsync(branchRequest.CreateUserId);
            User lastChangeUser = await _userHelper.GetUserByIdAsync(branchRequest.LastChangeUserId);
            Company company = await _context.Companies
                .FirstOrDefaultAsync(p => p.Id == branchRequest.CompanyId);

            Branch newBranch = new Branch
            {
                Id = 0,
                Name = branchRequest.Name,
                Active = true,
                CreateUserId = createUser.Id,
                CreateUserName = createUser.UserName,
                CreateDate = ahora,
                LastChangeDate = ahora,
                LastChangeUserId = lastChangeUser.Id,
                LastChangeName = lastChangeUser.FullName,
                Company = company
            };

            _context.Branches.Add(newBranch);

            try
            {
                await _context.SaveChangesAsync();

                BranchViewModel branchViewModel = new BranchViewModel
                {
                    Id = newBranch.Id,
                    Name = newBranch.Name,
                    CreateDate = newBranch.CreateDate,
                    CreateUserId = newBranch.CreateUserId,
                    CreateUserName = newBranch.CreateUserName,
                    LastChangeDate = newBranch.LastChangeDate,
                    LastChangeUserId = newBranch.LastChangeUserId,
                    LastChangeUserName = newBranch.LastChangeName,
                    Active = newBranch.Active,
                    CompanyId = newBranch.Company.Id,
                    CompanyName = newBranch.Company.Name,
                };

                return Ok(branchViewModel);
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException.Message.Contains("duplicada"))
                {
                    return BadRequest("Ya existe esta Sucursal.");
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
        public async Task<IActionResult> DeleteBranch(int id)
        {
            Branch branch = await _context.Branches.FindAsync(id);
            if (branch == null)
            {
                return NotFound();
            }

            _context.Branches.Remove(branch);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //-----------------------------------------------------------------------------------
        [HttpGet]
        [Route("combo/{companyId}")]
        public async Task<ActionResult> GetCombo(int companyId)
        {
            List<Branch> branches = await _context.Branches
             .OrderBy(x => x.Name)
             .Where(c => c.Active && c.Company.Id == companyId)
             .ToListAsync();

            List<BranchViewModel> list = new List<BranchViewModel>();

            foreach (Branch branch in branches)
            {
                BranchViewModel branchViewModel = new BranchViewModel
                {
                    Id = branch.Id,
                    Name = branch.Name,
                    CreateDate = branch.CreateDate,
                    CreateUserId = branch.CreateUserId,
                    CreateUserName = branch.CreateUserName,
                    LastChangeDate = branch.LastChangeDate,
                    LastChangeUserId = branch.LastChangeUserId,
                    LastChangeUserName = branch.LastChangeName,
                    Active = branch.Active,
                    Users = { },
                    CompanyId = branch.Company.Id,
                    CompanyName = branch.Company.Name,
                };

                list.Add(branchViewModel);
            }
            return Ok(list);
        }
    }
}