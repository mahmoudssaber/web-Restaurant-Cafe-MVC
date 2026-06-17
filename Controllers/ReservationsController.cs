using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using food_delivery.Data;
using food_delivery.Models;
using System.Data;


namespace food_delivery.Controllers
{
    [Authorize(Roles = "Manager,Default")]
    public class ReservationsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<CustomUser> _userManager;

        public ReservationsController(ApplicationDbContext context, UserManager<CustomUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Reservations
        public async Task<IActionResult> Index()
        {
            var guachincheContext = _context.Reservations.Include(r => r.CustomUser).Include(r => r.status).Include(r => r.restaurant);
            return View(await guachincheContext.ToListAsync());
        }
        // GET: Reservations/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Reservations == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.CustomUser)
                .Include(r => r.status)
                .Include(r => r.restaurant)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }
        // GET: Reservations/Create
      
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null || _context.restaurants == null)
            {
                return NotFound();
            }

            var restaurant = await _context.restaurants.FindAsync(id);
            var currentUser = _userManager.GetUserAsync(HttpContext.User);
            if (restaurant == null || currentUser.Result == null)
            {
                return NotFound();
            }
            if (currentUser.Result.FirstName == null)
            {
                return BadRequest();
            }
            ReservationDTO reservation = new ReservationDTO(currentUser.Result.Id, restaurant.RestaurantId, currentUser.Result.FirstName, restaurant.Name);

            return View(reservation);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("NumberOfGuests,ReservationDate,UserName,RestaurantName,UserId,RestId")] ReservationDTO reservation)
        {
            if (ModelState.IsValid)
            {
                if (reservation.UserId == null || _context.Reservations == null)
                {
                    return NotFound();
                }

                var user = await _context.CustomUsers.FirstOrDefaultAsync(e => e.Id == reservation.UserId);
                var restaurant = await _context.restaurants.FirstOrDefaultAsync(e => e.RestaurantId == reservation.RestId);
                var status = await _context.ReservationStatuses.FirstOrDefaultAsync(e => e.Name == "Pending");
                if (user == null || restaurant == null || status == null)
                {
                    return NotFound();
                }

                Reservation newReservation = new Reservation
                {
                    NumberOfGuests = reservation.NumberOfGuests,
                    ReservationDate = reservation.ReservationDate,
                    RestaurantId = reservation.RestId,
                    restaurant = restaurant,
                    CustomerUserId = reservation.UserId,
                    CustomUser = user,
                    status = status,
                    ReservationStatusId = status.Id
                };

                _context.Reservations.Add(newReservation);  
                //_context.Add(newReservation);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index", "Home");
            }

            return View(reservation);
        }
        // GET: Reservations/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Reservations == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }
            ReservationDTO res = new ReservationDTO(reservation.Id, reservation.NumberOfGuests, reservation.ReservationDate, reservation.RestaurantId);
            return View(res);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ReservationId,ReservationDate,NumberOfGuests")] ReservationDTO reservation)
        {
            if (id != reservation.ReservationId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                var reserv = await _context.Reservations.FindAsync(reservation.ReservationId);
                var status = await _context.ReservationStatuses.FirstOrDefaultAsync(e => e.Name == "Pending");
                if (reserv == null || status == null)
                {
                    return NotFound();
                }

                try
                {
                    reserv.status = status;
                    reserv.ReservationStatusId = status.Id;
                    reserv.NumberOfGuests = reservation.NumberOfGuests;
                    reserv.ReservationDate = reservation.ReservationDate;
                    _context.Update(reserv);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    throw;
                }
                return RedirectToAction("Reservation", "User");
            }
            return View(reservation);
        }
        public async Task<IActionResult> RateDishRestaurant(int id)
        {
            var restaurant = await _context.restaurants
                .Include(e => e.Dishes)
                .ThenInclude(e => e.type)
                .FirstOrDefaultAsync(e => e.RestaurantId == id);
            if (restaurant == null)
            {
                return NotFound();
            }
            ViewData["Restaurant"] = restaurant.RestaurantId;
            return View(restaurant.Dishes);
        }

        [HttpPost]
        public async Task<IActionResult> RateDish(int dishId, int restaurantId, int rating)
        {
            var dishRest = from a in _context.restaurantDishes
                           where a.RestaurantId == restaurantId
                           where a.DishId == dishId
                           select a;
            var dish = await _context.restaurantDishes.FirstOrDefaultAsync(e => e.Id == dishRest.ToList()[0].Id);

            if (dish == null)
            {
                return NotFound();
            }
            if (rating != 0)
            {
                if (dish.Ratings == null)
                {
                    dish.Ratings = new List<DishRating>();
                }
                dish.Ratings.Add(new DishRating { rating = rating });
                int total = 0;
                foreach (DishRating val in dish.Ratings)
                {
                    total += val.rating;
                }
                dish.Rating = total / dish.Ratings.Count;
                _context.Update(dish);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction("Reservation", "User");
        }



    }
}
