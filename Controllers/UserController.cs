
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using food_delivery.Data;
using food_delivery.Models;
using System.Linq;

namespace food_delivery.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _guachincheContext;
        private readonly UserManager<CustomUser> _userManager;
        private readonly SignInManager<CustomUser> _signInManager;
        public UserController(ApplicationDbContext context, UserManager<CustomUser> userManager, SignInManager<CustomUser> signInManager)
        {
            _guachincheContext = context;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _guachincheContext.Users.ToListAsync();
            var currentUser = _userManager.GetUserAsync(HttpContext.User);
            users.RemoveAll(u => u.Id == currentUser.Result.Id);
            List<CustomUserDTO> userList = new List<CustomUserDTO>();
            foreach (CustomUser user in users)
            {
                var role = await _userManager.GetRolesAsync(user);
                userList.Add(new CustomUserDTO
                {
                    Id = user.Id,
                    FirstName = user.FirstName,
                    Email = user.Email,
                    Role = role[0].ToString()
                });
            }

            return View(userList);
        }

        [Authorize(Roles = "Default,Manager")]
        public async Task<IActionResult> Details(string? id)
        {
            if (id == null || _guachincheContext.Users == null)
            {
                return NotFound();
            }

            var user = await _guachincheContext.CustomUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = _userManager.GetUserAsync(HttpContext.User);
            if (!user.Id.Equals(currentUser.Result.Id))
            {
                return BadRequest();
            }

            return View(user);
        }

        [Authorize(Roles = "Default,Manager")]
        public async Task<IActionResult> Edit(string? id)
        {
            if (id == null || _guachincheContext.CustomUsers == null)
            {
                return NotFound();
            }
            var user = await _guachincheContext.CustomUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = _userManager.GetUserAsync(HttpContext.User);
            if (!user.Id.Equals(currentUser.Result.Id))
            {
                return BadRequest();
            }
            CustomUserDTO userDTO = new CustomUserDTO(user.Id, user.FirstName, user.LastName, user.PhoneNumber, user.Email);

            return View(userDTO);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Default,Manager")]
        public async Task<IActionResult> Edit(string? id, [Bind("Id,Name,LastName,Phone")] CustomUserDTO user)
        {
            if (id != user.Id)
            {
                return NotFound();
            }

            var userSelected = await _guachincheContext.CustomUsers.FindAsync(id);
            if (userSelected == null)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    userSelected.FirstName = user.FirstName;
                    userSelected.LastName = user.LastName;
                    userSelected.PhoneNumber = user.PhoneNumber;
                    _guachincheContext.Update(userSelected);
                    await _guachincheContext.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(userSelected.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Details", "User", new { id = user.Id });
            }
            return View(user);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(string? id)
        {
            if (id == null || _guachincheContext.Users == null)
            {
                return NotFound();
            }

            var user = await _guachincheContext.CustomUsers.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            var adminUser = _userManager.GetUserAsync(HttpContext.User);
            if (user.Id.Equals(adminUser.Result.Id))
            {
                return BadRequest();
            }
            _guachincheContext.CustomUsers.Remove(user);
            await _guachincheContext.SaveChangesAsync();

            return RedirectToAction("Index", "User");
        }

        [Authorize(Roles = "Default")]
        public async Task<IActionResult> RestList(string? id)
        {
            if (id == null || _guachincheContext.CustomUsers == null)
            {
                return NotFound();
            }
            var user = await _guachincheContext.CustomUsers
                .Include(r => r.restaurants)
                .ThenInclude(i => i.Id_typeNavigation)
                .Include(r => r.reservations)
                //.ThenInclude(i => i.zone)
                .FirstOrDefaultAsync(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }
            var currentUser = _userManager.GetUserAsync(HttpContext.User);
            if (!user.Id.Equals(currentUser.Result.Id))
            {
                return BadRequest();
            }

            List<Restaurant> restaurants = new List<Restaurant>();
            if (user.restaurants != null)
            {
                restaurants = user.restaurants.ToList();
            }
            return View(restaurants);
        }

        [Authorize(Roles = "Default")]
        public async Task<IActionResult> DishList(string? id)
        {
            if (id == null || _guachincheContext.CustomUsers == null)
            {
                return NotFound();
            }
            var user = await _guachincheContext.CustomUsers
                .Include(r => r.dishes)
                .ThenInclude(e => e.restaurant)
                .Include(r => r.dishes)
                .ThenInclude(e => e.dish)
                .FirstOrDefaultAsync(i => i.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            var currentUser = _userManager.GetUserAsync(HttpContext.User);
            if (!user.Id.Equals(currentUser.Result.Id))
            {
                return BadRequest();
            }

            List<RestaurantDish> dishes = new List<RestaurantDish>();
            if (user.dishes != null)
            {
                dishes = user.dishes.ToList();
            }

            return View(dishes);
        }

        [Authorize(Roles = "Default")]
        public async Task<IActionResult> AddRestaurant(int? id)
        {
            if (id == null || _guachincheContext.restaurants == null)
            {
                return NotFound();
            }

            var restaurant = await _guachincheContext.restaurants.FindAsync(id);
            if (restaurant == null)
            {
                return NotFound();
            }

            var currentUser = _userManager.GetUserAsync(HttpContext.User);
            var user = await _guachincheContext.CustomUsers.Include(r => r.restaurants).FirstOrDefaultAsync(e => e.Id == currentUser.Result.Id);
            if (user == null)
            {
                return NotFound();
            }

            UserRestaurant userRest = new UserRestaurant();
            if (user.restaurants != null)
            {
                if (!user.restaurants.Contains(restaurant))
                {
                    userRest.restaurant = restaurant;
                    userRest.customUser = user;
                    userRest.UserId = user.Id;
                    userRest.RestaurantId = restaurant.RestaurantId;
                }
            }

            _guachincheContext.Add(userRest);
            await _guachincheContext.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "Default")]
        public async Task<IActionResult> DeleteRestaurant(int? id)
        {
            if (id == null || _guachincheContext.restaurants == null)
            {
                return NotFound();
            }

            var restaurant = await _guachincheContext.restaurants.FindAsync(id);
            var currentUser = _userManager.GetUserAsync(HttpContext.User);
            var user = await _guachincheContext.CustomUsers.Include(r => r.restaurants).FirstOrDefaultAsync(e => e.Id == currentUser.Result.Id);
            if ((user == null) || (restaurant == null))
            {
                return NotFound();
            }

            if (user.restaurants != null)
            {
                if (!user.restaurants.Contains(restaurant))
                {
                    return BadRequest();
                }

                user.restaurants.Remove(restaurant);
                await _guachincheContext.SaveChangesAsync();
            }

            return RedirectToAction("RestList", "User", new { id = user.Id });
        }

        [Authorize(Roles = "Default")]
        ///// elfarrrrrrrrr
        public async Task<IActionResult> AddDish(int? id)
        {
            if (id == null || _guachincheContext.UserRestaurantDishes == null)
            {
                return NotFound();
            }

            var dishRest = await _guachincheContext.UserRestaurantDishes.FindAsync(id);
            var currentUser = _userManager.GetUserAsync(HttpContext.User);
            var user = await _guachincheContext.CustomUsers.Include(p => p.dishes).FirstOrDefaultAsync(e => e.Id == currentUser.Result.Id);
            if ((dishRest == null) || (user == null))
            {
                return NotFound();
            }

            if (user.dishes == null)
            {
                user.dishes = new List<RestaurantDish>();
            }

            if (!user.dishes.Contains(dishRest.restaurantDish))
            {   
                user.dishes.Add(dishRest.restaurantDish);
                await _guachincheContext.SaveChangesAsync();
            }
            return NoContent();
        }

        [Authorize(Roles = "Default")]
        public async Task<IActionResult> DeleteDish(int? id)
        {
            if (id == null || _guachincheContext.restaurantDishes == null)
            {
                return NotFound();
            }

            var dishRest = await _guachincheContext.restaurantDishes.FindAsync(id);
            var currentUser = _userManager.GetUserAsync(HttpContext.User);
            var user = await _guachincheContext.CustomUsers.Include(p => p.dishes).FirstOrDefaultAsync(e => e.Id == currentUser.Result.Id);
            if ((dishRest == null) || (user == null))
            {
                return NotFound();
            }

            if (user.dishes != null)
            {
                if (!user.dishes.Contains(dishRest))
                {
                    return BadRequest();
                }
                user.dishes.Remove(dishRest);
                await _guachincheContext.SaveChangesAsync();
            }
            return RedirectToAction("DishList", "User", new { id = user.Id });
        }

        private bool UserExists(string id)
        {
            return (_guachincheContext.CustomUsers?.Any(e => e.Id == id)).GetValueOrDefault();
        }

        [Authorize(Roles = "Default")]
        public async Task<IActionResult> Reservation()
        {
            var currentUser = _userManager.GetUserAsync(HttpContext.User);
            var user = await _guachincheContext.CustomUsers
                .Include(e => e.reservations)
                .ThenInclude(e => e.restaurant)
                .Include(e => e.reservations)
                .ThenInclude(e => e.status)
                .FirstOrDefaultAsync(e => e.Id == currentUser.Result.Id);
            if ((user == null) || (user.reservations == null))
            {
                return NotFound();
            }
            return View(user.reservations);
        }

        [Authorize(Roles = "Default")]
        public async Task<IActionResult> Cancel(int id)
        {
            var reservation = await _guachincheContext.Reservations.Include(e => e.status).FirstOrDefaultAsync(e => e.Id == id);
            var status = await _guachincheContext.ReservationStatuses.FirstOrDefaultAsync(e => e.Name == "Cancelled");
            if ((reservation == null) || (status == null))
            {
                return NotFound();
            }
            reservation.status = status;
            reservation.ReservationStatusId = status.Id;
            _guachincheContext.Update(reservation);
            await _guachincheContext.SaveChangesAsync();
            return RedirectToAction(nameof(Reservation));
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}