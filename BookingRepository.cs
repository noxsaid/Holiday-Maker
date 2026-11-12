using System.ComponentModel.Design;
using System.Security;
using MySqlConnector;

public class BookingRepositry
{
    private readonly string _connectionString;
    public BookingRepositry(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<long> CreatBookingAsync(int customerId, int roomId, string checkIn, string checkOut, int guests)
    {
        Validatedates(checkIn, checkOut);

        using (MySqlConnection connection = new MySqlConnection(_connectionString))
        {
            await connection.OpenAsync();

            await VerifyRoomCapacityAsync(connection, roomId, guests);
            await CheckOverlapAsync(connection, roomId, checkIn, checkOut);

            return await InsertBookingAsync(connection, customerId, roomId, checkIn, checkOut, guests);
        }
    }

    private void Validatedates(string checkIn, string checkOut)
    {
        if (!DateTime.TryParse(checkIn, out DateTime startDate) ||
        !DateTime.TryParse(checkIn, out DateTime endDate) ||
        endDate <= startDate)
        {
            throw new ArgumentException("Utcheckningsdatumet måste vara efter incheckningsdatumet.");
        }
    }

    private async Task VerifyRoomCapacityAsync(MySqlConnection connection, int roomid, int guests)
    {
        string capacityQuery = "SELECT max_capacity FROM rooms WHERE id = @roomId;";
        using (MySqlCommand capacityCommand = new MySqlCommand(capacityQuery, connection))
        {
            capacityCommand.Parameters.AddWithValue("@roomId", roomid);

            object? capacityResult = await capacityCommand.ExecuteScalarAsync();
            if (capacityResult == null)
            {
                throw new InvalidOperationException("Rummet hittades inte.");
            }

            int maxCapacity = (int)capacityResult;
            if (guests > maxCapacity)
            {
                throw new InvalidOperationException($"Antalet gäster ({guests}) överskrider rummets maxkapacitet ({maxCapacity}).");
            }
        }
    }

    private async Task CheckOverlapAsync(MySqlConnection connection, int roomId, string checkIn, string checkOut)
    {
        string checkQuery = @"
        SELECT COUNT(*) 
        FROM bookings
        WHERE room_id = @roomId
          AND booking_start < @checkOut
          AND booking_end < @checkIn;";
        
        using (MySqlCommand checkCommand = new MySqlCommand(checkQuery, connection))
        {
            checkCommand.Parameters.AddWithValue("@roomId", roomId);
            checkCommand.Parameters.AddWithValue("@checkIn", checkIn);
            checkCommand.Parameters.AddWithValue("@checkOut", checkOut);

            object? overlapResults = await checkCommand.ExecuteScalarAsync();
            if (overlapResults == null)
            {
                throw new InvalidOperationException("Kunde inte verifiera tillgänglighet.");
            }

            long onverlappingCount = (long)overlapResults;
            if (onverlappingCount > 0)
            {
                throw new InvalidOperationException("Rummet är inte längre tillgängligt för de valda datumen.");
            }
        }
    }

    private async Task<long> InsertBookingAsync(MySqlConnection connection, int customerId, int roomId, string checkIn, string checkOut, int guests)
    {
        string insertQuery = @"
        INSERT INTO bookings (customer_id, room_id, booking_start, booking_end, guest_count)
        VALUES (@customerId, @roomId, @checkIn, @checkOut, @guests);";

        using (MySqlCommand insertCommand = new MySqlCommand(insertQuery, connection))
        {
            insertCommand.Parameters.AddWithValue("@customerId", customerId);
            insertCommand.Parameters.AddWithValue("@roomId", roomId);
            insertCommand.Parameters.AddWithValue("@checkIn", checkIn);
            insertCommand.Parameters.AddWithValue("@checkOut", checkOut);
            insertCommand.Parameters.AddWithValue("@guests", guests);

            await insertCommand.ExecuteNonQueryAsync();

            return insertCommand.LastInsertedId;
        }
    }
}