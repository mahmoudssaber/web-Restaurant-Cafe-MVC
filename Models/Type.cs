using System;
using System.Collections.Generic;
namespace food_delivery.Models
{
    public partial class Type
    {
            public int Id { get; set; }
            public string Name { get; set; }

            public virtual ICollection<Dish> dishes { get; set; }

            public Type()
            {
                dishes = new HashSet<Dish>();
            }
        
    }
}
