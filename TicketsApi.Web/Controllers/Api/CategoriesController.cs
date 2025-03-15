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
    public class CategoriesController : ControllerBase
    {
        private readonly DataContext _context;
        
        public CategoriesController(DataContext context)
        {
            _context = context;
        }

        //-----------------------------------------------------------------------------------
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            List<Category> categories = await _context.Categories
                .Include(x => x.Subcategories)
              .OrderBy(x => x.Name)
              .ToListAsync();
            return Ok(categories);
        }
        
        //-----------------------------------------------------------------------------------
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            if (id != category.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Category oldCategory = await _context.Categories.FirstOrDefaultAsync(o => o.Id == category.Id);
            oldCategory!.Name = category.Name;
            
            _context.Update(oldCategory);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException!.Message.Contains("duplicada"))
                {
                    return BadRequest("Ya existe una categoría con el mismo nombre.");
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

            return Ok(oldCategory);
        }
        
        //-----------------------------------------------------------------------------------
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _context.Categories.Add(category);

            try
            {
                await _context.SaveChangesAsync();
                return Ok(category);
            }
            catch (DbUpdateException dbUpdateException)
            {
                if (dbUpdateException.InnerException.Message.Contains("duplicada"))
                {
                    return BadRequest("Ya existe esta Categoría.");
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
        public async Task<IActionResult> DeleteCategory(int id)
        {
            Category category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        
        //-----------------------------------------------------------------------------------
        [HttpGet("combo")]
        public async Task<ActionResult> GetCombo()
        {
            List<Category> categories = await _context.Categories
             .OrderBy(x => x.Name)
             .ToListAsync();

            return Ok(categories);
        }

        //-----------------------------------------------------------------------------------

        [HttpPost]
        [Route("GetCategoryById/{id}")]
        public async Task<ActionResult<Category>> GetCategoryById(int id)
        {

            Category category = await _context.Categories
                .Include(u => u.Subcategories)
                .FirstOrDefaultAsync(p => p.Id == id);


            if (category == null)
            {
                return NotFound();
            }

            return category;
        }
    }
}
