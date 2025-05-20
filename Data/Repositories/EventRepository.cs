using Oracle.ManagedDataAccess.Client;
using System.Data;
using TennisCourtRentalSystem.Models;
using Microsoft.Extensions.Configuration;

namespace TennisCourtRentalSystem.Repositories;

public class EventRepository
{
    private readonly IConfiguration _config;

    public EventRepository(IConfiguration config)
    {
        _config = config;
    }

    private OracleConnection GetConnection()
    {
        return new OracleConnection(_config.GetConnectionString("OracleDb"));
    }

    public List<Event> GetAllEvents()
    {
        var events = new List<Event>();

        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                e.EventId, e.RentalId, e.EmployeeId, e.EventType,
                e.EventDescription, e.IsSpecialEvent, e.PoliciesSigned,
                e.PoliceNotified, e.ApprovalStatus
            FROM Event e";

        using var reader = cmd.ExecuteReader();

        while (reader.Read())
        {
            var ev = new Event
            {
                EventId = reader.GetInt32(0),
                RentalId = reader.GetInt32(1),
                EmployeeId = reader.GetInt32(2),
                EventType = reader.GetString(3),
                EventDescription = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsSpecialEvent = reader.GetBoolean(5),
                PoliciesSigned = reader.GetBoolean(6),
                PoliceNotified = reader.GetBoolean(7),
                ApprovalStatus = reader.GetString(8)
            };

            events.Add(ev);
        }

        return events;
    }

    public Event? GetEventById(int id)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                e.EventId, e.RentalId, e.EmployeeId, e.EventType,
                e.EventDescription, e.IsSpecialEvent, e.PoliciesSigned,
                e.PoliceNotified, e.ApprovalStatus
            FROM Event e
            WHERE e.EventId = :id";

        cmd.Parameters.Add(new OracleParameter("id", id));

        using var reader = cmd.ExecuteReader();

        if (reader.Read())
        {
            return new Event
            {
                EventId = reader.GetInt32(0),
                RentalId = reader.GetInt32(1),
                EmployeeId = reader.GetInt32(2),
                EventType = reader.GetString(3),
                EventDescription = reader.IsDBNull(4) ? null : reader.GetString(4),
                IsSpecialEvent = reader.GetBoolean(5),
                PoliciesSigned = reader.GetBoolean(6),
                PoliceNotified = reader.GetBoolean(7),
                ApprovalStatus = reader.GetString(8)
            };
        }

        return null;
    }
}
