using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using food_delivery.Data;
using food_delivery.Models;
using System;
namespace food_delivery.Controllers
{
    [Authorize(Roles = "Admin,Default")]
    public class DishesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webEnvironment;

        public DishesController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _webEnvironment = environment;
            _context = context;
        }
        // GET: Dishes
        public async Task<IActionResult> Index(string sortOrder, string searchString, string dishType, List<SelectListItem> typeDish)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "Name" : "";
            ViewData["TypeSortParm"] = sortOrder == "Type" ? "Type_desc" : "Type";
            ViewData["CurrentFilter"] = searchString;

            var dishes = (IQueryable<Dish>)_context.dishes.Include(p => p.type);

            if (typeDish.Count() == 0)
            {
                if (dishType == null || dishType == "All")
                {
                    typeDish.Add(new SelectListItem { Text = "All", Value = "All", Selected = true });
                }
                else
                {
                    typeDish.Add(new SelectListItem { Text = "All", Value = "All" });
                }

                foreach (string type in dishes.Select(o => o.type.Name).Distinct().ToList())
                {
                    if (dishType == type)
                    {
                        typeDish.Add(new SelectListItem { Text = type, Value = type, Selected = true });
                    }
                    else
                    {
                        typeDish.Add(new SelectListItem { Text = type, Value = type });
                    }
                }
            }

            ViewBag.DishType = typeDish;

            if (!String.IsNullOrEmpty(searchString))
            {
                dishes = dishes.Where(s => s.Name.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(dishType) && !dishType.Equals("All"))
            {
                dishes = dishes.Where(s => s.type.Name.Equals(dishType));
            }

            switch (sortOrder)
            {
                case "Name":
                    dishes = dishes.OrderBy(s => s.Name);
                    break;
                case "Type":
                    dishes = dishes.OrderBy(s => s.type.Name);
                    break;
                case "Type_desc":
                    dishes = dishes.OrderByDescending(s => s.type.Name);
                    break;
                default:
                    dishes = dishes.OrderByDescending(s => s.Name);
                    break;
            }

            return View(await dishes.AsNoTracking().ToListAsync());
        }
        [Authorize(Roles = "Default")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.dishes == null)
            {
                return NotFound();
            }

            var dish = await _context.dishes
                .Include(p => p.type)
                .Include(r => r.restaurants)
                .ThenInclude(e => e.Zone)
                .Include(r => r.restaurants)
                .ThenInclude(e => e.Id_typeNavigation)
                .FirstOrDefaultAsync(m => m.DishId == id);
            if (dish == null)
            {
                return NotFound();
            }

            return View(dish);
        }
        // GET: Dishes/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["TypeId"] = new SelectList(_context.types, "Id", "Name");
            return View();
        }
        // POST: Dishes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("DishId,Name,Description,TypeId")] Dish dish, [FromForm] IFormFile Image)
        {
            string filePath = Path.Combine(_webEnvironment.WebRootPath, "img");
            string restPath = Path.Combine(filePath, "dishes");

            if (ModelState.IsValid && Image != null)
            {
                if (Directory.Exists(restPath))
                {
                    using (Stream fileStream = new FileStream(Path.Combine(restPath, Image.FileName), FileMode.Create))
                    {
                        await Image.CopyToAsync(fileStream);
                    }
                }
                dish.ImageURL = Image.FileName;
                _context.Add(dish);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ViewData["error"] = "Image upload failed";
            }
            ViewData["TypeId"] = new SelectList(_context.types, "Id", "Name", dish.TypeId);
            return View(dish);
        }
        // GET: Dishes/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.dishes == null)
            {
                return NotFound();
            }

            var dish = await _context.dishes.FindAsync(id);
            if (dish == null)
            {
                return NotFound();
            }
            ViewData["TypeId"] = new SelectList(_context.types, "Id", "Name", dish.TypeId);
            return View(dish);
        }
        // POST: Dishes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("DishId,Name,Description,TypeId")] Dish dish, [FromForm] IFormFile Image)
        {
            if (id != dish.DishId)
            {
                return NotFound();
            }

            string filePath = Path.Combine(_webEnvironment.WebRootPath, "img");
            string restPath = Path.Combine(filePath, "restaurants");

            ModelState.Remove("Image");

            if (ModelState.IsValid)
            {
                var plate = await _context.dishes.FirstOrDefaultAsync(e => e.DishId == dish.DishId);
                if (plate == null)
                {
                    return NotFound();
                }
                try
                {
                    if (Directory.Exists(restPath) && Image != null)
                    {
                        using (Stream fileStream = new FileStream(Path.Combine(restPath, Image.FileName), FileMode.Create))
                        {
                            await Image.CopyToAsync(fileStream);
                        }
                        plate.ImageURL = Image.FileName;
                    }
                    plate.Description = dish.Description;
                    plate.Name = dish.Name;
                    plate.TypeId = dish.TypeId;
                    plate.type = dish.type;
                    plate.restaurants = dish.restaurants;

                    _context.Update(plate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DishExists(dish.DishId))
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
            ViewData["TypeId"] = new SelectList(_context.types, "Id", "Name", dish.TypeId);
            return View(dish);
        }
        // GET: Dishes/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.dishes == null)
            {
                return NotFound();
            }

            var dish = await _context.dishes
                .Include(p => p.type)
                .FirstOrDefaultAsync(m => m.DishId == id);
            if (dish == null)
            {
                return NotFound();
            }

            return View(dish);
        }

        // POST: Dishes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.dishes == null)
            {
                return Problem("Entity set 'GuachincheContext.Dishes' is null.");
            }
            var dish = await _context.dishes.FindAsync(id);
            if (dish != null)
            {
                _context.dishes.Remove(dish);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool DishExists(int id)
        {
            return (_context.dishes?.Any(e => e.DishId == id)).GetValueOrDefault();
          
        }

    }
}
