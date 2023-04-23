using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TodoApp.Data;
using TodoApp.Models;

namespace TodoApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class TodoController : ControllerBase
    {
        private readonly ApiDbContext _context;

        public TodoController(ApiDbContext context)
        {
            _context = context;
        }

        // GET: api/Todo
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ItemData>>> GetAll()
        {
            return await _context.Items.ToListAsync();
        }

        // GET: api/Todo/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemData>> Get(int id)
        {
            var itemData = await _context.Items.FindAsync(id);

            if (itemData == null)
            {
                return NotFound();
            }

            return itemData;
        }

        // PUT: api/Todo/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ItemData itemData)
        {
            if (id != itemData.Id) return BadRequest();

            _context.Entry(itemData).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Todo
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        [Route("Create")]
        public async Task<ActionResult<ItemData>> Create(ItemData itemData)
        {
            if (!ModelState.IsValid) return new JsonResult("something went wrong");

            _context.Items.Add(itemData);
            await _context.SaveChangesAsync();

            return CreatedAtAction("Get", new { id = itemData.Id }, itemData);
        }

        // DELETE: api/Todo/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var itemData = await _context.Items.FindAsync(id);
            if (itemData == null) return NotFound();

            _context.Items.Remove(itemData);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool Exists(int id)
        {
            return _context.Items.Any(e => e.Id == id);
        }
    }
}
