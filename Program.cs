using System;
using System.Runtime.CompilerServices;
using Holiday_Maker;
using MySqlConnector;

class Program
{
    static async Task Main(string[] args)
    {
        MySqlConnection connection;
        try
        {
            connection = Database.GetConnection();
            await connection.OpenAsync();
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Kunde inte ansluta till databasen: {ex.Message}");
            return;
        }

        await using (connection)
        {
        bool running = true;
        while (running)
        {
            Console.WriteLine("\n=== Holiday Maker ===");
            Console.WriteLine("1. Registrera kund");
            Console.WriteLine("2. Sök lediga rum");
            Console.WriteLine("3. Skapa bokning");
            Console.WriteLine("4. Visa kunds bokningar");
            Console.WriteLine("5. Ändra antal personer");
            Console.WriteLine("6. Avboka");
            Console.WriteLine("7. Kör tester");
            Console.WriteLine("0. Avsluta");
            Console.Write("Val: ");

            string? input = Console.ReadLine();

            if (input == null)
            {
                running = false;
                continue;
            }

            if (!int.TryParse(input, out int choice))
            {
                Console.WriteLine("Ogiltigt val, skriv en siffra.");
                continue;
            }

            try
            {
            switch (choice)
            {
                case 1:
                    await RegisterCustomerMenuAsync(connection);
                    break;

                case 2:
                    await SearchRoomsMenuAsync(connection);
                    break;

                case 3:
                    await CreateBookingMenuAsync(connection);
                    break;

                case 4:
                    await ShowCustomerBookingsMenuAsync(connection);
                    break;

                case 5:
                    await ChangeGuestsMenuAsync(connection);
                    break;

                case 6:
                    await CancelBookingMenuAsync(connection);
                    break;
                
                case 7:
                    await RunBookingTestAsync(connection);
                    break;

                case 0:
                    running = false;
                    break;

                default:
                    Console.WriteLine("Ogiltigt val.");
                    break;
            }
            }
            catch (MySqlException ex)
            {
                Console.WriteLine($"Databasfel: {ex.Message}");
            }
        }
        }
    }

    private static async Task<int> SelectCustomerAsync(MySqlConnection connection)
    {
        CustomerRepository customerRepo = new CustomerRepository(connection);
        List<CustomerRecord> customers = await customerRepo.GetAllCustomersAsync();

        if (customers.Count == 0)
        {
            Console.WriteLine("Det finns inga kunder. Registrera en kund först.");
            return 0;
        }

        Console.WriteLine("\nKunder:");
        foreach (CustomerRecord c in customers)
        {
            Console.WriteLine($"  {c.Id}: {c.Name} ({c.Email}, {c.Telnr})");
        }

        while (true)
        {
            int customerId = ReadInt("Ange kund-ID: ");
            foreach (CustomerRecord c in customers)
            {
                if (c.Id == customerId)
                {
                    return customerId;
                }
            }
            Console.WriteLine("Det finns ingen kund med det ID:t.");
        }
    }

