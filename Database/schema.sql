-- schema.sql
-- Create database if not exists
-- USE game_topup_db;

SET FOREIGN_KEY_CHECKS = 0;

-- 1. Table: users
CREATE TABLE IF NOT EXISTS users (
    id BIGINT SIGNED AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(100) NOT NULL,
    email VARCHAR(150) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    role INT DEFAULT 0, -- 0: Member, 1: Admin, 2: Staff
    is_active BOOLEAN DEFAULT TRUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX idx_users_username (username),
    INDEX idx_users_created (created_at)
) ENGINE=InnoDB;

-- 1.1 Table: wallets
CREATE TABLE IF NOT EXISTS wallets (
    id BIGINT SIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT SIGNED NOT NULL UNIQUE,
    balance DECIMAL(18, 2) DEFAULT 0.00,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_wallet_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    INDEX idx_wallets_user (user_id)
) ENGINE=InnoDB;

-- 2. Table: games
CREATE TABLE IF NOT EXISTS games (
    id BIGINT SIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    image_url TEXT,
    is_active BOOLEAN DEFAULT TRUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- 3. Table: game_packages
CREATE TABLE IF NOT EXISTS game_packages (
    id BIGINT SIGNED AUTO_INCREMENT PRIMARY KEY,
    name VARCHAR(150) NOT NULL,
    image_url TEXT,
    normalized_name VARCHAR(150) NOT NULL,
    game_id BIGINT SIGNED NOT NULL,
    sale_price DECIMAL(18, 2) NOT NULL,
    original_price DECIMAL(18, 2) NOT NULL,
    import_price DECIMAL(18, 2) NOT NULL,
    stock_quantity INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    CONSTRAINT fk_package_game FOREIGN KEY (game_id) REFERENCES games(id) ON DELETE CASCADE,
    INDEX idx_packages_lookup (game_id, is_active),
    INDEX idx_packages_normalized (normalized_name)
) ENGINE=InnoDB;

-- 4. Table: game_accounts
CREATE TABLE IF NOT EXISTS game_accounts (
    id BIGINT SIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT SIGNED NOT NULL,
    game_id BIGINT SIGNED NOT NULL,
    name VARCHAR(100) NOT NULL,
    account_identifier VARCHAR(150) NOT NULL,
    server VARCHAR(100),
    description TEXT,
    is_default BOOLEAN DEFAULT FALSE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_account_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_account_game FOREIGN KEY (game_id) REFERENCES games(id) ON DELETE CASCADE,
    INDEX idx_accounts_user_sort (user_id, is_default, created_at)
) ENGINE=InnoDB;


-- 6. Table: wallet_transactions
CREATE TABLE IF NOT EXISTS wallet_transactions (
    id BIGINT SIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT SIGNED NOT NULL,
    amount DECIMAL(18, 2) NOT NULL,
    balance_before DECIMAL(18, 2) NOT NULL,
    balance_after DECIMAL(18, 2) NOT NULL,
    type INT NOT NULL, -- 1: Deposit, 2: Withdraw, 3: PaidOrder, 4: Refund
    description TEXT,
    order_id BIGINT SIGNED NULL,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_tx_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_tx_order FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE SET NULL,
    INDEX idx_tx_user_sort (user_id, created_at)
) ENGINE=InnoDB;

-- 7. Table: orders
CREATE TABLE IF NOT EXISTS orders (
    id BIGINT SIGNED AUTO_INCREMENT PRIMARY KEY,
    user_id BIGINT SIGNED NOT NULL,
    game_account_info TEXT NOT NULL,
    game_package_id BIGINT SIGNED NOT NULL,
    unit_price DECIMAL(18, 2) NOT NULL,
    quantity INT NOT NULL,
    assign_to BIGINT SIGNED,
    assign_at DATETIME,
    status INT NOT NULL, -- 1: Pending, 2: Paid, 3: Processing, 4: Completed, 5: Cancelled
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    updated_at DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    is_pending TINYINT(1) GENERATED ALWAYS AS (CASE WHEN status = 1 THEN 1 ELSE NULL END) STORED,
    CONSTRAINT fk_order_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE,
    CONSTRAINT fk_order_package FOREIGN KEY (game_package_id) REFERENCES game_packages(id) ON DELETE CASCADE,
    INDEX idx_orders_user_sort (user_id, created_at),
    INDEX idx_orders_status (status),
    -- Đảm bảo mỗi khách hàng chỉ có tối đa 1 đơn hàng ở trạng thái Pending (1)
    UNIQUE INDEX idx_one_pending_per_user (user_id, is_pending)
) ENGINE=InnoDB;

-- 8. Table: order_history
CREATE TABLE IF NOT EXISTS order_history (
    id BIGINT SIGNED AUTO_INCREMENT PRIMARY KEY,
    order_id BIGINT SIGNED NOT NULL,
    from_status INT NOT NULL,
    to_status INT NOT NULL,
    note TEXT,
    action_by BIGINT SIGNED NOT NULL,
    is_admin BOOLEAN DEFAULT FALSE,
    created_at DATETIME DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_history_order FOREIGN KEY (order_id) REFERENCES orders(id) ON DELETE CASCADE,
    INDEX idx_history_order_sort (order_id, created_at)
) ENGINE=InnoDB;

SET FOREIGN_KEY_CHECKS = 1;
