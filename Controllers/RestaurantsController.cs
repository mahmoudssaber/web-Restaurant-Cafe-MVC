using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Identity;
using food_delivery.Data;
using food_delivery.Models;
using System.Data;
namespace food_delivery.Controllers
{
    [Authorize]
    public class RestaurantsController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webEnvironment;
        private readonly UserManager<CustomUser> _userManager;

        public RestaurantsController(ApplicationDbContext context, IWebHostEnvironment environment, UserManager<CustomUser> userManager)
        {
            _context = context;
            _webEnvironment = environment;
            _userManager = userManager;
        }

        [Authorize(Roles = "Admin,Default")]
        public async Task<IActionResult> Index(string sortOrder, string searchString, string restZone, List<SelectListItem> zoneRest)
        {
            ViewData["NameSortParm"] = String.IsNullOrEmpty(sortOrder) ? "Name" : "";
            ViewData["ValSortParm"] = sortOrder == "Val" ? "Val_asc" : "Val";
            ViewData["CurrentFilter"] = searchString;

            var restaurants = (IQueryable<Restaurant>)_context.restaurants.Include(r => r.Id_typeNavigation).Include(r => r.Zone);

            if (restaurants.Count() != 0)
            {
                if (restZone == null || restZone == "All")
                {
                    zoneRest.Add(new SelectListItem { Text = "All", Value = "All", Selected = true });
                }
                else
                {
                    zoneRest.Add(new SelectListItem { Text = "All", Value = "All" });
                }

                foreach (string zone in restaurants.Select(o => o.Zone.Name).Distinct().ToList())
                {
                    if (restZone == zone)
                    {
                        zoneRest.Add(new SelectListItem { Text = zone, Value = zone, Selected = true });
                    }

                    else
                    {
                        zoneRest.Add(new SelectListItem { Text = zone, Value = zone });
                    }
                }
            }
            ViewBag.RestZone = zoneRest;

            if (!String.IsNullOrEmpty(searchString))
            {
                restaurants = restaurants.Where(s => s.Name.Contains(searchString));
            }

            if (!String.IsNullOrEmpty(restZone) && !restZone.Equals("All"))
            {
                restaurants = restaurants.Where(s => s.Zone.Name.Equals(restZone));
            }

            switch (sortOrder)
            {
                case "Name":
                    restaurants = restaurants.OrderBy(s => s.Name);
                    break;
                case "Val_asc":
                    restaurants = restaurants.OrderBy(s => s.Rating);
                    break;
                case "Val":
                    restaurants = restaurants.OrderByDescending(s => s.Rating);
                    break;
                default:
                    restaurants = restaurants.OrderByDescending(s => s.Name);
                    break;
            }

            return View(await restaurants.AsNoTracking().ToListAsync());
        }
        [Authorize(Roles = "Manager,Default")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.restaurants == null)
            {
                return NotFound();
            }

            var restaurant = await _context.restaurants.Include(p => p.Dishes).ThenInclude(i => i.type)
                .Include(e => e.Zone).Include(e => e.Id_typeNavigation).FirstOrDefaultAsync(i => i.RestaurantId == id);

            if (restaurant == null)
            {
                return NotFound();
            }

            var dishRest = from t1 in _context.restaurantDishes
                           join t2 in _context.dishes on t1.DishId equals t2.DishId
                           where t1.RestaurantId == id
                           select new DishDTO
                           {
                               Id = t1.Id,
                               Name = t2.Name,
                               Type = t2.type.Name,
                               Rating = t1.Rating
                           };

