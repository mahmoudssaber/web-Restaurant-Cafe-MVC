using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace food_delivery.Models
{
   
        [Table("CustomUser")]
        public partial class CustomUser : IdentityUser
        {
            public String? FirstName { get; set; }

            public String? LastName { get; set; }

            public String? PhoneNumber { get; set; }

            public bool? IsActive { get; set; }

            public virtual ICollection<Restaurant> restaurants { get; set; }

            public virtual ICollection<RestaurantDish> dishes { get; set; }

            public virtual ICollection<Reservation> reservations { get; set; }
        }



   
}
