namespace TennisCourtRentalSystem.Models;

public partial class Customer : User
{
    public int CustomerId { get; set; }

    public string? OrganizationName { get; set; }

    public string? Gender { get; set; }

    public string TelNo { get; set; } = null!;

    public int AddressId { get; set; }

    public string CustomerType { get; set; } = null!;

    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
