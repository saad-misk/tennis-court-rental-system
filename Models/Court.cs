namespace TennisCourtRentalSystem.Models;

public partial class Court
{
    public int CourtNumber { get; set; }

    public string Location { get; set; } = null!;

    public string CourtStatus { get; set; } = null!;

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}