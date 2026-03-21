USE tradehub_db;

START TRANSACTION;

-- ======================
-- USERS
-- ======================
INSERT INTO `users` (`id`, `name`, `email`, `password_hash`, `balance`) VALUES
(5, 'khoa', 'khoa@gmail.com', '$2b$10$NyUW5g6HWLCOpzRQTMk6keOmdV/pw9DMeqE5vgXkLuN2pJyal1yxK', 0),
(6, 'Khoa 2 đây', 'mkhoa639@gmail.com', '$2b$10$IQpdSmIDSadCEVqxbua9Q.Agyc5/TrrOkoyp6VNODmfz.kvBJGDvO', 0);

-- ======================
-- PRODUCTS (FULL)
-- ======================
INSERT INTO `products` (`id`, `name`, `stock`, `price`, `description`, `seller_id`, `normalized_name`) VALUES
(13, 'Bánh mì', 5000, 15000, 'Bánh mì ngon', 5, 'banh mi'),
(14, 'Sữa tươi', 2000, 25000, 'Sữa tươi nguyên chất', 5, 'sua tuoi'),
(15, 'Trà sữa', 1000, 30000, 'Trà sữa thơm ngon', 5, 'tra sua'),
(16, 'Bánh gạo', 10000, 12345, 'Bánh gạo truyền thống', 5, 'banh gao'),
(17, 'Nước cam', 800, 20000, 'Nước cam tươi', 5, 'nuoc cam'),
(18, 'Snack khoai tây', 1500, 12000, 'Snack giòn', 5, 'snack khoai tay'),
(19, 'Cà phê sữa', 3000, 18000, 'Cà phê sữa thơm ngon', 5, 'ca phe sua'),
(20, 'Bánh quy', 2500, 15000, 'Bánh quy socola', 5, 'banh quy'),
(21, 'Kem vani', 1200, 20000, 'Kem vani mát lạnh', 5, 'kem vani'),
(22, 'Trà chanh', 1800, 15000, 'Trà chanh giải nhiệt', 5, 'tra chanh'),
(23, 'Nước dừa', 1000, 22000, 'Nước dừa tươi', 5, 'nuoc dua'),
(24, 'Bánh pizza mini', 900, 35000, 'Pizza mini hấp dẫn', 5, 'pizza mini'),
(25, 'Kẹo socola', 2000, 10000, 'Kẹo socola ngọt', 5, 'keo socola'),
(26, 'Mỳ Ý', 1500, 30000, 'Mỳ Ý ngon', 5, 'my y'),
(27, 'Trái cây mix', 1200, 25000, 'Hỗn hợp trái cây', 5, 'trai cay mix'),
(28, 'Bánh kem', 600, 45000, 'Bánh kem sinh nhật', 5, 'banh kem'),
(29, 'Sữa chua', 2000, 12000, 'Sữa chua thơm ngon', 5, 'sua chua'),
(30, 'Nước ép táo', 800, 18000, 'Nước ép táo tươi', 5, 'nuoc ep tao'),
(31, 'Snack ngô', 1400, 15000, 'Snack giòn tan', 5, 'snack ngo'),
(32, 'Bánh bao', 3000, 20000, 'Bánh bao nhân thịt', 5, 'banh bao');

-- ======================
-- CART (FULL - giữ duplicate)
-- ======================
INSERT INTO `cart_items` (`id`, `user_id`, `product_id`, `quantity`) VALUES
(1, 5, 13, 2),
(2, 5, 14, 1),
(3, 5, 15, 3),
(4, 5, 16, 1),
(5, 5, 17, 2);

-- ======================
-- ORDERS
-- ======================
INSERT INTO `orders` (`id`, `buyer_id`, `seller_id`, `total_amount`, `payment_method`, `status`, `created_at`, `updated_at`) VALUES
(1, 6, 5, 30000, 1, 1, '2026-03-21 07:18:47', '2026-03-21 07:18:47'),
(2, 6, 5, 75000, 2, 1, '2026-03-21 07:18:47', '2026-03-21 07:18:47'),
(3, 6, 5, 12345, 1, 0, '2026-03-21 07:18:47', '2026-03-21 07:18:47'),
(4, 6, 5, 20000, 1, 1, '2026-03-21 07:18:47', '2026-03-21 07:18:47'),
(5, 6, 5, 50000, 2, 2, '2026-03-21 07:18:47', '2026-03-21 07:18:47');

-- ======================
-- ORDER ITEMS (FULL duplicate)
-- ======================
INSERT INTO `order_items` (`id`, `order_id`, `product_id`, `unit_price`, `quantity`) VALUES
(1, 1, 13, 12345, 2),
(2, 2, 14, 25000, 1),
(3, 3, 15, 30000, 1),
(4, 4, 16, 20000, 1),
(5, 5, 17, 12000, 2);

-- ======================
-- ORDER HISTORY
-- ======================
INSERT INTO `order_history` (`id`, `order_id`, `from_status`, `to_status`, `changed_by`, `actor_type`, `note`, `created_at`) VALUES
(1, 1, 0, 1, 5, 1, 'Order created', '2026-03-21 07:20:25'),
(2, 2, 0, 1, 5, 1, 'Order created', '2026-03-21 07:20:25'),
(3, 3, 0, 1, 5, 1, 'Order created', '2026-03-21 07:20:25'),
(4, 4, 0, 1, 5, 1, 'Order created', '2026-03-21 07:20:25'),
(5, 5, 0, 1, 5, 1, 'Order created', '2026-03-21 07:20:25');

-- ======================
-- WALLETS
-- ======================
INSERT INTO `wallets` (`id`, `user_id`, `balance`, `created_at`, `updated_at`) VALUES
(1, 6, 0, '2026-03-21 07:46:13', '2026-03-21 07:46:13'),
(2, 5, 9990, '2026-03-21 08:22:55', '2026-03-21 08:22:55');

-- ======================
-- WALLET TRANSACTIONS
-- ======================
INSERT INTO `wallet_transactions` (`id`, `wallet_id`, `amount`, `type`, `reference_id`, `description`, `created_at`) VALUES
(1, 2, 10000, 1, NULL, 'Nạp tiền, số tiền 10000', '2026-03-21 08:23:11'),
(2, 2, -10, 2, NULL, 'Rút tiền, số tiền 10', '2026-03-21 08:23:28');

COMMIT;