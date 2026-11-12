using System;
using Holiday_Maker;
using MySqlConnector;

class Program
{
    static async Task Main(string[] args)
    {
        await using MySqlConnection connection = Database.GetConnection();
        await connection.OpenAsync();

        RoomRepository repository = new RoomRepository(connection);

        List<RoomSearchResults> results = await repository.SearchAvailableRoomsAsync(
            "Malmö",
            "2026-12-01",
            "2026-12-05",
            2
        );

        Console.WriteLine($"Found {results.Count} available rooms:\n");
        foreach (RoomSearchResults room in results)
        {
            Console.WriteLine($"[ID: {room.Id}] {room.RoomName} at {room.AccommodationName} ({room.City}) - Capacity: {room.maxCapacity} - {room.PricePerNight} SEK/night");
        }
    }
}