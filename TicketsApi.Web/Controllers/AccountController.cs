using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading.Tasks;
using TicketsApi.Common.Models;
using TicketsApi.Web.Data;
using TicketsApi.Web.Data.Entities;
using TicketsApi.Web.Helpers;
using TicketsApi.Web.Models;

namespace Vehicles2.Api.Controllers
{
    public class AccountController : Controller
    {
        private readonly DataContext _context;
        private readonly IUserHelper _userHelper;
        private readonly IImageHelper _imageHelper;
        private readonly IMailHelper _mailHelper;

        public AccountController(DataContext context,IUserHelper userHelper,IImageHelper imageHelper,IMailHelper mailHelper)
        {
            _context = context;
            _userHelper = userHelper;
            _imageHelper = imageHelper;
            _mailHelper = mailHelper;
        }

       
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return NotFound();
            }

            User user = await _userHelper.GetUserByIdAsync(new String(userId));
            if (user == null)
            {
                return NotFound();
            }

            IdentityResult result = await _userHelper.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                return NotFound();
            }

            return View();
        }


        public IActionResult ResetPassword(string token)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            User user = await _userHelper.GetUserAsync(model.UserName);

           
            if (user != null)
            {
                IdentityResult result = await _userHelper.ResetPasswordAsync(user, model.Token, model.Password);
                if (result.Succeeded)
                {
                    ViewBag.Message = "Contraseña cambiada.";
                    return View();
                }

                ViewBag.Message = "Error cambiando la contraseña.";
                return View(model);
            }

            ViewBag.Message = "Usuario no encontrado.";
            return View(model);
        }
    }
}