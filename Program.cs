using System;
using System.Data;
using System.Diagnostics.CodeAnalysis;
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
        long createdBookingId = 0;

        await ExecuteTestAsync("Test 1: Valid Booking", async () =>
        {
            createdBookingId = await repo.CreateBookingAsync(1, 7, "2027-01-01", "2027-01-05", 2);
            Console.WriteLine($"    -> Resultat: Lyckades! Nytt bokningnummer: {createdBookingId}");
        });

        await ExecuteTestAsync("Test 2: Overlap Booking", async() =>
        {
            long id = await repo.CreateBookingAsync(1, 7, "2027-01-03", "2027-01-07", 2);
            Console.WriteLine($"    -> Resultat: Lyckades! Nytt bokningnummer: {id}");
        });

        await ExecuteTestAsync("Test 3: Same-day handover", async () => 
        {
            long id = await repo.CreateBookingAsync(3, 7, "2027-01-05", "2027-01-06", 2);
            Console.WriteLine($"   -> Resultat: Lyckades! Nytt bokningsnummer: {id}");
        });

        await ExecuteTestAsync("Test 4: Too many guests", async () => 
        {
            long id = await repo.CreateBookingAsync(1, 7, "2027-01-10", "2027-01-12", 5);
            Console.WriteLine($"   -> Resultat: Lyckades! Nytt bokningsnummer: {id}");
        });

        await ExecuteTestAsync("Test 5: Bad dates", async () => {
            long id = await repo.CreateBookingAsync(1, 7, "2027-01-10", "2027-01-08", 2);
            Console.WriteLine($"   -> Resultat: Lyckades (Borde misslyckats! ID: {id})");
        });

        List<CustomerBookingRecord> customerBookings = await repo.GetCustomerBookingsAsync(1);
        Console.WriteLine($"\nBokningar för kund 1 (antal: {customerBookings.Count})");
        foreach (CustomerBookingRecord b in customerBookings)
        {
            Console.WriteLine($"-> Bokning #{b.BookingId}: {b.AccommodationName} ({b.RoomName}), {b.StartDate:yyyy-MM-dd} till {b.EndDate:yyyy-MM-dd}, {b.Guests} gäster, Totalt: {b.TotalPrice} SEK");
        }

        // Run update tests using the actively created booking ID before canceling it
        if (createdBookingId > 0)
        {
            int targetId = Convert.ToInt32(createdBookingId);

            try
            {
                bool updateSuccess = await repo.UpdateBookingGuestsAsync(targetId, 1);
                if (updateSuccess)
                {
                    Console.WriteLine("\nTest Update Guests: Antalet gäster har uppdaterats till 1.");
                }
                else
                {
                    Console.WriteLine("\nTest Update Guests: Kunde inte hitta bokningen.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\nTest Update Guests - Oväntat fel: {ex.Message}");
            }

            try
            {
                await repo.UpdateBookingGuestsAsync(targetId, 5);
                Console.WriteLine("\nTest Update Guests: Borde ha misslyckats pga kapacitet!");
            }
            catch (InvalidOperationException ex)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"    -> Fångat kapacitetsfel som förväntat: {ex.Message}");
                Console.ResetColor();
            }

            // Finally, cancel the booking to free up the room
            bool success = await repo.CancelBookingAsync(targetId);
            if (success)
            {
                Console.WriteLine("\nTest Cancel: Bokningen har avbokats framgångsrikt och rummet är frigjort.");
            }
            else
            {
                Console.WriteLine("\nTest Cancel: Kunde inte hitta bokningen.");
            }
        }

        CustomerRepository customerRepo = new CustomerRepository(connection);

        try
        {
            string uniqueEmail = $"testperson_{DateTime.Now.Ticks}@email.com";
            long newCustomerId = await customerRepo.RegisterCustomerAsync("Test Person", "0709998877", uniqueEmail);
            Console.WriteLine($"\nTest Register Customer: Lyckades! Nytt kund-ID: {newCustomerId}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\nTest Register Customer - Fel : {ex.Message}");
        }

        try
        {
            await customerRepo.RegisterCustomerAsync("Duplicate Person", "0701112233", "alex@example.com");
            Console.WriteLine("\nTest Register Duplicate: Borde ha misslyckats!");
        }
        catch (InvalidOperationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"    -> Fångat e-post duplikatfel som förväntat: {ex.Message}");
            Console.ResetColor();
        }
    }
}