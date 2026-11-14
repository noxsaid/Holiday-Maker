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
        string city,
        string checkIn,
        string checkOut,
        int guests,
        bool requirePool = false,
        bool requireRestaurant = false,
        int? maxBeachDistance = null,
        int? maxCenterDistance = null,
        int sortBy = 0)
    {
        List<RoomSearchResults> results = new List<RoomSearchResults>();

        string query = @"
        SELECT r.id AS room_id, r.name AS room_name, a.name AS accommodation_name,
               ci.name AS city, r.max_capacity, r.price_per_night
        FROM rooms r
        JOIN accommodations a ON r.accommodation_id = a.id
        JOIN cities ci ON a.city_id = ci.id
        WHERE ci.name = @city
          AND r.max_capacity >= @guests
          AND r.id NOT IN (
              SELECT room_id FROM bookings
              WHERE booking_start < @checkOut AND booking_end > @checkIn)";

        if (requirePool)
        {
            query += " AND a.has_pool = TRUE";
        }
        if (requireRestaurant)
        {
            query += " AND a.has_restaurant = TRUE";
        }
        if (maxBeachDistance.HasValue)
        {
            query += " AND a.distance_to_beach <= @maxBeach";
        }
        if (maxCenterDistance.HasValue)
        {
            query += " AND a.distance_to_center <= @maxCenter";
        }

        if (sortBy == 1)
        {
            query += " ORDER BY r.price_per_night ASC";
        }
        else if (sortBy == 2)
        {
            query += " ORDER BY a.rating DESC";
        }

        query += ";";

        await using MySqlCommand command = new MySqlCommand(query, _connection);
        command.Parameters.AddWithValue("@city", city);
        command.Parameters.AddWithValue("@guests", guests);
        command.Parameters.AddWithValue("@checkIn", checkIn);
        command.Parameters.AddWithValue("@checkOut", checkOut);

        if (maxBeachDistance.HasValue)
        {
            command.Parameters.AddWithValue("@maxBeach", maxBeachDistance.Value);
        }
        if (maxCenterDistance.HasValue)
        {
            command.Parameters.AddWithValue("@maxCenter", maxCenterDistance.Value);
        }

        await using MySqlDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            results.Add(new RoomSearchResults(
                reader.GetInt32("room_id"),
                reader.GetString("room_name"),
                reader.GetString("accommodation_name"),
                reader.GetString("city"),
                reader.GetInt32("max_capacity"),
                reader.GetInt32("price_per_night")));
        }

        return results;
    }
}