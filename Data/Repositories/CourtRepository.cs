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
    
}
