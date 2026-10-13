using InventoryApi.Data;
using InventoryApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace InventoryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryItemsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InventoryItemsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /api/inventoryitems
        [HttpGet]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetItems()
        {
            return Ok(await _context.InventoryItems.ToListAsync());
        }

        // GET: /api/inventoryitems/low-stock
        [HttpGet("low-stock")]
        public async Task<ActionResult<IEnumerable<InventoryItem>>> GetLowStock()
        {
            var lowStockItems = await _context.InventoryItems
                .Where(i => i.QuantityInStock <= i.ReorderLevel)
                .ToListAsync();

            return Ok(lowStockItems);
        }

        // GET: /api/inventoryitems/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<InventoryItem>> GetItem(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"Item with ID {id} not found." });
            }

            return Ok(item);
        }

        // POST: /api/inventoryitems
        [HttpPost]
        public async Task<ActionResult<InventoryItem>> CreateItem([FromBody] InventoryItem item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            bool skuExists = await _context.InventoryItems.AnyAsync(i => i.SKU == item.SKU);
            if (skuExists)
            {
                return Conflict(new { message = $"An item with SKU '{item.SKU}' already exists." });
            }

            _context.InventoryItems.Add(item);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetItem), new { id = item.Id }, item);
        }

        // PUT: /api/inventoryitems/{id}
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateItem(int id, [FromBody] InventoryItem item)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (id != item.Id && item.Id != 0)
            {
                return BadRequest(new { message = "Path ID does not match body ID." });
            }

            var existingItem = await _context.InventoryItems.FindAsync(id);
            if (existingItem == null)
            {
                return NotFound(new { message = $"Item with ID {id} not found." });
            }

            bool skuConflict = await _context.InventoryItems
                .AnyAsync(i => i.SKU == item.SKU && i.Id != id);
            if (skuConflict)
            {
                return Conflict(new { message = $"Another item already uses SKU '{item.SKU}'." });
            }

            existingItem.Name = item.Name;
            existingItem.SKU = item.SKU;
            existingItem.Category = item.Category;
            existingItem.QuantityInStock = item.QuantityInStock;
            existingItem.ReorderLevel = item.ReorderLevel;
            existingItem.UnitPrice = item.UnitPrice;

            await _context.SaveChangesAsync();

            return Ok(existingItem);
        }

        // DELETE: /api/inventoryitems/{id}
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var item = await _context.InventoryItems.FindAsync(id);
            if (item == null)
            {
                return NotFound(new { message = $"Item with ID {id} not found." });
            }

            _context.InventoryItems.Remove(item);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}