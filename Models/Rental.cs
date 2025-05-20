namespace TennisCourtRentalSystem.Models;

public partial class Rental
{
    public int RentalId { get; set; }

    public int CustomerId { get; set; }

    public int CourtNumber { get; set; }

    public int? EventId { get; set; }

    public DateOnly RentalDate { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int? ExpectedAttendance { get; set; }

    public decimal RentalFee { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public DateTime DateBooked { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? RenterSignature { get; set; }

    public string? SecondSignerSignature { get; set; }

    public virtual Court CourtNumberNavigation { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Event? Event { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}