using System;
using System.Collections.Generic;

namespace TennisCourtRentalSystem.Models;

public partial class CourtManager : User
{
    public int EmployeeId { get; set; }

    public string? OfficePhone { get; set; }

    public virtual ICollection<Event> Events { get; set; } = new List<Event>();
}
