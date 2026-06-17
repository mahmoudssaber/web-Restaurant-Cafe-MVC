using System.ComponentModel.DataAnnotations;

namespace food_delivery.Models
{
    public class ReservationDTO
    {
        public int ReservationId { get; set; }

        [Required(ErrorMessage = "The 'Number of Guests' field is required")]
        [Range(1, int.MaxValue, ErrorMessage = "The 'Number of Guests' must be greater than zero.")]
        public int NumberOfGuests { get; set; }

        [Required(ErrorMessage = "The 'Reservation Date' field is required")]
        public DateTime ReservationDate { get; set; }
        public string? UserId { get; set; }
        public int RestId { get; set; }
        public string? UserName { get; set; }

        public string? RestaurantName { get; set; }

        public ReservationDTO() { }

        public ReservationDTO(int numberOfGuests, DateTime reservationDate, string userName, string restaurantName, string userId, int restId)
        {
            this.NumberOfGuests = numberOfGuests;
            this.ReservationDate = reservationDate;
            this.UserName = userName;
            this.RestaurantName = restaurantName;
            this.UserId = userId;
            this.RestId = restId;
        }

        public ReservationDTO(string userId, int restId, string userName, string restaurantName)
        {
            this.UserName = userName;
            this.RestaurantName = restaurantName;
            this.UserId = userId;
            this.RestId = restId;
        }

        public ReservationDTO(int reservationId, int numberOfGuests, DateTime reservationDate, int rest)
        {
            this.ReservationId = reservationId;
            this.NumberOfGuests = numberOfGuests;
            this.ReservationDate = reservationDate;
            this.RestId = rest;
        }
    }
}
