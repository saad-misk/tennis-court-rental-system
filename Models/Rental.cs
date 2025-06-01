namespace TennisCourtRentalSystem.Models;

public partial class Rental
{
    public string RentalId { get; set; }

    public string CustomerId { get; set; }
    public int CourtNumber { get; set; }

    public string? EventId { get; set; }

    public DateTime  RentalDate { get; set; }

    public DateTime  StartTime { get; set; }

    public DateTime  EndTime { get; set; }

    public int? ExpectedAttendance { get; set; }

    public decimal RentalFee { get; set; }

    public string PaymentStatus { get; set; } = null!;

    public DateTime DateBooked { get; set; }

    public DateTime? PaymentDate { get; set; }

    public string? RenterSignature { get; set; }

    public string? SecondSignerSignature { get; set; }

    public virtual Court Court { get; set; } = null!;

    public virtual Customer Customer { get; set; } = null!;

    public virtual Event? Event { get; set; }
}