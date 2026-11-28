-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Máy chủ: localhost
-- Thời gian đã tạo: Th10 28, 2026 lúc 11:01 AM
-- Phiên bản máy phục vụ: 8.0.44
-- Phiên bản PHP: 8.2.12

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Cơ sở dữ liệu: `shoestore_db`
--

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `brands`
--

CREATE TABLE `brands` (
  `brand_id` int NOT NULL,
  `brand_name` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text COLLATE utf8mb4_unicode_ci
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `brands`
--

INSERT INTO `brands` (`brand_id`, `brand_name`, `description`) VALUES
(1, 'Nike', 'Thương hiệu thể thao hàng đầu thế giới từ Mỹ'),
(2, 'Adidas', 'Thương hiệu thể thao ba sọc huyền thoại từ Đức'),
(3, 'Puma', 'Thương hiệu thể thao thời trang và tốc độ'),
(4, 'Converse', 'Biểu tượng giày canvas kinh điển của giới trẻ'),
(5, 'Vans', 'Thương hiệu trượt ván đường phố kinh điển'),
(6, 'New Balance', 'Thương hiệu giày êm ái và thiết kế retro cổ điển');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `carts`
--

CREATE TABLE `carts` (
  `cart_id` int NOT NULL,
  `customer_id` int NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `carts`
--

INSERT INTO `carts` (`cart_id`, `customer_id`, `created_at`, `updated_at`) VALUES
(1, 3, '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(2, 4, '2026-11-21 16:08:44', '2026-11-28 14:49:56');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `cart_items`
--

CREATE TABLE `cart_items` (
  `cart_item_id` int NOT NULL,
  `cart_id` int NOT NULL,
  `variant_id` int NOT NULL,
  `quantity` int NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ;

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `categories`
--

CREATE TABLE `categories` (
  `category_id` int NOT NULL,
  `category_name` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `description` text COLLATE utf8mb4_unicode_ci
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `categories`
--

INSERT INTO `categories` (`category_id`, `category_name`, `description`) VALUES
(1, 'Giày thể thao', 'Giày thể thao năng động, tập luyện đa năng'),
(2, 'Giày chạy bộ', 'Giày chạy bộ chuyên nghiệp, siêu êm và nhẹ'),
(3, 'Giày thời trang', 'Giày sneaker phong cách lifestyle, đi chơi hằng ngày'),
(4, 'Giày tây', 'Giày da sang trọng, lịch sự cho công sở và tiệc'),
(5, 'Sandal & Dép', 'Dép và sandal quai ngang thoáng mát, tiện dụng');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `customers`
--

CREATE TABLE `customers` (
  `customer_id` int NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `customers`
--

INSERT INTO `customers` (`customer_id`) VALUES
(3),
(4);

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `customer_addresses`
--

CREATE TABLE `customer_addresses` (
  `address_id` int NOT NULL,
  `customer_id` int NOT NULL,
  `receiver_name` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `phone` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `address_line` varchar(1000) COLLATE utf8mb4_unicode_ci NOT NULL,
  `is_default` tinyint(1) NOT NULL DEFAULT '0',
  `default_customer_id` int GENERATED ALWAYS AS ((case when (`is_default` = true) then `customer_id` else NULL end)) STORED,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `customer_addresses`
--

INSERT INTO `customer_addresses` (`address_id`, `customer_id`, `receiver_name`, `phone`, `address_line`, `is_default`, `created_at`, `updated_at`) VALUES
(1, 3, 'Nguyễn Minh Anh', '0987654321', '123 Nguyễn Trãi, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh', 1, '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(2, 4, 'Bùi Anh Minh', '0869799470', '34 An Bình, Phường An Đông, HCM', 1, '2026-11-21 16:09:24', '2026-11-21 16:09:24');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `employees`
--

CREATE TABLE `employees` (
  `employee_id` int NOT NULL,
  `hire_date` date DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `employees`
--

INSERT INTO `employees` (`employee_id`, `hire_date`) VALUES
(1, '2024-01-15'),
(2, '2024-06-01'),
(5, '2026-11-21');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `orders`
--

CREATE TABLE `orders` (
  `order_id` int NOT NULL,
  `order_code` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `customer_id` int NOT NULL,
  `promotion_id` int DEFAULT NULL,
  `receiver_name` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `phone` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `shipping_address` varchar(1000) COLLATE utf8mb4_unicode_ci NOT NULL,
  `total_amount` decimal(15,2) NOT NULL,
  `discount_amount` decimal(15,2) NOT NULL DEFAULT '0.00',
  `final_amount` decimal(15,2) GENERATED ALWAYS AS ((`total_amount` - `discount_amount`)) STORED,
  `promo_code_snapshot` varchar(50) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `order_status` enum('Pending','Confirmed','Shipping','Completed','Cancelled') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Pending',
  `note` text COLLATE utf8mb4_unicode_ci,
  `cancel_reason` text COLLATE utf8mb4_unicode_ci,
  `cancelled_by_user_id` int DEFAULT NULL,
  `cancelled_at` datetime DEFAULT NULL,
  `completed_at` datetime DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ;

--
-- Đang đổ dữ liệu cho bảng `orders`
--

INSERT INTO `orders` (`order_id`, `order_code`, `customer_id`, `promotion_id`, `receiver_name`, `phone`, `shipping_address`, `total_amount`, `discount_amount`, `promo_code_snapshot`, `order_status`, `note`, `cancel_reason`, `cancelled_by_user_id`, `cancelled_at`, `completed_at`, `created_at`, `updated_at`) VALUES
(1, 'SS26110101', 3, 1, 'Nguyễn Minh Anh', '0987654321', '123 Nguyễn Trãi, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh', 2890000.00, 100000.00, 'CHAO100', 'Completed', 'Giao giờ hành chính', NULL, NULL, NULL, '2026-11-16 13:59:39', '2026-11-14 13:59:39', '2026-11-16 13:59:39'),
(2, 'SS26110702', 3, NULL, 'Nguyễn Minh Anh', '0987654321', '123 Nguyễn Trãi, Phường Bến Thành, Quận 1, TP. Hồ Chí Minh', 3850000.00, 0.00, NULL, 'Completed', 'Gọi trước khi giao', NULL, NULL, NULL, '2026-11-21 16:56:30', '2026-11-21 11:59:39', '2026-11-21 16:56:30'),
(3, 'SS261121161129aa4d8f8eb875', 4, NULL, 'Bùi Anh Minh', '0869799470', '34 An Bình, Phường An Đông, HCM', 3850000.00, 0.00, NULL, 'Completed', 'Giao vào giờ trưa cho tôi', NULL, NULL, NULL, '2026-11-21 16:20:03', '2026-11-21 16:11:30', '2026-11-21 16:20:03'),
(4, 'SS261128144955be5056c8ab12', 4, 1, 'Bùi Anh Minh', '0869799470', '34 An Bình, Phường An Đông, HCM', 2990000.00, 100000.00, 'CHAO100', 'Completed', '', NULL, NULL, NULL, '2026-11-28 14:50:59', '2026-11-28 14:49:55', '2026-11-28 14:50:59');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `order_details`
--

CREATE TABLE `order_details` (
  `order_detail_id` int NOT NULL,
  `order_id` int NOT NULL,
  `variant_id` int NOT NULL,
  `product_name_snapshot` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `sku_snapshot` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `size_snapshot` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `color_snapshot` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `quantity` int NOT NULL,
  `unit_price` decimal(15,2) NOT NULL,
  `line_total` decimal(25,2) GENERATED ALWAYS AS ((`quantity` * `unit_price`)) STORED
) ;

--
-- Đang đổ dữ liệu cho bảng `order_details`
--

INSERT INTO `order_details` (`order_detail_id`, `order_id`, `variant_id`, `product_name_snapshot`, `sku_snapshot`, `size_snapshot`, `color_snapshot`, `quantity`, `unit_price`) VALUES
(1, 1, 1, 'Nike Air Force 1 \'07', 'AF1-07-WHT-40', '40', 'Trắng', 1, 2890000.00),
(2, 2, 5, 'Adidas Ultraboost Light', 'UB-LIGHT-BLK-40', '40', 'Đen', 1, 3850000.00),
(3, 3, 6, 'Adidas Ultraboost Light', 'UB-LIGHT-BLK-41', '41', 'Đen', 1, 3850000.00),
(4, 4, 4, 'Nike Air Force 1 \'07', 'AF1-07-BLK-41', '41', 'Đen', 1, 2990000.00);

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `order_status_history`
--

CREATE TABLE `order_status_history` (
  `history_id` int NOT NULL,
  `order_id` int NOT NULL,
  `previous_status` enum('Pending','Confirmed','Shipping','Completed','Cancelled') COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `new_status` enum('Pending','Confirmed','Shipping','Completed','Cancelled') COLLATE utf8mb4_unicode_ci NOT NULL,
  `changed_by_user_id` int NOT NULL,
  `note` text COLLATE utf8mb4_unicode_ci,
  `changed_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `order_status_history`
--

INSERT INTO `order_status_history` (`history_id`, `order_id`, `previous_status`, `new_status`, `changed_by_user_id`, `note`, `changed_at`) VALUES
(1, 1, NULL, 'Pending', 3, 'Khách hàng đặt hàng', '2026-11-14 13:59:39'),
(2, 1, 'Pending', 'Confirmed', 1, 'Xác nhận đơn hàng', '2026-11-15 13:59:39'),
(3, 1, 'Confirmed', 'Shipping', 1, 'Bàn giao đơn vị vận chuyển', '2026-11-15 13:59:39'),
(4, 1, 'Shipping', 'Completed', 1, 'Giao hàng thành công và thu tiền COD', '2026-11-16 13:59:39'),
(5, 2, NULL, 'Pending', 3, 'Khách hàng đặt hàng', '2026-11-21 11:59:39'),
(6, 3, NULL, 'Pending', 4, 'Khách hàng đặt hàng', '2026-11-21 16:11:30'),
(7, 3, 'Pending', 'Confirmed', 1, 'Xác nhận đơn hàng đủ điều kiện giao', '2026-11-21 16:17:30'),
(8, 3, 'Confirmed', 'Shipping', 2, 'Bàn giao cho đơn vị vận chuyển', '2026-11-21 16:19:59'),
(9, 3, 'Shipping', 'Completed', 2, 'Khách đã nhận hàng thành công', '2026-11-21 16:20:03'),
(10, 2, 'Pending', 'Confirmed', 2, 'Xác nhận đơn hàng đủ điều kiện giao', '2026-11-21 16:56:29'),
(11, 2, 'Confirmed', 'Shipping', 2, 'Bàn giao cho đơn vị vận chuyển', '2026-11-21 16:56:30'),
(12, 2, 'Shipping', 'Completed', 2, 'Khách đã nhận hàng thành công', '2026-11-21 16:56:30'),
(13, 4, NULL, 'Pending', 4, 'Khách hàng đặt hàng', '2026-11-28 14:49:56'),
(14, 4, 'Pending', 'Confirmed', 2, 'Xác nhận đơn hàng đủ điều kiện giao', '2026-11-28 14:50:52'),
(15, 4, 'Confirmed', 'Shipping', 2, 'Bàn giao cho đơn vị vận chuyển', '2026-11-28 14:50:57'),
(16, 4, 'Shipping', 'Completed', 2, 'Khách đã nhận hàng thành công', '2026-11-28 14:50:59');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `payments`
--

CREATE TABLE `payments` (
  `payment_id` int NOT NULL,
  `order_id` int NOT NULL,
  `payment_method` enum('COD','BankTransfer') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'COD',
  `amount` decimal(15,2) NOT NULL,
  `payment_status` enum('Unpaid','Paid','Cancelled') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Unpaid',
  `transfer_reference` varchar(100) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `confirmed_by_user_id` int DEFAULT NULL,
  `confirmed_at` datetime DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ;

--
-- Đang đổ dữ liệu cho bảng `payments`
--

INSERT INTO `payments` (`payment_id`, `order_id`, `payment_method`, `amount`, `payment_status`, `transfer_reference`, `confirmed_by_user_id`, `confirmed_at`, `created_at`, `updated_at`) VALUES
(1, 1, 'COD', 2790000.00, 'Paid', NULL, 1, '2026-11-16 13:59:39', '2026-11-14 13:59:39', '2026-11-16 13:59:39'),
(2, 2, 'COD', 3850000.00, 'Paid', NULL, 2, '2026-11-21 16:56:30', '2026-11-21 11:59:39', '2026-11-21 16:56:30'),
(3, 3, 'COD', 3850000.00, 'Paid', NULL, 2, '2026-11-21 16:20:03', '2026-11-21 16:11:30', '2026-11-21 16:20:03'),
(4, 4, 'COD', 2890000.00, 'Paid', NULL, 2, '2026-11-28 14:50:59', '2026-11-28 14:49:56', '2026-11-28 14:50:59');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `products`
--

CREATE TABLE `products` (
  `product_id` int NOT NULL,
  `product_code` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `product_name` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `category_id` int NOT NULL,
  `brand_id` int NOT NULL,
  `supplier_id` int NOT NULL,
  `description` text COLLATE utf8mb4_unicode_ci,
  `thumbnail_url` varchar(500) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `status` enum('Selling','Discontinued') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Selling',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `products`
--

INSERT INTO `products` (`product_id`, `product_code`, `product_name`, `category_id`, `brand_id`, `supplier_id`, `description`, `thumbnail_url`, `status`, `created_at`, `updated_at`) VALUES
(1, 'AF1-07', 'Nike Air Force 1 \'07', 3, 1, 1, 'Đôi giày kinh điển của Nike mang phong cách thời thượng, đệm Air êm ái suốt cả ngày dài và chất liệu da bền bỉ.', '/images/products/nike_air_force_1.png', 'Selling', '2026-11-21 13:59:39', '2026-11-28 15:44:48'),
(2, 'UB-LIGHT', 'Adidas Ultraboost Light', 2, 2, 2, 'Mẫu giày chạy bộ hàng đầu với đệm Light BOOST nhẹ hơn 30%, phản hồi lực tối đa cho từng bước chạy.', '/images/products/adidas_ultraboost.png', 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(3, 'PUMA-VELO', 'Puma Velocity Nitro 3', 2, 3, 3, 'Được trang bị bọt NITRO siêu nhẹ và đàn hồi, Puma Velocity Nitro 3 hoàn hảo cho các bài tập chạy hằng ngày.', '/images/products/puma_velocity.png', 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(4, 'CV-CHUCK70', 'Converse Chuck 70 Vintage Canvas', 3, 4, 3, 'Phiên bản Chuck 70 tôn vinh thiết kế nguyên bản thập niên 70 với chất liệu vải canvas cao cấp và lót giày OrthoLite cực êm.', '/images/products/converse_chuck_70.png', 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(5, 'VANS-OLDSKOOL', 'Vans Old Skool Classic', 3, 5, 3, 'Đôi giày trượt ván biểu tượng với sọc Jazz Stripe đặc trưng, đế bánh quế Waffle bám đường siêu tốt.', '/images/products/vans_old_skool.png', 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(6, 'NB-574', 'New Balance 574 Core', 1, 6, 3, 'Mẫu giày kinh điển và linh hoạt nhất của New Balance với đệm ENCAP hỗ trợ tối đa cho bàn chân.', '/images/products/new_balance_574.png', 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(7, 'NIKE-PEG40', 'Nike Air Zoom Pegasus 40', 2, 1, 1, 'Giày chạy bộ quốc dân với 2 túi đệm Zoom Air kết hợp bọt React êm ái, thích hợp mọi cự ly.', '/images/products/nike_pegasus.png', 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(8, 'AD-SAMBA', 'Adidas Samba OG', 3, 2, 2, 'Cơn sốt thời trang đường phố toàn cầu, thiết kế da mềm viền da lộn chữ T kinh điển.', '/images/products/adidas_samba.png', 'Selling', '2026-11-21 13:59:39', '2026-11-28 15:32:44'),
(9, 'DEMO-001', 'Nike Revolution 7', 1, 1, 1, 'Nike Revolution 7 thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/nike_air_force_1.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(10, 'DEMO-002', 'Nike Court Vision Low', 2, 2, 2, 'Nike Court Vision Low thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/adidas_ultraboost.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(11, 'DEMO-003', 'Nike Air Max SC', 3, 3, 3, 'Nike Air Max SC thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/puma_velocity.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(12, 'DEMO-004', 'Nike Dunk Low Retro', 5, 4, 3, 'Nike Dunk Low Retro thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/converse_chuck_70.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(13, 'DEMO-005', 'Nike Downshifter 12', 1, 5, 3, 'Nike Downshifter 12 thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/vans_old_skool.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(14, 'DEMO-006', 'Nike Precision 6', 2, 6, 3, 'Nike Precision 6 thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/new_balance_574.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(15, 'DEMO-007', 'Adidas Breaknet 2.0', 3, 1, 1, 'Adidas Breaknet 2.0 thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/nike_pegasus.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(16, 'DEMO-008', 'Adidas Grand Court 2', 5, 2, 2, 'Adidas Grand Court 2 thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/adidas_samba.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(17, 'DEMO-009', 'Adidas Run Falcon 3', 1, 3, 3, 'Adidas Run Falcon 3 thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/nike_air_force_1.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(18, 'DEMO-010', 'Adidas Duramo Speed', 2, 4, 3, 'Adidas Duramo Speed thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/adidas_ultraboost.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(19, 'DEMO-011', 'Adidas Advantage Base', 3, 5, 3, 'Adidas Advantage Base thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/puma_velocity.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(20, 'DEMO-012', 'Adidas Response Runner', 5, 6, 3, 'Adidas Response Runner thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/converse_chuck_70.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(21, 'DEMO-013', 'Puma Skyrocket Lite', 1, 1, 1, 'Puma Skyrocket Lite thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/vans_old_skool.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(22, 'DEMO-014', 'Puma Softride Enzo', 2, 2, 2, 'Puma Softride Enzo thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/new_balance_574.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(23, 'DEMO-015', 'Puma Rebound Layup', 3, 3, 3, 'Puma Rebound Layup thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/nike_pegasus.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(24, 'DEMO-016', 'Puma Slipstream', 5, 4, 3, 'Puma Slipstream thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/adidas_samba.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(25, 'DEMO-017', 'Puma Caven 2.0', 1, 5, 3, 'Puma Caven 2.0 thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/nike_air_force_1.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(26, 'DEMO-018', 'Puma Court Pure', 2, 6, 3, 'Puma Court Pure thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/adidas_ultraboost.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(27, 'DEMO-019', 'Converse Run Star Hike', 3, 1, 1, 'Converse Run Star Hike thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/puma_velocity.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(28, 'DEMO-020', 'Converse One Star Pro', 5, 2, 2, 'Converse One Star Pro thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/converse_chuck_70.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(29, 'DEMO-021', 'Converse Chuck Taylor All Star', 1, 3, 3, 'Converse Chuck Taylor All Star thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/vans_old_skool.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(30, 'DEMO-022', 'Converse Pro Leather', 2, 4, 3, 'Converse Pro Leather thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/new_balance_574.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(31, 'DEMO-023', 'Converse Jack Purcell', 3, 5, 3, 'Converse Jack Purcell thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/nike_pegasus.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(32, 'DEMO-024', 'Converse Star Player 76', 5, 6, 3, 'Converse Star Player 76 thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/adidas_samba.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(33, 'DEMO-025', 'Vans Authentic Classic', 1, 1, 1, 'Vans Authentic Classic thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/nike_air_force_1.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(34, 'DEMO-026', 'Vans Era ComfyCush', 2, 2, 2, 'Vans Era ComfyCush thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/adidas_ultraboost.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(35, 'DEMO-027', 'Vans Knu Skool', 3, 3, 3, 'Vans Knu Skool thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/puma_velocity.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(36, 'DEMO-028', 'Vans Slip-On Checkerboard', 5, 4, 3, 'Vans Slip-On Checkerboard thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/converse_chuck_70.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(37, 'DEMO-029', 'Vans Filmore Hi', 1, 5, 3, 'Vans Filmore Hi thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/vans_old_skool.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(38, 'DEMO-030', 'Vans Skate Old Skool', 2, 6, 3, 'Vans Skate Old Skool thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/new_balance_574.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(39, 'DEMO-031', 'New Balance 530', 3, 1, 1, 'New Balance 530 thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/nike_pegasus.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(40, 'DEMO-032', 'New Balance 327', 5, 2, 2, 'New Balance 327 thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/adidas_samba.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(41, 'DEMO-033', 'New Balance 1906R', 1, 3, 3, 'New Balance 1906R thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/nike_air_force_1.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(42, 'DEMO-034', 'New Balance Fresh Foam Arishi', 2, 4, 3, 'New Balance Fresh Foam Arishi thuộc bộ sưu tập thời trang thể thao. Thiết kế năng động, đệm êm ái, phù hợp đi hàng ngày.', '/images/products/adidas_ultraboost.png', 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `product_images`
--

CREATE TABLE `product_images` (
  `image_id` int NOT NULL,
  `product_id` int NOT NULL,
  `image_url` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `sort_order` int NOT NULL DEFAULT '0',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `product_images`
--

INSERT INTO `product_images` (`image_id`, `product_id`, `image_url`, `sort_order`, `created_at`) VALUES
(1, 8, '/images/products/adidas_samba.png', 0, '2026-11-28 15:32:44'),
(2, 8, '/uploads/products/8_648841574f9747e3b791162afab8146a.webp', 1, '2026-11-28 15:32:44'),
(3, 1, '/images/products/nike_air_force_1.png', 0, '2026-11-28 15:44:48'),
(4, 1, '/uploads/products/1_706b44ce035243cabda0af8866d455dd.webp', 1, '2026-11-28 15:44:48');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `product_reviews`
--

CREATE TABLE `product_reviews` (
  `review_id` int NOT NULL,
  `product_id` int NOT NULL,
  `customer_id` int NOT NULL,
  `rating` tinyint NOT NULL,
  `comment` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ;

--
-- Đang đổ dữ liệu cho bảng `product_reviews`
--

INSERT INTO `product_reviews` (`review_id`, `product_id`, `customer_id`, `rating`, `comment`, `created_at`, `updated_at`) VALUES
(1, 1, 3, 5, 'Giày đi rất êm và ôm chân, da xịn bóng đẹp, form chuẩn chính hãng. Giao hàng nhanh đóng gói cẩn thận 5 sao!', '2026-11-17 13:59:39', '2026-11-17 13:59:39'),
(2, 1, 4, 5, 'Giày đúng kích cỡ, mang êm. Đi cả ngày vẫn thoải mái.', '2026-11-28 15:10:55', '2026-11-28 15:10:55');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `product_variants`
--

CREATE TABLE `product_variants` (
  `variant_id` int NOT NULL,
  `product_id` int NOT NULL,
  `sku` varchar(100) COLLATE utf8mb4_unicode_ci NOT NULL,
  `size` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `color` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `price` decimal(15,2) NOT NULL,
  `stock_quantity` int NOT NULL DEFAULT '0',
  `low_stock_threshold` int NOT NULL DEFAULT '5',
  `status` enum('Selling','Discontinued') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Selling',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ;

--
-- Đang đổ dữ liệu cho bảng `product_variants`
--

INSERT INTO `product_variants` (`variant_id`, `product_id`, `sku`, `size`, `color`, `price`, `stock_quantity`, `low_stock_threshold`, `status`, `created_at`, `updated_at`) VALUES
(1, 1, 'AF1-07-WHT-40', '40', 'Trắng', 2890000.00, 15, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(2, 1, 'AF1-07-WHT-41', '41', 'Trắng', 2890000.00, 12, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(3, 1, 'AF1-07-WHT-42', '42', 'Trắng', 2890000.00, 20, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(4, 1, 'AF1-07-BLK-41', '41', 'Đen', 2990000.00, 7, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-28 14:49:56'),
(5, 2, 'UB-LIGHT-BLK-40', '40', 'Đen', 3850000.00, 10, 4, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(6, 2, 'UB-LIGHT-BLK-41', '41', 'Đen', 3850000.00, 13, 4, 'Selling', '2026-11-21 13:59:39', '2026-11-21 16:11:30'),
(7, 2, 'UB-LIGHT-WHT-42', '42', 'Trắng', 3950000.00, 6, 4, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(8, 2, 'UB-LIGHT-BLU-42', '42', 'Xanh Navy', 3850000.00, 13, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 16:25:10'),
(9, 3, 'PUMA-VN3-ORG-40', '40', 'Cam', 2590000.00, 9, 4, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(10, 3, 'PUMA-VN3-ORG-41', '41', 'Cam', 2590000.00, 11, 4, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(11, 3, 'PUMA-VN3-BLK-42', '42', 'Đen', 2650000.00, 7, 4, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(12, 4, 'CV-C70-BLK-39', '39', 'Đen', 1950000.00, 18, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(13, 4, 'CV-C70-BLK-40', '40', 'Đen', 1950000.00, 22, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(14, 4, 'CV-C70-PAR-41', '41', 'Vàng Kem', 2050000.00, 10, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(15, 5, 'VN-OS-BW-40', '40', 'Đen Trắng', 1850000.00, 16, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(16, 5, 'VN-OS-BW-41', '41', 'Đen Trắng', 1850000.00, 19, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(17, 5, 'VN-OS-BW-42', '42', 'Đen Trắng', 1850000.00, 15, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(18, 6, 'NB-574-GRY-40', '40', 'Xám', 2390000.00, 14, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(19, 6, 'NB-574-GRY-41', '41', 'Xám', 2390000.00, 12, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(20, 6, 'NB-574-NVY-42', '42', 'Xanh Navy', 2450000.00, 4, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(21, 7, 'PEG40-WHT-41', '41', 'Trắng Xanh', 3490000.00, 10, 4, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(22, 7, 'PEG40-BLK-42', '42', 'Đen', 3490000.00, 15, 4, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(23, 8, 'SAMBA-OG-WHT-40', '40', 'Trắng Sọc Đen', 2700000.00, 8, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(24, 8, 'SAMBA-OG-WHT-41', '41', 'Trắng Sọc Đen', 2700000.00, 7, 5, 'Selling', '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(25, 9, 'DEMO-001-WHT-39', '39', 'Trắng', 1450000.00, 6, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(26, 9, 'DEMO-001-BLK-40', '40', 'Đen', 1500000.00, 7, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(27, 9, 'DEMO-001-WHT-41', '41', 'Trắng', 1450000.00, 8, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(28, 9, 'DEMO-001-BLK-42', '42', 'Đen', 1500000.00, 9, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(29, 10, 'DEMO-002-GRY-39', '39', 'Xám', 1630000.00, 7, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(30, 10, 'DEMO-002-NVY-40', '40', 'Xanh Navy', 1680000.00, 8, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(31, 10, 'DEMO-002-GRY-41', '41', 'Xám', 1630000.00, 9, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(32, 10, 'DEMO-002-NVY-42', '42', 'Xanh Navy', 1680000.00, 10, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(33, 11, 'DEMO-003-BLKWHT-39', '39', 'Đen Trắng', 1810000.00, 8, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(34, 11, 'DEMO-003-BE-40', '40', 'Be', 1860000.00, 9, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(35, 11, 'DEMO-003-BLKWHT-41', '41', 'Đen Trắng', 1810000.00, 10, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(36, 11, 'DEMO-003-BE-42', '42', 'Be', 1860000.00, 11, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(37, 12, 'DEMO-004-WHT-39', '39', 'Trắng', 1990000.00, 9, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(38, 12, 'DEMO-004-BLK-40', '40', 'Đen', 2040000.00, 10, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(39, 12, 'DEMO-004-WHT-41', '41', 'Trắng', 1990000.00, 11, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(40, 12, 'DEMO-004-BLK-42', '42', 'Đen', 2040000.00, 12, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(41, 13, 'DEMO-005-GRY-39', '39', 'Xám', 2170000.00, 10, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(42, 13, 'DEMO-005-NVY-40', '40', 'Xanh Navy', 2220000.00, 11, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(43, 13, 'DEMO-005-GRY-41', '41', 'Xám', 2170000.00, 12, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(44, 13, 'DEMO-005-NVY-42', '42', 'Xanh Navy', 2220000.00, 13, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(45, 14, 'DEMO-006-BLKWHT-39', '39', 'Đen Trắng', 2350000.00, 11, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(46, 14, 'DEMO-006-BE-40', '40', 'Be', 2400000.00, 12, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(47, 14, 'DEMO-006-BLKWHT-41', '41', 'Đen Trắng', 2350000.00, 13, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(48, 14, 'DEMO-006-BE-42', '42', 'Be', 2400000.00, 14, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(49, 15, 'DEMO-007-WHT-39', '39', 'Trắng', 2530000.00, 12, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(50, 15, 'DEMO-007-BLK-40', '40', 'Đen', 2580000.00, 13, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(51, 15, 'DEMO-007-WHT-41', '41', 'Trắng', 2530000.00, 14, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(52, 15, 'DEMO-007-BLK-42', '42', 'Đen', 2580000.00, 15, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(53, 16, 'DEMO-008-GRY-39', '39', 'Xám', 2710000.00, 13, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(54, 16, 'DEMO-008-NVY-40', '40', 'Xanh Navy', 2760000.00, 14, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(55, 16, 'DEMO-008-GRY-41', '41', 'Xám', 2710000.00, 15, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(56, 16, 'DEMO-008-NVY-42', '42', 'Xanh Navy', 2760000.00, 16, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(57, 17, 'DEMO-009-BLKWHT-39', '39', 'Đen Trắng', 2890000.00, 14, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(58, 17, 'DEMO-009-BE-40', '40', 'Be', 2940000.00, 15, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(59, 17, 'DEMO-009-BLKWHT-41', '41', 'Đen Trắng', 2890000.00, 16, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(60, 17, 'DEMO-009-BE-42', '42', 'Be', 2940000.00, 17, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(61, 18, 'DEMO-010-WHT-39', '39', 'Trắng', 1450000.00, 15, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(62, 18, 'DEMO-010-BLK-40', '40', 'Đen', 1500000.00, 16, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(63, 18, 'DEMO-010-WHT-41', '41', 'Trắng', 1450000.00, 17, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(64, 18, 'DEMO-010-BLK-42', '42', 'Đen', 1500000.00, 18, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(65, 19, 'DEMO-011-GRY-39', '39', 'Xám', 1630000.00, 16, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(66, 19, 'DEMO-011-NVY-40', '40', 'Xanh Navy', 1680000.00, 17, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(67, 19, 'DEMO-011-GRY-41', '41', 'Xám', 1630000.00, 18, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(68, 19, 'DEMO-011-NVY-42', '42', 'Xanh Navy', 1680000.00, 19, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(69, 20, 'DEMO-012-BLKWHT-39', '39', 'Đen Trắng', 1810000.00, 17, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(70, 20, 'DEMO-012-BE-40', '40', 'Be', 1860000.00, 18, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(71, 20, 'DEMO-012-BLKWHT-41', '41', 'Đen Trắng', 1810000.00, 19, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(72, 20, 'DEMO-012-BE-42', '42', 'Be', 1860000.00, 20, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(73, 21, 'DEMO-013-WHT-39', '39', 'Trắng', 1990000.00, 18, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(74, 21, 'DEMO-013-BLK-40', '40', 'Đen', 2040000.00, 19, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(75, 21, 'DEMO-013-WHT-41', '41', 'Trắng', 1990000.00, 20, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(76, 21, 'DEMO-013-BLK-42', '42', 'Đen', 2040000.00, 21, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(77, 22, 'DEMO-014-GRY-39', '39', 'Xám', 2170000.00, 19, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(78, 22, 'DEMO-014-NVY-40', '40', 'Xanh Navy', 2220000.00, 20, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(79, 22, 'DEMO-014-GRY-41', '41', 'Xám', 2170000.00, 21, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(80, 22, 'DEMO-014-NVY-42', '42', 'Xanh Navy', 2220000.00, 22, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(81, 23, 'DEMO-015-BLKWHT-39', '39', 'Đen Trắng', 2350000.00, 20, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(82, 23, 'DEMO-015-BE-40', '40', 'Be', 2400000.00, 21, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(83, 23, 'DEMO-015-BLKWHT-41', '41', 'Đen Trắng', 2350000.00, 22, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(84, 23, 'DEMO-015-BE-42', '42', 'Be', 2400000.00, 23, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(85, 24, 'DEMO-016-WHT-39', '39', 'Trắng', 2530000.00, 21, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(86, 24, 'DEMO-016-BLK-40', '40', 'Đen', 2580000.00, 22, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(87, 24, 'DEMO-016-WHT-41', '41', 'Trắng', 2530000.00, 23, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(88, 24, 'DEMO-016-BLK-42', '42', 'Đen', 2580000.00, 6, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(89, 25, 'DEMO-017-GRY-39', '39', 'Xám', 2710000.00, 22, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(90, 25, 'DEMO-017-NVY-40', '40', 'Xanh Navy', 2760000.00, 23, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(91, 25, 'DEMO-017-GRY-41', '41', 'Xám', 2710000.00, 6, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(92, 25, 'DEMO-017-NVY-42', '42', 'Xanh Navy', 2760000.00, 7, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(93, 26, 'DEMO-018-BLKWHT-39', '39', 'Đen Trắng', 2890000.00, 23, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(94, 26, 'DEMO-018-BE-40', '40', 'Be', 2940000.00, 6, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(95, 26, 'DEMO-018-BLKWHT-41', '41', 'Đen Trắng', 2890000.00, 7, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(96, 26, 'DEMO-018-BE-42', '42', 'Be', 2940000.00, 8, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(97, 27, 'DEMO-019-WHT-39', '39', 'Trắng', 1450000.00, 6, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(98, 27, 'DEMO-019-BLK-40', '40', 'Đen', 1500000.00, 7, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(99, 27, 'DEMO-019-WHT-41', '41', 'Trắng', 1450000.00, 8, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(100, 27, 'DEMO-019-BLK-42', '42', 'Đen', 1500000.00, 9, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(101, 28, 'DEMO-020-GRY-39', '39', 'Xám', 1630000.00, 7, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(102, 28, 'DEMO-020-NVY-40', '40', 'Xanh Navy', 1680000.00, 8, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(103, 28, 'DEMO-020-GRY-41', '41', 'Xám', 1630000.00, 9, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(104, 28, 'DEMO-020-NVY-42', '42', 'Xanh Navy', 1680000.00, 10, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(105, 29, 'DEMO-021-BLKWHT-39', '39', 'Đen Trắng', 1810000.00, 8, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(106, 29, 'DEMO-021-BE-40', '40', 'Be', 1860000.00, 9, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(107, 29, 'DEMO-021-BLKWHT-41', '41', 'Đen Trắng', 1810000.00, 10, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(108, 29, 'DEMO-021-BE-42', '42', 'Be', 1860000.00, 11, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(109, 30, 'DEMO-022-WHT-39', '39', 'Trắng', 1990000.00, 9, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(110, 30, 'DEMO-022-BLK-40', '40', 'Đen', 2040000.00, 10, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(111, 30, 'DEMO-022-WHT-41', '41', 'Trắng', 1990000.00, 11, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(112, 30, 'DEMO-022-BLK-42', '42', 'Đen', 2040000.00, 12, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(113, 31, 'DEMO-023-GRY-39', '39', 'Xám', 2170000.00, 10, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(114, 31, 'DEMO-023-NVY-40', '40', 'Xanh Navy', 2220000.00, 11, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(115, 31, 'DEMO-023-GRY-41', '41', 'Xám', 2170000.00, 12, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(116, 31, 'DEMO-023-NVY-42', '42', 'Xanh Navy', 2220000.00, 13, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(117, 32, 'DEMO-024-BLKWHT-39', '39', 'Đen Trắng', 2350000.00, 11, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(118, 32, 'DEMO-024-BE-40', '40', 'Be', 2400000.00, 12, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(119, 32, 'DEMO-024-BLKWHT-41', '41', 'Đen Trắng', 2350000.00, 13, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(120, 32, 'DEMO-024-BE-42', '42', 'Be', 2400000.00, 14, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(121, 33, 'DEMO-025-WHT-39', '39', 'Trắng', 2530000.00, 12, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(122, 33, 'DEMO-025-BLK-40', '40', 'Đen', 2580000.00, 13, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(123, 33, 'DEMO-025-WHT-41', '41', 'Trắng', 2530000.00, 14, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(124, 33, 'DEMO-025-BLK-42', '42', 'Đen', 2580000.00, 15, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(125, 34, 'DEMO-026-GRY-39', '39', 'Xám', 2710000.00, 13, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(126, 34, 'DEMO-026-NVY-40', '40', 'Xanh Navy', 2760000.00, 14, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(127, 34, 'DEMO-026-GRY-41', '41', 'Xám', 2710000.00, 15, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(128, 34, 'DEMO-026-NVY-42', '42', 'Xanh Navy', 2760000.00, 16, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(129, 35, 'DEMO-027-BLKWHT-39', '39', 'Đen Trắng', 2890000.00, 14, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(130, 35, 'DEMO-027-BE-40', '40', 'Be', 2940000.00, 15, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(131, 35, 'DEMO-027-BLKWHT-41', '41', 'Đen Trắng', 2890000.00, 16, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(132, 35, 'DEMO-027-BE-42', '42', 'Be', 2940000.00, 17, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(133, 36, 'DEMO-028-WHT-39', '39', 'Trắng', 1450000.00, 15, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(134, 36, 'DEMO-028-BLK-40', '40', 'Đen', 1500000.00, 16, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(135, 36, 'DEMO-028-WHT-41', '41', 'Trắng', 1450000.00, 17, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(136, 36, 'DEMO-028-BLK-42', '42', 'Đen', 1500000.00, 18, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(137, 37, 'DEMO-029-GRY-39', '39', 'Xám', 1630000.00, 16, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(138, 37, 'DEMO-029-NVY-40', '40', 'Xanh Navy', 1680000.00, 17, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(139, 37, 'DEMO-029-GRY-41', '41', 'Xám', 1630000.00, 18, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(140, 37, 'DEMO-029-NVY-42', '42', 'Xanh Navy', 1680000.00, 19, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(141, 38, 'DEMO-030-BLKWHT-39', '39', 'Đen Trắng', 1810000.00, 17, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(142, 38, 'DEMO-030-BE-40', '40', 'Be', 1860000.00, 18, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(143, 38, 'DEMO-030-BLKWHT-41', '41', 'Đen Trắng', 1810000.00, 19, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(144, 38, 'DEMO-030-BE-42', '42', 'Be', 1860000.00, 20, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(145, 39, 'DEMO-031-WHT-39', '39', 'Trắng', 1990000.00, 18, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(146, 39, 'DEMO-031-BLK-40', '40', 'Đen', 2040000.00, 19, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(147, 39, 'DEMO-031-WHT-41', '41', 'Trắng', 1990000.00, 20, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(148, 39, 'DEMO-031-BLK-42', '42', 'Đen', 2040000.00, 21, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(149, 40, 'DEMO-032-GRY-39', '39', 'Xám', 2170000.00, 19, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(150, 40, 'DEMO-032-NVY-40', '40', 'Xanh Navy', 2220000.00, 20, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(151, 40, 'DEMO-032-GRY-41', '41', 'Xám', 2170000.00, 21, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(152, 40, 'DEMO-032-NVY-42', '42', 'Xanh Navy', 2220000.00, 22, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(153, 41, 'DEMO-033-BLKWHT-39', '39', 'Đen Trắng', 2350000.00, 20, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(154, 41, 'DEMO-033-BE-40', '40', 'Be', 2400000.00, 21, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(155, 41, 'DEMO-033-BLKWHT-41', '41', 'Đen Trắng', 2350000.00, 22, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(156, 41, 'DEMO-033-BE-42', '42', 'Be', 2400000.00, 23, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(157, 42, 'DEMO-034-WHT-39', '39', 'Trắng', 2530000.00, 21, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(158, 42, 'DEMO-034-BLK-40', '40', 'Đen', 2580000.00, 22, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(159, 42, 'DEMO-034-WHT-41', '41', 'Trắng', 2530000.00, 23, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(160, 42, 'DEMO-034-BLK-42', '42', 'Đen', 2580000.00, 6, 5, 'Selling', '2026-11-28 16:17:26', '2026-11-28 16:17:26'),
(161, 1, 'AF1-07-SEED-C1-39', '39', 'Trắng', 2915000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(162, 1, 'AF1-07-SEED-C1-43', '43', 'Trắng', 2915000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(163, 1, 'AF1-07-SEED-C2-39', '39', 'Đen', 2915000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(164, 1, 'AF1-07-SEED-C2-40', '40', 'Đen', 2915000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(165, 1, 'AF1-07-SEED-C2-42', '42', 'Đen', 2915000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(166, 1, 'AF1-07-SEED-C2-43', '43', 'Đen', 2915000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(167, 2, 'UB-LIGHT-SEED-C1-39', '39', 'Đen', 3875000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(168, 2, 'UB-LIGHT-SEED-C1-42', '42', 'Đen', 3875000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(169, 2, 'UB-LIGHT-SEED-C1-43', '43', 'Đen', 3875000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(170, 2, 'UB-LIGHT-SEED-C2-39', '39', 'Trắng', 3875000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(171, 2, 'UB-LIGHT-SEED-C2-40', '40', 'Trắng', 3875000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(172, 2, 'UB-LIGHT-SEED-C2-41', '41', 'Trắng', 3875000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(173, 2, 'UB-LIGHT-SEED-C2-43', '43', 'Trắng', 3875000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(174, 2, 'UB-LIGHT-SEED-C3-39', '39', 'Xanh Navy', 3875000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(175, 2, 'UB-LIGHT-SEED-C3-40', '40', 'Xanh Navy', 3875000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(176, 2, 'UB-LIGHT-SEED-C3-41', '41', 'Xanh Navy', 3875000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(177, 2, 'UB-LIGHT-SEED-C3-43', '43', 'Xanh Navy', 3875000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(178, 3, 'PUMA-VELO-SEED-C1-39', '39', 'Cam', 2610000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(179, 3, 'PUMA-VELO-SEED-C1-42', '42', 'Cam', 2610000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(180, 3, 'PUMA-VELO-SEED-C1-43', '43', 'Cam', 2610000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(181, 3, 'PUMA-VELO-SEED-C2-39', '39', 'Đen', 2610000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(182, 3, 'PUMA-VELO-SEED-C2-40', '40', 'Đen', 2610000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(183, 3, 'PUMA-VELO-SEED-C2-41', '41', 'Đen', 2610000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(184, 3, 'PUMA-VELO-SEED-C2-43', '43', 'Đen', 2610000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(185, 4, 'CV-CHUCK70-SEED-C1-41', '41', 'Đen', 1983333.33, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(186, 4, 'CV-CHUCK70-SEED-C1-42', '42', 'Đen', 1983333.33, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(187, 4, 'CV-CHUCK70-SEED-C1-43', '43', 'Đen', 1983333.33, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(188, 4, 'CV-CHUCK70-SEED-C2-39', '39', 'Vàng Kem', 1983333.33, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(189, 4, 'CV-CHUCK70-SEED-C2-40', '40', 'Vàng Kem', 1983333.33, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(190, 4, 'CV-CHUCK70-SEED-C2-42', '42', 'Vàng Kem', 1983333.33, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(191, 4, 'CV-CHUCK70-SEED-C2-43', '43', 'Vàng Kem', 1983333.33, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(192, 5, 'VANS-OLDSKOOL-SEED-C1-39', '39', 'Đen Trắng', 1850000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(193, 5, 'VANS-OLDSKOOL-SEED-C1-43', '43', 'Đen Trắng', 1850000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(194, 5, 'VANS-OLDSKOOL-SEED-C2-39', '39', 'Trắng', 1850000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(195, 5, 'VANS-OLDSKOOL-SEED-C2-40', '40', 'Trắng', 1850000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(196, 5, 'VANS-OLDSKOOL-SEED-C2-41', '41', 'Trắng', 1850000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(197, 5, 'VANS-OLDSKOOL-SEED-C2-42', '42', 'Trắng', 1850000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(198, 5, 'VANS-OLDSKOOL-SEED-C2-43', '43', 'Trắng', 1850000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(199, 6, 'NB-574-SEED-C1-39', '39', 'Xám', 2410000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(200, 6, 'NB-574-SEED-C1-42', '42', 'Xám', 2410000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(201, 6, 'NB-574-SEED-C1-43', '43', 'Xám', 2410000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(202, 6, 'NB-574-SEED-C2-39', '39', 'Xanh Navy', 2410000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(203, 6, 'NB-574-SEED-C2-40', '40', 'Xanh Navy', 2410000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(204, 6, 'NB-574-SEED-C2-41', '41', 'Xanh Navy', 2410000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(205, 6, 'NB-574-SEED-C2-43', '43', 'Xanh Navy', 2410000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(206, 7, 'NIKE-PEG40-SEED-C1-39', '39', 'Trắng Xanh', 3490000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(207, 7, 'NIKE-PEG40-SEED-C1-40', '40', 'Trắng Xanh', 3490000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(208, 7, 'NIKE-PEG40-SEED-C1-42', '42', 'Trắng Xanh', 3490000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(209, 7, 'NIKE-PEG40-SEED-C1-43', '43', 'Trắng Xanh', 3490000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(210, 7, 'NIKE-PEG40-SEED-C2-39', '39', 'Đen', 3490000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(211, 7, 'NIKE-PEG40-SEED-C2-40', '40', 'Đen', 3490000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(212, 7, 'NIKE-PEG40-SEED-C2-41', '41', 'Đen', 3490000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(213, 7, 'NIKE-PEG40-SEED-C2-43', '43', 'Đen', 3490000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(214, 8, 'AD-SAMBA-SEED-C1-39', '39', 'Trắng Sọc Đen', 2700000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(215, 8, 'AD-SAMBA-SEED-C1-42', '42', 'Trắng Sọc Đen', 2700000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(216, 8, 'AD-SAMBA-SEED-C1-43', '43', 'Trắng Sọc Đen', 2700000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(217, 8, 'AD-SAMBA-SEED-C2-39', '39', 'Trắng', 2700000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(218, 8, 'AD-SAMBA-SEED-C2-40', '40', 'Trắng', 2700000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(219, 8, 'AD-SAMBA-SEED-C2-41', '41', 'Trắng', 2700000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(220, 8, 'AD-SAMBA-SEED-C2-42', '42', 'Trắng', 2700000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(221, 8, 'AD-SAMBA-SEED-C2-43', '43', 'Trắng', 2700000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(222, 9, 'DEMO-001-SEED-C1-40', '40', 'Trắng', 1475000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(223, 9, 'DEMO-001-SEED-C1-42', '42', 'Trắng', 1475000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(224, 9, 'DEMO-001-SEED-C1-43', '43', 'Trắng', 1475000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(225, 9, 'DEMO-001-SEED-C2-39', '39', 'Đen', 1475000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(226, 9, 'DEMO-001-SEED-C2-41', '41', 'Đen', 1475000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(227, 9, 'DEMO-001-SEED-C2-43', '43', 'Đen', 1475000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(228, 10, 'DEMO-002-SEED-C1-40', '40', 'Xám', 1655000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(229, 10, 'DEMO-002-SEED-C1-42', '42', 'Xám', 1655000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(230, 10, 'DEMO-002-SEED-C1-43', '43', 'Xám', 1655000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(231, 10, 'DEMO-002-SEED-C2-39', '39', 'Xanh Navy', 1655000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(232, 10, 'DEMO-002-SEED-C2-41', '41', 'Xanh Navy', 1655000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(233, 10, 'DEMO-002-SEED-C2-43', '43', 'Xanh Navy', 1655000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(234, 11, 'DEMO-003-SEED-C1-40', '40', 'Đen Trắng', 1835000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(235, 11, 'DEMO-003-SEED-C1-42', '42', 'Đen Trắng', 1835000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(236, 11, 'DEMO-003-SEED-C1-43', '43', 'Đen Trắng', 1835000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(237, 11, 'DEMO-003-SEED-C2-39', '39', 'Be', 1835000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(238, 11, 'DEMO-003-SEED-C2-41', '41', 'Be', 1835000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(239, 11, 'DEMO-003-SEED-C2-43', '43', 'Be', 1835000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(240, 12, 'DEMO-004-SEED-C1-40', '40', 'Trắng', 2015000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(241, 12, 'DEMO-004-SEED-C1-42', '42', 'Trắng', 2015000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(242, 12, 'DEMO-004-SEED-C1-43', '43', 'Trắng', 2015000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(243, 12, 'DEMO-004-SEED-C2-39', '39', 'Đen', 2015000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(244, 12, 'DEMO-004-SEED-C2-41', '41', 'Đen', 2015000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(245, 12, 'DEMO-004-SEED-C2-43', '43', 'Đen', 2015000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(246, 13, 'DEMO-005-SEED-C1-40', '40', 'Xám', 2195000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(247, 13, 'DEMO-005-SEED-C1-42', '42', 'Xám', 2195000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(248, 13, 'DEMO-005-SEED-C1-43', '43', 'Xám', 2195000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(249, 13, 'DEMO-005-SEED-C2-39', '39', 'Xanh Navy', 2195000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(250, 13, 'DEMO-005-SEED-C2-41', '41', 'Xanh Navy', 2195000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(251, 13, 'DEMO-005-SEED-C2-43', '43', 'Xanh Navy', 2195000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(252, 14, 'DEMO-006-SEED-C1-40', '40', 'Đen Trắng', 2375000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(253, 14, 'DEMO-006-SEED-C1-42', '42', 'Đen Trắng', 2375000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(254, 14, 'DEMO-006-SEED-C1-43', '43', 'Đen Trắng', 2375000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(255, 14, 'DEMO-006-SEED-C2-39', '39', 'Be', 2375000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(256, 14, 'DEMO-006-SEED-C2-41', '41', 'Be', 2375000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(257, 14, 'DEMO-006-SEED-C2-43', '43', 'Be', 2375000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(258, 15, 'DEMO-007-SEED-C1-40', '40', 'Trắng', 2555000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(259, 15, 'DEMO-007-SEED-C1-42', '42', 'Trắng', 2555000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(260, 15, 'DEMO-007-SEED-C1-43', '43', 'Trắng', 2555000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(261, 15, 'DEMO-007-SEED-C2-39', '39', 'Đen', 2555000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(262, 15, 'DEMO-007-SEED-C2-41', '41', 'Đen', 2555000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(263, 15, 'DEMO-007-SEED-C2-43', '43', 'Đen', 2555000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(264, 16, 'DEMO-008-SEED-C1-40', '40', 'Xám', 2735000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(265, 16, 'DEMO-008-SEED-C1-42', '42', 'Xám', 2735000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(266, 16, 'DEMO-008-SEED-C1-43', '43', 'Xám', 2735000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(267, 16, 'DEMO-008-SEED-C2-39', '39', 'Xanh Navy', 2735000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(268, 16, 'DEMO-008-SEED-C2-41', '41', 'Xanh Navy', 2735000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(269, 16, 'DEMO-008-SEED-C2-43', '43', 'Xanh Navy', 2735000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(270, 17, 'DEMO-009-SEED-C1-40', '40', 'Đen Trắng', 2915000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(271, 17, 'DEMO-009-SEED-C1-42', '42', 'Đen Trắng', 2915000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(272, 17, 'DEMO-009-SEED-C1-43', '43', 'Đen Trắng', 2915000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(273, 17, 'DEMO-009-SEED-C2-39', '39', 'Be', 2915000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(274, 17, 'DEMO-009-SEED-C2-41', '41', 'Be', 2915000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(275, 17, 'DEMO-009-SEED-C2-43', '43', 'Be', 2915000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(276, 18, 'DEMO-010-SEED-C1-40', '40', 'Trắng', 1475000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(277, 18, 'DEMO-010-SEED-C1-42', '42', 'Trắng', 1475000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(278, 18, 'DEMO-010-SEED-C1-43', '43', 'Trắng', 1475000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(279, 18, 'DEMO-010-SEED-C2-39', '39', 'Đen', 1475000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(280, 18, 'DEMO-010-SEED-C2-41', '41', 'Đen', 1475000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(281, 18, 'DEMO-010-SEED-C2-43', '43', 'Đen', 1475000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(282, 19, 'DEMO-011-SEED-C1-40', '40', 'Xám', 1655000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(283, 19, 'DEMO-011-SEED-C1-42', '42', 'Xám', 1655000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(284, 19, 'DEMO-011-SEED-C1-43', '43', 'Xám', 1655000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(285, 19, 'DEMO-011-SEED-C2-39', '39', 'Xanh Navy', 1655000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(286, 19, 'DEMO-011-SEED-C2-41', '41', 'Xanh Navy', 1655000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(287, 19, 'DEMO-011-SEED-C2-43', '43', 'Xanh Navy', 1655000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(288, 20, 'DEMO-012-SEED-C1-40', '40', 'Đen Trắng', 1835000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(289, 20, 'DEMO-012-SEED-C1-42', '42', 'Đen Trắng', 1835000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(290, 20, 'DEMO-012-SEED-C1-43', '43', 'Đen Trắng', 1835000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(291, 20, 'DEMO-012-SEED-C2-39', '39', 'Be', 1835000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(292, 20, 'DEMO-012-SEED-C2-41', '41', 'Be', 1835000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(293, 20, 'DEMO-012-SEED-C2-43', '43', 'Be', 1835000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(294, 21, 'DEMO-013-SEED-C1-40', '40', 'Trắng', 2015000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(295, 21, 'DEMO-013-SEED-C1-42', '42', 'Trắng', 2015000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(296, 21, 'DEMO-013-SEED-C1-43', '43', 'Trắng', 2015000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(297, 21, 'DEMO-013-SEED-C2-39', '39', 'Đen', 2015000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(298, 21, 'DEMO-013-SEED-C2-41', '41', 'Đen', 2015000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(299, 21, 'DEMO-013-SEED-C2-43', '43', 'Đen', 2015000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(300, 22, 'DEMO-014-SEED-C1-40', '40', 'Xám', 2195000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(301, 22, 'DEMO-014-SEED-C1-42', '42', 'Xám', 2195000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(302, 22, 'DEMO-014-SEED-C1-43', '43', 'Xám', 2195000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(303, 22, 'DEMO-014-SEED-C2-39', '39', 'Xanh Navy', 2195000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(304, 22, 'DEMO-014-SEED-C2-41', '41', 'Xanh Navy', 2195000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(305, 22, 'DEMO-014-SEED-C2-43', '43', 'Xanh Navy', 2195000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(306, 23, 'DEMO-015-SEED-C1-40', '40', 'Đen Trắng', 2375000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(307, 23, 'DEMO-015-SEED-C1-42', '42', 'Đen Trắng', 2375000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(308, 23, 'DEMO-015-SEED-C1-43', '43', 'Đen Trắng', 2375000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(309, 23, 'DEMO-015-SEED-C2-39', '39', 'Be', 2375000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(310, 23, 'DEMO-015-SEED-C2-41', '41', 'Be', 2375000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(311, 23, 'DEMO-015-SEED-C2-43', '43', 'Be', 2375000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(312, 24, 'DEMO-016-SEED-C1-40', '40', 'Trắng', 2555000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(313, 24, 'DEMO-016-SEED-C1-42', '42', 'Trắng', 2555000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(314, 24, 'DEMO-016-SEED-C1-43', '43', 'Trắng', 2555000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(315, 24, 'DEMO-016-SEED-C2-39', '39', 'Đen', 2555000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(316, 24, 'DEMO-016-SEED-C2-41', '41', 'Đen', 2555000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(317, 24, 'DEMO-016-SEED-C2-43', '43', 'Đen', 2555000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(318, 25, 'DEMO-017-SEED-C1-40', '40', 'Xám', 2735000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(319, 25, 'DEMO-017-SEED-C1-42', '42', 'Xám', 2735000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(320, 25, 'DEMO-017-SEED-C1-43', '43', 'Xám', 2735000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(321, 25, 'DEMO-017-SEED-C2-39', '39', 'Xanh Navy', 2735000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(322, 25, 'DEMO-017-SEED-C2-41', '41', 'Xanh Navy', 2735000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(323, 25, 'DEMO-017-SEED-C2-43', '43', 'Xanh Navy', 2735000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(324, 26, 'DEMO-018-SEED-C1-40', '40', 'Đen Trắng', 2915000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(325, 26, 'DEMO-018-SEED-C1-42', '42', 'Đen Trắng', 2915000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(326, 26, 'DEMO-018-SEED-C1-43', '43', 'Đen Trắng', 2915000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(327, 26, 'DEMO-018-SEED-C2-39', '39', 'Be', 2915000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(328, 26, 'DEMO-018-SEED-C2-41', '41', 'Be', 2915000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(329, 26, 'DEMO-018-SEED-C2-43', '43', 'Be', 2915000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(330, 27, 'DEMO-019-SEED-C1-40', '40', 'Trắng', 1475000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(331, 27, 'DEMO-019-SEED-C1-42', '42', 'Trắng', 1475000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(332, 27, 'DEMO-019-SEED-C1-43', '43', 'Trắng', 1475000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(333, 27, 'DEMO-019-SEED-C2-39', '39', 'Đen', 1475000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(334, 27, 'DEMO-019-SEED-C2-41', '41', 'Đen', 1475000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(335, 27, 'DEMO-019-SEED-C2-43', '43', 'Đen', 1475000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(336, 28, 'DEMO-020-SEED-C1-40', '40', 'Xám', 1655000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(337, 28, 'DEMO-020-SEED-C1-42', '42', 'Xám', 1655000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(338, 28, 'DEMO-020-SEED-C1-43', '43', 'Xám', 1655000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(339, 28, 'DEMO-020-SEED-C2-39', '39', 'Xanh Navy', 1655000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(340, 28, 'DEMO-020-SEED-C2-41', '41', 'Xanh Navy', 1655000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(341, 28, 'DEMO-020-SEED-C2-43', '43', 'Xanh Navy', 1655000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(342, 29, 'DEMO-021-SEED-C1-40', '40', 'Đen Trắng', 1835000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(343, 29, 'DEMO-021-SEED-C1-42', '42', 'Đen Trắng', 1835000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(344, 29, 'DEMO-021-SEED-C1-43', '43', 'Đen Trắng', 1835000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(345, 29, 'DEMO-021-SEED-C2-39', '39', 'Be', 1835000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(346, 29, 'DEMO-021-SEED-C2-41', '41', 'Be', 1835000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(347, 29, 'DEMO-021-SEED-C2-43', '43', 'Be', 1835000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(348, 30, 'DEMO-022-SEED-C1-40', '40', 'Trắng', 2015000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(349, 30, 'DEMO-022-SEED-C1-42', '42', 'Trắng', 2015000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(350, 30, 'DEMO-022-SEED-C1-43', '43', 'Trắng', 2015000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(351, 30, 'DEMO-022-SEED-C2-39', '39', 'Đen', 2015000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(352, 30, 'DEMO-022-SEED-C2-41', '41', 'Đen', 2015000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(353, 30, 'DEMO-022-SEED-C2-43', '43', 'Đen', 2015000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(354, 31, 'DEMO-023-SEED-C1-40', '40', 'Xám', 2195000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(355, 31, 'DEMO-023-SEED-C1-42', '42', 'Xám', 2195000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(356, 31, 'DEMO-023-SEED-C1-43', '43', 'Xám', 2195000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(357, 31, 'DEMO-023-SEED-C2-39', '39', 'Xanh Navy', 2195000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(358, 31, 'DEMO-023-SEED-C2-41', '41', 'Xanh Navy', 2195000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(359, 31, 'DEMO-023-SEED-C2-43', '43', 'Xanh Navy', 2195000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(360, 32, 'DEMO-024-SEED-C1-40', '40', 'Đen Trắng', 2375000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(361, 32, 'DEMO-024-SEED-C1-42', '42', 'Đen Trắng', 2375000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(362, 32, 'DEMO-024-SEED-C1-43', '43', 'Đen Trắng', 2375000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(363, 32, 'DEMO-024-SEED-C2-39', '39', 'Be', 2375000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(364, 32, 'DEMO-024-SEED-C2-41', '41', 'Be', 2375000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(365, 32, 'DEMO-024-SEED-C2-43', '43', 'Be', 2375000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(366, 33, 'DEMO-025-SEED-C1-40', '40', 'Trắng', 2555000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(367, 33, 'DEMO-025-SEED-C1-42', '42', 'Trắng', 2555000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(368, 33, 'DEMO-025-SEED-C1-43', '43', 'Trắng', 2555000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(369, 33, 'DEMO-025-SEED-C2-39', '39', 'Đen', 2555000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(370, 33, 'DEMO-025-SEED-C2-41', '41', 'Đen', 2555000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(371, 33, 'DEMO-025-SEED-C2-43', '43', 'Đen', 2555000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(372, 34, 'DEMO-026-SEED-C1-40', '40', 'Xám', 2735000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(373, 34, 'DEMO-026-SEED-C1-42', '42', 'Xám', 2735000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(374, 34, 'DEMO-026-SEED-C1-43', '43', 'Xám', 2735000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(375, 34, 'DEMO-026-SEED-C2-39', '39', 'Xanh Navy', 2735000.00, 17, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(376, 34, 'DEMO-026-SEED-C2-41', '41', 'Xanh Navy', 2735000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(377, 34, 'DEMO-026-SEED-C2-43', '43', 'Xanh Navy', 2735000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(378, 35, 'DEMO-027-SEED-C1-40', '40', 'Đen Trắng', 2915000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(379, 35, 'DEMO-027-SEED-C1-42', '42', 'Đen Trắng', 2915000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(380, 35, 'DEMO-027-SEED-C1-43', '43', 'Đen Trắng', 2915000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(381, 35, 'DEMO-027-SEED-C2-39', '39', 'Be', 2915000.00, 18, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(382, 35, 'DEMO-027-SEED-C2-41', '41', 'Be', 2915000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(383, 35, 'DEMO-027-SEED-C2-43', '43', 'Be', 2915000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(384, 36, 'DEMO-028-SEED-C1-40', '40', 'Trắng', 1475000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(385, 36, 'DEMO-028-SEED-C1-42', '42', 'Trắng', 1475000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(386, 36, 'DEMO-028-SEED-C1-43', '43', 'Trắng', 1475000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(387, 36, 'DEMO-028-SEED-C2-39', '39', 'Đen', 1475000.00, 19, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(388, 36, 'DEMO-028-SEED-C2-41', '41', 'Đen', 1475000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(389, 36, 'DEMO-028-SEED-C2-43', '43', 'Đen', 1475000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(390, 37, 'DEMO-029-SEED-C1-40', '40', 'Xám', 1655000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(391, 37, 'DEMO-029-SEED-C1-42', '42', 'Xám', 1655000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(392, 37, 'DEMO-029-SEED-C1-43', '43', 'Xám', 1655000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(393, 37, 'DEMO-029-SEED-C2-39', '39', 'Xanh Navy', 1655000.00, 20, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(394, 37, 'DEMO-029-SEED-C2-41', '41', 'Xanh Navy', 1655000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(395, 37, 'DEMO-029-SEED-C2-43', '43', 'Xanh Navy', 1655000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(396, 38, 'DEMO-030-SEED-C1-40', '40', 'Đen Trắng', 1835000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(397, 38, 'DEMO-030-SEED-C1-42', '42', 'Đen Trắng', 1835000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(398, 38, 'DEMO-030-SEED-C1-43', '43', 'Đen Trắng', 1835000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(399, 38, 'DEMO-030-SEED-C2-39', '39', 'Be', 1835000.00, 0, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(400, 38, 'DEMO-030-SEED-C2-41', '41', 'Be', 1835000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(401, 38, 'DEMO-030-SEED-C2-43', '43', 'Be', 1835000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(402, 39, 'DEMO-031-SEED-C1-40', '40', 'Trắng', 2015000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(403, 39, 'DEMO-031-SEED-C1-42', '42', 'Trắng', 2015000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(404, 39, 'DEMO-031-SEED-C1-43', '43', 'Trắng', 2015000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(405, 39, 'DEMO-031-SEED-C2-39', '39', 'Đen', 2015000.00, 9, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(406, 39, 'DEMO-031-SEED-C2-41', '41', 'Đen', 2015000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(407, 39, 'DEMO-031-SEED-C2-43', '43', 'Đen', 2015000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(408, 40, 'DEMO-032-SEED-C1-40', '40', 'Xám', 2195000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(409, 40, 'DEMO-032-SEED-C1-42', '42', 'Xám', 2195000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(410, 40, 'DEMO-032-SEED-C1-43', '43', 'Xám', 2195000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(411, 40, 'DEMO-032-SEED-C2-39', '39', 'Xanh Navy', 2195000.00, 10, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09');
INSERT INTO `product_variants` (`variant_id`, `product_id`, `sku`, `size`, `color`, `price`, `stock_quantity`, `low_stock_threshold`, `status`, `created_at`, `updated_at`) VALUES
(412, 40, 'DEMO-032-SEED-C2-41', '41', 'Xanh Navy', 2195000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(413, 40, 'DEMO-032-SEED-C2-43', '43', 'Xanh Navy', 2195000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(414, 41, 'DEMO-033-SEED-C1-40', '40', 'Đen Trắng', 2375000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(415, 41, 'DEMO-033-SEED-C1-42', '42', 'Đen Trắng', 2375000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(416, 41, 'DEMO-033-SEED-C1-43', '43', 'Đen Trắng', 2375000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(417, 41, 'DEMO-033-SEED-C2-39', '39', 'Be', 2375000.00, 11, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(418, 41, 'DEMO-033-SEED-C2-41', '41', 'Be', 2375000.00, 13, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(419, 41, 'DEMO-033-SEED-C2-43', '43', 'Be', 2375000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(420, 42, 'DEMO-034-SEED-C1-40', '40', 'Trắng', 2555000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(421, 42, 'DEMO-034-SEED-C1-42', '42', 'Trắng', 2555000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(422, 42, 'DEMO-034-SEED-C1-43', '43', 'Trắng', 2555000.00, 15, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(423, 42, 'DEMO-034-SEED-C2-39', '39', 'Đen', 2555000.00, 12, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(424, 42, 'DEMO-034-SEED-C2-41', '41', 'Đen', 2555000.00, 14, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09'),
(425, 42, 'DEMO-034-SEED-C2-43', '43', 'Đen', 2555000.00, 16, 5, 'Selling', '2026-11-28 16:30:09', '2026-11-28 16:30:09');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `promotions`
--

CREATE TABLE `promotions` (
  `promotion_id` int NOT NULL,
  `promo_code` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `discount_amount` decimal(15,2) NOT NULL,
  `min_order_value` decimal(15,2) NOT NULL DEFAULT '0.00',
  `start_date` datetime NOT NULL,
  `end_date` datetime NOT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ;

--
-- Đang đổ dữ liệu cho bảng `promotions`
--

INSERT INTO `promotions` (`promotion_id`, `promo_code`, `discount_amount`, `min_order_value`, `start_date`, `end_date`, `is_active`, `created_at`, `updated_at`) VALUES
(1, 'CHAO100', 100000.00, 1500000.00, '2026-10-22 13:59:39', '2027-11-21 13:59:39', 1, '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(2, 'TET2026', 200000.00, 2000000.00, '2026-11-11 13:59:39', '2027-11-21 13:59:39', 1, '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(3, 'VIP50', 50000.00, 500000.00, '2026-10-22 13:59:39', '2027-11-21 13:59:39', 1, '2026-11-21 13:59:39', '2026-11-21 13:59:39');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `roles`
--

CREATE TABLE `roles` (
  `role_id` int NOT NULL,
  `role_name` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `roles`
--

INSERT INTO `roles` (`role_id`, `role_name`) VALUES
(1, 'Admin'),
(3, 'Customer'),
(2, 'Employee');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `stock_documents`
--

CREATE TABLE `stock_documents` (
  `document_id` int NOT NULL,
  `document_code` varchar(50) COLLATE utf8mb4_unicode_ci NOT NULL,
  `document_type` enum('PurchaseReceipt','AdjustmentIn','AdjustmentOut','OrderIssue','OrderCancellationReceipt') COLLATE utf8mb4_unicode_ci NOT NULL,
  `document_status` enum('Draft','Confirmed') COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT 'Draft',
  `supplier_id` int DEFAULT NULL,
  `order_id` int DEFAULT NULL,
  `reason` text COLLATE utf8mb4_unicode_ci NOT NULL,
  `created_by_user_id` int NOT NULL,
  `confirmed_by_user_id` int DEFAULT NULL,
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `confirmed_at` datetime DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `stock_documents`
--

INSERT INTO `stock_documents` (`document_id`, `document_code`, `document_type`, `document_status`, `supplier_id`, `order_id`, `reason`, `created_by_user_id`, `confirmed_by_user_id`, `created_at`, `confirmed_at`) VALUES
(1, 'PN-20261101-01', 'PurchaseReceipt', 'Confirmed', 1, NULL, 'Nhập hàng đầu kỳ từ nhà cung cấp Nike Việt Nam', 1, 1, '2026-11-11 13:59:40', '2026-11-11 13:59:40'),
(2, 'XK-SS261121161129aa4d8f8eb875', 'OrderIssue', 'Confirmed', NULL, 3, 'Tự động xuất kho cho đơn hàng SS261121161129aa4d8f8eb875', 4, 4, '2026-11-21 16:11:30', '2026-11-21 16:11:30'),
(3, 'PN-261121162509f8938de982c2', 'PurchaseReceipt', 'Confirmed', 3, NULL, 'Nhập hàng bổ sung từ nhà cung cấp', 1, 1, '2026-11-21 16:25:10', '2026-11-21 16:25:10'),
(4, 'XK-SS261128144955be5056c8ab12', 'OrderIssue', 'Confirmed', NULL, 4, 'Tự động xuất kho cho đơn hàng SS261128144955be5056c8ab12', 4, 4, '2026-11-28 14:49:56', '2026-11-28 14:49:56');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `stock_document_items`
--

CREATE TABLE `stock_document_items` (
  `document_item_id` int NOT NULL,
  `document_id` int NOT NULL,
  `variant_id` int NOT NULL,
  `quantity` int NOT NULL,
  `unit_cost` decimal(15,2) DEFAULT NULL,
  `note` text COLLATE utf8mb4_unicode_ci
) ;

--
-- Đang đổ dữ liệu cho bảng `stock_document_items`
--

INSERT INTO `stock_document_items` (`document_item_id`, `document_id`, `variant_id`, `quantity`, `unit_cost`, `note`) VALUES
(1, 1, 1, 15, 1734000.00, 'Lô hàng mới nguyên hộp'),
(2, 1, 2, 12, 1734000.00, 'Lô hàng mới nguyên hộp'),
(3, 1, 3, 20, 1734000.00, 'Lô hàng mới nguyên hộp'),
(4, 1, 4, 8, 1794000.00, 'Lô hàng mới nguyên hộp'),
(5, 1, 5, 10, 2310000.00, 'Lô hàng mới nguyên hộp'),
(6, 2, 6, 1, 3850000.00, 'Xuất cho đơn SS261121161129aa4d8f8eb875'),
(7, 3, 8, 10, NULL, ''),
(8, 4, 4, 1, 2990000.00, 'Xuất cho đơn SS261128144955be5056c8ab12');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `suppliers`
--

CREATE TABLE `suppliers` (
  `supplier_id` int NOT NULL,
  `supplier_name` varchar(200) COLLATE utf8mb4_unicode_ci NOT NULL,
  `phone` varchar(20) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `email` varchar(255) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `address` varchar(1000) COLLATE utf8mb4_unicode_ci DEFAULT NULL,
  `is_cooperating` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `suppliers`
--

INSERT INTO `suppliers` (`supplier_id`, `supplier_name`, `phone`, `email`, `address`, `is_cooperating`, `created_at`, `updated_at`) VALUES
(1, 'Công ty TNHH Nike Việt Nam', '02838221234', 'supply@nike.com.vn', 'Tầng 8, Tòa nhà Bitexco, Q.1, TP. Hồ Chí Minh', 1, '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(2, 'Công ty TNHH Adidas Việt Nam', '02839115678', 'vietnam@adidas.com', 'Tòa nhà Deutsches Haus, Q.1, TP. Hồ Chí Minh', 1, '2026-11-21 13:59:39', '2026-11-21 13:59:39'),
(3, 'Công ty CP Thời trang thể thao Đông Á', '02837449999', 'contact@donga-sport.vn', 'Lô B2, KCN Tân Bình, TP. Hồ Chí Minh', 1, '2026-11-21 13:59:39', '2026-11-21 13:59:39');

-- --------------------------------------------------------

--
-- Cấu trúc bảng cho bảng `users`
--

CREATE TABLE `users` (
  `user_id` int NOT NULL,
  `email` varchar(255) COLLATE utf8mb4_unicode_ci NOT NULL,
  `password_hash` varchar(500) COLLATE utf8mb4_unicode_ci NOT NULL,
  `full_name` varchar(150) COLLATE utf8mb4_unicode_ci NOT NULL,
  `phone` varchar(20) COLLATE utf8mb4_unicode_ci NOT NULL,
  `role_id` int NOT NULL,
  `is_active` tinyint(1) NOT NULL DEFAULT '1',
  `created_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP,
  `updated_at` datetime NOT NULL DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

--
-- Đang đổ dữ liệu cho bảng `users`
--

INSERT INTO `users` (`user_id`, `email`, `password_hash`, `full_name`, `phone`, `role_id`, `is_active`, `created_at`, `updated_at`) VALUES
(1, 'admin@shoestore.vn', '$2a$11$zSvcODooMQN8kEgL3AZSl.s8pLm3/ySIJIDiqVIvOdYzBraj.rXMG', 'Administrator', '0901234567', 1, 1, '2026-11-21 13:59:38', '2026-11-21 15:28:29'),
(2, 'linh@shoestore.com', '$2a$11$Tnmqdtz9nvwD1pA25oCpv.J/J4oJfWGuolNaZQUA7VTdLwyZjEHA2', 'Trần Thảo Linh', '0912345678', 2, 1, '2026-11-21 13:59:38', '2026-11-21 13:59:38'),
(3, 'minhanh@example.com', '$2a$11$flrIjdqSdNRni05Y.2hJEeFaL9keer9t0ZFVqRpfI/.FK.eZfV9kO', 'Nguyễn Minh Anh', '0987654321', 3, 1, '2026-11-21 13:59:38', '2026-11-21 13:59:38'),
(4, 'anhminh142857@gmail.com', '$2a$11$CqHkiag3GWJM23aNI28eC.do.xqqTmAd8cMf1OV6mgYXbjnHZO1yG', 'Bùi Anh Minh', '0869799470', 3, 1, '2026-11-21 16:08:43', '2026-11-21 16:08:43'),
(5, 'nvkiet@gmail.com', '$2a$11$tFS41VMq8ZxC2OMJqybTY.c2iML1HeacUfs3VcT6RhrO7.92Zotfe', 'Bùi Anh Kiệt', '0909090909', 2, 1, '2026-11-21 16:14:38', '2026-11-21 16:14:38');

--
-- Chỉ mục cho các bảng đã đổ
--

--
-- Chỉ mục cho bảng `brands`
--
ALTER TABLE `brands`
  ADD PRIMARY KEY (`brand_id`),
  ADD UNIQUE KEY `uq_brands_name` (`brand_name`);

--
-- Chỉ mục cho bảng `carts`
--
ALTER TABLE `carts`
  ADD PRIMARY KEY (`cart_id`),
  ADD UNIQUE KEY `uq_carts_customer` (`customer_id`);

--
-- Chỉ mục cho bảng `cart_items`
--
ALTER TABLE `cart_items`
  ADD PRIMARY KEY (`cart_item_id`),
  ADD UNIQUE KEY `uq_cart_items_variant` (`cart_id`,`variant_id`),
  ADD KEY `fk_cart_items_variant` (`variant_id`);

--
-- Chỉ mục cho bảng `categories`
--
ALTER TABLE `categories`
  ADD PRIMARY KEY (`category_id`),
  ADD UNIQUE KEY `uq_categories_name` (`category_name`);

--
-- Chỉ mục cho bảng `customers`
--
ALTER TABLE `customers`
  ADD PRIMARY KEY (`customer_id`);

--
-- Chỉ mục cho bảng `customer_addresses`
--
ALTER TABLE `customer_addresses`
  ADD PRIMARY KEY (`address_id`),
  ADD UNIQUE KEY `uq_customer_default_address` (`default_customer_id`),
  ADD KEY `fk_addresses_customer` (`customer_id`);

--
-- Chỉ mục cho bảng `employees`
--
ALTER TABLE `employees`
  ADD PRIMARY KEY (`employee_id`);

--
-- Chỉ mục cho bảng `orders`
--
ALTER TABLE `orders`
  ADD PRIMARY KEY (`order_id`),
  ADD UNIQUE KEY `uq_orders_code` (`order_code`),
  ADD KEY `fk_orders_promotion` (`promotion_id`),
  ADD KEY `fk_orders_cancelled_by` (`cancelled_by_user_id`),
  ADD KEY `ix_orders_customer_date` (`customer_id`,`created_at`),
  ADD KEY `ix_orders_status_date` (`order_status`,`created_at`),
  ADD KEY `ix_orders_completed_at` (`completed_at`);

--
-- Chỉ mục cho bảng `order_details`
--
ALTER TABLE `order_details`
  ADD PRIMARY KEY (`order_detail_id`),
  ADD UNIQUE KEY `uq_order_details_variant` (`order_id`,`variant_id`),
  ADD KEY `fk_order_details_variant` (`variant_id`);

--
-- Chỉ mục cho bảng `order_status_history`
--
ALTER TABLE `order_status_history`
  ADD PRIMARY KEY (`history_id`),
  ADD KEY `fk_order_history_user` (`changed_by_user_id`),
  ADD KEY `ix_order_history_order_time` (`order_id`,`changed_at`);

--
-- Chỉ mục cho bảng `payments`
--
ALTER TABLE `payments`
  ADD PRIMARY KEY (`payment_id`),
  ADD UNIQUE KEY `uq_payments_order` (`order_id`),
  ADD KEY `fk_payments_confirmed_by` (`confirmed_by_user_id`),
  ADD KEY `ix_payments_status` (`payment_status`);

--
-- Chỉ mục cho bảng `products`
--
ALTER TABLE `products`
  ADD PRIMARY KEY (`product_id`),
  ADD UNIQUE KEY `uq_products_code` (`product_code`),
  ADD KEY `fk_products_category` (`category_id`),
  ADD KEY `fk_products_brand` (`brand_id`),
  ADD KEY `fk_products_supplier` (`supplier_id`),
  ADD KEY `ix_products_name` (`product_name`),
  ADD KEY `ix_products_status` (`status`);

--
-- Chỉ mục cho bảng `product_images`
--
ALTER TABLE `product_images`
  ADD PRIMARY KEY (`image_id`),
  ADD KEY `ix_product_images_product_sort` (`product_id`,`sort_order`);

--
-- Chỉ mục cho bảng `product_reviews`
--
ALTER TABLE `product_reviews`
  ADD PRIMARY KEY (`review_id`),
  ADD UNIQUE KEY `uq_reviews_customer_product` (`product_id`,`customer_id`),
  ADD KEY `fk_reviews_customer` (`customer_id`),
  ADD KEY `ix_reviews_product_rating` (`product_id`,`rating`);

--
-- Chỉ mục cho bảng `product_variants`
--
ALTER TABLE `product_variants`
  ADD PRIMARY KEY (`variant_id`),
  ADD UNIQUE KEY `uq_variants_sku` (`sku`),
  ADD UNIQUE KEY `uq_variants_product_size_color` (`product_id`,`size`,`color`),
  ADD KEY `ix_variants_status_price` (`status`,`price`);

--
-- Chỉ mục cho bảng `promotions`
--
ALTER TABLE `promotions`
  ADD PRIMARY KEY (`promotion_id`),
  ADD UNIQUE KEY `uq_promotions_code` (`promo_code`);

--
-- Chỉ mục cho bảng `roles`
--
ALTER TABLE `roles`
  ADD PRIMARY KEY (`role_id`),
  ADD UNIQUE KEY `uq_roles_name` (`role_name`);

--
-- Chỉ mục cho bảng `stock_documents`
--
ALTER TABLE `stock_documents`
  ADD PRIMARY KEY (`document_id`),
  ADD UNIQUE KEY `uq_stock_documents_code` (`document_code`),
  ADD UNIQUE KEY `uq_stock_documents_order_type` (`order_id`,`document_type`),
  ADD KEY `fk_stock_documents_supplier` (`supplier_id`),
  ADD KEY `fk_stock_documents_created_by` (`created_by_user_id`),
  ADD KEY `fk_stock_documents_confirmed_by` (`confirmed_by_user_id`),
  ADD KEY `ix_stock_documents_type_date` (`document_type`,`created_at`),
  ADD KEY `ix_stock_documents_status` (`document_status`);

--
-- Chỉ mục cho bảng `stock_document_items`
--
ALTER TABLE `stock_document_items`
  ADD PRIMARY KEY (`document_item_id`),
  ADD UNIQUE KEY `uq_stock_items_document_variant` (`document_id`,`variant_id`),
  ADD KEY `fk_stock_items_variant` (`variant_id`);

--
-- Chỉ mục cho bảng `suppliers`
--
ALTER TABLE `suppliers`
  ADD PRIMARY KEY (`supplier_id`),
  ADD KEY `ix_suppliers_name` (`supplier_name`),
  ADD KEY `ix_suppliers_phone` (`phone`);

--
-- Chỉ mục cho bảng `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`user_id`),
  ADD UNIQUE KEY `uq_users_email` (`email`),
  ADD KEY `fk_users_role` (`role_id`),
  ADD KEY `ix_users_full_name` (`full_name`),
  ADD KEY `ix_users_phone` (`phone`);

--
-- AUTO_INCREMENT cho các bảng đã đổ
--

--
-- AUTO_INCREMENT cho bảng `brands`
--
ALTER TABLE `brands`
  MODIFY `brand_id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=7;

--
-- AUTO_INCREMENT cho bảng `carts`
--
ALTER TABLE `carts`
  MODIFY `cart_id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT cho bảng `cart_items`
--
ALTER TABLE `cart_items`
  MODIFY `cart_item_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT cho bảng `categories`
--
ALTER TABLE `categories`
  MODIFY `category_id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- AUTO_INCREMENT cho bảng `customer_addresses`
--
ALTER TABLE `customer_addresses`
  MODIFY `address_id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=3;

--
-- AUTO_INCREMENT cho bảng `orders`
--
ALTER TABLE `orders`
  MODIFY `order_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT cho bảng `order_details`
--
ALTER TABLE `order_details`
  MODIFY `order_detail_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT cho bảng `order_status_history`
--
ALTER TABLE `order_status_history`
  MODIFY `history_id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=17;

--
-- AUTO_INCREMENT cho bảng `payments`
--
ALTER TABLE `payments`
  MODIFY `payment_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT cho bảng `products`
--
ALTER TABLE `products`
  MODIFY `product_id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=43;

--
-- AUTO_INCREMENT cho bảng `product_images`
--
ALTER TABLE `product_images`
  MODIFY `image_id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT cho bảng `product_reviews`
--
ALTER TABLE `product_reviews`
  MODIFY `review_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT cho bảng `product_variants`
--
ALTER TABLE `product_variants`
  MODIFY `variant_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT cho bảng `promotions`
--
ALTER TABLE `promotions`
  MODIFY `promotion_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT cho bảng `roles`
--
ALTER TABLE `roles`
  MODIFY `role_id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT cho bảng `stock_documents`
--
ALTER TABLE `stock_documents`
  MODIFY `document_id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=5;

--
-- AUTO_INCREMENT cho bảng `stock_document_items`
--
ALTER TABLE `stock_document_items`
  MODIFY `document_item_id` int NOT NULL AUTO_INCREMENT;

--
-- AUTO_INCREMENT cho bảng `suppliers`
--
ALTER TABLE `suppliers`
  MODIFY `supplier_id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=4;

--
-- AUTO_INCREMENT cho bảng `users`
--
ALTER TABLE `users`
  MODIFY `user_id` int NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=6;

--
-- Các ràng buộc cho các bảng đã đổ
--

--
-- Các ràng buộc cho bảng `carts`
--
ALTER TABLE `carts`
  ADD CONSTRAINT `fk_carts_customer` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`customer_id`) ON DELETE CASCADE;

--
-- Các ràng buộc cho bảng `cart_items`
--
ALTER TABLE `cart_items`
  ADD CONSTRAINT `fk_cart_items_cart` FOREIGN KEY (`cart_id`) REFERENCES `carts` (`cart_id`) ON DELETE CASCADE,
  ADD CONSTRAINT `fk_cart_items_variant` FOREIGN KEY (`variant_id`) REFERENCES `product_variants` (`variant_id`) ON DELETE RESTRICT;

--
-- Các ràng buộc cho bảng `customers`
--
ALTER TABLE `customers`
  ADD CONSTRAINT `fk_customers_user` FOREIGN KEY (`customer_id`) REFERENCES `users` (`user_id`) ON DELETE CASCADE;

--
-- Các ràng buộc cho bảng `customer_addresses`
--
ALTER TABLE `customer_addresses`
  ADD CONSTRAINT `fk_addresses_customer` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`customer_id`) ON DELETE RESTRICT;

--
-- Các ràng buộc cho bảng `employees`
--
ALTER TABLE `employees`
  ADD CONSTRAINT `fk_employees_user` FOREIGN KEY (`employee_id`) REFERENCES `users` (`user_id`) ON DELETE CASCADE;

--
-- Các ràng buộc cho bảng `orders`
--
ALTER TABLE `orders`
  ADD CONSTRAINT `fk_orders_cancelled_by` FOREIGN KEY (`cancelled_by_user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT,
  ADD CONSTRAINT `fk_orders_customer` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`customer_id`) ON DELETE RESTRICT,
  ADD CONSTRAINT `fk_orders_promotion` FOREIGN KEY (`promotion_id`) REFERENCES `promotions` (`promotion_id`) ON DELETE RESTRICT;

--
-- Các ràng buộc cho bảng `order_details`
--
ALTER TABLE `order_details`
  ADD CONSTRAINT `fk_order_details_order` FOREIGN KEY (`order_id`) REFERENCES `orders` (`order_id`) ON DELETE RESTRICT,
  ADD CONSTRAINT `fk_order_details_variant` FOREIGN KEY (`variant_id`) REFERENCES `product_variants` (`variant_id`) ON DELETE RESTRICT;

--
-- Các ràng buộc cho bảng `order_status_history`
--
ALTER TABLE `order_status_history`
  ADD CONSTRAINT `fk_order_history_order` FOREIGN KEY (`order_id`) REFERENCES `orders` (`order_id`) ON DELETE RESTRICT,
  ADD CONSTRAINT `fk_order_history_user` FOREIGN KEY (`changed_by_user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT;

--
-- Các ràng buộc cho bảng `payments`
--
ALTER TABLE `payments`
  ADD CONSTRAINT `fk_payments_confirmed_by` FOREIGN KEY (`confirmed_by_user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT,
  ADD CONSTRAINT `fk_payments_order` FOREIGN KEY (`order_id`) REFERENCES `orders` (`order_id`) ON DELETE RESTRICT;

--
-- Các ràng buộc cho bảng `products`
--
ALTER TABLE `products`
  ADD CONSTRAINT `fk_products_brand` FOREIGN KEY (`brand_id`) REFERENCES `brands` (`brand_id`) ON DELETE RESTRICT,
  ADD CONSTRAINT `fk_products_category` FOREIGN KEY (`category_id`) REFERENCES `categories` (`category_id`) ON DELETE RESTRICT,
  ADD CONSTRAINT `fk_products_supplier` FOREIGN KEY (`supplier_id`) REFERENCES `suppliers` (`supplier_id`) ON DELETE RESTRICT;

--
-- Các ràng buộc cho bảng `product_images`
--
ALTER TABLE `product_images`
  ADD CONSTRAINT `fk_product_images_product` FOREIGN KEY (`product_id`) REFERENCES `products` (`product_id`) ON DELETE CASCADE;

--
-- Các ràng buộc cho bảng `product_reviews`
--
ALTER TABLE `product_reviews`
  ADD CONSTRAINT `fk_reviews_customer` FOREIGN KEY (`customer_id`) REFERENCES `customers` (`customer_id`) ON DELETE RESTRICT,
  ADD CONSTRAINT `fk_reviews_product` FOREIGN KEY (`product_id`) REFERENCES `products` (`product_id`) ON DELETE RESTRICT;

--
-- Các ràng buộc cho bảng `product_variants`
--
ALTER TABLE `product_variants`
  ADD CONSTRAINT `fk_variants_product` FOREIGN KEY (`product_id`) REFERENCES `products` (`product_id`) ON DELETE RESTRICT;

--
-- Các ràng buộc cho bảng `stock_documents`
--
ALTER TABLE `stock_documents`
  ADD CONSTRAINT `fk_stock_documents_confirmed_by` FOREIGN KEY (`confirmed_by_user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT,
  ADD CONSTRAINT `fk_stock_documents_created_by` FOREIGN KEY (`created_by_user_id`) REFERENCES `users` (`user_id`) ON DELETE RESTRICT,
  ADD CONSTRAINT `fk_stock_documents_order` FOREIGN KEY (`order_id`) REFERENCES `orders` (`order_id`) ON DELETE RESTRICT,
  ADD CONSTRAINT `fk_stock_documents_supplier` FOREIGN KEY (`supplier_id`) REFERENCES `suppliers` (`supplier_id`) ON DELETE RESTRICT;

--
-- Các ràng buộc cho bảng `stock_document_items`
--
ALTER TABLE `stock_document_items`
  ADD CONSTRAINT `fk_stock_items_document` FOREIGN KEY (`document_id`) REFERENCES `stock_documents` (`document_id`) ON DELETE RESTRICT,
  ADD CONSTRAINT `fk_stock_items_variant` FOREIGN KEY (`variant_id`) REFERENCES `product_variants` (`variant_id`) ON DELETE RESTRICT;

--
-- Các ràng buộc cho bảng `users`
--
ALTER TABLE `users`
  ADD CONSTRAINT `fk_users_role` FOREIGN KEY (`role_id`) REFERENCES `roles` (`role_id`) ON DELETE RESTRICT;
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
