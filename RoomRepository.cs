namespace Holiday_Maker;


using MySqlConnector;


public class RoomRepository
{
    private readonly MySqlConnection _connection;
    
    public RoomRepository(MySqlConnection connection)
    {
        if (connection == null )
        {
            throw new ArgumentNullException(nameof(connection));
        }
      _connection = connection;
    }
    public async Task<List<RoomSearchResults>> SearchAvailableRoomsAsync(
        string City,
        string checkIn,
        string checkOut,
        int guests)
    {
        List<RoomSearchResults> results = new List<RoomSearchResults>();

        string sql = @"
        SELECT r.id AS room_id, r.name AS room_name, a.name AS accommodation_name,
                ci.name AS city, r.max_capacity, r.price_per_night
        FROM rooms r
        JOIN accommodations a ON r.accommodation_id = a.id
        JOIN cities ci ON a.city_id = ci.id
        WHERE ci.name = @city
          AND r.max_capacity >= @guests
          AND r.id NOT IN (
              SELECT room_id FROM bookings
              WHERE booking_start < @checkOut AND booking_end > checkIn
              );
        ";
        await using MySqlCommand command = new MySqlCommand(sql, _connection);

        return results;
    
    }
}