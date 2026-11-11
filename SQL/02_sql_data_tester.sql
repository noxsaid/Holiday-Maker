USE holiday_maker;

SET FOREIGN_KEY_CHECKS = 0;
TRUNCATE TABLE bookings;
TRUNCATE TABLE rooms;
TRUNCATE TABLE accommodations;
TRUNCATE TABLE customers;
TRUNCATE TABLE cities;
SET FOREIGN_KEY_CHECKS = 1;

-- 1. Cities
INSERT INTO cities (name) VALUES 
('Malmö'), 
('Stockholm');

-- 2. Accommodations
INSERT INTO accommodations (name, city_id, has_pool, has_restaurant, distance_to_beach, distance_to_center, rating) VALUES 
('Malmö City Hotel', 1, TRUE, TRUE, 500, 100, 4),
('Beach Resort Malmö', 1, TRUE, FALSE, 50, 2000, 5),
('Stockholm Grand Hotel', 2, FALSE, TRUE, 1500, 50, 5),
('Stockholm Budget Inn', 2, FALSE, FALSE, 2000, 1000, 2),
('Coastal Pensionat Malmö', 1, FALSE, TRUE, 200, 800, 3);

-- 3. Rooms 
INSERT INTO rooms (name, accommodation_id, max_capacity, price_per_night) VALUES 
-- Accommodation 1 (Malmö City Hotel)
('101', 1, 2, 800), ('102', 1, 2, 850), ('201', 1, 4, 1400), ('202', 1, 1, 600),
-- Accommodation 2 (Beach Resort Malmö)
('A1', 2, 2, 1200), ('A2', 2, 4, 2000), ('B1', 2, 2, 1300), ('B2', 2, 4, 2500),
-- Accommodation 3 (Stockholm Grand Hotel)
('301', 3, 2, 1800), ('302', 3, 2, 1900), ('401', 3, 4, 3200), ('402', 3, 1, 1200),
-- Accommodation 4 (Stockholm Budget Inn)
('1', 4, 2, 500), ('2', 4, 2, 500), ('3', 4, 4, 900), ('4', 4, 1, 400),
-- Accommodation 5 (Coastal Pensionat Malmö)
('21', 5, 2, 900), ('22', 5, 2, 950), ('23', 5, 4, 1600), ('24', 5, 1, 700);

-- 4. Customers
INSERT INTO customers (name, telnr, email) VALUES 
('Alex Strand', '0701234567', 'alex@example.com'),
('Anna Svensson', '0702345678', 'anna@example.com'),
('Erik Lund', '0703456789', 'erik@example.com'),
('Sara Andersson', '0704567890', 'sara@example.com'),
('Mohammed Ali', '0705678901', 'mohammed@example.com');

-- 5. Bookings
INSERT INTO bookings (customer_id, room_id, booking_start, booking_end, guest_count) VALUES 
(1, 1, '2026-12-01', '2026-12-05', 2),
(2, 2, '2026-12-01', '2026-12-04', 2),
(3, 3, '2026-12-02', '2026-12-06', 4),
(4, 4, '2026-12-01', '2026-12-03', 1),
(5, 5, '2026-12-05', '2026-12-10', 2),
(1, 6, '2026-12-04', '2026-12-08', 3),
(2, 7, '2026-12-06', '2026-12-11', 2),
(3, 8, '2026-12-03', '2026-12-07', 4),
(4, 9, '2026-12-02', '2026-12-05', 2),
(5, 10, '2026-12-08', '2026-12-12', 2),
(1, 11, '2026-12-10', '2026-12-15', 4),
(2, 12, '2026-12-09', '2026-12-14', 1),
(3, 13, '2026-12-01', '2026-12-04', 2),
(4, 14, '2026-12-05', '2026-12-09', 2),
(5, 15, '2026-12-06', '2026-12-10', 4),
(1, 16, '2026-12-03', '2026-12-08', 1),
(2, 17, '2026-12-10', '2026-12-13', 2),
(3, 18, '2026-12-11', '2026-12-15', 2),
(4, 19, '2026-12-12', '2026-12-18', 4),
(5, 20, '2026-12-02', '2026-12-05', 1),
(1, 2, '2026-12-10', '2026-12-15', 2),
(2, 4, '2026-12-06', '2026-12-09', 1),
(3, 6, '2026-12-12', '2026-12-16', 3),
(4, 8, '2026-12-10', '2026-12-14', 4),
(5, 10, '2026-12-14', '2026-12-20', 2);