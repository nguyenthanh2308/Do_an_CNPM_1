CREATE DATABASE IF NOT EXISTS hotel_app
  DEFAULT CHARACTER SET utf8mb4
  DEFAULT COLLATE utf8mb4_unicode_ci;
USE hotel_app;

-- Đảm bảo engine & chế độ
SET NAMES utf8mb4;
SET sql_notes = 0;

-- 2.1 Users (tài khoản backoffice)
CREATE TABLE users (
  id                BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  username          VARCHAR(64)  NOT NULL,
  password_hash     VARCHAR(255) NOT NULL,
  role              ENUM('Admin','Manager','Receptionist','Housekeeping') NOT NULL,
  email             VARCHAR(128) NULL,
  created_at        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY uq_users_username (username),
  UNIQUE KEY uq_users_email (email)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 2.2 Hotels (giai đoạn này có thể 1 khách sạn, nhưng để mở rộng)
CREATE TABLE hotels (
  id          BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  name        VARCHAR(128) NOT NULL,
  address     VARCHAR(255) NULL,
  timezone    VARCHAR(64)  NOT NULL DEFAULT 'Asia/Ho_Chi_Minh',
  created_at  DATETIME     NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3.1 Amenities
CREATE TABLE amenities (
  id    BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  name  VARCHAR(64) NOT NULL,
  UNIQUE KEY uq_amenities_name (name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3.2 RoomTypes
CREATE TABLE room_types (
  id                BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  hotel_id          BIGINT UNSIGNED NOT NULL,
  name              VARCHAR(128) NOT NULL,
  capacity          TINYINT UNSIGNED NOT NULL DEFAULT 2,
  base_price        DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  description       TEXT NULL,
  default_image_url VARCHAR(255) NULL,
  created_at        DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_roomtypes_hotel
    FOREIGN KEY (hotel_id) REFERENCES hotels(id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  UNIQUE KEY uq_roomtypes_hotel_name (hotel_id, name)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3.3 Phân bổ tiện nghi cho loại phòng (N-N)
CREATE TABLE room_type_amenities (
  room_type_id  BIGINT UNSIGNED NOT NULL,
  amenity_id    BIGINT UNSIGNED NOT NULL,
  PRIMARY KEY (room_type_id, amenity_id),
  CONSTRAINT fk_rta_roomtype
    FOREIGN KEY (room_type_id) REFERENCES room_types(id)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_rta_amenity
    FOREIGN KEY (amenity_id) REFERENCES amenities(id)
    ON UPDATE CASCADE ON DELETE RESTRICT
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 3.4 Rooms
CREATE TABLE rooms (
  id           BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  hotel_id     BIGINT UNSIGNED NOT NULL,
  room_type_id BIGINT UNSIGNED NOT NULL,
  number       VARCHAR(16) NOT NULL,   -- số phòng
  floor        SMALLINT     NULL,
  status       ENUM('Vacant','Occupied','Cleaning','Maintenance') NOT NULL DEFAULT 'Vacant',
  created_at   DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_rooms_hotel
    FOREIGN KEY (hotel_id) REFERENCES hotels(id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_rooms_roomtype
    FOREIGN KEY (room_type_id) REFERENCES room_types(id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  UNIQUE KEY uq_rooms_hotel_number (hotel_id, number),
  KEY idx_rooms_status (status),
  KEY idx_rooms_roomtype (room_type_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4.1 RatePlans
CREATE TABLE rateplans (
  id                         BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  room_type_id               BIGINT UNSIGNED NOT NULL,
  name                       VARCHAR(128) NOT NULL,
  type                       ENUM('Flexible','NonRefundable') NOT NULL,
  free_cancel_until_hours    INT NULL,   -- chỉ áp dụng với Flexible
  start_date                 DATE NOT NULL,
  end_date                   DATE NOT NULL,
  price                      DECIMAL(12,2) NOT NULL,
  weekend_rule_json          JSON NULL,  -- quy tắc tăng giá cuối tuần / peak
  created_at                 DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_rateplans_roomtype
    FOREIGN KEY (room_type_id) REFERENCES room_types(id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  KEY idx_rateplans_roomtype (room_type_id),
  KEY idx_rateplans_daterange (start_date, end_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 4.2 Promotions
CREATE TABLE promotions (
  id               BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  code             VARCHAR(32) NOT NULL,
  type             ENUM('Percent','Amount') NOT NULL,
  value            DECIMAL(12,2) NOT NULL,
  start_date       DATE NOT NULL,
  end_date         DATE NOT NULL,
  conditions_json  JSON NULL,
  created_at       DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY uq_promotions_code (code),
  KEY idx_promotions_daterange (start_date, end_date)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5.1 Guests
CREATE TABLE guests (
  id         BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  full_name  VARCHAR(128) NOT NULL,
  email      VARCHAR(128) NULL,
  phone      VARCHAR(32)  NULL,
  id_number  VARCHAR(32)  NULL,  -- CMND/CCCD (nếu cần)
  created_at DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  UNIQUE KEY uq_guests_email (email),
  KEY idx_guests_phone (phone)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5.2 Bookings
CREATE TABLE bookings (
  id                       BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  hotel_id                 BIGINT UNSIGNED NOT NULL,
  guest_id                 BIGINT UNSIGNED NOT NULL,
  check_in_date            DATE NOT NULL,
  check_out_date           DATE NOT NULL,
  status                   ENUM('Pending','Confirmed','CheckedIn','CheckedOut','Cancelled') NOT NULL DEFAULT 'Pending',
  total_amount             DECIMAL(12,2) NOT NULL DEFAULT 0.00,
  payment_status           ENUM('Unpaid','Paid','Refunded','Failed') NOT NULL DEFAULT 'Unpaid',
  rateplan_snapshot_json   JSON NULL, -- lưu ảnh chụp điều kiện giá/hủy tại thời điểm đặt
  created_at               DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_bookings_hotel
    FOREIGN KEY (hotel_id) REFERENCES hotels(id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  CONSTRAINT fk_bookings_guest
    FOREIGN KEY (guest_id) REFERENCES guests(id)
    ON UPDATE CASCADE ON DELETE RESTRICT,
  -- Chỉ mục cho truy vấn tìm phòng theo khoảng ngày
  KEY idx_bookings_hotel_dates (hotel_id, check_in_date, check_out_date),
  KEY idx_bookings_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5.3 BookingRooms (mỗi dòng biểu diễn 1 phòng gắn với booking)
CREATE TABLE booking_rooms (
  id               BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  booking_id       BIGINT UNSIGNED NOT NULL,
  room_id          BIGINT UNSIGNED NULL, -- có thể NULL lúc chưa gán số phòng (trước check-in)
  price_per_night  DECIMAL(12,2) NOT NULL,
  nights           INT NOT NULL,
  CONSTRAINT fk_brooms_booking
    FOREIGN KEY (booking_id) REFERENCES bookings(id)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_brooms_room
    FOREIGN KEY (room_id) REFERENCES rooms(id)
    ON UPDATE CASCADE ON DELETE SET NULL,
  KEY idx_brooms_booking (booking_id),
  KEY idx_brooms_room (room_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5.4 Payments
CREATE TABLE payments (
  id          BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  booking_id  BIGINT UNSIGNED NOT NULL,
  method      ENUM('Mock','PayAtProperty') NOT NULL,
  amount      DECIMAL(12,2) NOT NULL,
  txn_code    VARCHAR(64) NULL,
  status      ENUM('Unpaid','Paid','Refunded','Failed') NOT NULL DEFAULT 'Unpaid',
  created_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_payments_booking
    FOREIGN KEY (booking_id) REFERENCES bookings(id)
    ON UPDATE CASCADE ON DELETE CASCADE,
  KEY idx_payments_booking (booking_id),
  KEY idx_payments_status (status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- 5.5 Invoices (mỗi booking 1 invoice chính - tuỳ luận, set UNIQUE)
CREATE TABLE invoices (
  id          BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  booking_id  BIGINT UNSIGNED NOT NULL,
  number      VARCHAR(32) NOT NULL,
  pdf_url     VARCHAR(255) NULL,
  created_at  DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_invoices_booking
    FOREIGN KEY (booking_id) REFERENCES bookings(id)
    ON UPDATE CASCADE ON DELETE CASCADE,
  UNIQUE KEY uq_invoices_number (number),
  UNIQUE KEY uq_invoices_booking (booking_id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

CREATE TABLE housekeeping_tasks (
  id                   BIGINT UNSIGNED PRIMARY KEY AUTO_INCREMENT,
  room_id              BIGINT UNSIGNED NOT NULL,
  assigned_to_user_id  BIGINT UNSIGNED NULL, -- nhân viên buồng phòng
  due_time             DATETIME NULL,
  status               ENUM('Todo','InProgress','Done','Blocked') NOT NULL DEFAULT 'Todo',
  notes                TEXT NULL,
  created_at           DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
  CONSTRAINT fk_hk_room
    FOREIGN KEY (room_id) REFERENCES rooms(id)
    ON UPDATE CASCADE ON DELETE CASCADE,
  CONSTRAINT fk_hk_user
    FOREIGN KEY (assigned_to_user_id) REFERENCES users(id)
    ON UPDATE CASCADE ON DELETE SET NULL,
  KEY idx_hk_room_status (room_id, status)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;




