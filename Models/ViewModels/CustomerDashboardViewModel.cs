namespace TennisCourtRentalSystem.Models.ViewModels;
public class CustomerDashboardViewModel
{
    public Customer Customer { get; set; }
    public int ActiveRentalsCount { get; set; }
    public int UpcomingRentalsCount { get; set; }
    public decimal TotalSpent { get; set; }
    public List<Rental> UpcomingRentals { get; set; }
    public List<Rental> PastRentals { get; set; }
}