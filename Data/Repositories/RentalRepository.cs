using Oracle.ManagedDataAccess.Client;
using TennisCourtRentalSystem.Models;

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

    public List<Rental> GetRentalsByCustomer(int customerId)
    {
        var rentals = new List<Rental>();

        using var conn = GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT RentalID, RentalDate, StartTime, EndTime, RentalFee, CourtNumber
            FROM Rental
            WHERE CustomerID = :customerId
            ORDER BY RentalDate DESC";

        cmd.Parameters.Add(new OracleParameter("customerId", customerId));

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var rentalDateTime = reader.GetDateTime(1);
            var readerRentalDate = DateOnly.FromDateTime(rentalDateTime);

            var startTime = reader.GetDateTime(2);
            var readerStartTime = TimeOnly.FromDateTime(startTime);

            var endTime = reader.GetDateTime(3);
            var readerEndTime = TimeOnly.FromDateTime(endTime);
            rentals.Add(new Rental
            {
                RentalId = reader.GetInt32(0),
                RentalDate = readerRentalDate,
                StartTime = readerStartTime,
                EndTime = readerEndTime,
                RentalFee = reader.GetDecimal(4),
                CourtNumber = reader.GetInt32(5)
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
            INSERT INTO Rental (RentalID, CustomerID, CourtNumber, RentalDate, StartTime, EndTime, RentalFee)
            VALUES (:id, :custId, :court, :rdate, :start, :end, :fee)";

        cmd.Parameters.Add(new OracleParameter("id", rental.RentalId));
        cmd.Parameters.Add(new OracleParameter("custId", rental.CustomerId));
        cmd.Parameters.Add(new OracleParameter("court", rental.CourtNumber));
        cmd.Parameters.Add(new OracleParameter("rdate", rental.RentalDate));
        cmd.Parameters.Add(new OracleParameter("start", rental.StartTime));
        cmd.Parameters.Add(new OracleParameter("end", rental.EndTime));
        cmd.Parameters.Add(new OracleParameter("fee", rental.RentalFee));

        cmd.ExecuteNonQuery();
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

    public bool HasConflict(int courtNumber, DateOnly date, TimeOnly startTime, TimeOnly endTime)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(*) 
            FROM Rental 
            WHERE CourtNumber = :courtNumber 
            AND RentalDate = :date 
            AND (
                (StartTime < :endTime AND EndTime > :startTime) OR
                (StartTime >= :startTime AND StartTime < :endTime) OR
                (EndTime > :startTime AND EndTime <= :endTime)
            )";

        cmd.Parameters.AddRange(new OracleParameter[]
        {
            new("courtNumber", courtNumber),
            new("date", date.ToString("yyyy-MM-dd")),
            new("startTime", startTime.ToString("HH:mm")),
            new("endTime", endTime.ToString("HH:mm"))
        });

        var count = Convert.ToInt32(cmd.ExecuteScalar());
        return count > 0;
    }

    // Get rentals filtered by payment status (with customer/court data)
    public List<Rental> GetByStatus(string status)
    {
        var rentals = new List<Rental>();
        
        using var conn = GetConnection();
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                r.RentalId, r.CustomerId, r.CourtNumber, r.RentalDate, 
                r.StartTime, r.EndTime, r.RentalFee, r.PaymentStatus,
                c.UserName, c.TelNo,
                court.Location
            FROM Rental r
            JOIN Customer c ON r.CustomerId = c.CustomerId
            JOIN Court court ON r.CourtNumber = court.CourtNumber
            WHERE r.PaymentStatus = :status";
        
        cmd.Parameters.Add(new OracleParameter("status", status));

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var rental = new Rental
            {
                RentalId = reader.GetInt32(0),
                CustomerId = reader.GetInt32(1),
                CourtNumber = reader.GetInt32(2),
                RentalDate = DateOnly.FromDateTime(reader.GetDateTime(3)),
                StartTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(4)),
                EndTime = TimeOnly.FromTimeSpan(reader.GetTimeSpan(5)),
                RentalFee = reader.GetDecimal(6),
                PaymentStatus = reader.GetString(7),
                Customer = new Customer
                {
                    UserName = reader.GetString(8),
                    TelNo = reader.GetString(9)
                },
                CourtNumberNavigation = new Court
                {
                    Location = reader.GetString(10)
                }
            };
            rentals.Add(rental);
        }
        return rentals;
    }

    // Check court availability
    public bool IsCourtAvailable(int courtNumber, DateTime date)
    {
        using var conn = GetConnection();
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT COUNT(*) 
            FROM Rental 
            WHERE CourtNumber = :courtNumber 
            AND RentalDate = TO_DATE(:date, 'YYYY-MM-DD')";
        
        cmd.Parameters.Add(new OracleParameter("courtNumber", courtNumber));
        cmd.Parameters.Add(new OracleParameter("date", date.ToString("yyyy-MM-dd")));

        var count = Convert.ToInt32(cmd.ExecuteScalar());
        return count == 0;
    }

    // Calculate revenue between dates
    public decimal GetRevenueByDateRange(DateTime startDate, DateTime endDate)
    {
        using var conn = GetConnection();
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT NVL(SUM(RentalFee), 0) 
            FROM Rental 
            WHERE RentalDate BETWEEN TO_DATE(:startDate, 'YYYY-MM-DD') 
            AND TO_DATE(:endDate, 'YYYY-MM-DD')";
        
        cmd.Parameters.Add(new OracleParameter("startDate", startDate.ToString("yyyy-MM-dd")));
        cmd.Parameters.Add(new OracleParameter("endDate", endDate.ToString("yyyy-MM-dd")));

        var result = cmd.ExecuteScalar();
        return Convert.ToDecimal(result);
    }
}
