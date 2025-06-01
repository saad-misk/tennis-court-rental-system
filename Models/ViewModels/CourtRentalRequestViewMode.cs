using System.ComponentModel.DataAnnotations;

namespace TennisCourtRentalSystem.Models.ViewModels;

public class CourtRentalRequestViewModel
{
    [Required]
    [DateWithin14Days]
    public DateTime RentalDate { get; set; }

    [Required]
    [DataType(DataType.Time)]
    public DateTime StartTime { get; set; }

    [Required]
    public string Duration { get; set; }

    // This will be populated via AJAX, but used for display
    public List<CourtSelectionViewModel>? AvailableCourts { get; set; }

    // This will hold the court chosen by the user
    [Required(ErrorMessage = "Please select an available court.")]
    [Display(Name = "Selected Court")]
    public int? SelectedCourtNumber { get; set; }

    public class CourtSelectionViewModel
    {
        public int CourtNumber { get; set; }
        public string Description { get; set; }
        public bool IsSelected { get; set; }
    }
}

public class DateWithin14DaysAttribute : ValidationAttribute
{
    protected override ValidationResult IsValid(object value, ValidationContext context)
    {
        if (value is DateTime date)
        {
            if (date < DateTime.Today)
                return new ValidationResult("Date cannot be in the past");
            
            if (date > DateTime.Today.AddDays(14))
                return new ValidationResult("Maximum booking window is 14 days");
        }
        return ValidationResult.Success;
    }
}