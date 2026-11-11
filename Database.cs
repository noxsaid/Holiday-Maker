using MySqlConnector;

namespace Holiday_Maker;


public static class Database
{
    private static string connectionStrign = "Server=localhost;Port=3306;Database=holiday_maker;user ID=root;password=kurslosenord";
    
    public static MySqlConnection GetConnection()
    {
        return new MySqlConnection(connectionStrign);
    }
}