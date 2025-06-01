namespace TennisCourtRentalSystem.Models.ViewModels;

public class CourtRentalViewModel
{
    public string RentalId { get; set; }
    public DateTime RentalDate { get; set; }
    public decimal Fee { get; set; }
    public int CourtNumber { get; set; }
}