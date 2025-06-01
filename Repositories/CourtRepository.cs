using Oracle.ManagedDataAccess.Client;
using TennisCourtRentalSystem.Models;

namespace TennisCourtRentalSystem.Repositories;

public class CourtRepository
{
    private readonly IConfiguration _config;

    public CourtRepository(IConfiguration config)
    {
        _config = config;
    }

    private OracleConnection GetConnection()
    {
        return new OracleConnection(_config.GetConnectionString("OracleDb"));
    }

    public void AddCourt(Court court)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "INSERT INTO Court (CourtNumber, Location, CourtStatus) VALUES (:num, :loc, :cStatus)";
        cmd.Parameters.AddRange(new OracleParameter[] {
            new("num", court.CourtNumber),
            new("loc", court.Location),
            new("cStatus", court.CourtStatus),
        });
        cmd.ExecuteNonQuery();
    }

    public void UpdateCourt(Court court)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Court SET Location = :loc, CourtStatus = :cStatus WHERE CourtNumber = :num";
        cmd.Parameters.AddRange(new OracleParameter[] {
            new("loc", court.Location),
            new("cStatus", court.CourtStatus),
            new("num", court.CourtNumber)
        });
        cmd.ExecuteNonQuery();
    }

    public void DeleteCourt(int courtNumber)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "DELETE FROM Court WHERE CourtNumber = :num";
        cmd.Parameters.Add(new OracleParameter("num", courtNumber));
        cmd.ExecuteNonQuery();
    }

    public List<Court> GetAllCourts()
    {
        var courts = new List<Court>();

        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CourtNumber, Location, CourtStatus FROM Court";

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

    public List<Court> GetAvailableCourts(DateTime date, DateTime startTime, DateTime endTime)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT c.CourtNumber, c.Location 
            FROM Court c
            WHERE NOT EXISTS (
                SELECT 1 FROM Rental r 
                WHERE r.CourtNumber = c.CourtNumber 
                AND r.RentalDate = :rentalDate
                AND (
                    (r.StartTime < :endTime AND r.EndTime > :startTime)
                )
            )";

        cmd.Parameters.Add(new OracleParameter("rentalDate", date.Date)); 
        cmd.Parameters.Add(new OracleParameter("startTime", startTime));
        cmd.Parameters.Add(new OracleParameter("endTime", endTime));

        var availableCourts = new List<Court>();
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            availableCourts.Add(new Court
            {
                CourtNumber = reader.GetInt32(0),
                Location = reader.GetString(1)
            });
        }

        return availableCourts;
    }

    public List<Court> GetAvailableCourts()
    {
        var courts = new List<Court>();

        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CourtNumber, Location, CourtStatus FROM Court WHERE CourtStatus = 'Available'";

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

    public List<Court> GetCourtsWithRentalStatus(DateTime startDate, DateTime endDate)
    {
        var courts = new List<Court>();

        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                c.CourtNumber,
                c.Location,
                c.CourtStatus,
                CASE 
                    WHEN r.CourtNumber IS NOT NULL THEN 'RENTED'
                    ELSE 'NOT RENTED'
                END AS RentalStatus
            FROM Court c
            LEFT JOIN Rental r 
                ON c.CourtNumber = r.CourtNumber
                AND r.RentalDate = TRUNC(:rentalDate)
                AND r.StartTime < :endTime
                AND r.EndTime > :startTime
            ORDER BY c.CourtNumber";

        cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("rentalDate", startDate.Date));
        cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("endTime", endDate));
        cmd.Parameters.Add(new Oracle.ManagedDataAccess.Client.OracleParameter("startTime", startDate));

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            courts.Add(new Court
            {
                CourtNumber = reader.GetInt32(0),
                Location = reader.GetString(1),
                CourtStatus = reader.GetString(2),
                RentalStatus = reader.GetString(3)
            });
        }

        return courts;
    }

    public Court? GetCourtByNumber(int courtNumber)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT CourtNumber, Location, CourtStatus FROM Court WHERE CourtNumber = :courtNumber";
        cmd.Parameters.Add(new OracleParameter("courtNumber", courtNumber));

        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return new Court
            {
                CourtNumber = reader.GetInt32(0),
                Location = reader.GetString(1),
                CourtStatus = reader.GetString(2)
            };
        }

        return null;
    }

    public void UpdateCourtStatus(int courtNumber, string newStatus)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "UPDATE Court SET CourtStatus = :status WHERE CourtNumber = :number";
        cmd.Parameters.Add(new OracleParameter("status", newStatus));
        cmd.Parameters.Add(new OracleParameter("number", courtNumber));

        cmd.ExecuteNonQuery();
    }

    public List<CourtAvailability> GetCourtRentalStatus(DateOnly rentalDate, TimeOnly startTime, TimeOnly endTime)
    {
        var availability = new List<CourtAvailability>();

        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                c.CourtNumber,
                c.Location,
                CASE
                    WHEN r.RentalId IS NOT NULL THEN 'Rented'
                    ELSE 'Not Rented'
                END AS Status
            FROM Court c
            LEFT JOIN Rental r
                ON c.CourtNumber = r.CourtNumber
                AND r.RentalDate = :rentalDate
                AND (
                    r.StartTime < :endTime AND r.EndTime > :startTime
                )";

        cmd.Parameters.Add(new OracleParameter("rentalDate", rentalDate.ToDateTime(TimeOnly.MinValue)));
        cmd.Parameters.Add(new OracleParameter("endTime", rentalDate.ToDateTime(endTime)));
        cmd.Parameters.Add(new OracleParameter("startTime", rentalDate.ToDateTime(startTime)));

    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        availability.Add(new CourtAvailability
        {
            CourtNumber = reader.GetInt32(0),
            Location = reader.GetString(1),
            Status = reader.GetString(2)
        });
    }

    return availability;
}

    public int? GetCourtCount()
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Court";

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetInt32(0);
        }
        return null;
    }

    public bool IsCourtAvailable(int courtNumber, DateTime date, DateTime startTime, DateTime endTime)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM Rental WHERE CourtNumber = :courtNumber AND RentalDate BETWEEN :startDate AND :endDate";
        cmd.Parameters.Add(new OracleParameter("courtNumber", courtNumber));
        cmd.Parameters.Add(new OracleParameter("startDate", startTime));
        cmd.Parameters.Add(new OracleParameter("endDate", endTime));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return reader.GetInt32(0) == 0;
        }
        return false;
    }
        
}