    private static string ReadText(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? text = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(text))
            {
                return text.Trim();
            }
            Console.WriteLine("Fältet får inte vara tomt.");
        }
    }

    private static int ReadInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            if (int.TryParse(Console.ReadLine(), out int value))
            {
                return value;
            }
            Console.WriteLine("Ogiltigt tal, försök igen.");
        }
    }

    private static async Task RegisterCustomerMenuAsync(MySqlConnection connection)
    {
        CustomerRepository customerRepo = new CustomerRepository(connection);

        string name = ReadText("Namn: ");
        string telnr = ReadText("Telefonnummer: ");
        string email = ReadText("E-post: ");

        try
        {
            long customerId = await customerRepo.RegisterCustomerAsync(name, telnr, email);
            Console.WriteLine($"Kunden är registrerad. Kund-ID: {customerId}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    private static async Task ShowCustomerBookingsMenuAsync(MySqlConnection connection)
    {
        int customerId = await SelectCustomerAsync(connection);
        if (customerId == 0)
        {
            return;
        }

        BookingRepository repo = new BookingRepository(connection);
        List<CustomerBookingRecord> bookings = await repo.GetCustomerBookingsAsync(customerId);

        if (bookings.Count == 0)
        {
            Console.WriteLine("\nKunden har inga bokningar.");
            return;
        }

        Console.WriteLine($"\nBokningar för kund {customerId} (antal: {bookings.Count}):");
        foreach (CustomerBookingRecord b in bookings)
        {
            Console.WriteLine($"-> Bokning #{b.BookingId}: {b.AccommodationName} ({b.RoomName}), {b.StartDate:yyyy-MM-dd} till {b.EndDate:yyyy-MM-dd}, {b.Guests} gäster, Totalt: {b.TotalPrice} SEK");
        }
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
        long handoverBookingId = 0;

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
            handoverBookingId = await repo.CreateBookingAsync(3, 7, "2027-01-05", "2027-01-06", 2);
            Console.WriteLine($"   -> Resultat: Lyckades! Nytt bokningsnummer: {handoverBookingId}");
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

        if (handoverBookingId > 0)
        {
            await repo.CancelBookingAsync(Convert.ToInt32(handoverBookingId));
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

    private static async Task CreateBookingMenuAsync(MySqlConnection connection)
    {
        int customerId = await SelectCustomerAsync(connection);
        if (customerId == 0)
        {
            return;
        }

        string city = ReadText("Ange stad att söka i: ");
        string checkIn = ReadText("Ange incheckningsdatum (YYYY-MM-DD): ");
        string checkOut = ReadText("Ange utcheckningsdatum (YYYY-MM-DD): ");
        int guests = ReadInt("Ange antal gäster: ");

        try
        {
            if (!DateTime.TryParse(checkIn, out DateTime startDate) ||
                !DateTime.TryParse(checkOut, out DateTime endDate) ||
                endDate <= startDate)
            {
                throw new ArgumentException("Utcheckningsdatumet måste vara efter incheckningsdatumet.");
            }

            RoomRepository roomRepo = new RoomRepository(connection);
            List<RoomSearchResults> rooms = await roomRepo.SearchAvailableRoomsAsync(city, checkIn, checkOut, guests);

            if (rooms.Count == 0)
            {
                Console.WriteLine("\nInga tillgängliga rum hittades för de angivna valen.");
                return;
            }

            Console.WriteLine($"\nTillgängliga rum i {city} ({rooms.Count} st):");
            foreach (RoomSearchResults r in rooms)
            {
                Console.WriteLine($"  ID: {r.Id} | {r.AccommodationName} - {r.RoomName} | Max gäster: {r.maxCapacity} | Pris/natt: {r.PricePerNight} SEK");
            }

            int roomId = 0;
            RoomSearchResults? selectedRoom = null;

            while (true)
            {
                roomId = ReadInt("\nAnge rum-ID att boka: ");

                foreach (RoomSearchResults r in rooms)
                {
                    if (r.Id == roomId)
                    {
                        selectedRoom = r;
                        break;
                    }
                }

                if (selectedRoom != null)
                {
                    break;
                }

                Console.WriteLine("Det finns inget rum med det ID:t i sökresultatet.");
            }

            BookingRepository bookingRepo = new BookingRepository(connection);
        long bookingId = await bookingRepo.CreateBookingAsync(customerId, roomId, checkIn, checkOut, guests);

        int nights = (int)(endDate - startDate).TotalDays;
        decimal totalPrice = nights * selectedRoom.PricePerNight;
        string nightText = nights == 1 ? "natt" : "nätter";

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\nBokning genomförd!");
        Console.WriteLine($"Bokningsnummer: #{bookingId}");
        Console.WriteLine($"Totalt pris: {totalPrice:F2} SEK ({nights} {nightText} à {selectedRoom.PricePerNight} SEK)");
        Console.ResetColor();
    }
    catch (ArgumentException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nOgiltig indata: {ex.Message}");
            Console.ResetColor();
        }
        catch (InvalidOperationException ex)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\nKunde inte genomföra bokningen: {ex.Message}");
            Console.ResetColor();
        }
        catch (MySqlException ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\nDatabasfel: {ex.Message}");
            Console.ResetColor();
        }
    }

    private static async Task<int> SelectBookingAsync(MySqlConnection connection)
    {
        int customerId = await SelectCustomerAsync(connection);
        if (customerId == 0)
        {
            return 0;
        }

        BookingRepository repo = new BookingRepository(connection);
        List<CustomerBookingRecord> bookings = await repo.GetCustomerBookingsAsync(customerId);

        if(bookings.Count == 0)
        {
            Console.WriteLine("\nKunden har inga bokningar.");
            return 0;
        }

        Console.WriteLine($"\nBokningar för kund {customerId}:");
        foreach (CustomerBookingRecord b in bookings)
        {
            Console.WriteLine($"  #{b.BookingId}: {b.AccommodationName} ({b.RoomName}), {b.StartDate:yyyy-MM-dd} till {b.EndDate:yyyy-MM-dd}, {b.Guests} gäster, Totalt: {b.TotalPrice} SEK");
        }

        while (true)
        {
            int bookingId = ReadInt("Ange bokningsnummer (0 för att avbryta): ");
            if (bookingId == 0)
            {
                return 0;
            }

            foreach (CustomerBookingRecord b in bookings)
            {
                if (b.BookingId == bookingId)
                {
                    return bookingId;
                }
            }
            Console.WriteLine("Kunden har ingen bokning med det numret.");
        }
    }

    private static async Task ChangeGuestsMenuAsync(MySqlConnection connection)
    {
        int bookingId = await SelectBookingAsync(connection);
        if (bookingId == 0)
        {
            return;
        }

        int newGuests = ReadInt("Nytt antal personer: ");

        BookingRepository repo = new BookingRepository(connection);
        try
        {
            bool updated = await repo.UpdateBookingGuestsAsync(bookingId, newGuests);
            if (updated)
            {
                Console.WriteLine($"Bokning #{bookingId} har nu {newGuests} personer.");
            }
            else
            {
                Console.WriteLine("Kunde inte hitta bokningen.");
            }
        }
        catch (ArgumentException ex)
        {
            Console.WriteLine($"Ogiltig indata: {ex.Message}");
        }
        catch (InvalidOperationException ex)
        {
            Console.WriteLine($"Kunde inte ändra: {ex.Message}");
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Databasfel: {ex.Message}");
        }
    }

    private static async Task CancelBookingMenuAsync(MySqlConnection connection)
    {
        int bookingId = await SelectBookingAsync(connection);
        if (bookingId == 0)
        {
            return;
        }

        BookingRepository repo = new BookingRepository(connection);
        try
        {
            bool cancelled = await repo.CancelBookingAsync(bookingId);
            if (cancelled)
            {
                Console.WriteLine($"Bokning #{bookingId} är avbokad och rummet är ledigt igen.");
            }
            else
            {
                Console.WriteLine("Kunde inte hitta bokningen.");
            }
        }
        catch (MySqlException ex)
        {
            Console.WriteLine($"Databasfel: {ex.Message}");
        }
    }

    private static async Task SearchRoomsMenuAsync(MySqlConnection connection)
    {
        string city = ReadText("Ange stad att söka I: ");
        string checkInStr = ReadText("Ange incheckningsdatum (YYYY-MM-DD): ");
        string checkOutStr = ReadText("Ange utcheckningsdatum (YYYY-MM-DD): ");
        int guests = ReadInt("Ange antal gäster: ");

        try
        {
            if(!DateTime.TryParse(checkInStr, out DateTime startDate) ||
               !DateTime.TryParse(checkOutStr, out DateTime endDate)||
               endDate <= startDate)
            {
                throw new ArgumentException("Utcheckningsdatumet måste vara efter incheckningsdatumet.");
            }

            string checkIn = startDate.ToString("yyyy-MM-dd");
            string checkOut = endDate.ToString("yyyy-MM-dd");

            RoomRepository roomRepo = new RoomRepository(connection);
            List<RoomSearchResults> rooms = await roomRepo.SearchAvailableRoomsAsync(city, checkIn, checkOut, guests);

            if (rooms.Count == 0)
            {
                Console.WriteLine("\nInga tillgängliga rum hittades för de angivna valen.");
                return;
            }

            Console.WriteLine($"\nTillgängliga rum i {city} ({rooms.Count} st):");
            foreach (RoomSearchResults r in rooms)
            {
                Console.WriteLine($"  ID: {r.Id} | {r.AccommodationName} - {r.RoomName} | Max gäster: {r.maxCapacity} | Pris/natt: {r.PricePerNight} SEK");
            }

            Console.WriteLine("\nSortering:");
            Console.WriteLine("1. Standard");
            Console.WriteLine("2. Pris (lägst först)");
            Console.WriteLine("3. Pris (högst först)");
            int sortChoice = ReadInt("Välj sortering: ");

            if (sortChoice == 2)
            {
                rooms = rooms.OrderBy(r => r.PricePerNight).ToList();
            }   
            else if (sortChoice == 3)
            {
                rooms = rooms.OrderByDescending(r => r.PricePerNight).ToList();
            }

            Console.WriteLine($"\nTillgängliga rum i {city} ({rooms.Count} st):");
            foreach (RoomSearchResults r in rooms)
            {
                Console.WriteLine($"  ID: {r.Id} | {r.AccommodationName} - {r.RoomName} | Max gäster: {r.maxCapacity} | Pris/natt: {r.PricePerNight} SEK");
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

