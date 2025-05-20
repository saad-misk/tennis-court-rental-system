using System;
using System.Collections.Generic;

namespace TennisCourtRentalSystem.Models;

public partial class Event
{
    public int EventId { get; set; }

    public int RentalId { get; set; }

    public int EmployeeId { get; set; }

    public string EventType { get; set; } = null!;

    public string? EventDescription { get; set; }

    public bool IsSpecialEvent { get; set; }

    public bool PoliciesSigned { get; set; }

    public bool PoliceNotified { get; set; }

    public string ApprovalStatus { get; set; } = null!;

    public virtual CourtManager Employee { get; set; } = null!;

    public virtual Rental Rental { get; set; } = null!;

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
