using System;
using MySqlConnector;
using Holiday_Maker;

namespace Holiday_Maker;

public static class ConsoleInput
{
    public static string ReadText(string prompt)
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

    public static int ReadInt(string prompt)
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

    public static bool AskYesNo(string prompt)
    {
        Console.Write(prompt);
        string? answer = Console.ReadLine();
        return answer != null && answer.Trim().ToLower() == "j";
    }

    public static int? ReadOptionalInt(string prompt)
    {
        while (true)
        {
            Console.Write(prompt);
            string? text = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(text))
            {
                return null;
            }
            if (int.TryParse(text, out int value) && value >= 0)
            {
                return value;
            }
            Console.WriteLine("Ange ett positivt heltal eller lämna tomt.");
        }
    }
}