            List<DishDTO> dishes = new List<DishDTO>();
            if (dishRest != null)
            {
                foreach (var a in dishRest)
                {
                    dishes.Add(a);
                }
            }
            ViewBag.DishRest = dishes;
            return View(restaurant);
        }
        [Authorize(Roles = "Admin")]
        public IActionResult Create(string? id)
        {
            ViewData["TypeId"] = new SelectList(_context.restaurantTypes, "Id", "Name");
            ViewData["ZoneId"] = new SelectList(_context.zones, "ZoneId", "Name");
            ViewData["ManagerId"] = id;
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(string? id, [Bind("RestaurantId,Name,PhoneNumber,Rating,Description,TypeId,ZoneId")] Restaurant restaurant, [FromForm] IFormFile Image)
        {
            if (id == null || _context.CustomUsers == null)
            {
                return NotFound();
            }

            // Find the manager (custom user)
            var manager = await _context.CustomUsers
                .Include(e => e.restaurants)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (manager == null)
            {
                return NotFound();
            }

            // Define file paths for storing the image
            string filePath = Path.Combine(_webEnvironment.WebRootPath, "img");
            string restPath = Path.Combine(filePath, "restaurants");

            if (!Directory.Exists(restPath))
            {
                Directory.CreateDirectory(restPath);  // Ensure the directory exists
            }

            if (ModelState.IsValid)
            {
                if (Image != null && Image.Length > 0)
                {
                    // Generate a unique file name to avoid conflicts
                    string uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(Image.FileName);
                    string fullFilePath = Path.Combine(restPath, uniqueFileName);

                    // Save the file to the specified location
                    using (Stream fileStream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await Image.CopyToAsync(fileStream);
                    }

                    // Save the file name (or relative path) in the Restaurant entity
                    restaurant.RestUrl = Path.Combine("/img/restaurants", uniqueFileName);

                    // Add the restaurant to the context
                    _context.Add(restaurant);

                    // Create the UserRestaurant association
                    UserRestaurant userRest = new UserRestaurant
                    {
                        UserId = id,
                        RestaurantId = restaurant.RestaurantId,
                        restaurant = restaurant,
                        customUser = manager
                    };
                    _context.Add(userRest);

                    // Save changes to the database
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    ViewData["error"] = "Restaurant image not uploaded";
                }
            }

            // Populate ViewData for dropdowns and return view with errors
            ViewData["TypeId"] = new SelectList(_context.restaurantTypes, "Id", "Name", restaurant.TypeId);
            ViewData["ZoneId"] = new SelectList(_context.zones, "ZoneId", "Name", restaurant.ZoneId);
            return View(restaurant);
        }


        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.restaurants == null)
            {
                return NotFound();
            }

            var restaurant = await _context.restaurants.FindAsync(id);
            if (restaurant == null)
            {
                return NotFound();
            }
            ViewData["TypeId"] = new SelectList(_context.restaurantTypes, "Id", "Name", restaurant.TypeId);
            ViewData["ZoneId"] = new SelectList(_context.zones, "ZoneId", "Name", restaurant.ZoneId);
            return View(restaurant);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> Edit(int id, [Bind("RestaurantId,Name,Phone,Rating,Description,TypeId,ZoneId")] Restaurant restaurant, [FromForm] IFormFile Image)
        {
            if (id != restaurant.RestaurantId)
            {
                return NotFound();
            }

            string filePath = Path.Combine(_webEnvironment.WebRootPath, "img");
            string restPath = Path.Combine(filePath, "restaurants");

            ModelState.Remove("Image");

            if (ModelState.IsValid)
            {
                var existingRestaurant = await _context.restaurants.FirstOrDefaultAsync(e => e.RestaurantId == restaurant.RestaurantId);
                if (existingRestaurant == null)
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
                        existingRestaurant.RestUrl = Image.FileName;
                    }
                    existingRestaurant.Description = restaurant.Description;
                    existingRestaurant.Name = restaurant.Name;
                    existingRestaurant.PhoneNumber = restaurant.PhoneNumber;
                    existingRestaurant.Id_typeNavigation = restaurant.Id_typeNavigation;
                    existingRestaurant.TypeId = restaurant.TypeId;

                    _context.Update(existingRestaurant);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                var currentUser = _userManager.GetUserAsync(HttpContext.User);
                return RedirectToAction("List", "Manager", new { id = currentUser.Result.Id });
            }
            ViewData["TypeId"] = new SelectList(_context.restaurantTypes, "Id", "Name", restaurant.TypeId);
            ViewData["ZoneId"] = new SelectList(_context.zones, "ZoneId", "Name", restaurant.ZoneId);
            return View(restaurant);
        }
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.restaurants == null)
            {
                return NotFound();
            }

            var restaurant = await _context.restaurants
                .Include(r => r.Id_typeNavigation)
                .Include(r => r.Zone)
                .FirstOrDefaultAsync(m => m.RestaurantId == id);
            if (restaurant == null)
            {
                return NotFound();
            }

            return View(restaurant);

        }
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.restaurants == null)
            {
                return Problem("Entity set 'GuachincheContext.Restaurants' is null.");
            }
            var restaurant = await _context.restaurants.FindAsync(id);
            if (restaurant != null)
            {
                _context.restaurants.Remove(restaurant);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
        private bool RestaurantExists(int id)
        {
            return (_context.restaurants?.Any(e => e.RestaurantId == id)).GetValueOrDefault();
        }

        [HttpPost]
        public async Task<IActionResult> Rate(int restaurantId, int rating)
        {
            var restaurant = await _context.restaurants.Include(e => e.Ratings).FirstOrDefaultAsync(i => i.RestaurantId == restaurantId);
            if (restaurant == null)
            {
                return NotFound();
            }
            if (rating != 0)
            {
                if (restaurant.Ratings == null)
                {
                    restaurant.Ratings = new List<RestaurantRating>();
                }
                restaurant.Ratings.Add(new RestaurantRating { Rating = rating });
                int total = 0;
                foreach (RestaurantRating val in restaurant.Ratings)
                {
                    total += val.Rating;
                }
                restaurant.Rating = total / restaurant.Ratings.Count;
                _context.Update(restaurant);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Reservation", "User");
        }
    }
}
