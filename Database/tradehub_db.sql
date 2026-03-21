-- phpMyAdmin SQL Dump
-- version 6.0.0-dev+20250914.f72491a1c0
-- https://www.phpmyadmin.net/
--
-- Host: localhost:3306
-- Generation Time: Mar 21, 2026 at 01:25 AM
-- Server version: 8.4.3
-- PHP Version: 8.3.16

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `tradehub_db`
--

-- --------------------------------------------------------

--
-- Table structure for table `cart_items`
--

CREATE TABLE `cart_items` (
  `id` bigint NOT NULL,
  `user_id` bigint NOT NULL,
  `product_id` bigint NOT NULL,
  `quantity` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `cart_items`
--

INSERT INTO `cart_items` (`id`, `user_id`, `product_id`, `quantity`) VALUES
(1, 5, 12, 2),
(2, 5, 13, 1),
(3, 5, 14, 3),
(4, 5, 15, 1),
(5, 5, 16, 2),
(6, 5, 12, 2),
(7, 5, 13, 1),
(8, 5, 14, 3),
(9, 5, 15, 1),
(10, 5, 16, 2);

-- --------------------------------------------------------

--
-- Table structure for table `orders`
--

CREATE TABLE `orders` (
  `id` bigint NOT NULL,
  `buyer_id` bigint NOT NULL,
  `seller_id` bigint NOT NULL,
  `total_amount` int NOT NULL,
  `payment_method` int NOT NULL,
  `status` int NOT NULL,
  `created_at` datetime NOT NULL,
  `updated_at` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `orders`
--

INSERT INTO `orders` (`id`, `buyer_id`, `seller_id`, `total_amount`, `payment_method`, `status`, `created_at`, `updated_at`) VALUES
(1, 6, 5, 30000, 1, 1, '2026-03-21 07:18:47', '2026-03-21 07:18:47'),
(2, 6, 5, 75000, 2, 1, '2026-03-21 07:18:47', '2026-03-21 07:18:47'),
(3, 6, 5, 12345, 1, 0, '2026-03-21 07:18:47', '2026-03-21 07:18:47'),
(4, 6, 5, 20000, 1, 1, '2026-03-21 07:18:47', '2026-03-21 07:18:47'),
(5, 6, 5, 50000, 2, 2, '2026-03-21 07:18:47', '2026-03-21 07:18:47');

-- --------------------------------------------------------

--
-- Table structure for table `order_history`
--

CREATE TABLE `order_history` (
  `id` bigint NOT NULL,
  `order_id` bigint NOT NULL,
  `from_status` int DEFAULT NULL,
  `to_status` int NOT NULL,
  `changed_by` bigint NOT NULL,
  `actor_type` int NOT NULL,
  `note` text NOT NULL,
  `created_at` datetime NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `order_history`
--

INSERT INTO `order_history` (`id`, `order_id`, `from_status`, `to_status`, `changed_by`, `actor_type`, `note`, `created_at`) VALUES
(1, 1, 0, 1, 5, 1, 'Order created', '2026-03-21 07:20:25'),
(2, 2, 0, 1, 5, 1, 'Order created', '2026-03-21 07:20:25'),
(3, 3, 0, 1, 5, 1, 'Order created', '2026-03-21 07:20:25'),
(4, 4, 0, 1, 5, 1, 'Order created', '2026-03-21 07:20:25'),
(5, 5, 0, 1, 5, 1, 'Order created', '2026-03-21 07:20:25');

-- --------------------------------------------------------

--
-- Table structure for table `order_items`
--

CREATE TABLE `order_items` (
  `id` bigint NOT NULL,
  `order_id` bigint NOT NULL,
  `product_id` bigint NOT NULL,
  `unit_price` int NOT NULL,
  `quantity` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `order_items`
--

INSERT INTO `order_items` (`id`, `order_id`, `product_id`, `unit_price`, `quantity`) VALUES
(1, 1, 12, 12345, 2),
(2, 2, 13, 25000, 1),
(3, 3, 14, 30000, 1),
(4, 4, 15, 20000, 1),
(5, 5, 16, 12000, 2),
(6, 1, 12, 12345, 2),
(7, 2, 13, 25000, 1),
(8, 3, 14, 30000, 1),
(9, 4, 15, 20000, 1),
(10, 5, 16, 12000, 2);

-- --------------------------------------------------------

--
-- Table structure for table `products`
--

CREATE TABLE `products` (
  `id` bigint NOT NULL,
  `name` varchar(50) NOT NULL,
  `stock` int NOT NULL DEFAULT '0',
  `price` int NOT NULL,
  `description` text,
  `seller_id` bigint NOT NULL,
  `normalized_name` varchar(50) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `products`
--

INSERT INTO `products` (`id`, `name`, `stock`, `price`, `description`, `seller_id`, `normalized_name`) VALUES
(12, 'Bánh gạo', 10000, 12345, '123343', 5, 'banh gao'),
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
(32, 'Bánh bao', 3000, 20000, 'Bánh bao nhân thịt', 5, 'banh bao'),
(33, 'Bánh mì', 5000, 15000, 'Bánh mì ngon', 5, 'banh mi'),
(34, 'Sữa tươi', 2000, 25000, 'Sữa tươi nguyên chất', 5, 'sua tuoi'),
(35, 'Trà sữa', 1000, 30000, 'Trà sữa thơm ngon', 5, 'tra sua'),
(36, 'Bánh gạo', 10000, 12345, 'Bánh gạo truyền thống', 5, 'banh gao'),
(37, 'Nước cam', 800, 20000, 'Nước cam tươi', 5, 'nuoc cam'),
(38, 'Snack khoai tây', 1500, 12000, 'Snack giòn', 5, 'snack khoai tay'),
(39, 'Cà phê sữa', 3000, 18000, 'Cà phê sữa thơm ngon', 5, 'ca phe sua'),
(40, 'Bánh quy', 2500, 15000, 'Bánh quy socola', 5, 'banh quy'),
(41, 'Kem vani', 1200, 20000, 'Kem vani mát lạnh', 5, 'kem vani'),
(42, 'Trà chanh', 1800, 15000, 'Trà chanh giải nhiệt', 5, 'tra chanh'),
(43, 'Nước dừa', 1000, 22000, 'Nước dừa tươi', 5, 'nuoc dua'),
(44, 'Bánh pizza mini', 900, 35000, 'Pizza mini hấp dẫn', 5, 'pizza mini'),
(45, 'Kẹo socola', 2000, 10000, 'Kẹo socola ngọt', 5, 'keo socola'),
(46, 'Mỳ Ý', 1500, 30000, 'Mỳ Ý ngon', 5, 'my y'),
(47, 'Trái cây mix', 1200, 25000, 'Hỗn hợp trái cây', 5, 'trai cay mix'),
(48, 'Bánh kem', 600, 45000, 'Bánh kem sinh nhật', 5, 'banh kem'),
(49, 'Sữa chua', 2000, 12000, 'Sữa chua thơm ngon', 5, 'sua chua'),
(50, 'Nước ép táo', 800, 18000, 'Nước ép táo tươi', 5, 'nuoc ep tao'),
(51, 'Snack ngô', 1400, 15000, 'Snack giòn tan', 5, 'snack ngo'),
(52, 'Bánh bao', 3000, 20000, 'Bánh bao nhân thịt', 5, 'banh bao'),
(53, 'Bánh mì', 5000, 15000, 'Bánh mì ngon', 5, 'banh mi'),
(54, 'Sữa tươi', 2000, 25000, 'Sữa tươi nguyên chất', 5, 'sua tuoi'),
(55, 'Trà sữa', 1000, 30000, 'Trà sữa thơm ngon', 5, 'tra sua'),
(56, 'Bánh gạo', 10000, 12345, 'Bánh gạo truyền thống', 5, 'banh gao'),
(57, 'Nước cam', 800, 20000, 'Nước cam tươi', 5, 'nuoc cam'),
(58, 'Snack khoai tây', 1500, 12000, 'Snack giòn', 5, 'snack khoai tay'),
(59, 'Cà phê sữa', 3000, 18000, 'Cà phê sữa thơm ngon', 5, 'ca phe sua'),
(60, 'Bánh quy', 2500, 15000, 'Bánh quy socola', 5, 'banh quy'),
(61, 'Kem vani', 1200, 20000, 'Kem vani mát lạnh', 5, 'kem vani'),
(62, 'Trà chanh', 1800, 15000, 'Trà chanh giải nhiệt', 5, 'tra chanh'),
(63, 'Nước dừa', 1000, 22000, 'Nước dừa tươi', 5, 'nuoc dua'),
(64, 'Bánh pizza mini', 900, 35000, 'Pizza mini hấp dẫn', 5, 'pizza mini'),
(65, 'Kẹo socola', 2000, 10000, 'Kẹo socola ngọt', 5, 'keo socola'),
(66, 'Mỳ Ý', 1500, 30000, 'Mỳ Ý ngon', 5, 'my y'),
(67, 'Trái cây mix', 1200, 25000, 'Hỗn hợp trái cây', 5, 'trai cay mix'),
(68, 'Bánh kem', 600, 45000, 'Bánh kem sinh nhật', 5, 'banh kem'),
(69, 'Sữa chua', 2000, 12000, 'Sữa chua thơm ngon', 5, 'sua chua'),
(70, 'Nước ép táo', 800, 18000, 'Nước ép táo tươi', 5, 'nuoc ep tao'),
(71, 'Snack ngô', 1400, 15000, 'Snack giòn tan', 5, 'snack ngo'),
(72, 'Bánh bao', 3000, 20000, 'Bánh bao nhân thịt', 5, 'banh bao'),
(73, 'Bánh mì', 5000, 15000, 'Bánh mì ngon', 5, 'banh mi'),
(74, 'Sữa tươi', 2000, 25000, 'Sữa tươi nguyên chất', 5, 'sua tuoi'),
(75, 'Trà sữa', 1000, 30000, 'Trà sữa thơm ngon', 5, 'tra sua'),
(76, 'Bánh gạo', 10000, 12345, 'Bánh gạo truyền thống', 5, 'banh gao'),
(77, 'Nước cam', 800, 20000, 'Nước cam tươi', 5, 'nuoc cam'),
(78, 'Snack khoai tây', 1500, 12000, 'Snack giòn', 5, 'snack khoai tay'),
(79, 'Cà phê sữa', 3000, 18000, 'Cà phê sữa thơm ngon', 5, 'ca phe sua'),
(80, 'Bánh quy', 2500, 15000, 'Bánh quy socola', 5, 'banh quy'),
(81, 'Kem vani', 1200, 20000, 'Kem vani mát lạnh', 5, 'kem vani'),
(82, 'Trà chanh', 1800, 15000, 'Trà chanh giải nhiệt', 5, 'tra chanh'),
(83, 'Nước dừa', 1000, 22000, 'Nước dừa tươi', 5, 'nuoc dua'),
(84, 'Bánh pizza mini', 900, 35000, 'Pizza mini hấp dẫn', 5, 'pizza mini'),
(85, 'Kẹo socola', 2000, 10000, 'Kẹo socola ngọt', 5, 'keo socola'),
(86, 'Mỳ Ý', 1500, 30000, 'Mỳ Ý ngon', 5, 'my y'),
(87, 'Trái cây mix', 1200, 25000, 'Hỗn hợp trái cây', 5, 'trai cay mix'),
(88, 'Bánh kem', 600, 45000, 'Bánh kem sinh nhật', 5, 'banh kem'),
(89, 'Sữa chua', 2000, 12000, 'Sữa chua thơm ngon', 5, 'sua chua'),
(90, 'Nước ép táo', 800, 18000, 'Nước ép táo tươi', 5, 'nuoc ep tao'),
(91, 'Snack ngô', 1400, 15000, 'Snack giòn tan', 5, 'snack ngo'),
(92, 'Bánh bao', 3000, 20000, 'Bánh bao nhân thịt', 5, 'banh bao'),
(93, 'Bành mỳ bơ sữa đây', 20497, 132307, '', 6, 'banh my bo sua day');

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `id` bigint NOT NULL,
  `name` varchar(50) NOT NULL,
  `email` varchar(255) NOT NULL,
  `password_hash` varchar(255) NOT NULL,
  `balance` int NOT NULL DEFAULT '0'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`id`, `name`, `email`, `password_hash`, `balance`) VALUES
(5, 'khoa', 'khoa@gmail.com', '$2b$10$NyUW5g6HWLCOpzRQTMk6keOmdV/pw9DMeqE5vgXkLuN2pJyal1yxK', 0),
(6, 'Khoa 2 đây', 'mkhoa639@gmail.com', '$2b$10$IQpdSmIDSadCEVqxbua9Q.Agyc5/TrrOkoyp6VNODmfz.kvBJGDvO', 0);

-- --------------------------------------------------------

--
-- Table structure for table `wallets`
--

CREATE TABLE `wallets` (
  `id` bigint NOT NULL,
  `user_id` bigint NOT NULL,
  `balance` bigint NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `wallets`
--

INSERT INTO `wallets` (`id`, `user_id`, `balance`, `created_at`, `updated_at`) VALUES
(1, 6, 0, '2026-03-21 07:46:13', '2026-03-21 07:46:13'),
(2, 5, 9990, '2026-03-21 08:22:55', '2026-03-21 08:22:55');

-- --------------------------------------------------------

--
-- Table structure for table `wallet_transactions`
--

CREATE TABLE `wallet_transactions` (
  `id` int NOT NULL,
  `wallet_id` bigint NOT NULL,
  `amount` int NOT NULL COMMENT '>0: Tiền vào, <0: Tiền ra',
  `type` smallint NOT NULL COMMENT '1=Deposit,2=Withdraw,3=PaidOrder,4=Refund',
  `reference_id` bigint DEFAULT NULL COMMENT 'OrderId hoặc tham chiếu khác',
  `description` text,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;

--
-- Dumping data for table `wallet_transactions`
--

INSERT INTO `wallet_transactions` (`id`, `wallet_id`, `amount`, `type`, `reference_id`, `description`, `created_at`) VALUES
(1, 2, 10000, 1, NULL, 'Nạp tiền, số tiền 10000', '2026-03-21 08:23:11'),
(2, 2, -10, 2, NULL, 'Rút tiền, số tiền 10', '2026-03-21 08:23:28');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `cart_items`
--
ALTER TABLE `cart_items`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_cart_user` (`user_id`),
  ADD KEY `idx_cart_product` (`product_id`);

--
-- Indexes for table `orders`
--
ALTER TABLE `orders`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_orders_buyer` (`buyer_id`),
  ADD KEY `idx_orders_seller` (`seller_id`);

--
-- Indexes for table `order_history`
--
ALTER TABLE `order_history`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_order_history_order` (`order_id`),
  ADD KEY `changed_by` (`changed_by`);

--
-- Indexes for table `order_items`
--
ALTER TABLE `order_items`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_order_items_order` (`order_id`),
  ADD KEY `idx_order_items_product` (`product_id`);

--
-- Indexes for table `products`
--
ALTER TABLE `products`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `wallets`
--
ALTER TABLE `wallets`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `user_id` (`user_id`);

--
-- Indexes for table `wallet_transactions`
--
ALTER TABLE `wallet_transactions`
  ADD PRIMARY KEY (`id`),
  ADD KEY `idx_wallet_id` (`wallet_id`),
  ADD KEY `idx_reference_id` (`reference_id`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `cart_items`
--
ALTER TABLE `cart_items`
  MODIFY `id` bigint NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `orders`
--
ALTER TABLE `orders`
  MODIFY `id` bigint NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `order_history`
--
ALTER TABLE `order_history`
  MODIFY `id` bigint NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT for table `order_items`
--
ALTER TABLE `order_items`
  MODIFY `id` bigint NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `products`
--
ALTER TABLE `products`
  MODIFY `id` bigint NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=94;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `id` bigint NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT for table `wallets`
--
ALTER TABLE `wallets`
  MODIFY `id` bigint NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT for table `wallet_transactions`
--
ALTER TABLE `wallet_transactions`
  MODIFY `id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `cart_items`
--
ALTER TABLE `cart_items`
  ADD CONSTRAINT `fk_cart_product` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_cart_user` FOREIGN KEY (`user_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `orders`
--
ALTER TABLE `orders`
  ADD CONSTRAINT `fk_orders_buyer` FOREIGN KEY (`buyer_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_orders_seller` FOREIGN KEY (`seller_id`) REFERENCES `users` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `order_history`
--
ALTER TABLE `order_history`
  ADD CONSTRAINT `fk_order_history_order` FOREIGN KEY (`order_id`) REFERENCES `orders` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `order_history_ibfk_1` FOREIGN KEY (`changed_by`) REFERENCES `users` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT;

--
-- Constraints for table `order_items`
--
ALTER TABLE `order_items`
  ADD CONSTRAINT `fk_order_items_order` FOREIGN KEY (`order_id`) REFERENCES `orders` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  ADD CONSTRAINT `fk_order_items_product` FOREIGN KEY (`product_id`) REFERENCES `products` (`id`) ON DELETE CASCADE ON UPDATE CASCADE;

--
-- Constraints for table `wallet_transactions`
--
ALTER TABLE `wallet_transactions`
  ADD CONSTRAINT `wallet_transactions_ibfk_1` FOREIGN KEY (`wallet_id`) REFERENCES `wallets` (`id`) ON DELETE RESTRICT ON UPDATE RESTRICT;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
