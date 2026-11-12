using MySqlConnector;

namespace Holiday_Maker
{
    public record CustomerBookingRecord(
        long BookingId,
        string AccommodationName,
        string RoomName,
        DateTime StartDate,
        DateTime EndDate,
        int Guests,
        long Nights,
        decimal TotalPrice
    );
}