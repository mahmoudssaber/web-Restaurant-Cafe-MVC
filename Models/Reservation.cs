using food_delivery.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace food_delivery.Models
{
    [Table("Reservations")]
    public partial class Reservation
    {

            public int Id { get; set; }
            public DateTime ReservationDate { get; set; }

            public int NumberOfGuests { get; set; }
            public string? CustomerUserId { get; set; }
            public int RestaurantId { get; set; }
            public int ReservationStatusId { get; set; }

            public virtual CustomUser CustomUser { get; set; }
            public virtual Restaurant restaurant { get; set; }
            public virtual ReservationStatus status { get; set; }
        
    }
}
