using System;
using Holiday_Maker;
using MySqlConnector;

namespace Holiday_Maker;

public static class RoomSearchMenu
{
    public static async Task SearchRoomsMenuAsync(MySqlConnection connection)
    {
        string city = ConsoleInput.ReadText("Ange stad att söka i: ");
        string checkInStr = ConsoleInput.ReadText("Ange incheckningsdatum (YYYY-MM-DD): ");
        string checkOutStr = ConsoleInput.ReadText("Ange utcheckningsdatum (YYYY-MM-DD): ");
        int guests = ConsoleInput.ReadInt("Ange antal gäster: ");

        Console.WriteLine("\n--- Valfria filter (tryck Enter för att hoppa över) ---");
        bool requirePool = ConsoleInput.AskYesNo("Måste ha pool (j/n): ");
        bool requireRestaurant = ConsoleInput.AskYesNo("Måste ha restaurang (j/n): ");
        int? maxBeach = ConsoleInput.ReadOptionalInt("Max avstånd till strand i meter (Enter = ingen gräns): ");
        int? maxCenter = ConsoleInput.ReadOptionalInt("Max avstånd till centrum i meter (Enter = ingen gräns): ");

        Console.WriteLine("\nSortering:");
        Console.WriteLine("1. Pris, lägst först");
        Console.WriteLine("2. Omdöme, högst först");
        Console.WriteLine("Enter. Ingen sortering");
        Console.Write("Välj: ");
        int sortBy = 0;
        string? sortInput = Console.ReadLine();
        if (sortInput == "1")
        {
            sortBy = 1;
        }
        else if (sortInput == "2")
        {
            sortBy = 2;
        }

        try
        {
            if (!DateTime.TryParse(checkInStr, out DateTime startDate) ||
                !DateTime.TryParse(checkOutStr, out DateTime endDate) ||
                endDate <= startDate)
            {
                throw new ArgumentException("Utcheckningsdatumet måste vara efter incheckningsdatumet.");
            }

            string checkIn = startDate.ToString("yyyy-MM-dd");
            string checkOut = endDate.ToString("yyyy-MM-dd");

            RoomRepository roomRepo = new RoomRepository(connection);
            List<RoomSearchResults> rooms = await roomRepo.SearchAvailableRoomsAsync(
                city, checkIn, checkOut, guests,
                requirePool, requireRestaurant, maxBeach, maxCenter, sortBy);

            if (rooms.Count == 0)
            {
                Console.WriteLine("\nInga tillgängliga rum hittades för de angivna valen.");
                return;
            }

            Console.WriteLine($"\nTillgängliga rum ({rooms.Count} st):");
            foreach (RoomSearchResults r in rooms)
            {
                Console.WriteLine($"  ID: {r.Id} | {r.AccommodationName}, {r.City} | Rum {r.RoomName} | Max gäster: {r.maxCapacity} | Pris/natt: {r.PricePerNight} SEK");
            }
        }
        catch (ArgumentException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nOgiltig indata: {ex.Message}");
            Console.ResetColor();
        }
        catch (MySqlException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nDatabasfel: {ex.Message}");
            Console.ResetColor();
        }
    }
}