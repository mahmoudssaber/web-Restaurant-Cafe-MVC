
using food_delivery.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace food_delivery.Models
{
    public partial class Dish
    {
        public int DishId { get; set; }

        [Required(ErrorMessage = "The 'Name' field is required")]
        public string Name { get; set; }

        [DisplayName("Description")]
        [Required(ErrorMessage = "The 'Description' field is required")]
        public string Description { get; set; }
        public int TypeId { get; set; }

        [AllowNull]
        public string ImageURL { get; set; }
        public virtual Type type { get; set; }
        public virtual ICollection<Restaurant> restaurants { get; set; }
    }
}
