-- seed.sql
-- USE tradehub_db;

START TRANSACTION;

-- ======================
-- USERS
-- ======================
INSERT INTO `users` (`id`, `username`, `email`, `password_hash`, `role`) VALUES
(1, 'admin', 'admin@tradehub.com', '$2b$10$NyUW5g6HWLCOpzRQTMk6keOmdV/pw9DMeqE5vgXkLuN2pJyal1yxK', 1),
(2, 'member1', 'member1@gmail.com', '$2b$10$IQpdSmIDSadCEVqxbua9Q.Agyc5/TrrOkoyp6VNODmfz.kvBJGDvO', 0),
(3, 'staff1', 'staff1@tradehub.com', '$2b$10$NyUW5g6HWLCOpzRQTMk6keOmdV/pw9DMeqE5vgXkLuN2pJyal1yxK', 2);

-- ======================
-- WALLETS
-- ======================
INSERT INTO `wallets` (`user_id`, `balance`) VALUES
(1, 1000000.00),
(2, 500000.00),
(3, 0.00);

-- ======================
-- GAMES
-- ======================
INSERT INTO `games` (`id`, `name`, `image_url`, `is_active`) VALUES
(1, 'Free Fire', 'https://example.com/ff.png', 1),
(2, 'Liên Quân Mobile', 'https://example.com/lq.png', 1),
(3, 'PUBG Mobile', 'https://example.com/pubg.png', 1);

-- ======================
-- GAME PACKAGES
-- ======================
INSERT INTO `game_packages` (`id`, `name`, `normalized_name`, `game_id`, `sale_price`, `original_price`, `import_price`, `package_budget`, `spent_amount`) VALUES
-- Free Fire
(1, '100 Kim Cương', '100-kim-cuong', 1, 20000.00, 25000.00, 15000.00, 1000000.00, 0.00),
(2, '500 Kim Cương', '500-kim-cuong', 1, 95000.00, 120000.00, 75000.00, 5000000.00, 0.00),
-- Liên Quân
(3, '100 Quân Huy', '100-quan-huy', 2, 50000.00, 60000.00, 40000.00, 2000000.00, 0.00),
(4, '500 Quân Huy', '500-quan-huy', 2, 240000.00, 300000.00, 200000.00, 10000000.00, 0.00),
-- PUBG
(5, '60 UC', '60-uc', 3, 22000.00, 25000.00, 18000.00, 1000000.00, 0.00),
(6, '325 UC', '325-uc', 3, 110000.00, 130000.00, 90000.00, 5000000.00, 0.00);

-- ======================
-- GAME ACCOUNTS (Address Book)
-- ======================
INSERT INTO `game_accounts` (`user_id`, `game_id`, `name`, `account_identifier`, `server`, `is_default`) VALUES
(2, 1, 'Acc Chính FF', '123456789', 'Việt Nam', 1),
(2, 2, 'Acc Leo Rank LQ', 'member1_lq', 'Mặt Trời', 1);

-- ======================
-- CART ITEMS
-- ======================
INSERT INTO `cart_items` (`user_id`, `game_package_id`, `quantity`) VALUES
(2, 1, 2), -- 2 gói 100 Kim Cương
(2, 3, 1); -- 1 gói 100 Quân Huy

-- ======================
-- WALLET TRANSACTIONS
-- ======================
INSERT INTO `wallet_transactions` (`id`, `user_id`, `amount`, `balance_after`, `type`, `description`) VALUES
(1, 2, 500000.00, 500000.00, 1, 'Nạp tiền khởi tạo hệ thống');

COMMIT;