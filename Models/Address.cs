namespace TennisCourtRentalSystem.Models;

public partial class Address
{
    public string AddressId { get; set; }

    public string State { get; set; } = null!;

    public string City { get; set; } = null!;

    public string Street { get; set; } = null!;

    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
