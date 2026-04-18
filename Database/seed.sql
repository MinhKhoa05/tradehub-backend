USE tradehub_db;

START TRANSACTION;

-- ======================
-- USERS (Added a few more for variety)
-- ======================
INSERT INTO `users` (`id`, `name`, `email`, `password_hash`) VALUES
(1, 'Khoa Seller', 'khoa.seller@gmail.com', '$2b$10$NyUW5g6HWLCOpzRQTMk6keOmdV/pw9DMeqE5vgXkLuN2pJyal1yxK'),
(2, 'Khoa Buyer', 'khoa.buyer@gmail.com', '$2b$10$IQpdSmIDSadCEVqxbua9Q.Agyc5/TrrOkoyp6VNODmfz.kvBJGDvO'),
(3, 'Minh Electronics', 'minh.elec@gmail.com', '$2b$10$NyUW5g6HWLCOpzRQTMk6keOmdV/pw9DMeqE5vgXkLuN2pJyal1yxK'),
(4, 'Lan Fashion', 'lan.fashion@gmail.com', '$2b$10$NyUW5g6HWLCOpzRQTMk6keOmdV/pw9DMeqE5vgXkLuN2pJyal1yxK'),
(5, 'Tuan Grocery', 'tuan.grocery@gmail.com', '$2b$10$NyUW5g6HWLCOpzRQTMk6keOmdV/pw9DMeqE5vgXkLuN2pJyal1yxK');

-- ======================
-- PRODUCTS (Total 50 products across various sellers)
-- ======================
INSERT INTO `products` (`id`, `name`, `stock`, `price`, `description`, `seller_id`, `normalized_name`) VALUES
-- Seller 1: Food & Bev
(1, 'Bánh mì thịt', 100, 15000, 'Bánh mì kẹp thịt đặc biệt', 1, 'banh mi thit'),
(2, 'Sữa tươi Vinamilk', 200, 25000, 'Sữa tươi tiệt trùng 1L', 1, 'sua tuoi vinamilk'),
(3, 'Trà sữa Trân châu', 50, 30000, 'Trà sữa handmade thơm ngon', 1, 'tra sua tran chau'),
(4, 'Bánh gạo One One', 500, 12000, 'Bánh gạo vị mặn', 1, 'banh gao one one'),
(5, 'Nước cam ép', 80, 20000, 'Nước cam tươi nguyên chất', 1, 'nuoc cam ep'),
(6, 'Snack Pringles', 150, 45000, 'Khoai tây chiên ống lớn', 1, 'snack pringles'),
(7, 'Cà phê Highland', 300, 18000, 'Cà phê sữa đóng lon 235ml', 1, 'ca phe highland'),
(8, 'Bánh quy Oreo', 250, 15000, 'Bánh quy socola nhân kem 133g', 1, 'banh quy oreo'),
(9, 'Kem Merino', 120, 20000, 'Kem que đậu đỏ mát lạnh', 1, 'kem merino'),
(10, 'Trà chanh Lipton', 180, 15000, 'Trà chanh hòa tan hộp 16 gói', 1, 'tra chanh lipton'),

-- Seller 3: Electronics
(11, 'iPhone 15 Pro Max', 10, 32000000, 'Bản 256GB - VN/A Titan tự nhiên', 3, 'iphone 15 pro max'),
(12, 'Samsung Galaxy S24', 15, 22000000, 'Flagship Samsung mới nhất', 3, 'samsung galaxy s24'),
(13, 'MacBook Air M2', 5, 24500000, 'Màu Gray - 8GB/256GB', 3, 'macbook air m2'),
(14, 'Chuột Logitech G502', 45, 1200000, 'Chuột gaming quốc dân', 3, 'chuot logitech g502'),
(15, 'Bàn phím cơ AKKO', 30, 1500000, 'Switch Blue - LED RGB', 3, 'ban phim co akko'),
(16, 'Tai nghe Sony WH-1000XM5', 12, 8500000, 'Chống ồn đỉnh cao', 3, 'tai nghe sony wh 1000xm5'),
(17, 'Màn hình Dell UltraSharp', 8, 7200000, '25 inch 2K chuyên đồ họa', 3, 'man hinh dell ultrasharp'),
(18, 'Loa Marshall Emberton', 20, 3800000, 'Loa bluetooth phong cách cổ điển', 3, 'loa marshall emberton'),
(19, 'Sạc dự phòng Anker', 100, 850000, '20000mAh - Sạc nhanh 22.5W', 3, 'sac du phong anker'),
(20, 'Cáp sạc USB-C Apple', 200, 450000, 'Dài 1m - Chính hãng Apple', 3, 'cap sac usb c apple'),

