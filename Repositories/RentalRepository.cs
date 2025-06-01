using Oracle.ManagedDataAccess.Client;
using TennisCourtRentalSystem.Models;

namespace TennisCourtRentalSystem.Repositories;

public class RentalRepository
{
    private readonly IConfiguration _config;

    public RentalRepository(IConfiguration config)
    {
        _config = config;
    }

    private OracleConnection GetConnection()
    {
        return new OracleConnection(_config.GetConnectionString("OracleDb"));
    }

    public Rental? GetRentalById(int rentalId)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT * FROM Rental 
            WHERE RentalId = :rentalId";
        cmd.Parameters.Add(new OracleParameter("rentalId", rentalId));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Rental
            {
                RentalId = reader.GetString(0),
                CustomerId = reader.GetString(1),
                CourtNumber = reader.GetInt32(2),
                RentalDate = reader.GetDateTime(3),
                StartTime = reader.GetDateTime(4),
                EndTime = reader.GetDateTime(5),
                ExpectedAttendance = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                RentalFee = reader.GetDecimal(7),
                PaymentStatus = reader.GetString(8),
                DateBooked = reader.GetDateTime(9),
                PaymentDate = reader.IsDBNull(10) ? null : reader.GetDateTime(10)
            };
        }
        return null;
    }

    public Rental GetRentalWithDetails(string rentalId)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT r.*, c.Location, cu.UserName, cu.Email 
            FROM Rental r
            JOIN Court c ON r.CourtNumber = c.CourtNumber
            JOIN Customer cu ON r.CustomerId = cu.CustomerId
            WHERE r.RentalId = :rentalId";
        cmd.Parameters.Add(new OracleParameter("rentalId", rentalId));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Rental
            {
                RentalId = reader.GetString(0),
                CustomerId = reader.GetString(1),
                CourtNumber = reader.GetInt32(2),
                RentalDate = reader.IsDBNull(4) ? DateTime.MinValue : reader.GetDateTime(4),
                StartTime = reader.IsDBNull(5) ? DateTime.MinValue : reader.GetDateTime(5),
                EndTime = reader.IsDBNull(6) ? DateTime.MinValue : reader.GetDateTime(6),
                RentalFee = reader.IsDBNull(8) ? 0 : reader.GetDecimal(8),
                Court = new Court { Location = reader.GetString(14) },
                Customer = new Customer
                {
                    UserName = reader.GetString(15),
                    Email = reader.GetString(16)
                }
            };
        }
        return null;
    }

    public void UpdateRental(Rental rental)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            UPDATE Rental SET 
                CourtNumber = :courtNumber,
                RentalDate = :rentalDate,
                StartTime = :startTime,
                EndTime = :endTime,
                ExpectedAttendance = :expectedAttendance,
                RentalFee = :rentalFee
            WHERE RentalId = :rentalId";

        cmd.Parameters.AddRange(new OracleParameter[] {
            new("courtNumber", rental.CourtNumber),
            new("rentalDate", rental.RentalDate),
            new("startTime", rental.StartTime),
            new("endTime", rental.EndTime),
            new("expectedAttendance", rental.ExpectedAttendance ?? (object)DBNull.Value),
            new("rentalFee", rental.RentalFee),
            new("rentalId", rental.RentalId)
        });

        cmd.ExecuteNonQuery();
    }

    public void DeleteRental(int rentalId)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Rental WHERE RentalId = :rentalId";
        cmd.Parameters.Add(new OracleParameter("rentalId", rentalId));
        cmd.ExecuteNonQuery();
    }

    public List<Rental> GetRentalsByCustomerId(string customerId)
    {
        var rentals = new List<Rental>();

        using var conn = GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT r.RentalID, r.RentalDate, r.StartTime, r.EndTime, 
                r.RentalFee, r.CourtNumber, c.Location
            FROM Rental r
            JOIN Court c ON r.CourtNumber = c.CourtNumber
            WHERE r.CustomerID = :customerId
            ORDER BY r.RentalDate DESC";

        cmd.Parameters.Add(new OracleParameter("customerId", customerId));

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            rentals.Add(new Rental
            {
                RentalId = reader.GetString(0),
                RentalDate = reader.GetDateTime(1),
                StartTime = reader.GetDateTime(2),
                EndTime = reader.GetDateTime(3),
                RentalFee = reader.GetDecimal(4),
                CourtNumber = reader.GetInt32(5),
                Court = new Court
                {
                    CourtNumber = reader.GetInt32(5),
                    Location = reader.GetString(6)
                }
            });
        }

        return rentals;
    }

    public void CreateRental(Rental rental)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        
        cmd.CommandText = @"
            INSERT INTO Rental 
                (RentalId, CustomerId, CourtNumber, RentalDate, StartTime, EndTime, 
                ExpectedAttendance, RentalFee, PaymentStatus, DateBooked) 
            VALUES 
                (:rentalId, :customerId, :courtNumber, :rentalDate, :startTime, :endTime, 
                :expectedAttendance, :rentalFee, :paymentStatus, :dateBooked)";

        // Modified parameter handling
        cmd.Parameters.AddRange(new OracleParameter[] {
            new("rentalId", GenerateRentalId()),
            new("customerId", rental.CustomerId),
            new("courtNumber", rental.CourtNumber),
            new("rentalDate", rental.RentalDate),
            new("startTime", rental.StartTime),
            new("endTime", rental.EndTime),
            new("expectedAttendance", rental.ExpectedAttendance ?? 2), // Default to 2
            new("rentalFee", rental.RentalFee),
            new("paymentStatus", "Pending"),
            new("dateBooked", DateTime.UtcNow)
        });

        cmd.ExecuteNonQuery();
    }

    private string GenerateRentalId()
    {
        using var conn = GetConnection();
        conn.Open();
        var cmd = new OracleCommand("SELECT MAX(RENTALID) FROM RENTAL", conn);
        var maxId = cmd.ExecuteScalar()?.ToString() ?? "R000";
        int nextId = int.Parse(maxId.Replace("R", "")) + 1;
        return $"R{nextId:D3}";
    }

    public List<Court> GetAvailableCourts(DateTime rentalDate, DateTime startTime, DateTime endTime)
    {
        var courts = new List<Court>();

        using var conn = GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();

        cmd.CommandText = @"
            SELECT CourtNumber, Location, CourtStatus
            FROM Court
            WHERE CourtStatus = 'Not Rented' OR CourtNumber NOT IN (
                SELECT CourtNumber
                FROM Rental
                WHERE RentalDate = :rdate
                AND ((StartTime < :endTime AND EndTime > :startTime))
            )";

        cmd.Parameters.Add(new OracleParameter("rdate", rentalDate));
        cmd.Parameters.Add(new OracleParameter("endTime", endTime));
        cmd.Parameters.Add(new OracleParameter("startTime", startTime));

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            courts.Add(new Court
            {
                CourtNumber = reader.GetInt32(0),
                Location = reader.GetString(1),
                CourtStatus = reader.GetString(2)
            });
        }

        return courts;
    }

    public List<Rental> GetActiveRentals(DateTime currentTime)
    {
        var rentals = new List<Rental>();

        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT r.*, c.Location, cu.UserName 
            FROM Rental r
            JOIN Court c ON r.CourtNumber = c.CourtNumber
            JOIN Customer cu ON r.CustomerId = cu.CustomerId
            WHERE r.RentalDate = TRUNC(:currentTime)
            AND r.StartTime <= :currentTime
            AND r.EndTime >= :currentTime";

        cmd.Parameters.Add(new OracleParameter("currentTime", currentTime));

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            rentals.Add(new Rental
            {
                RentalId = reader.GetString(0),
                CustomerId = reader.GetString(1),
                CourtNumber = reader.GetInt32(2),
                RentalDate = reader.GetDateTime(3), // DateTime
                StartTime = reader.GetDateTime(4),  // DateTime
                EndTime = reader.GetDateTime(5),    // DateTime
                ExpectedAttendance = reader.GetInt32(6),
                RentalFee = reader.GetDecimal(7),
                PaymentStatus = reader.GetString(8),
                DateBooked = reader.GetDateTime(9),
                PaymentDate = reader.IsDBNull(10) ? null : reader.GetDateTime(10),
                Court = new Court { Location = reader.GetString(11) },
                Customer = new Customer { UserName = reader.GetString(12) }
            });
        }

        return rentals;
    }

    public Dictionary<int, decimal> GetRevenueByCourt(DateOnly startDate, DateOnly endDate)
    {
        var revenueData = new Dictionary<int, decimal>();

        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT r.CourtNumber, SUM(r.RentalFee) AS TotalRevenue
            FROM Rental r
            WHERE r.RentalDate BETWEEN :startDate AND :endDate
            GROUP BY r.CourtNumber";

        cmd.Parameters.Add(new OracleParameter("startDate", startDate));
        cmd.Parameters.Add(new OracleParameter("endDate", endDate));

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            revenueData.Add(
                reader.GetInt32(0),
                reader.GetDecimal(1)
            );
        }

        return revenueData;
    }

    public void UpdatePaymentStatus(int rentalId, string status)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Rental SET PaymentStatus = :status, PaymentDate = :date WHERE RentalId = :id";
        cmd.Parameters.AddRange(new OracleParameter[] {
            new("status", status),
            new("date", status == "Paid" ? DateTime.UtcNow : (object)DBNull.Value),
            new("id", rentalId)
        });
        cmd.ExecuteNonQuery();
    }

    public int? GetActiveRentalCount()
    {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Rental WHERE RentalDate >= TRUNC(:currentTime)";
        cmd.Parameters.Add(new OracleParameter("currentTime", DateTime.UtcNow));
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetInt32(0);
        }
        return null;
    }

    public decimal GetDailyRevenue()
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
        SELECT COALESCE(SUM(RentalFee), 0) 
        FROM Rental 
        WHERE TRUNC(RentalDate) = TRUNC(SYSDATE)";

        object result = cmd.ExecuteScalar();

        if (result == null || result == DBNull.Value)
        {
            return 0m;
        }

        return Convert.ToDecimal(result);
    }

    public int GetActiveUserCount()
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"SELECT count(*) 
                            FROM Customer 
                            WHERE CustomerId IN (
                                SELECT CustomerId 
                                FROM Rental 
                                GROUP BY CustomerId 
                                HAVING COUNT(*) >= 2
                            )";
        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetInt32(0);
        }
        return 0;
    }

    public List<CurrentUsageViewModel> GetCurrentCourtUsage()
    {
        var currentDateTime = DateTime.Now;
        var currentDate = currentDateTime.Date;

        using var conn = GetConnection();
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT c.UserName AS CustomerName,
                r.CourtNumber,
                r.StartTime,
                r.EndTime
            FROM Rental r
            JOIN Customer c ON r.CustomerId = c.CustomerId
            WHERE r.RentalDate = TRUNC(SYSDATE)
            AND r.StartTime <= :currentDateTime
            AND r.EndTime >= :currentDateTime
            ORDER BY r.CourtNumber";

        cmd.Parameters.Add(new OracleParameter("currentDateTime", currentDateTime));

        var currentUsage = new List<CurrentUsageViewModel>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            currentUsage.Add(new CurrentUsageViewModel
            {
                CustomerName = reader["CustomerName"].ToString(),
                CourtNumber = int.Parse(reader["CourtNumber"].ToString()),
                StartTime = (DateTime)reader["StartTime"],
                EndTime = (DateTime)reader["EndTime"]
            });
        }
        
        return currentUsage;
    }
}