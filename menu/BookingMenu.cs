using System;
using MySqlConnector;
using Holiday_Maker;

namespace Holiday_Maker;

public static class BookingMenu
{
    public static async Task ShowCustomerBookingsMenuAsync(MySqlConnection connection)
    {
        int customerId = await CustomerMenu.SelectCustomerAsync(connection);
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

    public static async Task CreateBookingMenuAsync(MySqlConnection connection)
    {
        int customerId = await CustomerMenu.SelectCustomerAsync(connection);
        if (customerId == 0)
        {
            return;
        }

        string city = ConsoleInput.ReadText("Ange stad att söka i: ");
        string checkInStr = ConsoleInput.ReadText("Ange incheckningsdatum (YYYY-MM-DD): ");
        string checkOutStr = ConsoleInput.ReadText("Ange utcheckningsdatum (YYYY-MM-DD): ");
        int guests = ConsoleInput.ReadInt("Ange antal gäster: ");

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
            List<RoomSearchResults> rooms = await roomRepo.SearchAvailableRoomsAsync(city, checkIn, checkOut, guests);

            if (rooms.Count == 0)
            {
                Console.WriteLine("\nInga tillgängliga rum hittades för de angivna valen.");
                return;
            }

            Console.WriteLine($"\nTillgängliga rum i {city} ({rooms.Count} st):");
            foreach (RoomSearchResults r in rooms)
            {
                Console.WriteLine($"  ID: {r.Id} | {r.AccommodationName}, {r.City} | Rum {r.RoomName} | Max gäster: {r.maxCapacity} | Pris/natt: {r.PricePerNight} SEK");
            }

            RoomSearchResults? selectedRoom = null;
            while (selectedRoom == null)
            {
                int roomId = ConsoleInput.ReadInt("\nAnge rum-ID att boka: ");

                foreach (RoomSearchResults r in rooms)
                {
                    if (r.Id == roomId)
                    {
                        selectedRoom = r;
                        break;
                    }
                }

                if (selectedRoom == null)
                {
                    Console.WriteLine("Det finns inget rum med det ID:t i sökresultatet.");
                }
            }

            BookingRepository bookingRepo = new BookingRepository(connection);
            long bookingId = await bookingRepo.CreateBookingAsync(customerId, selectedRoom.Id, checkIn, checkOut, guests);

            int nights = (int)(endDate - startDate).TotalDays;
            decimal totalPrice = nights * selectedRoom.PricePerNight;
            string nightText = nights == 1 ? "natt" : "nätter";

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nBokning genomförd!");
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
        int customerId = await CustomerMenu.SelectCustomerAsync(connection);
        if (customerId == 0)
        {
            return 0;
        }

        BookingRepository repo = new BookingRepository(connection);
        List<CustomerBookingRecord> bookings = await repo.GetCustomerBookingsAsync(customerId);

        if (bookings.Count == 0)
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
            int bookingId = ConsoleInput.ReadInt("Ange bokningsnummer (0 för att avbryta): ");
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

    public static async Task ChangeGuestsMenuAsync(MySqlConnection connection)
    {
        int bookingId = await SelectBookingAsync(connection);
        if (bookingId == 0)
        {
            return;
        }

        int newGuests = ConsoleInput.ReadInt("Nytt antal personer: ");

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

    public static async Task CancelBookingMenuAsync(MySqlConnection connection)
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
}