-- Seller 4: Fashion
(21, 'Áo thun Uniqlo', 100, 299000, 'Cotton 100% - Thấm hút mồ hôi', 4, 'ao thun uniqlo'),
(22, 'Quần Jean Levi\'s 501', 50, 1850000, 'Dáng đứng huyền thoại', 4, 'quan jean levis 501'),
(23, 'Giày Nike Air Force 1', 30, 2900000, 'Màu trắng - Full box', 4, 'giay nike air force 1'),
(24, 'Áo khoác Bomber', 40, 550000, 'Vải dù 2 lớp chống nước nhẹ', 4, 'ao khoac bomber'),
(25, 'Mũ lưỡi trai Adidas', 80, 450000, 'Logo thêu nổi bật', 4, 'mu luoi trai adidas'),
(26, 'Tất Nike cổ cao', 300, 50000, 'Gói 3 đôi cực êm', 4, 'tat nike co cao'),
(27, 'Thắt lưng da cá sấu', 15, 1200000, 'Da thật 100% - Bảo hành 12 tháng', 4, 'that lung da ca sau'),
(28, 'Ví da nam Pedro', 25, 850000, 'Thiết kế sang trọng', 4, 'vi da nam pedro'),
(29, 'Váy hoa nhí', 60, 350000, 'Phong cách vintage', 4, 'vay hoa nhi'),
(30, 'Giày cao gót Zara', 20, 1250000, 'Cao 7cm - Êm chân', 4, 'giay cao got zara'),

-- Seller 5: Grocery
(31, 'Gạo ST25', 1000, 185000, 'Gạo ngon nhất thế giới', 5, 'gao st25'),
(32, 'Dầu ăn Neptune', 500, 55000, 'Chai 1 lít Gold', 5, 'dau an neptune'),
(33, 'Nước mắm Nam Ngư', 400, 45000, 'Nước mắm đệ nhị chai lớn', 5, 'nuoc mam nam ngu'),
(34, 'Tương ớt Chinsu', 600, 15000, 'Chai 250g - Cay vừa', 5, 'tuong ot chinsu'),
(35, 'Hạt nêm Knorr', 450, 75000, 'Gói 900g chiết xuất thịt thăn', 5, 'hat nem knorr'),
(36, 'Trứng gà CP', 1000, 35000, 'Vỉ 10 quả sạch', 5, 'trung ga cp'),
(37, 'Mỳ Kokomi', 2000, 3500, 'Đại chiến 90g', 5, 'my kokomi'),
(38, 'Xúc xích Đức Việt', 250, 45000, 'Gói 500g hấp hoặc chiên', 5, 'xuc xich duc viet'),
(39, 'Bột giặt Ariel', 150, 185000, 'Túi 3.8kg hương nắng mai', 5, 'bot giat ariel'),
(40, 'Nước rửa bát Sunlight', 300, 30000, 'Chai 750g hương chanh', 5, 'nuoc rua bat sunlight'),

-- Random more
(41, 'Đèn bàn học Rạng Đông', 50, 150000, 'LED chống cận', 1, 'den ban hoc rang dong'),
(42, 'Bình nước Lock&Lock', 100, 250000, 'Inox 304 giữ nhiệt 12h', 1, 'binh nuoc locknlock'),
(43, 'Ô che mưa gấp gọn', 80, 120000, 'Khung thép chắc chắn', 4, 'o che mua gap gon'),
(44, 'Thảm lau chân san hô', 200, 45000, 'Siêu thấm nước', 4, 'tham lau chan san ho'),
(45, 'Khăn mặt lông cừu', 150, 25000, 'Mềm mại tuyệt đối', 4, 'khan mat long cuu'),
(46, 'USB Sandisk 64GB', 300, 150000, 'Chuẩn 3.0 tốc độ cao', 3, 'usb sandisk 64gb'),
(47, 'Thẻ nhớ MicroSD 128GB', 200, 350000, 'Class 10 cho điện thoại/camera', 3, 'the nho microsd 128gb'),
(48, 'Nồi cơm điện Sharp', 30, 850000, 'Dung tích 1.8L nắp gài', 5, 'noi com dien sharp'),
(49, 'Ấm siêu tốc Bluestone', 40, 450000, 'Inox 2 lớp, ngắt tự động', 5, 'am sieu toc bluestone'),
(50, 'Quạt đứng Senko', 25, 550000, 'Cánh 40cm, chạy êm', 5, 'quat dung senko');

