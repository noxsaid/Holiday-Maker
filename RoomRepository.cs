namespace Holiday_Maker;


using MySqlConnector;


public class RoomRepository
{
    private readonly MySqlConnection _connection;
    
    public RoomRepository(MySqlConnection connection)
    {
      _connection = connection;
    }
}