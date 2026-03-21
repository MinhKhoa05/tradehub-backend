SET SQL_MODE = 'NO_AUTO_VALUE_ON_ZERO';
START TRANSACTION;
SET time_zone = '+00:00';

CREATE DATABASE IF NOT EXISTS tradehub_db;

USE tradehub_db;
-- =====================
-- TABLES
-- =====================

CREATE TABLE users (
  id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(50) NOT NULL,
  email VARCHAR(255) NOT NULL UNIQUE,
  password_hash VARCHAR(255) NOT NULL,
  balance INT NOT NULL DEFAULT 0,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci;

CREATE TABLE products (
  id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  name VARCHAR(50) NOT NULL,
  normalized_name VARCHAR(50) NOT NULL,
  stock INT NOT NULL DEFAULT 0,
  price INT NOT NULL,
  description TEXT,
  seller_id BIGINT UNSIGNED NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

  INDEX idx_products_seller (seller_id)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci;

CREATE TABLE cart_items (
  id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  user_id BIGINT UNSIGNED NOT NULL,
  product_id BIGINT UNSIGNED NOT NULL,
  quantity INT NOT NULL CHECK (quantity > 0),

  UNIQUE KEY uniq_cart (user_id, product_id),
  INDEX idx_cart_user (user_id),
  INDEX idx_cart_product (product_id)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci;

CREATE TABLE orders (
  id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  buyer_id BIGINT UNSIGNED NOT NULL,
  seller_id BIGINT UNSIGNED NOT NULL,
  total_amount INT NOT NULL,
  payment_method TINYINT NOT NULL,
  status TINYINT NOT NULL,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,

  INDEX idx_orders_buyer (buyer_id),
  INDEX idx_orders_seller (seller_id)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci;

CREATE TABLE order_items (
  id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  order_id BIGINT UNSIGNED NOT NULL,
  product_id BIGINT UNSIGNED NOT NULL,
  unit_price INT NOT NULL,
  quantity INT NOT NULL CHECK (quantity > 0),

  INDEX idx_order_items_order (order_id),
  INDEX idx_order_items_product (product_id)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci;

CREATE TABLE order_history (
  id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  order_id BIGINT UNSIGNED NOT NULL,
  from_status TINYINT NULL,
  to_status TINYINT NOT NULL,
  changed_by BIGINT UNSIGNED NOT NULL,
  actor_type TINYINT NOT NULL,
  note TEXT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

  INDEX idx_order_history_order (order_id)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci;

CREATE TABLE wallets (
  id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  user_id BIGINT UNSIGNED NOT NULL UNIQUE,
  balance BIGINT NOT NULL DEFAULT 0,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
  updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci;

CREATE TABLE wallet_transactions (
  id BIGINT UNSIGNED AUTO_INCREMENT PRIMARY KEY,
  wallet_id BIGINT UNSIGNED NOT NULL,
  amount INT NOT NULL,
  type SMALLINT NOT NULL,
  reference_id BIGINT NULL,
  description TEXT,
  created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,

  INDEX idx_wallet_id (wallet_id)
) ENGINE=InnoDB 
DEFAULT CHARSET=utf8mb4 
COLLATE=utf8mb4_unicode_ci;

-- =====================
-- FOREIGN KEYS
-- =====================

ALTER TABLE products
  ADD CONSTRAINT fk_products_seller
  FOREIGN KEY (seller_id) REFERENCES users(id)
  ON DELETE CASCADE;

ALTER TABLE cart_items
  ADD CONSTRAINT fk_cart_user
  FOREIGN KEY (user_id) REFERENCES users(id)
  ON DELETE CASCADE,
  ADD CONSTRAINT fk_cart_product
  FOREIGN KEY (product_id) REFERENCES products(id)
  ON DELETE CASCADE;

ALTER TABLE orders
  ADD CONSTRAINT fk_orders_buyer
  FOREIGN KEY (buyer_id) REFERENCES users(id),
  ADD CONSTRAINT fk_orders_seller
  FOREIGN KEY (seller_id) REFERENCES users(id);

ALTER TABLE order_items
  ADD CONSTRAINT fk_order_items_order
  FOREIGN KEY (order_id) REFERENCES orders(id)
  ON DELETE CASCADE,
  ADD CONSTRAINT fk_order_items_product
  FOREIGN KEY (product_id) REFERENCES products(id);

ALTER TABLE order_history
  ADD CONSTRAINT fk_order_history_order
  FOREIGN KEY (order_id) REFERENCES orders(id)
  ON DELETE CASCADE,
  ADD CONSTRAINT fk_order_history_user
  FOREIGN KEY (changed_by) REFERENCES users(id);

ALTER TABLE wallet_transactions
  ADD CONSTRAINT fk_wallet_tx_wallet
  FOREIGN KEY (wallet_id) REFERENCES wallets(id)
  ON DELETE CASCADE;

COMMIT;