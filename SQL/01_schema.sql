DROP DATABASE IF EXISTS holiday_maker;
CREATE DATABASE holiday_maker;
USE holiday_maker;

CREATE TABLE cities
(
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(64) NOT NULL,
    UNIQUE (name)
);

CREATE TABLE accommodations
(
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(64) NOT NULL,
    city_id INT UNSIGNED NOT NULL,
    has_pool BOOLEAN NOT NULL,
    has_restaurant BOOLEAN NOT NULL,
    distance_to_beach INT UNSIGNED NOT NULL,
    distance_to_center INT UNSIGNED NOT NULL,
    rating TINYINT NOT NULL,
    CHECK (rating >= 1 AND rating <= 5),
    FOREIGN KEY (city_id) REFERENCES cities(id)
);

CREATE TABLE customers
(
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(64) NOT NULL,
    telnr VARCHAR(32) NOT NULL,
    email VARCHAR(128) NULL,
    UNIQUE (email)
);

CREATE TABLE rooms
(
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(64) NOT NULL,
    accommodation_id INT UNSIGNED NOT NULL,
    max_capacity INT UNSIGNED NOT NULL,
    price_per_night INT UNSIGNED NOT NULL,
    CHECK (max_capacity >= 1),
    CHECK (price_per_night > 0),
    UNIQUE (accommodation_id, name),
    FOREIGN KEY (accommodation_id) REFERENCES accommodations(id)
);

CREATE TABLE bookings
(
    id INT UNSIGNED NOT NULL AUTO_INCREMENT PRIMARY KEY,
    customer_id INT UNSIGNED NOT NULL,
    room_id INT UNSIGNED NOT NULL,
    booking_start DATE NOT NULL,
    booking_end DATE NOT NULL,
    guest_count INT UNSIGNED NOT NULL,
    CHECK (booking_end > booking_start),
    CHECK (guest_count >= 1),
    FOREIGN KEY (customer_id) REFERENCES customers(id),
    FOREIGN KEY (room_id) REFERENCES rooms(id)
);
