using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace TennisCourtRental.ViewModels
{
    public class RentalViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Renter Name")]
        public string RenterName { get; set; }

        [Display(Name = "Organization")]
        public string Organization { get; set; }

        [Required(ErrorMessage = "Street address is required")]
        [Display(Name = "Street Address")]
        public string StreetAddress { get; set; }

        [Required(ErrorMessage = "City and state are required")]
        [Display(Name = "City, State")]
        public string CityState { get; set; }

        [Required(ErrorMessage = "Zip code is required")]
        [Display(Name = "Zip Code")]
        [RegularExpression(@"^\d{5}(-\d{4})?$", ErrorMessage = "Please enter a valid zip code")]
        public string ZipCode { get; set; }

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Event type is required")]
        [Display(Name = "Event Type")]
        public string EventType { get; set; }

        [Required(ErrorMessage = "Rental date is required")]
        [Display(Name = "Rental Date")]
        [DataType(DataType.Date)]
        public DateTime RentalDate { get; set; }

        [Required(ErrorMessage = "Start time is required")]
        [Display(Name = "Start Time")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        [Display(Name = "End Time")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; }

        [Required(ErrorMessage = "Court selection is required")]
        [Display(Name = "Court Number")]
        public int CourtNumber { get; set; }

        [Required(ErrorMessage = "Expected attendance is required")]
        [Display(Name = "Expected Attendance")]
        [Range(1, 200, ErrorMessage = "Attendance must be between 1 and 200 people")]
        public int ExpectedAttendance { get; set; }

        [Display(Name = "Renter Signature")]
        public IFormFile RenterSignatureFile { get; set; }

        [Display(Name = "Second Signer Signature")]
        public IFormFile SecondSignerSignatureFile { get; set; }
    }

    public class RentalSummaryViewModel
    {
        public int RentalId { get; set; }
        public DateTime Date { get; set; }
        public int CourtNumber { get; set; }
        public string TimeRange { get; set; }
        public string Status { get; set; }
    }

    public class RentalDetailViewModel : RentalViewModel
    {
        public int RentalId { get; set; }
        public string Status { get; set; }
        public decimal RentalFee { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime DateBooked { get; set; }
        public DateTime? PaymentDate { get; set; }
        public string RenterSignaturePath { get; set; }
        public string SecondSignerSignaturePath { get; set; }
        public string AdminNotes { get; set; }
        public bool IsAdmin { get; set; }
    }

    public class CourtSelectorViewModel
    {
        public IEnumerable<SelectListItem> Courts { get; set; }
        public int SelectedCourt { get; set; }
    }
}