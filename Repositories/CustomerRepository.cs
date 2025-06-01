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

    // public bool TestConnection()
    // {
    //     try
    //     {
    //         using (var connection = GetConnection())
    //         {
    //             connection.Open();
    //             var command = new OracleCommand("SELECT 1 FROM DUAL", connection);
    //             var result = command.ExecuteScalar();
    //             // Console.WriteLine($"Test query result: {result}"); 
    //             return true;
    //         }
    //     }
    //     catch (OracleException ex)
    //     {
    //         Console.WriteLine($"Oracle Error: {ex.Message}");
    //         return false;
    //     }
    // }

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
                a.AddressId, a.State, a.City, a.Street
            FROM Customer c
            JOIN Address a ON c.AddressId = a.AddressId
            ORDER BY c.UserName ASC";

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            var customer = new Customer
            {
                CustomerId = reader.GetString(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Email = reader.GetString(3),
                DateCreated = reader.GetDateTime(4),
                OrganizationName = reader.IsDBNull(5) ? null : reader.GetString(5),
                Gender = reader.GetInt32(6) == 1 ? "Male" : "Female",
                TelNo = reader.GetString(7),
                AddressId = reader.GetString(8),
                CustomerType = reader.GetInt16(9),
                Address = new Address
                {
                    AddressId = reader.GetString(10),
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
                a.AddressId, a.State, a.City, a.Street
            FROM Customer c
            JOIN Address a ON c.AddressId = a.AddressId
            WHERE c.CustomerId = :id";

        cmd.Parameters.Add(new OracleParameter("id", id));

        using var reader = cmd.ExecuteReader();
        if (reader.Read())
        {
            return new Customer
            {
                CustomerId = reader.GetString(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Email = reader.GetString(3),
                DateCreated = reader.GetDateTime(4),
                OrganizationName = reader.IsDBNull(5) ? null : reader.GetString(5),
                Gender = reader.GetInt32(6) == 1 ? "Male" : "Female",
                TelNo = reader.GetString(7),
                AddressId = reader.GetString(8),
                CustomerType = reader.GetInt16(9),
                Address = new Address
                {
                    AddressId = reader.GetString(10),
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
        var command = new OracleCommand(@"
            SELECT 
                CustomerId, UserName, PasswordHash, Email, DateCreated, 
                OrganizationName, Gender, TelNo, AddressId, CustomerType
            FROM Customer 
            WHERE UserName = :username", connection);
        command.Parameters.Add(new OracleParameter("username", username));

        using var reader = command.ExecuteReader();
        if (reader.Read())
        {
            return new Customer
            {
                CustomerId = reader.GetString(0),
                UserName = reader.GetString(1),
                PasswordHash = reader.GetString(2),
                Email = reader.GetString(3),
                DateCreated = reader.GetDateTime(4),
                OrganizationName = reader.IsDBNull(5) ? null : reader.GetString(5),
                Gender = reader.GetInt32(6) == 1 ? "Male" : "Female",
                TelNo = reader.GetString(7),
                AddressId = reader.GetString(8),
                CustomerType = reader.GetInt32(9)
            };
        }
        return null;
    }

    public void CreateCustomer(Customer customer)
    {
        using var connection = GetConnection();
        connection.Open();

        string customerId = GenerateCustomerId(); 
        Console.WriteLine("\n\n\nid here\n" + customerId + "\nid here\n\n\n\n");

        var command = new OracleCommand(@"
            INSERT INTO CUSTOMER 
                (CUSTOMERID, USERNAME, PASSWORDHASH, EMAIL, DATECREATED, 
                ORGANIZATIONNAME, GENDER, TELNO, ADDRESSID, CUSTOMERTYPE) 
            VALUES 
                (:id, :username, :passwordhash, :email, :datecreated, 
                :organizationname, :gender, :telno, :addressid, :customertype)", connection);

        command.Parameters.AddRange(new OracleParameter[] {
            new("id", customerId),
            new("username", customer.UserName),
            new("passwordhash", customer.PasswordHash),
            new("email", customer.Email),
            new("datecreated", customer.DateCreated),
            new("organizationname", customer.OrganizationName ?? (object)DBNull.Value),
            new("gender", customer.Gender == "Male" ? 0 : 1),
            new("telno", customer.TelNo.PadRight(10).Substring(0, 10)),
            new("addressid", customer.AddressId),
            new("customertype", customer.CustomerType)
        });

        command.ExecuteNonQuery();
    }

    private string GenerateCustomerId()
    {
        using var conn = GetConnection();
        conn.Open();
        var cmd = new OracleCommand("SELECT MAX(CUSTOMERID) FROM CUSTOMER", conn);
        var maxId = cmd.ExecuteScalar()?.ToString() ?? "C000";
        int nextId = int.Parse(maxId.Replace("C", "")) + 1;
        return $"C{nextId:D3}";
    }

    public List<Customer> GetAllWithRentals()
    {
        var customers = new Dictionary<string, Customer>();
        
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
            var customerId = reader.GetString(0);
            
            if (!customers.ContainsKey(customerId))
            {
                customers[customerId] = new Customer
                {
                    CustomerId = customerId,
                    UserName = reader.GetString(1),
                    CustomerType = reader.GetInt16(2),
                    Rentals = new List<Rental>()
                };
            }

            if (!reader.IsDBNull(3))
            {
                customers[customerId].Rentals.Add(new Rental
                {
                    RentalId = reader.GetString(3),
                    RentalDate = reader.GetDateTime(4),
                    RentalFee = reader.GetDecimal(5)
                });
            }
        }
        return customers.Values.ToList();
    }
}
