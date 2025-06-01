using Oracle.ManagedDataAccess.Client;
using TennisCourtRentalSystem.Models;
using TennisCourtRentalSystem.Models.ViewModels;

namespace TennisCourtRentalSystem.Repositories;

public class CourtManagerRepository
{
    private readonly IConfiguration _config;

    public CourtManagerRepository(IConfiguration config)
    {
        _config = config;
    }

    private OracleConnection GetConnection()
    {
        return new OracleConnection(_config.GetConnectionString("OracleDb"));
    }

    public CourtManager? GetManagerByUsername(string username)
    {
        using var connection = GetConnection();
        connection.Open();
        var command = new OracleCommand(@"
            SELECT 
                EmployeeId, UserName, PasswordHash, Email, DateCreated
            FROM courtmanager 
            WHERE UserName = :username", connection);
        command.Parameters.Add(new OracleParameter("username", username));

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new CourtManager
            {
                EmployeeId = reader.GetString(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Email = reader.GetString(3),
                DateCreated = reader.GetDateTime(4)
            };
        }
        return null;
    }
    public CourtManager? AuthenticateManager(string username, string passwordHash)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT EmployeeId, UserName, PasswordHash, Email, DateCreated, OfficePhone FROM CourtManager WHERE UserName = :username AND PasswordHash = :password";
        cmd.Parameters.Add(new OracleParameter("username", username));
        cmd.Parameters.Add(new OracleParameter("password", passwordHash));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new CourtManager
            {
                EmployeeId = reader.GetString(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Email = reader.GetString(3),
                DateCreated = reader.GetDateTime(4),
                OfficePhone = reader.IsDBNull(5) ? null : reader.GetString(5)
            };
        }

        return null;
    }

    public RentalBill? GetRentalBillById(int rentalId)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                r.RentalID,
                c.UserName,
                r.RentalDate,
                r.StartTime,
                r.EndTime,
                ROUND((r.EndTime - r.StartTime) * 24, 2) AS HoursBooked,
                CASE 
                    WHEN c.IsResident = 'Y' THEN 10 
                    ELSE 20 
                END AS HourlyRate,
                ROUND(
                    ((r.EndTime - r.StartTime) * 24) * 
                    CASE 
                        WHEN c.IsResident = 1 THEN 10 
                        ELSE 20 
                    END, 
                2) AS CalculatedFee,
                r.RentalFee,
                r.PaymentStatus
            FROM Rental r
            JOIN Customer c ON r.CustomerID = c.CustomerID
            WHERE r.RentalID = :rentalId";

        cmd.Parameters.Add(new OracleParameter("rentalId", rentalId));

        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return new RentalBill
            {
                RentalId = reader.GetString(0),
                UserName = reader.GetString(1),
                RentalDate = reader.GetDateTime(2),
                StartTime = reader.GetDateTime(3),
                EndTime = reader.GetDateTime(4),
                HoursBooked = reader.GetDouble(5),
                HourlyRate = reader.GetDecimal(6),
                CalculatedFee = reader.GetDecimal(7),
                RentalFee = reader.GetDecimal(8),
                PaymentStatus = reader.GetString(9)
            };
        }

        return null;
    }

    public List<CourtRentalViewModel> GetRentalsForCourt(int courtNumber)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT r.RentalId, 
                r.RentalDate, 
                r.RentalFee,
                c.Location
            FROM Rental r
            JOIN Court c ON c.CourtNumber = r.CourtNumber
            WHERE c.CourtNumber = :courtNumber
            ORDER BY r.RentalDate DESC";

        cmd.Parameters.Add(new OracleParameter("courtNumber", courtNumber));

        var rentals = new List<CourtRentalViewModel>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            rentals.Add(new CourtRentalViewModel
            {   
                RentalId = reader.GetString(0),
                RentalDate = reader.GetDateTime(1),
                Fee = reader.GetDecimal(2),
                CourtNumber = courtNumber
            });
        }

        return rentals;
    }

    public List<Activity> GetRecentActivities()
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                r.RentalID,
                r.RentalDate,
                c.Location,
                r.StartTime,
                r.EndTime,
                CASE WHEN r.RentalId IS NULL THEN 'Available' ELSE 'Booked' END AS Status
            FROM Rental r
            JOIN Court c ON r.CourtNumber = c.CourtNumber
            WHERE r.RentalDate >= TRUNC(:currentTime)
            ORDER BY r.RentalDate DESC";

        cmd.Parameters.Add(new OracleParameter("currentTime", DateTime.Now));

        using var reader = cmd.ExecuteReader();
        var recentActivities = new List<Activity>();
        while (reader.Read())
        {
            var activity = new Activity
            {
                Description = reader.GetString(0),
                Timestamp = reader.GetDateTime(1),
                Location = reader.GetString(2),
                StartTime = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                EndTime = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                Status = reader.GetString(5)
            };
            recentActivities.Add(activity);
        }

        return recentActivities;
    }
}
