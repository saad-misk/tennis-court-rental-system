using System.Data;
using Oracle.ManagedDataAccess.Client;
using TennisCourtRentalSystem.Models;

namespace TennisCourtRentalSystem.Repositories;

public class AddressRepository
{
    private readonly IConfiguration _config;

    public AddressRepository(IConfiguration config)
    {
        _config = config;
    }

    private OracleConnection GetConnection()
    {
        return new OracleConnection(_config.GetConnectionString("OracleDb"));
    }

    public string CreateAddress(Address address)
    {
        using var conn = GetConnection();
        conn.Open();
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            INSERT INTO Address (State, City, Street) 
            VALUES (:state, :city, :street)
            RETURNING AddressId INTO :addressId";

        cmd.Parameters.Add(new OracleParameter("state", address.State));
        cmd.Parameters.Add(new OracleParameter("city", address.City));
        cmd.Parameters.Add(new OracleParameter("street", address.Street));
        var addressIdParam = new OracleParameter("addressId", OracleDbType.Int32)
        {
            Direction = ParameterDirection.Output
        };
        cmd.Parameters.Add(addressIdParam);

        cmd.ExecuteNonQuery();
        return addressIdParam.Value.ToString();
    }
}