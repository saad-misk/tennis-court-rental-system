namespace TennisCourtRentalSystem.Models;

public class Activity
{
    public string Description { get; set; }
    public DateTime Timestamp { get; set; }
    public string Location { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string Status { get; set; }
}