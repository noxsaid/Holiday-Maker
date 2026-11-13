using System;
using Holiday_Maker;
using MySqlConnector;

class Program
{
    static async Task Main(string[] args)
    {
        await using MySqlConnection connection = Database.GetConnection();
        await connection.OpenAsync();

        await RunBookingTestAsync(connection);
    }

    private static async Task ExecuteTestAsync(string testname, Func<Task> testAction)
    {
        Console.WriteLine($"\nkör {testname}...");
        try
        {
            await testAction();
        }
        catch (ArgumentException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"    -> Fångat ArgumentException: {ex.Message}");
            Console.ResetColor();
        }
        catch (InvalidOperationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"    -> Fångat InvalidOperationException (Vänta för affärsregel): {ex.Message}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"    -> Oväntat fel: {ex.Message}");
            Console.ResetColor();
        }
    }

    public static async Task RunBookingTestAsync(MySqlConnection connection)
    {
        BookingRepository repo = new BookingRepository(connection);

        await ExecuteTestAsync("Test 1: Valid Booking", async () =>
        {
            long id = await repo.CreateBookingAsync(1, 7, "2026-12-01", "2026-12-05", 2);
            Console.WriteLine($"    -> Resultat: Lyckades! Nytt bokningnummer: {id}");
        });

        await ExecuteTestAsync("Test 2: Overlap Booking", async() =>
        {
            long id = await repo.CreateBookingAsync(1, 7, "2026-12-03", "2026-12-07", 2);
            Console.WriteLine($"    -> Resultat: Lyckades! Nytt bokningnummer: {id}");
        });

        await ExecuteTestAsync("Test 3: Same-day handover", async () => 
        {
            long id = await repo.CreateBookingAsync(3, 7, "2026-12-05", "2026-12-06", 2);
            Console.WriteLine($"   -> Resultat: Lyckades! Nytt bokningsnummer: {id}");
        });

        await ExecuteTestAsync("Test 4: Too many guests", async () => 
        {
            long id = await repo.CreateBookingAsync(1, 7, "2026-12-10", "2026-12-12", 5);
            Console.WriteLine($"   -> Resultat: Lyckades! Nytt bokningsnummer: {id}");
        });

        await ExecuteTestAsync("Test 5: Bad dates", async () => {
            long id = await repo.CreateBookingAsync(1, 7, "2026-12-10", "2026-12-08", 2);
            Console.WriteLine($"   -> Resultat: Lyckades (Borde misslyckats! ID: {id})");
        });

        List<CustomerBookingRecord> customerBookings = await repo.GetCustomerBookingsAsync(1);
        Console.WriteLine($"\nBokningar för kund 1 (antal: {customerBookings.Count});");
        foreach (CustomerBookingRecord b in customerBookings)
        {
            Console.WriteLine($"-> Bokning #{b.BookingId}: {b.AccommodationName} ({b.RoomName}), {b.StartDate:yyyy-MM-dd} till {b.EndDate:yyyy-MM-dd}, {b.Guests} gäster, Totalt: {b.TotalPrice} SEK");
        }
    }

    
}