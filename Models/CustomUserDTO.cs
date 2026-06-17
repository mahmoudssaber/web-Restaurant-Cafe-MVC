using System.ComponentModel.DataAnnotations;
using System.ComponentModel;

namespace food_delivery.Models
{
    public class CustomUserDTO
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "The 'First Name' field is required")]
        public String? FirstName { get; set; }

        [Required(ErrorMessage = "The 'Last Name' field is required")]
        public String? LastName { get; set; }

        [DisplayName("Phone Number")]
        [Required(ErrorMessage = "The 'Phone Number' field is required")]
        public String? PhoneNumber { get; set; }

        [DisplayName("Email Address")]
        [Required(ErrorMessage = "The 'Email Address' field is required")]
        public String? Email { get; set; }

        public String? Role { get; set; }

        public CustomUserDTO() { }

        public CustomUserDTO(string? id, string? firstName, string? lastName, string? phoneNumber, string? email, string role)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Email = email;
            Role = role;
        }

        public CustomUserDTO(string? id, string? firstName, string? lastName, string? phoneNumber, string? email)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            PhoneNumber = phoneNumber;
            Email = email;
        }
    }
}
//stands for "Custom User Data Transfer Object." It is a class used to transfer user data between different layers of an application, typically between the client and server. The class includes properties such as Id, FirstName, LastName, PhoneNumber, Email, and Role, and it includes validation attributes to ensure required fields are provided.
