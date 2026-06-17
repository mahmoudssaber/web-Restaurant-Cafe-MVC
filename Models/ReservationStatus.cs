using System.ComponentModel.DataAnnotations;

namespace food_delivery.Models
{
    public partial class ReservationStatus
    {
        [Key]
        public int Id { get; set; }

        public string? Name { get; set; }

        public virtual ICollection<Reservation> listReservations { get; set; }
    }
}
