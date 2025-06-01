namespace TennisCourtRentalSystem.Models;
public class CourtAvailability
{
    public int CourtNumber { get; set; }
    public string Location { get; set; }
    public string Status { get; set; } // "Rented" or "Not Rented"
}
