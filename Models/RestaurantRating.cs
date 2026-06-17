using food_delivery.Models;

namespace food_delivery.Models
{
    public class RestaurantRating
    {
        public int Id { get; set; }

        public int Rating { get; set; }

        public int RestaurantId { get; set; }

        public Restaurant restaurant { get; set; }
    }
}