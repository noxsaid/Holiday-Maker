using MySqlConnector;

namespace Holiday_Maker;


public static class Database
{
    private static string connectionString = "Server=localhost;Port=3306;Database=holiday_maker;user ID=root;password=kurslosenord";
    
    public static MySqlConnection GetConnection()
    {
        return new MySqlConnection(connectionString);
    }
}