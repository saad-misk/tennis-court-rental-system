namespace TennisCourtRentalSystem.Models;

public partial class Customer : User
{
    public string CustomerId { get; set; }

    public string? OrganizationName { get; set; }

    public string? Gender { get; set; }

    public string TelNo { get; set; } = null!;

    public string AddressId { get; set; }

    public int CustomerType { get; set; } // 1:Resident, 0:NonResident

    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<Rental> Rentals { get; set; } = new List<Rental>();
}
