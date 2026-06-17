using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using food_delivery.Data;
using food_delivery.Models;


namespace food_delivery.Controllers
{
    [Authorize(Roles = "Manager")]
    public class ManagerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public ManagerController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ManagerController
        public async Task<IActionResult> List(string? id)
        {
            if (id == null || _context.CustomUsers == null)
            {
                return NotFound();
            }

            var manager = await _context.CustomUsers
                .Include(e => e.restaurants)
                .ThenInclude(e => e.Zone)
                .Include(e => e.restaurants)
                .ThenInclude(e => e.Id_typeNavigation)
                .FirstOrDefaultAsync(e => e.Id == id);

            if (manager == null)
            {
                return NotFound();
            }
            ViewData["Dishes"] = new SelectList(_context.dishes, "DishId", "Name");
            ManagerDTO data = new ManagerDTO
            {
                restaurants = manager.restaurants.ToList()
            };
            return View(data);
        }
        [HttpPost]
        public async Task<IActionResult> AddDish([Bind("RestaurantId,DishId,ManagerId")] ManagerDTO dish)
        {
            if (ModelState.IsValid)
            {
                var restaurant = await _context.restaurants
                    .Include(p => p.Dishes)
                    .FirstOrDefaultAsync(e => e.RestaurantId == dish.RestaurantId);
                var selectedDish = await _context.dishes.FindAsync(dish.DishId);
                if (restaurant == null || selectedDish == null)
                {
                    return NotFound();
                }

                if (restaurant.Dishes == null)
                {
                    restaurant.Dishes = new List<Dish>();
                }

                if (!restaurant.Dishes.Contains(selectedDish))
                {
                    RestaurantDish restaurantDish = new RestaurantDish
                    {
                        dish = selectedDish,
                        restaurant = restaurant,
                        DishId = selectedDish.DishId,
                        RestaurantId = restaurant.RestaurantId,
                        Rating = 1,
                        IsActive = true
                    };
                    _context.Add(restaurantDish);
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction("List", "Manager", new { id = dish.ManagerId });
        }
        public async Task<IActionResult> DeleteDish(int? id)
        {
            if (id == null || _context.restaurantDishes == null)
            {
                return NotFound();
            }

            var dish = await _context.restaurantDishes.FindAsync(id);
            if (dish == null)
            {
                return NotFound();
            }

            var restaurant = await _context.restaurants
                .Include(e => e.Dishes)
                .FirstOrDefaultAsync(e => e.RestaurantId == dish.RestaurantId);
            if (restaurant == null)
            {
                return NotFound();
            }

            restaurant.Dishes.Remove(dish.dish);
            await _context.SaveChangesAsync();
            return RedirectToAction("Details", "Restaurants", new { id = dish.RestaurantId });
        }
        public async Task<IActionResult> Reservations()
        {
            var currentUser = _userManager.GetUserAsync(HttpContext.User);
            var manager = await _context.CustomUsers.Include(e => e.restaurants)
                .ThenInclude(i => i.Reservations)
                .ThenInclude(f => f.status)
                .Include(e => e.restaurants)
                .ThenInclude(i => i.Reservations)
                .ThenInclude(f => f.CustomUser)
                .FirstOrDefaultAsync(e => e.Id == currentUser.Result.Id);
            if (manager == null)
            {
                return BadRequest();
            }

            List<Reservation> reservations = new List<Reservation>();
            if (manager.restaurants != null)
            {
                foreach (Restaurant rest in manager.restaurants)
                {
                    if (rest.Reservations != null)
                    {
                        foreach (Reservation reservation in rest.Reservations)
                        {
                            reservations.Add(reservation);
                        }
                    }
                }
            }
            return View(reservations);
        }
        public async Task<IActionResult> Confirm(int id)
        {
            var reservation = await _context.Reservations.Include(e => e.status).FirstOrDefaultAsync(e => e.Id == id);

            var status = await _context.ReservationStatuses.FirstOrDefaultAsync(e => e.Name == "Active");
            if (reservation == null || status == null)
            {
                return NotFound();
            }
            reservation.status = status;
            reservation.ReservationStatusId = status.Id;
            _context.Update(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Reservations));
        }
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _context.Reservations.Include(e => e.status).FirstOrDefaultAsync(e => e.Id == id);
            var status = await _context.ReservationStatuses.FirstOrDefaultAsync(e => e.Name == "Cancelled");
            if (reservation == null || status == null)
            {
                return NotFound();
            }
            reservation.status = status;
            reservation.ReservationStatusId = status.Id;
            _context.Update(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Reservations));
        }

        public async Task<IActionResult> Complete(int id)
        {
            var reservation = await _context.Reservations.Include(e => e.status).FirstOrDefaultAsync(e => e.Id == id);
            var status = await _context.ReservationStatuses.FirstOrDefaultAsync(e => e.Name == "Completed");
            if (reservation == null || status == null)
            {
                return NotFound();
            }
            reservation.status = status;
            reservation.ReservationStatusId = status.Id;
            _context.Update(reservation);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Reservations));
        }
    }
}
