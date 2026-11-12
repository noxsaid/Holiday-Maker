using MySqlConnector;

namespace Holiday_Maker
{
    public record CustomerBookingRecord(
        long BookingId,
        string AccommodationName,
        string RoomName,
        string startDate,
        string endDate,
        int Guests,
        long Nights,
        decimal TotalPrice
    );
}