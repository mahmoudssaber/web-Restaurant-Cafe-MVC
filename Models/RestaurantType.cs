using System;
using System.Collections.Generic;

namespace food_delivery.Models
{
    public partial class RestaurantType
    {
        public RestaurantType()
        {
            restaurants = new HashSet<Restaurant>();
        }

        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Restaurant> restaurants { get; set; }
    }
}
