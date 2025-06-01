using System.ComponentModel.DataAnnotations;

namespace TennisCourtRentalSystem.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string TelNo { get; set; }

        [Required]
        public int CustomerType { get; set; } // Resident, NonResident

        // Address
        [Required]
        public string State { get; set; }

        [Required]
        public string City { get; set; }

        [Required]
        public string Street { get; set; }

        // Optional
        public string? Gender { get; set; }
        public string? OrganizationName { get; set; }
    }
}