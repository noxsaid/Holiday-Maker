namespace Holiday_Maker;

public record RoomSearchResults(
    int Id,
    string RoomName,
    string AccommodationName,
    string City,
    int maxCapacity,
    decimal PricePerNight
);