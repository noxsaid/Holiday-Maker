using System;
using Holiday_Maker;
using MySqlConnector;

namespace Holiday_Maker;

public static class CustomerMenu
{
    public static async Task<int> SelectCustomerAsync(MySqlConnection connection)
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
            int customerId = ConsoleInput.ReadInt("Ange kund-ID: ");
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

    public static async Task RegisterCustomerMenuAsync(MySqlConnection connection)
    {
        CustomerRepository customerRepo = new CustomerRepository(connection);

        string name = ConsoleInput.ReadText("Namn: ");
        string telnr = ConsoleInput.ReadText("Telefonnummer: ");
        string email = ConsoleInput.ReadText("E-post: ");

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
}