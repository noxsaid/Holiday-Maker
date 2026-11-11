using System;
using Holiday_Maker;
using MySqlConnector;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            using var connection = Database.GetConnection();
            connection.Open();
            Console.WriteLine("Connection successful!");

            string query = "SELECT COUNT(*) FROM cities;";
            using var command = new MySqlCommand(query, connection);
            
            // Safe unboxing handling potential null/empty results
            object? result = command.ExecuteScalar();
            long cityCount = result != null ? Convert.ToInt64(result) : 0;
            
            Console.WriteLine($"Total cities in database: {cityCount}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Database error: {ex.Message}");
        }
    }
}