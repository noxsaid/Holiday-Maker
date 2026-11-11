USE holiday_maker;

-- Search for rooms matching City, Capacity, and non-overlapping Dates
SELECT r.id, r.name AS room_name, a.name AS accommodation_name, ci.name AS city, r.max_capacity, r.price_per_night
FROM rooms r
JOIN accommodations a ON r.accommodation_id = a.id
JOIN cities ci ON a.city_id = ci.id
WHERE ci.name = 'Malmö'
  AND r.max_capacity >= 2
  AND r.id NOT IN (
      SELECT room_id FROM bookings
      WHERE booking_start < '2026-12-05' AND booking_end > '2026-12-01'
  );