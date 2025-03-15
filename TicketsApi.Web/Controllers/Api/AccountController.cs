using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using TicketsApi.Common.Enums;
using TicketsApi.Web.Data;
using TicketsApi.Web.Data.Entities;
using TicketsApi.Web.Helpers;
using TicketsApi.Web.Models;
using TicketsApi.Web.Models.Request;

namespace TicketsApi.Àpi.Controllers.Àpi
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IUserHelper _userHelper;
        private readonly IConfiguration _configuration;
        private readonly DataContext _context;
        private readonly IMailHelper _mailHelper;
        private readonly IImageHelper _imageHelper;
        

        public AccountController(IUserHelper userHelper, IConfiguration configuration, DataContext context, IMailHelper mailHelper, IImageHelper imageHelper )
        {
            _userHelper = userHelper;
            _configuration = configuration;
            _context = context;
            _mailHelper = mailHelper;
            _imageHelper = imageHelper;
        }

        //-------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("CreateToken")]
        public async Task<IActionResult> CreateToken([FromBody] LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user2 = await _userHelper.GetUserAsync(model.Username);
                if (user2 != null)
                {
                    var result = await _userHelper.ValidatePasswordAsync(user2, model.Password);

                    UserViewModel user = new UserViewModel
                    {
                        Id = user2.Id,
                        FirstName = user2.FirstName,
                        LastName = user2.LastName,
                        UserTypeId = (int)user2.UserType,
                        UserTypeName = user2.UserType.ToString(),
                        Email = user2.Email,
                        EmailConfirm = user2.EmailConfirmed,
                        PhoneNumber = user2.PhoneNumber,
                        CompanyId = user2.Company != null ? user2.Company.Id : 1,
                        CompanyName = user2.Company != null ? user2.Company.Name : "KeyPress",
                        CreateDate = user2.CreateDate,
                        CreateUserId = user2.CreateUserId,
                        CreateUserName = user2.CreateUserName,
                        LastChangeDate = user2.LastChangeDate,
                        LastChangeUserId = user2.LastChangeUserId,
                        LastChangeUserName = user2.LastChangeUserName,
                        Active = user2.Active,
                    };

                    if (result.Succeeded)
                    {
                        Claim[] claims = new[]
                        {
                            new Claim(JwtRegisteredClaimNames.Sub, user.Email),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                        };

                        SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Tokens:Key"]));
                        SigningCredentials credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
                        JwtSecurityToken token = new JwtSecurityToken(
                            _configuration["Tokens:Issuer"],
                            _configuration["Tokens:Audience"],
                            claims,
                            expires: DateTime.UtcNow.AddDays(99),
                            signingCredentials: credentials);
                        var results = new
                        {
                            token = new JwtSecurityTokenHandler().WriteToken(token),
                            expiration = token.ValidTo,
                            user
                        };

                        return Created(string.Empty, results);
                    }
                }
            }

            return BadRequest();
        }

        //-------------------------------------------------------------------------------------------------
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("CreateUser")]
        public async Task<ActionResult<User>> PostUser(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            User user = await _userHelper.GetUserAsync(request.Email);
            if (user != null)
            {
                return BadRequest("Ya existe un usuario registrado con  ese email.");
            }

            DateTime ahora = DateTime.Now;

            Company company = await _context.Companies.FirstOrDefaultAsync(o => o.Id == request.IdCompany);
            User createUser = await _userHelper.GetUserByIdAsync(request.CreateUserId);
            User lastChangeUser = await _userHelper.GetUserByIdAsync(request.LastChangeUserId);

            user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PhoneNumber = request.PhoneNumber,
                UserName = request.Email,
                UserType = request.IdUserType == 0 ? UserType.AdminKP : request.IdUserType == 1 ?UserType.Admin :UserType.User, 
                CompanyId = company.Id,
                CreateUserId= createUser.Id,
                CreateUserName = createUser.FullName,
                LastChangeUserId = lastChangeUser.Id,
                LastChangeUserName = lastChangeUser.FullName,
                CreateDate = ahora,
                LastChangeDate= ahora,
                Active=true,
            };

            await _userHelper.AddUserAsync(user, request.Password);
            await _userHelper.AddUserToRoleAsync(user, user.UserType.ToString());
            await SendConfirmationEmailAsync(user);

            UserViewModel user2 = new UserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserTypeId = (int)user.UserType,
                UserTypeName = user.UserType.ToString(),
                Email = user.Email,
                EmailConfirm = user.EmailConfirmed,
                PhoneNumber = user.PhoneNumber,
                CompanyId = user.Company != null ? user.Company.Id : 1,
                CompanyName = user.Company != null ? user.Company.Name : "KeyPress",
                CreateDate = user.CreateDate,
                CreateUserId = user.CreateUserId,
                CreateUserName = user.CreateUserName,
                LastChangeDate = user.LastChangeDate,
                LastChangeUserId = user.LastChangeUserId,
                LastChangeUserName = user.LastChangeUserName,
                Active = user.Active,
            };

            return Ok(user2);
        }

        //-------------------------------------------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(string id, UserRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id.ToString() != request.Id)
            {
                return BadRequest();
            }

            User user = await _userHelper.GetUserAsync(request.Email);
            if (user == null)
            {
                return BadRequest("No existe el usuario.");
            }

            DateTime ahora = DateTime.Now;

            Company company = await _context.Companies.FirstOrDefaultAsync(o => o.Id == request.IdCompany);
            User lastChangeUser = await _userHelper.GetUserByIdAsync(request.LastChangeUserId);

            user.FirstName = request.FirstName;
            user.LastName = request.LastName;
            user.PhoneNumber = request.PhoneNumber;
            user.Company = company;
            user.Active= request.Active;
            user.LastChangeUserId= lastChangeUser.Id;
            user.LastChangeUserName = lastChangeUser.FullName;
            user.LastChangeDate = ahora;
            user.UserType = request.IdUserType == 0 ? UserType.AdminKP : request.IdUserType == 1 ? UserType.Admin : UserType.User;

            await _userHelper.UpdateUserAsync(user);
            return NoContent();
        }

        //-------------------------------------------------------------------------------------------------
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                string email = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier).Value;
                User user = await _userHelper.GetUserAsync(email);
                if (user != null)
                {
                    IdentityResult result = await _userHelper.ChangePasswordAsync(user, model.OldPassword, model.NewPassword);
                    if (result.Succeeded)
                    {
                        return NoContent();
                    }
                    else
                    {
                        return BadRequest(result.Errors.FirstOrDefault().Description);
                    }
                }
                else
                {
                    return BadRequest("Usuario no encontrado.");
                }
            }

            return BadRequest(ModelState);
        }

        //-------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("RecoverPassword")]
        public async Task<IActionResult> RecoverPassword(RecoverPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userHelper.GetUserAsync(model.Email);
                if (user == null)
                {
                    return BadRequest("El correo ingresado no corresponde a ningún usuario.");
                }

                string myToken = await _userHelper.GeneratePasswordResetTokenAsync(user);
                string link = Url.Action(
                    "ResetPassword",
                    "Account",
                    new { token = myToken }, protocol: HttpContext.Request.Scheme);
                _mailHelper.SendMail(model.Email, "Tickets - Reseteo de contraseña", $"<h1>Tickets - Reseteo de contraseña</h1>" +
                    $"Para establecer una nueva contraseña haga clic en el siguiente enlace:</br></br>" +
                    $"<a href = \"{link}\">Cambio de Contraseña</a>");
                return Ok("Las instrucciones para el cambio de contraseña han sido enviadas a su email.");
            }

            return BadRequest(model);
        }

        //-------------------------------------------------------------------------------------------------
        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            List<User> users = await _context.Users
                .Include(x => x.Company)
                .Include(x => x.Tickets)
                .OrderBy(x => x.Company.Name + x.LastName + x.FirstName)
                .ToListAsync();

            List<UserViewModel> list = new List<UserViewModel>();
            foreach (User user in users)
            {
                UserViewModel userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserTypeId = (int)user.UserType,
                    UserTypeName = user.UserType.ToString(),
                    Email = user.Email,
                    EmailConfirm = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    CompanyId = user.Company != null ? user.Company.Id : 1,
                    CompanyName = user.Company != null ? user.Company.Name:"KeyPress",
                    CreateDate = user.CreateDate,
                    CreateUserId = user.CreateUserId,
                    CreateUserName = user.CreateUserName,
                    LastChangeDate = user.LastChangeDate,
                    LastChangeUserId = user.LastChangeUserId,
                    LastChangeUserName = user.LastChangeUserName,
                    Active = user.Active,
                    Tickets = user.Tickets?.Select(ticket => new TicketCabViewModel
                    {
                        Id = ticket.Id,
                        CreateDate = ticket.CreateDate,
                        CreateUserId = ticket.UserId,
                        CreateUserName = ticket.UserName,
                        CompanyId = ticket.CompanyId,
                        CompanyName = ticket.CompanyName,
                        Title = ticket.Title,
                        TicketState = ticket.TicketState,
                        AsignDate = ticket.AsignDate,
                        InProgressDate = ticket.InProgressDate,
                        FinishDate = ticket.FinishDate,
                    }).ToList(),
                };

                list.Add(userViewModel);
            }
            return Ok(list);
        }

        //-------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("GetUsers/{CompanyId}")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsersByCompanyId(int CompanyId)

        {
            List<User> users = await _context.Users
                .Include(x => x.Company)
                .Include(x => x.Tickets)
                .Where(x=>x.Company.Id == CompanyId)
                .OrderBy(x => x.Company.Name + x.LastName + x.FirstName)
                .ToListAsync();

            List<UserViewModel> list = new List<UserViewModel>();
            foreach (User user in users)
            {
                UserViewModel userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserTypeId = (int)user.UserType,
                    UserTypeName = user.UserType.ToString(),
                    Email = user.Email,
                    EmailConfirm = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    CompanyId = user.Company != null ? user.Company.Id : 1,
                    CompanyName = user.Company != null ? user.Company.Name : "KeyPress",
                    CreateDate = user.CreateDate,
                    CreateUserId = user.CreateUserId,
                    CreateUserName = user.CreateUserName,
                    LastChangeDate = user.LastChangeDate,
                    LastChangeUserId = user.LastChangeUserId,
                    LastChangeUserName = user.LastChangeUserName,
                    Active = user.Active,
                    Tickets = user.Tickets?.Select(ticket => new TicketCabViewModel
                    {
                        Id = ticket.Id,
                        CreateDate = ticket.CreateDate,
                        CreateUserId = ticket.UserId,
                        CreateUserName = ticket.UserName,
                        CompanyId = ticket.CompanyId,
                        CompanyName = ticket.CompanyName,
                        Title = ticket.Title,
                        TicketState = ticket.TicketState,
                        AsignDate = ticket.AsignDate,
                        InProgressDate = ticket.InProgressDate,
                        FinishDate = ticket.FinishDate,
                    }).ToList(),
                };
                list.Add(userViewModel);
            }
            return Ok(list);
        }

        //-------------------------------------------------------------------------------------------------
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [Route("GetUserByEmail")]
        public async Task<IActionResult> GetUser(EmailRequest email)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var user = await _context.Users.FirstOrDefaultAsync(o => o.Email.ToLower() == email.Email.ToLower());

            if (user == null)
            {
                return BadRequest();
            }
            return Ok(user);
        }

        //-------------------------------------------------------------------------------------------------
        [HttpPost("ResendToken")]
        public async Task<IActionResult> ResendTokenAsync([FromBody] EmailRequest model)
        {
            if (ModelState.IsValid)
            {
                User user = await _userHelper.GetUserAsync(model.Email);
                if (user == null)
                {
                    return BadRequest("El correo ingresado no corresponde a ningún usuario.");
                }
                
                await SendConfirmationEmailAsync(user);

                return NoContent();
            }

            return BadRequest(model);
        }

        //-------------------------------------------------------------------------------------------------
        private async Task<IActionResult> SendConfirmationEmailAsync(User user)
        {
            string myToken = await _userHelper.GenerateEmailConfirmationTokenAsync(user);
            string tokenLink = Url.Action("ConfirmEmail", "Account", new
            {
                userid = user.Id,
                token = myToken
            }, protocol: HttpContext.Request.Scheme);

            _mailHelper.SendMail(user.Email, "Tickets - Confirmación de cuenta", $"<h1>Tickets - Confirmación de cuenta</h1>" +
                $"Para habilitar el usuario, " +
                $"por favor hacer clic en el siguiente enlace: </br></br><a href = \"{tokenLink}\">Confirmar Email</a>");

            return Ok(user);
        }


        //-------------------------------------------------------------------------------------------------
        [HttpDelete]
        [Route("DeleteUserById/{Id}")]
        public async Task<IActionResult> DeleteUserById(string Id)
        {
            User user = await _userHelper.GetUserAsync(new Guid(Id));
            if (user == null)
            {
                return NotFound();
            }
            await _userHelper.DeleteUserAsync(user);
            return NoContent();
        }

        //-----------------------------------------------------------------------------------
     
        [HttpGet("combo/{Id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public async Task<ActionResult<IEnumerable<User>>> GetCombo(int Id)
        {
            List<User> users = await _context.Users
                .Include(x => x.Company)
                .OrderBy(x => x.FirstName + x.LastName)
                .Where(c => c.Active && c.CompanyId == Id && c.UserType==UserType.User)
                .ToListAsync();

            List<UserViewModel> list = new List<UserViewModel>();
            foreach (User user in users)
            {
                UserViewModel userViewModel = new UserViewModel
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    UserTypeId = (int)user.UserType,
                    UserTypeName = user.UserType.ToString(),
                    Email = user.Email,
                    EmailConfirm = user.EmailConfirmed,
                    PhoneNumber = user.PhoneNumber,
                    CompanyId = user.Company != null ? user.Company.Id : 1,
                    CompanyName = user.Company != null ? user.Company.Name : "KeyPress",
                    CreateDate = user.CreateDate,
                    CreateUserId = user.CreateUserId,
                    CreateUserName = user.CreateUserName,
                    LastChangeDate = user.LastChangeDate,
                    LastChangeUserId = user.LastChangeUserId,
                    LastChangeUserName = user.LastChangeUserName,
                    Active = user.Active,
                    Tickets = {},
                };

                list.Add(userViewModel);
            }
            return Ok(list);
        }
    }
}