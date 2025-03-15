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

namespace TicketsApi.Web.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class SubcategoriesController : ControllerBase
    {
        private readonly DataContext _context;
        
        public SubcategoriesController(DataContext context)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------
        [HttpPost]
        [Route("GetSubcategories/{id}")]
        public async Task<ActionResult<IEnumerable<Subcategory>>> GetSubcategories(int id)
        {
            List<Subcategory> subcategories = await _context.Subcategories
                .Where(x => x.CategoryId == id)
              .OrderBy(x => x.Name)
              .ToListAsync();
            return Ok(subcategories);
        }
        
        //-----------------------------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSubcategory(int id, Subcategory subcategory)
        {
            if (id != subcategory.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Subcategory oldSubcategory = await _context.Subcategories.FirstOrDefaultAsync(o => o.Id == subcategory.Id);
            oldSubcategory!.Name = subcategory.Name;
            
            _context.Update(oldSubcategory);
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

            return Ok(oldSubcategory);
        }
        
        //-----------------------------------------------------------------------------------
        [HttpPost]
        public async Task<ActionResult<Subcategory>> PostSubcategory(Subcategory subcategory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Subcategories.Add(subcategory);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(subcategory);
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
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubcategory(int id)
        {
            Subcategory subcategory = await _context.Subcategories.FindAsync(id);
            if (subcategory == null)
            {
                return NotFound();
            }

            _context.Subcategories.Remove(subcategory);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        //-----------------------------------------------------------------------------------
        [HttpGet("combo/{id}")]
        public async Task<ActionResult> GetCombo(int id)
        {
            List<Subcategory> subcategories = await _context.Subcategories
                .Where(x => x.CategoryId == id)
             .OrderBy(x => x.Name)
             .ToListAsync();

            return Ok(subcategories);
        }
    }
}
