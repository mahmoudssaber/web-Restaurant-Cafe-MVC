using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace food_delivery.Models
{
    public partial class Restaurant
    {
        [Key]
        public int RestaurantId { get; set; }

        [Required(ErrorMessage = "The 'Name' field is required")]
        public string Name { get; set; }
        public string RestUrl { get; set; }

        [Required(ErrorMessage = "The 'Phone Number' field is required")]
        [StringLength(11, ErrorMessage = "The 'Phone Number' must be 11 digits long")]
        [DisplayName("Phone Number")]
        public string PhoneNumber { get; set; }

        [DisplayName("Rating")]
        [Required(ErrorMessage = "The 'Rating' field is required")]
        [Range(0, 5, ErrorMessage = "The 'Rating' must be between 0 and 5")]
        public int? Rating { get; set; }
        public int TypeId { get; set; }
        public int ZoneId { get; set; }

        [DisplayName("Description")]
        [Required(ErrorMessage = "The 'Description' field is required")]
        public string Description { get; set; }

        [DisplayName("Specialty")]
        public virtual RestaurantType Id_typeNavigation { get; set; }
        public virtual Zone Zone { get; set; }
        public virtual ICollection<Dish> Dishes { get; set; }
        public virtual ICollection<CustomUser> Users { get; set; }

        public virtual ICollection<Reservation> Reservations { get; set; }

        public ICollection<RestaurantRating> Ratings { get; set; }
    }
}

