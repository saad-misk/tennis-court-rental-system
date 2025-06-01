namespace TennisCourtRentalSystem.Models;

public class RentalBill
{
    public string RentalId { get; set; }
    public string UserName { get; set; }
    public DateTime RentalDate { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public double HoursBooked { get; set; }
    public decimal HourlyRate { get; set; }
    public decimal CalculatedFee { get; set; }
    public decimal RentalFee { get; set; }
    public string PaymentStatus { get; set; } = String.Empty;
}
