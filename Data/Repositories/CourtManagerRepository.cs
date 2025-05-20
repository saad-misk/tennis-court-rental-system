using Oracle.ManagedDataAccess.Client;
using TennisCourtRentalSystem.Models;

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

    public List<CourtManager> GetAllManagers()
    {
        var managers = new List<CourtManager>();

        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT EmployeeId, UserName, PasswordHash, Email, DateCreated, OfficePhone FROM CourtManager";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            managers.Add(new CourtManager
            {
                EmployeeId = reader.GetInt32(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Email = reader.GetString(3),
                DateCreated = reader.GetDateTime(4),
                OfficePhone = reader.IsDBNull(5) ? null : reader.GetString(5)
            });
        }

        return managers;
    }

    public CourtManager? GetManagerById(int id)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT EmployeeId, UserName, PasswordHash, Email, DateCreated, OfficePhone FROM CourtManager WHERE EmployeeId = :id";
        cmd.Parameters.Add(new OracleParameter("id", id));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new CourtManager
            {
                EmployeeId = reader.GetInt32(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Email = reader.GetString(3),
                DateCreated = reader.GetDateTime(4),
                OfficePhone = reader.IsDBNull(5) ? null : reader.GetString(5)
            };
        }

        return null;
    }

    public CourtManager? GetManagerByUsername(string username)
    {
        using var connection = GetConnection();
        connection.Open();
        var command = new OracleCommand("SELECT * FROM CourtManager WHERE UserName = :username", connection);
        command.Parameters.Add(new OracleParameter("username", username));

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new CourtManager
            {
                EmployeeId = reader.GetInt32(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Email = reader.GetString(3),
                DateCreated = reader.GetDateTime(4)
            };
        }

        return null;
    }

    public void AddManager(CourtManager manager)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO CourtManager (UserName, PasswordHash, Email, DateCreated, OfficePhone)
            VALUES (:username, :password, :email, :dateCreated, :officePhone)";

        cmd.Parameters.Add(new OracleParameter("username", manager.UserName));
        cmd.Parameters.Add(new OracleParameter("password", manager.PasswordHash));
        cmd.Parameters.Add(new OracleParameter("email", manager.Email));
        cmd.Parameters.Add(new OracleParameter("dateCreated", manager.DateCreated));
        cmd.Parameters.Add(new OracleParameter("officePhone", manager.OfficePhone ?? (object)DBNull.Value));

        cmd.ExecuteNonQuery();
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
                EmployeeId = reader.GetInt32(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Email = reader.GetString(3),
                DateCreated = reader.GetDateTime(4),
                OfficePhone = reader.IsDBNull(5) ? null : reader.GetString(5)
            };
        }

        return null;
    }
}
