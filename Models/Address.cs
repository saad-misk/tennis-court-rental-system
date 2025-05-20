using System;
using System.Collections.Generic;

namespace TennisCourtRentalSystem.Models;

public partial class Address
{
    public int AddressId { get; set; }

    public string State { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Street { get; set; } = null!;

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
