using System;
using System.Threading.Tasks;
using MySqlConnector;

public class BookingRepositry
{
    private readonly MySqlConnection _connection;

    public BookingRepositry(MySqlConnection connection)
    {
        _connection = connection;
    }

    public async Task<long> CreatBookingAsync(int customerId, int roomId, string checkIn, string checkOut, int guests)
    {
        Validatedates(checkIn, checkOut);

        // Uses the shared connection directly without creating a new one
        await VerifyRoomCapacityAsync(_connection, roomId, guests);
        await CheckOverlapAsync(_connection, roomId, checkIn, checkOut);

        return await InsertBookingAsync(_connection, customerId, roomId, checkIn, checkOut, guests);
    }

    private void Validatedates(string checkIn, string checkOut)
    {
        if (!DateTime.TryParse(checkIn, out DateTime startDate) ||
            !DateTime.TryParse(checkOut, out DateTime endDate) ||
            endDate <= startDate)
        {
            throw new ArgumentException("Utcheckningsdatumet måste vara efter incheckningsdatumet.");
        }
    }

    private async Task VerifyRoomCapacityAsync(MySqlConnection connection, int roomId, int guests)
    {
        string capacityQuery = "SELECT max_capacity FROM rooms WHERE id = @roomId;";
        using (MySqlCommand capacityCommand = new MySqlCommand(capacityQuery, connection))
        {
            capacityCommand.Parameters.AddWithValue("@roomId", roomId);

            object? capacityResult = await capacityCommand.ExecuteScalarAsync();
            if (capacityResult == null)
            {
                throw new InvalidOperationException("Rummet hittades inte.");
            }

            int maxCapacity = Convert.ToInt32(capacityResult);
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
          AND booking_end > @checkIn;";
        
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

            long overlappingCount = Convert.ToInt64(overlapResults);
            if (overlappingCount > 0)
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