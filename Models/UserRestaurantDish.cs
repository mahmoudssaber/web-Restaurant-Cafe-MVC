using System;
using System.Collections.Generic;
namespace food_delivery.Models
{
    public partial class UserRestaurantDish
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int RestaurantDishId { get; set; }

        public virtual RestaurantDish restaurantDish { get; set; }

        public virtual CustomUser customUser { get; set; }
    }
}
