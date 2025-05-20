using Oracle.ManagedDataAccess.Client;
using TennisCourtRentalSystem.Models;

namespace TennisCourtRentalSystem.Repositories;

public class CustomerRepository
{
    private readonly IConfiguration _config;

    public CustomerRepository(IConfiguration config)
    {
        _config = config;
    }

    private OracleConnection GetConnection()
    {
        return new OracleConnection(_config.GetConnectionString("OracleDb"));
    }

    public List<Customer> GetAllCustomers()
    {
        var customers = new List<Customer>();

        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                c.CustomerId, c.UserName, c.PasswordHash, c.Email, 
                c.DateCreated, c.OrganizationName, c.Gender, 
                c.TelNo, c.AddressId, c.CustomerType,
                a.AddressId, a.State, a.City, a.StreetAddress
            FROM Customer c
            JOIN Address a ON c.AddressId = a.AddressId";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var customer = new Customer
            {
                CustomerId = reader.GetInt32(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Email = reader.GetString(3),
                DateCreated = reader.GetDateTime(4),
                OrganizationName = reader.IsDBNull(5) ? null : reader.GetString(5),
                Gender = reader.IsDBNull(6) ? null : reader.GetString(6),
                TelNo = reader.GetString(7),
                AddressId = reader.GetInt32(8),
                CustomerType = reader.GetString(9),
                Address = new Address
                {
                    AddressId = reader.GetInt32(10),
                    State = reader.IsDBNull(11) ? null : reader.GetString(11),
                    City = reader.IsDBNull(12) ? null : reader.GetString(12),
                    Street = reader.IsDBNull(13) ? null : reader.GetString(13),
                }
            };

            customers.Add(customer);
        }

        return customers;
    }

    public Customer? GetCustomerById(int id)
    {
        using var conn = GetConnection();
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                c.CustomerId, c.UserName, c.PasswordHash, c.Email, 
                c.DateCreated, c.OrganizationName, c.Gender, 
                c.TelNo, c.AddressId, c.CustomerType,
                a.AddressId, a.State, a.City, a.StreetAddress
            FROM Customer c
            JOIN Address a ON c.AddressId = a.AddressId
            WHERE c.CustomerId = :id";

        cmd.Parameters.Add(new OracleParameter("id", id));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Customer
            {
                CustomerId = reader.GetInt32(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Email = reader.GetString(3),
                DateCreated = reader.GetDateTime(4),
                OrganizationName = reader.IsDBNull(5) ? null : reader.GetString(5),
                Gender = reader.IsDBNull(6) ? null : reader.GetString(6),
                TelNo = reader.GetString(7),
                AddressId = reader.GetInt32(8),
                CustomerType = reader.GetString(9),
                Address = new Address
                {
                    AddressId = reader.GetInt32(10),
                    State = reader.IsDBNull(11) ? null : reader.GetString(11),
                    City = reader.IsDBNull(12) ? null : reader.GetString(12),
                    Street = reader.IsDBNull(13) ? null : reader.GetString(13),
                }
            };
        }

        return null;
    }

    public Customer? GetCustomerByUsername(string username)
    {
        using var connection = GetConnection();
        connection.Open();
        var command = new OracleCommand("SELECT * FROM Customer WHERE UserName = :username", connection);
        command.Parameters.Add(new OracleParameter("username", username));

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Customer
            {
                CustomerId = reader.GetInt32(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Email = reader.GetString(3),
                DateCreated = reader.GetDateTime(4),
            };
        }

        return null;
    }

    public void CreateCustomer(Customer customer)
    {
        using var connection = GetConnection();
        connection.Open();
        var command = new OracleCommand(@"INSERT INTO Customer 
            (UserName, PasswordHash, Email, DateCreated, TelNo, AddressId, CustomerType) 
            VALUES (:user, :pass, :email, :dateCreated, :tel, :addr, :type)", connection);

        command.Parameters.Add(new OracleParameter("user", customer.UserName));
        command.Parameters.Add(new OracleParameter("pass", customer.PasswordHash));
        command.Parameters.Add(new OracleParameter("email", customer.Email));
        command.Parameters.Add(new OracleParameter("dateCreated", customer.DateCreated));
        command.Parameters.Add(new OracleParameter("tel", customer.TelNo));
        command.Parameters.Add(new OracleParameter("addr", customer.AddressId));
        command.Parameters.Add(new OracleParameter("type", customer.CustomerType));

        command.ExecuteNonQuery();
    }

    // Get customers with their rental history
    public List<Customer> GetAllWithRentals()
    {
        var customers = new Dictionary<int, Customer>();
        
        using var conn = GetConnection();
        conn.Open();
        
        using var cmd = conn.CreateCommand();
        cmd.CommandText = @"
            SELECT 
                c.CustomerId, c.UserName, c.CustomerType,
                r.RentalId, r.RentalDate, r.RentalFee
            FROM Customer c
            LEFT JOIN Rental r ON c.CustomerId = r.CustomerId
            ORDER BY c.CustomerId";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var customerId = reader.GetInt32(0);
            
            if (!customers.ContainsKey(customerId))
            {
                customers[customerId] = new Customer
                {
                    CustomerId = customerId,
                    UserName = reader.GetString(1),
                    CustomerType = reader.GetString(2),
                    Rentals = new List<Rental>()
                };
            }

            if (!reader.IsDBNull(3))
            {
                customers[customerId].Rentals.Add(new Rental
                {
                    RentalId = reader.GetInt32(3),
                    RentalDate = DateOnly.FromDateTime(reader.GetDateTime(4)),
                    RentalFee = reader.GetDecimal(5)
                });
            }
        }
        return customers.Values.ToList();
    }
}
