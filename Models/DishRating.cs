
using System.ComponentModel.DataAnnotations;
namespace food_delivery.Models
{
    public class DishRating
    {
        [Key]
        public int Id { get; set; }

        public int rating { get; set; }

        public int restaurantDishId { get; set; }

        public RestaurantDish restaurantDish { get; set; }
    }
}