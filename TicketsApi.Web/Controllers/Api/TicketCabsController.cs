using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using TicketsApi.Web.Data;
using TicketsApi.Web.Data.Entities;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using TicketsApi.Common.Enums;
using System.Linq;

using TicketsApi.Web.Models;
using TicketsApi.Web.Models.Request;
using System.IO;
using TicketsApi.Common.Helpers;
using TicketsApi.Web.Helpers;
using Org.BouncyCastle.Asn1.Ocsp;

namespace TicketsApi.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TicketCabsController : ControllerBase
    {
        private readonly DataContext _context;
        private readonly IFilesHelper _filesHelper;
        private readonly IMailHelper _mailHelper;

        public TicketCabsController(DataContext context, IFilesHelper filesHelper, IMailHelper mailHelper)
        {
            _context = context;
            _filesHelper = filesHelper;
            _mailHelper = mailHelper;
        }

        //-----------------------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TicketCab>>> GetTicketCabs()
        {
            List<TicketCab> ticketCabs = await _context.TicketCabs
                .Include(x => x.TicketDets)
              .OrderBy(x => x.CompanyName)
              .OrderBy(x => x.Id)
              .ToListAsync();

            List<TicketCabViewModel> list = new List<TicketCabViewModel>();

            foreach (TicketCab ticketCab in ticketCabs)
            {
                User createUser = await _context.Users
                .FirstOrDefaultAsync(p => p.Id == ticketCab.UserId);

                string ticketStateName = "";

                if (ticketCab.TicketState == TicketState.Enviado)
                {
                    ticketStateName = "Enviado";
                }
                if (ticketCab.TicketState == TicketState.Devuelto)
                {
                    ticketStateName = "Devuelto";
                }
                if (ticketCab.TicketState == TicketState.Asignado)
                {
                    ticketStateName = "Asignado";
                }
                if (ticketCab.TicketState == TicketState.Encurso)
                {
                    ticketStateName = "Encurso";
                }
                if (ticketCab.TicketState == TicketState.Resuelto)
                {
                    ticketStateName = "Resuelto";
                }
                if (ticketCab.TicketState == TicketState.Derivado)
                {
                    ticketStateName = "Derivado";
                }

                TicketCabViewModel ticketCabViewModel = new TicketCabViewModel
                {
                    Id = ticketCab.Id,
                    CreateDate = ticketCab.CreateDate,
                    CreateUserId = createUser.Id,
                    CreateUserName = createUser.FullName,
                    CompanyId = ticketCab.CompanyId,
                    CompanyName = ticketCab.CompanyName,
                    CategoryId = ticketCab.CategoryId,
                    CategoryName = ticketCab.CategoryName,
                    SubcategoryId = ticketCab.SubcategoryId,
                    SubcategoryName = ticketCab.SubcategoryName,
                    Title = ticketCab.Title,
                    TicketState = ticketCab.TicketState,
                    AsignDate = ticketCab.AsignDate,
                    UserAsign = ticketCab.UserAsign,
                    UserAsignName = ticketCab.UserAsignName,
                    InProgressDate = ticketCab.InProgressDate,
                    FinishDate = ticketCab.FinishDate,
                    TicketDets = ticketCab.TicketDets?.Select(ticketCab => new TicketDetViewModel
                    {
                        Id = ticketCab.Id,
                        Description = ticketCab.Description,
                        TicketState = ticketStateName,
                        StateDate = ticketCab.StateDate,
                        StateUserId = ticketCab.StateUserId,
                        StateUserName = ticketCab.StateUserName,
                        Image = ticketCab.Image,
                    }).ToList(),
                };
                list.Add(ticketCabViewModel);
            }
            return Ok(list);
        }

        //-----------------------------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTicketCab(int id, TicketCabRequest ticketCabRequest)
        {
            if (id != ticketCabRequest.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TicketCab oldTicketCab = await _context.TicketCabs.FirstOrDefaultAsync(o => o.Id == ticketCabRequest.Id);

            DateTime ahora = DateTime.Now;

            if (ticketCabRequest.TicketState == 0)
            {
                oldTicketCab.TicketState = TicketState.Enviado;
            }
            if (ticketCabRequest.TicketState == 1)
            {
                oldTicketCab.TicketState = TicketState.Devuelto;
            }
            if (ticketCabRequest.TicketState == 2)
            {
                oldTicketCab.TicketState = TicketState.Asignado;
            }
            if (ticketCabRequest.TicketState == 3)
            {
                oldTicketCab.TicketState = TicketState.Encurso;
            }
            if (ticketCabRequest.TicketState == 4)
            {
                oldTicketCab.TicketState = TicketState.Resuelto;
            }
            if (ticketCabRequest.TicketState == 5)
            {
                oldTicketCab.TicketState = TicketState.Derivado;
            }

            oldTicketCab.AsignDate = ticketCabRequest.AsignDate;
            oldTicketCab.InProgressDate = ticketCabRequest.InProgressDate;
            oldTicketCab.FinishDate = ticketCabRequest.FinishDate;
            oldTicketCab.UserAsign = ticketCabRequest.UserAsign;
            oldTicketCab.UserAsignName = ticketCabRequest.UserAsignName;

            oldTicketCab.CategoryId = ticketCabRequest.CategoryId;
            oldTicketCab.CategoryName = ticketCabRequest.CategoryName;
            oldTicketCab.SubcategoryId = ticketCabRequest.SubcategoryId;
            oldTicketCab.SubcategoryName = ticketCabRequest.SubcategoryName;

            _context.Update(oldTicketCab);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbUpdateException)
            {
                return BadRequest(dbUpdateException.InnerException.Message);
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }

            return Ok(oldTicketCab);
        }

        //-----------------------------------------------------------------------------------
        [HttpPost]
        [Route("PostTicketCab")]
        public async Task<ActionResult<TicketCab>> PostTicketCab(TicketCab ticketCab)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DateTime ahora = DateTime.Now;

            TicketCab newTicketCab = new TicketCab
            {
                Id = 0,
                CreateDate = ahora,
                UserId = ticketCab.UserId,
                UserName = ticketCab.UserName,
                CompanyId = ticketCab.CompanyId,
                CompanyName = ticketCab.CompanyName,
                Title = ticketCab.Title,
                TicketState = TicketState.Enviado,
                AsignDate = null,
                UserAsign = ticketCab.UserAsign,
                UserAsignName = ticketCab.UserAsignName,
                InProgressDate = null,
                FinishDate = null,
                CategoryId = ticketCab.CategoryId,
                CategoryName = ticketCab.CategoryName,
                SubcategoryId = ticketCab.SubcategoryId,
                SubcategoryName = ticketCab.SubcategoryName,
            };

            _context.TicketCabs.Add(newTicketCab);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(newTicketCab);
            }
            catch (DbUpdateException dbUpdateException)
            {
                return BadRequest(dbUpdateException.InnerException.Message);
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }

        //-----------------------------------------------------------------------------------
        [HttpPost]
        [Route("PostTicketDet")]
        public async Task<ActionResult<TicketCab>> PostTicketDet(TicketDetRequest ticketDetRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DateTime ahora = DateTime.Now;
            TicketCab ticketCab = await _context.TicketCabs.FirstOrDefaultAsync(o => o.Id == ticketDetRequest.TicketCabId);

            TicketState ticketState = TicketState.Enviado;

            if (ticketDetRequest.TicketState == 0)
            {
                ticketState = TicketState.Enviado;
            }
            if (ticketDetRequest.TicketState == 1)
            {
                ticketState = TicketState.Devuelto;
            }
            if (ticketDetRequest.TicketState == 2)
            {
                ticketState = TicketState.Asignado;
            }
            if (ticketDetRequest.TicketState == 3)
            {
                ticketState = TicketState.Encurso;
            }
            if (ticketDetRequest.TicketState == 4)
            {
                ticketState = TicketState.Resuelto;
            }
            if (ticketDetRequest.TicketState == 5)
            {
                ticketState = TicketState.Derivado;
            }

            TicketDet newTicketDet = new TicketDet
            {
                Id = 0,
                TicketCab = ticketCab,
                Description = ticketDetRequest.Description,
                StateDate = ahora,
                TicketState = ticketState,
                StateUserId = ticketDetRequest.StateUserId,
                StateUserName = ticketDetRequest.StateUserName,
            };

            //Foto
            if (ticketDetRequest.ImageArray != null)
            {
                var stream = new MemoryStream(ticketDetRequest.ImageArray);
                var fileName = ticketDetRequest.FileName.Substring(0, ticketDetRequest.FileName.IndexOf(".", 0));
                var guid = $"{fileName}{Guid.NewGuid().ToString()}";
                var file = $"{guid}.{ticketDetRequest.FileExtension}";
                var folder = "wwwroot\\images\\Tickets";
                var fullPath = $"~/images/Tickets/{file}";
                var response = _filesHelper.UploadPhoto(stream, folder, file);

                if (response)
                {
                    newTicketDet.Image = fullPath;
                }
            }

            _context.TicketDets.Add(newTicketDet);

            try
            {
                await _context.SaveChangesAsync();

                string ticketStateName = "";

                if (newTicketDet.TicketState == TicketState.Enviado)
                {
                    ticketStateName = "Enviado";
                }
                if (newTicketDet.TicketState == TicketState.Devuelto)
                {
                    ticketStateName = "Devuelto";
                }
                if (newTicketDet.TicketState == TicketState.Asignado)
                {
                    ticketStateName = "Asignado";
                }
                if (newTicketDet.TicketState == TicketState.Encurso)
                {
                    ticketStateName = "Encurso";
                }
                if (newTicketDet.TicketState == TicketState.Resuelto)
                {
                    ticketStateName = "Resuelto";
                }
                if (newTicketDet.TicketState == TicketState.Derivado)
                {
                    ticketStateName = "Derivado";
                }

                TicketDetViewModel ticketDetViewModel = new TicketDetViewModel
                {
                    Id = newTicketDet.Id,
                    Description = newTicketDet.Description,
                    TicketState = ticketStateName,
                    StateDate = ahora,
                    StateUserId = newTicketDet.StateUserId,
                    StateUserName = newTicketDet.StateUserName,
                    Image = newTicketDet.Image
                };

                return Ok(ticketDetViewModel);
            }
            catch (Exception exception)
            {
                return BadRequest(exception.Message);
            }
        }

        //-----------------------------------------------------------------------------------
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicketCab(int id)
        {
            TicketCab ticketCab = await _context.TicketCabs.FindAsync(id);
            if (ticketCab == null)
            {
                return NotFound();
            }

            _context.TicketCabs.Remove(ticketCab);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //-----------------------------------------------------------------------------------
        [HttpGet("{id}")]
        public async Task<ActionResult<TicketCabViewModel>> GetTicketCab(int id)
        {
            TicketCab ticketCab = await _context.TicketCabs
                .Include(u => u.TicketDets)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (ticketCab == null)
            {
                return NotFound();
            }

            User createUser = await _context.Users
                .FirstOrDefaultAsync(p => p.Id == ticketCab.UserId);

            string ticketStateName = "";

            if (ticketCab.TicketState == TicketState.Enviado)
            {
                ticketStateName = "Enviado";
            }
            if (ticketCab.TicketState == TicketState.Devuelto)
            {
                ticketStateName = "Devuelto";
            }
            if (ticketCab.TicketState == TicketState.Asignado)
            {
                ticketStateName = "Asignado";
            }
            if (ticketCab.TicketState == TicketState.Encurso)
            {
                ticketStateName = "Encurso";
            }
            if (ticketCab.TicketState == TicketState.Resuelto)
            {
                ticketStateName = "Resuelto";
            }
            if (ticketCab.TicketState == TicketState.Derivado)
            {
                ticketStateName = "Derivado";
            }

            TicketCabViewModel ticketCabViewModel = new TicketCabViewModel
            {
                Id = ticketCab.Id,
                CreateDate = ticketCab.CreateDate,
                CreateUserId = createUser.Id,
                CreateUserName = createUser.FullName,
                CompanyId = ticketCab.CompanyId,
                CompanyName = ticketCab.CompanyName,
                CategoryId = ticketCab.CategoryId,
                CategoryName = ticketCab.CategoryName,
                SubcategoryId = ticketCab.SubcategoryId,
                SubcategoryName = ticketCab.SubcategoryName,
                Title = ticketCab.Title,
                TicketState = ticketCab.TicketState,
                AsignDate = ticketCab.AsignDate,
                UserAsign = ticketCab.UserAsign,
                UserAsignName = ticketCab.UserAsignName,
                InProgressDate = ticketCab.InProgressDate,
                FinishDate = ticketCab.FinishDate,
                TicketDets = ticketCab.TicketDets?.Select(ticketDet => new TicketDetViewModel
                {
                    Id = ticketCab.Id,
                    Description = ticketDet.Description,
                    TicketState = ticketDet.TicketState.ToString(),
                    StateDate = ticketDet.StateDate,
                    StateUserId = ticketDet.StateUserId,
                    StateUserName = ticketDet.StateUserName,
                    Image = ticketDet.Image,
                }).ToList()
            };

            return ticketCabViewModel;
        }

        //-----------------------------------------------------------------------------------
        [HttpPost]
        [Route("GetTicketUser/{id}")]
        public async Task<ActionResult<IEnumerable<TicketCab>>> GetTicketUser(string id)
        {
            List<TicketCab> ticketCabs = await _context.TicketCabs
                .Include(x => x.TicketDets)
                .Where(x => x.UserId == id && x.TicketState != TicketState.Resuelto)
              .OrderBy(x => x.CompanyName)
              .OrderBy(x => x.Id)
              .ToListAsync();

            List<TicketCabViewModel> list = new List<TicketCabViewModel>();

            foreach (TicketCab ticketCab in ticketCabs)
            {
                User createUser = await _context.Users
                .FirstOrDefaultAsync(p => p.Id == ticketCab.UserId);

                string ticketStateName = "";

                if (ticketCab.TicketState == TicketState.Enviado)
                {
                    ticketStateName = "Enviado";
                }
                if (ticketCab.TicketState == TicketState.Devuelto)
                {
                    ticketStateName = "Devuelto";
                }
                if (ticketCab.TicketState == TicketState.Asignado)
                {
                    ticketStateName = "Asignado";
                }
                if (ticketCab.TicketState == TicketState.Encurso)
                {
                    ticketStateName = "Encurso";
                }
                if (ticketCab.TicketState == TicketState.Resuelto)
                {
                    ticketStateName = "Resuelto";
                }
                if (ticketCab.TicketState == TicketState.Derivado)
                {
                    ticketStateName = "Derivado";
                }

                TicketCabViewModel ticketCabViewModel = new TicketCabViewModel
                {
                    Id = ticketCab.Id,
                    CreateDate = ticketCab.CreateDate,
                    CreateUserId = createUser.Id,
                    CreateUserName = createUser.FullName,
                    CompanyId = ticketCab.CompanyId,
                    CompanyName = ticketCab.CompanyName,
                    CategoryId = ticketCab.CategoryId,
                    CategoryName = ticketCab.CategoryName,
                    SubcategoryId = ticketCab.SubcategoryId,
                    SubcategoryName = ticketCab.SubcategoryName,
                    Title = ticketCab.Title,
                    TicketState = ticketCab.TicketState,
                    AsignDate = ticketCab.AsignDate,
                    UserAsign = ticketCab.UserAsign,
                    UserAsignName = ticketCab.UserAsignName,
                    InProgressDate = ticketCab.InProgressDate,
                    FinishDate = ticketCab.FinishDate,
                    TicketDets = ticketCab.TicketDets?.Select(ticketCab => new TicketDetViewModel
                    {
                        Id = ticketCab.Id,
                        Description = ticketCab.Description,
                        TicketState = ticketStateName,
                        StateDate = ticketCab.StateDate,
                        StateUserId = ticketCab.StateUserId,
                        StateUserName = ticketCab.StateUserName,
                        Image = ticketCab.Image,
                    }).ToList(),
                };
                list.Add(ticketCabViewModel);
            }
            return Ok(list);
        }

        //-----------------------------------------------------------------------------------
        [HttpPost]
        [Route("GetTicketAdmin/{id}")]
        public async Task<ActionResult<IEnumerable<TicketCab>>> GetTicketAdmin(int id)
        {
            List<TicketCab> ticketCabs = await _context.TicketCabs
                .Include(x => x.TicketDets)
                .Where(x => x.CompanyId == id && x.TicketState != TicketState.Resuelto && x.TicketState != TicketState.Devuelto)
              .OrderBy(x => x.CompanyName)
              .OrderBy(x => x.Id)
              .ToListAsync();

            List<TicketCabViewModel> list = new List<TicketCabViewModel>();

            foreach (TicketCab ticketCab in ticketCabs)
            {
                User createUser = await _context.Users
                .FirstOrDefaultAsync(p => p.Id == ticketCab.UserId);

                string ticketStateName = "";
                if (ticketCab.TicketState == TicketState.Enviado)
                {
                    ticketStateName = "Enviado";
                }
                if (ticketCab.TicketState == TicketState.Devuelto)
                {
                    ticketStateName = "Devuelto";
                }
                if (ticketCab.TicketState == TicketState.Asignado)
                {
                    ticketStateName = "Asignado";
                }
                if (ticketCab.TicketState == TicketState.Encurso)
                {
                    ticketStateName = "Encurso";
                }
                if (ticketCab.TicketState == TicketState.Resuelto)
                {
                    ticketStateName = "Resuelto";
                }
                if (ticketCab.TicketState == TicketState.Derivado)
                {
                    ticketStateName = "Derivado";
                }

                TicketCabViewModel ticketCabViewModel = new TicketCabViewModel
                {
                    Id = ticketCab.Id,
                    CreateDate = ticketCab.CreateDate,
                    CreateUserId = createUser.Id,
                    CreateUserName = createUser.FullName,
                    CompanyId = ticketCab.CompanyId,
                    CompanyName = ticketCab.CompanyName,
                    CategoryId = ticketCab.CategoryId,
                    CategoryName = ticketCab.CategoryName,
                    SubcategoryId = ticketCab.SubcategoryId,
                    SubcategoryName = ticketCab.SubcategoryName,
                    Title = ticketCab.Title,
                    TicketState = ticketCab.TicketState,
                    AsignDate = ticketCab.AsignDate,
                    UserAsign = ticketCab.UserAsign,
                    UserAsignName = ticketCab.UserAsignName,
                    InProgressDate = ticketCab.InProgressDate,
                    FinishDate = ticketCab.FinishDate,
                    TicketDets = ticketCab.TicketDets?.Select(ticketCab => new TicketDetViewModel
                    {
                        Id = ticketCab.Id,
                        Description = ticketCab.Description,
                        TicketState = ticketStateName,
                        StateDate = ticketCab.StateDate,
                        StateUserId = ticketCab.StateUserId,
                        StateUserName = ticketCab.StateUserName,
                        Image = ticketCab.Image,
                    }).ToList(),
                };
                list.Add(ticketCabViewModel);
            }
            return Ok(list);
        }

        //-----------------------------------------------------------------------------------
        [HttpPost()]
        [Route("GetTicketAdminKP")]
        public async Task<ActionResult<IEnumerable<TicketCab>>> GetTicketAdminKP()
        {
            List<TicketCab> ticketCabs = await _context.TicketCabs
                .Include(x => x.TicketDets)
                .Where(x => x.TicketState == TicketState.Asignado || x.TicketState == TicketState.Encurso)
              .OrderBy(x => x.CompanyName)
              .OrderBy(x => x.Id)
              .ToListAsync();

            List<TicketCabViewModel> list = new List<TicketCabViewModel>();

            foreach (TicketCab ticketCab in ticketCabs)
            {
                User createUser = await _context.Users
                .FirstOrDefaultAsync(p => p.Id == ticketCab.UserId);

                string ticketStateName = "";

                if (ticketCab.TicketState == TicketState.Enviado)
                {
                    ticketStateName = "Enviado";
                }
                if (ticketCab.TicketState == TicketState.Devuelto)
                {
                    ticketStateName = "Devuelto";
                }
                if (ticketCab.TicketState == TicketState.Asignado)
                {
                    ticketStateName = "Asignado";
                }
                if (ticketCab.TicketState == TicketState.Encurso)
                {
                    ticketStateName = "Encurso";
                }
                if (ticketCab.TicketState == TicketState.Resuelto)
                {
                    ticketStateName = "Resuelto";
                }
                if (ticketCab.TicketState == TicketState.Derivado)
                {
                    ticketStateName = "Derivado";
                }

                TicketCabViewModel ticketCabViewModel = new TicketCabViewModel
                {
                    Id = ticketCab.Id,
                    CreateDate = ticketCab.CreateDate,
                    CreateUserId = createUser.Id,
                    CreateUserName = createUser.FullName,
                    CompanyId = ticketCab.CompanyId,
                    CompanyName = ticketCab.CompanyName,
                    CategoryId = ticketCab.CategoryId,
                    CategoryName = ticketCab.CategoryName,
                    SubcategoryId = ticketCab.SubcategoryId,
                    SubcategoryName = ticketCab.SubcategoryName,
                    Title = ticketCab.Title,
                    TicketState = ticketCab.TicketState,
                    AsignDate = ticketCab.AsignDate,
                    UserAsign = ticketCab.UserAsign,
                    UserAsignName = ticketCab.UserAsignName,
                    InProgressDate = ticketCab.InProgressDate,
                    FinishDate = ticketCab.FinishDate,
                    TicketDets = ticketCab.TicketDets?.Select(ticketCab => new TicketDetViewModel
                    {
                        Id = ticketCab.Id,
                        Description = ticketCab.Description,
                        TicketState = ticketStateName,
                        StateDate = ticketCab.StateDate,
                        StateUserId = ticketCab.StateUserId,
                        StateUserName = ticketCab.StateUserName,
                        Image = ticketCab.Image,
                    }).ToList(),
                };
                list.Add(ticketCabViewModel);
            }
            return Ok(list);
        }

        //-----------------------------------------------------------------------------------
        [HttpPost()]
        [Route("GetTicketResueltos")]
        public async Task<ActionResult<IEnumerable<TicketCab>>> GetTicketResueltos(TicketResueltosRequest request)
        {
            List<TicketCab> ticketCabs = new List<TicketCab>();

            if (request.UserType == 0)
            {
                ticketCabs = await _context.TicketCabs
                .Include(x => x.TicketDets)
                .Where(x => x.TicketState == TicketState.Resuelto && x.CreateDate >= request.Desde && x.CreateDate <= request.Hasta.AddDays(1))
                .OrderBy(x => x.CompanyName)
                .OrderBy(x => x.Id)
                .ToListAsync();
            }
            if (request.UserType == 1)
            {
                ticketCabs = await _context.TicketCabs
                .Include(x => x.TicketDets)
                .Where(x => x.CompanyId == request.CompanyId && x.TicketState == TicketState.Resuelto && x.CreateDate >= request.Desde && x.CreateDate <= request.Hasta.AddDays(1))
                .OrderBy(x => x.CompanyName)
                .OrderBy(x => x.Id)
                .ToListAsync();
            }
            if (request.UserType == 2)
            {
                ticketCabs = await _context.TicketCabs
                .Include(x => x.TicketDets)
                .Where(x => x.UserId == request.UserId && x.TicketState == TicketState.Resuelto && x.CreateDate >= request.Desde && x.CreateDate <= request.Hasta.AddDays(1))
                .OrderBy(x => x.CompanyName)
                .OrderBy(x => x.Id)
                .ToListAsync();
            }

            List<TicketCabViewModel> list = new List<TicketCabViewModel>();

            foreach (TicketCab ticketCab in ticketCabs)
            {
                User createUser = await _context.Users
                .FirstOrDefaultAsync(p => p.Id == ticketCab.UserId);

                string ticketStateName = "";
                if (ticketCab.TicketState == TicketState.Enviado)
                {
                    ticketStateName = "Enviado";
                }
                if (ticketCab.TicketState == TicketState.Devuelto)
                {
                    ticketStateName = "Devuelto";
                }
                if (ticketCab.TicketState == TicketState.Asignado)
                {
                    ticketStateName = "Asignado";
                }
                if (ticketCab.TicketState == TicketState.Encurso)
                {
                    ticketStateName = "Encurso";
                }
                if (ticketCab.TicketState == TicketState.Resuelto)
                {
                    ticketStateName = "Resuelto";
                }
                if (ticketCab.TicketState == TicketState.Derivado)
                {
                    ticketStateName = "Derivado";
                }

                TicketCabViewModel ticketCabViewModel = new TicketCabViewModel
                {
                    Id = ticketCab.Id,
                    CreateDate = ticketCab.CreateDate,
                    CreateUserId = createUser.Id,
                    CreateUserName = createUser.FullName,
                    CompanyId = ticketCab.CompanyId,
                    CompanyName = ticketCab.CompanyName,
                    CategoryId = ticketCab.CategoryId,
                    CategoryName = ticketCab.CategoryName,
                    SubcategoryId = ticketCab.SubcategoryId,
                    SubcategoryName = ticketCab.SubcategoryName,
                    Title = ticketCab.Title,
                    TicketState = ticketCab.TicketState,
                    AsignDate = ticketCab.AsignDate,
                    UserAsign = ticketCab.UserAsign,
                    UserAsignName = ticketCab.UserAsignName,
                    InProgressDate = ticketCab.InProgressDate,
                    FinishDate = ticketCab.FinishDate,
                    TicketDets = ticketCab.TicketDets?.Select(ticketCab => new TicketDetViewModel
                    {
                        Id = ticketCab.Id,
                        Description = ticketCab.Description,
                        TicketState = ticketStateName,
                        StateDate = ticketCab.StateDate,
                        StateUserId = ticketCab.StateUserId,
                        StateUserName = ticketCab.StateUserName,
                        Image = ticketCab.Image,
                    }).ToList(),
                };
                list.Add(ticketCabViewModel);
            }
            return Ok(list);
        }

        //-----------------------------------------------------------------------------------
        [HttpPost()]
        [Route("GetTicketParaResolver/{id}")]
        public async Task<ActionResult<IEnumerable<TicketCab>>> GetTicketParaResolver(String id)
        {
            List<TicketCab> ticketCabs = new List<TicketCab>();

            ticketCabs = await _context.TicketCabs
            .Include(x => x.TicketDets)
            .Where(x => x.UserAsign == id && x.TicketState == TicketState.Derivado)
            .OrderBy(x => x.AsignDate)
            .ToListAsync();

            List<TicketCabViewModel> list = new List<TicketCabViewModel>();

            foreach (TicketCab ticketCab in ticketCabs)
            {
                User createUser = await _context.Users
                .FirstOrDefaultAsync(p => p.Id == ticketCab.UserId);

                string ticketStateName = "";

                if (ticketCab.TicketState == TicketState.Enviado)
                {
                    ticketStateName = "Enviado";
                }
                if (ticketCab.TicketState == TicketState.Devuelto)
                {
                    ticketStateName = "Devuelto";
                }
                if (ticketCab.TicketState == TicketState.Asignado)
                {
                    ticketStateName = "Asignado";
                }
                if (ticketCab.TicketState == TicketState.Encurso)
                {
                    ticketStateName = "Encurso";
                }
                if (ticketCab.TicketState == TicketState.Resuelto)
                {
                    ticketStateName = "Resuelto";
                }
                if (ticketCab.TicketState == TicketState.Derivado)
                {
                    ticketStateName = "Derivado";
                }

                TicketCabViewModel ticketCabViewModel = new TicketCabViewModel
                {
                    Id = ticketCab.Id,
                    CreateDate = ticketCab.CreateDate,
                    CreateUserId = createUser.Id,
                    CreateUserName = createUser.FullName,
                    CompanyId = ticketCab.CompanyId,
                    CompanyName = ticketCab.CompanyName,
                    CategoryId = ticketCab.CategoryId,
                    CategoryName = ticketCab.CategoryName,
                    SubcategoryId = ticketCab.SubcategoryId,
                    SubcategoryName = ticketCab.SubcategoryName,
                    Title = ticketCab.Title,
                    TicketState = ticketCab.TicketState,
                    AsignDate = ticketCab.AsignDate,
                    UserAsign = ticketCab.UserAsign,
                    UserAsignName = ticketCab.UserAsignName,
                    InProgressDate = ticketCab.InProgressDate,
                    FinishDate = ticketCab.FinishDate,
                    TicketDets = ticketCab.TicketDets?.Select(ticketCab => new TicketDetViewModel
                    {
                        Id = ticketCab.Id,
                        Description = ticketCab.Description,
                        TicketState = ticketStateName,
                        StateDate = ticketCab.StateDate,
                        StateUserId = ticketCab.StateUserId,
                        StateUserName = ticketCab.StateUserName,
                        Image = ticketCab.Image,
                    }).ToList(),
                };
                list.Add(ticketCabViewModel);
            }
            return Ok(list);
        }

        //-----------------------------------------------------------------------------------
        [HttpPost()]
        [Route("GetTicketProcessing/{id}")]
        public async Task<ActionResult<IEnumerable<TicketCab>>> GetTicketProcessing(int id)
        {
            List<TicketCab> ticketCabs = new List<TicketCab>();

            ticketCabs = await _context.TicketCabs
            .Include(x => x.TicketDets)
            .Where(x => x.CompanyId == id && (x.TicketState == TicketState.Devuelto || x.TicketState == TicketState.Asignado || x.TicketState == TicketState.Encurso || x.TicketState == TicketState.Derivado))
            .OrderBy(x => x.Id)
            .ToListAsync();

            List<TicketCabViewModel> list = new List<TicketCabViewModel>();

            foreach (TicketCab ticketCab in ticketCabs)
            {
                User createUser = await _context.Users
                .FirstOrDefaultAsync(p => p.Id == ticketCab.UserId);

                string ticketStateName = "";

                if (ticketCab.TicketState == TicketState.Enviado)
                {
                    ticketStateName = "Enviado";
                }
                if (ticketCab.TicketState == TicketState.Devuelto)
                {
                    ticketStateName = "Devuelto";
                }
                if (ticketCab.TicketState == TicketState.Asignado)
                {
                    ticketStateName = "Asignado";
                }
                if (ticketCab.TicketState == TicketState.Encurso)
                {
                    ticketStateName = "Encurso";
                }
                if (ticketCab.TicketState == TicketState.Resuelto)
                {
                    ticketStateName = "Resuelto";
                }
                if (ticketCab.TicketState == TicketState.Derivado)
                {
                    ticketStateName = "Derivado";
                }

                TicketCabViewModel ticketCabViewModel = new TicketCabViewModel
                {
                    Id = ticketCab.Id,
                    CreateDate = ticketCab.CreateDate,
                    CreateUserId = createUser.Id,
                    CreateUserName = createUser.FullName,
                    CompanyId = ticketCab.CompanyId,
                    CompanyName = ticketCab.CompanyName,
                    CategoryId = ticketCab.CategoryId,
                    CategoryName = ticketCab.CategoryName,
                    SubcategoryId = ticketCab.SubcategoryId,
                    SubcategoryName = ticketCab.SubcategoryName,
                    Title = ticketCab.Title,
                    TicketState = ticketCab.TicketState,
                    AsignDate = ticketCab.AsignDate,
                    UserAsign = ticketCab.UserAsign,
                    UserAsignName = ticketCab.UserAsignName,
                    InProgressDate = ticketCab.InProgressDate,
                    FinishDate = ticketCab.FinishDate,
                    TicketDets = ticketCab.TicketDets?.Select(ticketCab => new TicketDetViewModel
                    {
                        Id = ticketCab.Id,
                        Description = ticketCab.Description,
                        TicketState = ticketStateName,
                        StateDate = ticketCab.StateDate,
                        StateUserId = ticketCab.StateUserId,
                        StateUserName = ticketCab.StateUserName,
                        Image = ticketCab.Image,
                    }).ToList(),
                };
                list.Add(ticketCabViewModel);
            }
            return Ok(list);
        }

        //-------------------------------------------------------------------------------------------------
        [HttpPost]
        [Route("SendEmail")]
        public void SendEmail(SendEmailRequest request)
        {
            _mailHelper.SendMail(request.to, request.cc, request.subject, request.body);
        }
    }
}