-- ======================
-- WALLETS (For all users)
-- ======================
INSERT INTO `wallets` (`id`, `user_id`, `balance`, `created_at`, `updated_at`) VALUES
(1, 1, 5000000, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(2, 2, 10000000, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(3, 3, 50000000, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(4, 4, 15000000, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP),
(5, 5, 20000000, CURRENT_TIMESTAMP, CURRENT_TIMESTAMP);

-- ======================
-- CART ITEMS
-- ======================
INSERT INTO `cart_items` (`id`, `user_id`, `product_id`, `quantity`) VALUES
(1, 2, 11, 1),
(2, 2, 1, 5),
(3, 2, 34, 2),
(4, 1, 14, 10),
(5, 1, 23, 1),
(6, 4, 11, 1),
(7, 4, 31, 2);

-- ======================
-- ORDERS (A variety of orders with different statuses)
-- ======================
INSERT INTO `orders` (`id`, `buyer_id`, `seller_id`, `total_amount`, `payment_method`, `status`, `created_at`, `updated_at`) VALUES
-- Status: 6 (Completed), 7 (Cancelled), 1 (Pending), 2 (Confirmed)
(1, 2, 3, 32000000, 2, 6, '2026-04-10 09:00:00', '2026-04-12 15:00:00'),
(2, 2, 1, 75000, 1, 6, '2026-04-12 10:30:00', '2026-04-12 11:00:00'),
(3, 2, 5, 370000, 2, 7, '2026-04-13 14:00:00', '2026-04-13 14:30:00'),
(4, 1, 5, 185000, 2, 6, '2026-04-15 08:00:00', '2026-04-16 10:00:00'),
(5, 4, 3, 1200000, 2, 2, '2026-04-17 20:00:00', '2026-04-18 09:00:00'),
(6, 4, 1, 60000, 1, 1, '2026-04-18 11:00:00', '2026-04-18 11:00:00'),
(7, 2, 4, 1850000, 2, 2, '2026-04-18 12:00:00', '2026-04-18 12:30:00'),
(8, 5, 3, 1500000, 2, 6, '2026-04-14 09:00:00', '2026-04-15 16:00:00');

-- ======================
-- ORDER ITEMS
-- ======================
INSERT INTO `order_items` (`id`, `order_id`, `product_id`, `unit_price`, `quantity`) VALUES
(1, 1, 11, 32000000, 1),
(2, 2, 1, 15000, 5),
(3, 3, 31, 185000, 2),
(4, 4, 31, 185000, 1),
(5, 5, 14, 1200000, 1),
(6, 6, 3, 30000, 2),
(7, 7, 22, 1850000, 1),
(8, 8, 15, 1500000, 1);

-- ======================
-- ORDER HISTORY
-- ======================
INSERT INTO `order_history` (`id`, `order_id`, `from_status`, `to_status`, `changed_by`, `actor_type`, `note`, `created_at`) VALUES
(1, 1, 1, 2, 3, 2, 'Shop confirmed order', '2026-04-10 09:30:00'),
(2, 1, 2, 6, 2, 1, 'Buyer confirmed received', '2026-04-12 15:00:00'),
(3, 3, 1, 7, 2, 1, 'Buyer cancelled the order', '2026-04-13 14:30:00'),
(4, 5, 1, 2, 3, 2, 'Preparing stocks', '2026-04-18 09:00:00'),
(5, 7, 1, 2, 4, 2, 'Confirmed', '2026-04-18 12:30:00');

-- ======================
-- WALLET TRANSACTIONS (Rich history for User 2)
-- ======================
INSERT INTO `wallet_transactions` (`id`, `wallet_id`, `amount`, `type`, `reference_id`, `description`, `created_at`) VALUES
(1, 2, 50000000, 1, NULL, 'Nạp tiền qua ngân hàng', '2026-04-01 10:00:00'),
(2, 2, -32000000, 3, 1, 'Thanh toán đơn hàng #1', '2026-04-10 09:00:05'),
(3, 2, -370000, 3, 3, 'Thanh toán đơn hàng #3 (Tạm giữ)', '2026-04-13 14:00:05'),
(4, 2, 370000, 1, 3, 'Hoàn tiền đơn hàng #3 (Hủy)', '2026-04-13 14:30:10'),
(5, 2, -1850000, 3, 7, 'Thanh toán đơn hàng #7', '2026-04-18 12:00:05'),
(6, 3, 10000000, 1, NULL, 'Doanh thu tháng 3', '2026-04-01 00:00:00'),
(7, 3, 32000000, 1, 1, 'Tiền bán iPhone x1 (Đơn #1)', '2026-04-12 15:00:10'),
(8, 1, 2000000, 1, NULL, 'Nạp tiền mặt tại quầy', '2026-04-05 14:00:00'),
(9, 4, 5000000, 1, NULL, 'Quà tặng tân thủ', '2026-04-10 08:00:00');

COMMIT;