using System;
using System.Runtime.CompilerServices;
using MySqlConnector;
using Holiday_Maker;

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
                            await CustomerMenu.RegisterCustomerMenuAsync(connection);
                            break;

                        case 2:
                            await RoomSearchMenu.SearchRoomsMenuAsync(connection);
                            break;

                        case 3:
                            await BookingMenu.CreateBookingMenuAsync(connection);
                            break;

                        case 4:
                            await BookingMenu.ShowCustomerBookingsMenuAsync(connection);
                            break;

                        case 5:
                            await BookingMenu.ChangeGuestsMenuAsync(connection);
                            break;

                        case 6:
                            await BookingMenu.CancelBookingMenuAsync(connection);
                            break;

                        case 7:
                            await TestRunner.RunBookingTestAsync(connection);
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
}