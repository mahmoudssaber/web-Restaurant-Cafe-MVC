using food_delivery.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using food_delivery.Data;


namespace food_delivery.Controllers
{
    [Authorize(Roles = "Default,Admin")]
    public class ZonesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ZonesController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            return _context.zones != null ?
                        View(await _context.zones.ToListAsync()) :
                        Problem("Entity set 'GuachincheContext.Zones' is null.");
        }

        [Authorize(Roles = "Default")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.zones == null)
            {
                return NotFound();
            }

            var zone = await _context.zones
                .Include(e => e.restaurants)
                .ThenInclude(i => i.Id_typeNavigation)
                .FirstOrDefaultAsync(m => m.ZoneId == id);
            if (zone == null)
            {
                return NotFound();
            }

            return View(zone);
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("ZoneId,Name,Description")] Zone zone)
        {
            if (ModelState.IsValid)
            {
                _context.Add(zone);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(zone);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.zones == null)
            {
                return NotFound();
            }

            var zone = await _context.zones.FindAsync(id);
            if (zone == null)
            {
                return NotFound();
            }
            return View(zone);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("ZoneId,Name,Description")] Zone zone)
        {
            if (id != zone.ZoneId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(zone);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ZoneExists(zone.ZoneId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(zone);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.zones == null)
            {
                return NotFound();
            }

            var zone = await _context.zones
                .FirstOrDefaultAsync(m => m.ZoneId == id);
            if (zone == null)
            {
                return NotFound();
            }

            return View(zone);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.zones == null)
            {
                return Problem("Entity set 'GuachincheContext.Zones' is null.");
            }
            var zone = await _context.zones.FindAsync(id);
            if (zone != null)
            {
                _context.zones.Remove(zone);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ZoneExists(int id)
        {
            return (_context.zones?.Any(e => e.ZoneId == id)).GetValueOrDefault();
        }
    }
}