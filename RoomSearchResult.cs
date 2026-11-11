namespace Holiday_Maker;

public record RoomSearchResults(
    int Id,
    string RoomName,
    string AccommodationName,
    string City,
    int MaxCapacity,
    decimal PricePerNight
);