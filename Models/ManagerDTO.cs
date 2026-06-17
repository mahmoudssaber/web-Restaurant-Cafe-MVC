using food_delivery.Models;

namespace food_delivery.Models
{
    public class ManagerDTO
    {
        public int? RestaurantId { get; set; }
        public int? DishId { get; set; }
        public string? ManagerId { get; set; }

        public List<Restaurant>? restaurants { get; set; }

        public ManagerDTO()
        {
        }

        public ManagerDTO(int restaurantId, int dishId, string managerId, List<Restaurant> rest)
        {
            this.RestaurantId = restaurantId;
            this.DishId = dishId;
            this.ManagerId = managerId;
            this.restaurants = rest.ToList();
        }
    }
}
