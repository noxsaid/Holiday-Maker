using MySqlConnector;


public class CustomerRepository
{
    private readonly MySqlConnection _connection;

    public CustomerRepository(MySqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<long> RegisterCustomerAsync(string name, string telnr, string email)
    {
        string query = "INSERT INTO customers (name, telnr, email) values (@name, @telnr, @email);";

        try
        {
            using MySqlCommand command = new MySqlCommand(query, _connection);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@telnr", telnr);
            command.Parameters.AddWithValue("@email", email);

            await command.ExecuteNonQueryAsync();
            return command.LastInsertedId;
        }
        catch (MySqlException ex) when (ex.Number == 1062)
        {
            throw new InvalidOperationException("E-postadressen är redan registrerad i systemet.", ex);
        }
    }
}