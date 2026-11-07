-- 1. Roles
INSERT INTO roles (role_name) VALUES ('Admin'), ('Employee'), ('Customer');

-- 2. Users (Admin, Employee, Customer)
INSERT INTO users (email, password_hash, full_name, phone, role_id, is_active) VALUES
('admin@shoestore.com', '$2a$11$FakeHashPasswordAdmin123', 'Nguyễn Văn Quản Lý', '0901111111', 1, TRUE),
('employee@shoestore.com', '$2a$11$FakeHashPasswordEmp123', 'Trần Thị Nhân Viên', '0902222222', 2, TRUE),
('customer1@gmail.com', '$2a$11$FakeHashPasswordCus123', 'Lê Văn Khách Hàng', '0903333333', 3, TRUE);

-- 3. Employees
INSERT INTO employees (employee_id, hire_date) VALUES
(1, '2025-01-10'),
(2, '2025-03-15');

-- 4. Customers
INSERT INTO customers (customer_id, membership_level) VALUES
(3, 'VIP');

-- 5. Customer Addresses
INSERT INTO customer_addresses (customer_id, receiver_name, phone, address_line, is_default) VALUES
(3, 'Lê Văn Khách Hàng', '0903333333', '123 Đường Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM', TRUE);

-- 6. Categories
INSERT INTO categories (category_name, description) VALUES
('Giày thể thao', 'Giày chuyên dụng cho tập gym và vận động'),
('Giày chạy bộ', 'Giày thiết kế đệm khí tối ưu cho chạy bộ'),
('Giày tây', 'Giày da lịch lãm công sở');

-- 7. Brands
INSERT INTO brands (brand_name, description) VALUES
('Nike', 'Thương hiệu thể thao hàng đầu thế giới'),
('Adidas', 'Thương hiệu thời trang thể thao Đức'),
('Clarks', 'Thương hiệu giày da cao cấp');

-- 8. Suppliers
INSERT INTO suppliers (supplier_name, phone, email, address, is_cooperating) VALUES
('Công ty TNHH Phân Phối Giày Việt', '02838111222', 'contact@giayviet.vn', 'KCN Tân Bình, TP.HCM', TRUE);

-- 9. Products
INSERT INTO products (product_code, product_name, category_id, brand_id, supplier_id, description, thumbnail_url, status) VALUES
('NK-AF1-01', 'Nike Air Force 1 07', 1, 1, 1, 'Giày thể thao huyền thoại năng động', '/images/nike-af1.jpg', 'Selling'),
('AD-UB-02', 'Adidas Ultraboost Light', 2, 2, 1, 'Giày chạy bộ siêu nhẹ thoải mái', '/images/adidas-ub.jpg', 'Selling');

-- 10. Product Images
INSERT INTO product_images (product_id, image_url) VALUES
(1, '/images/nike-af1-detail1.jpg'),
(2, '/images/adidas-ub-detail1.jpg');

-- 11. Product Variants (SKU, Size, Màu, Giá)
INSERT INTO product_variants (product_id, sku, size, color, price, status) VALUES
(1, 'NK-AF1-WHT-40', '40', 'Trắng', 2500000.00, 'Selling'),
(1, 'NK-AF1-WHT-41', '41', 'Trắng', 2500000.00, 'Selling'),
(2, 'AD-UB-BLK-42', '42', 'Đen', 3200000.00, 'Selling');

-- 12. Inventory
INSERT INTO inventory (variant_id, stock_quantity, low_stock_threshold) VALUES
(1, 20, 5),
(2, 15, 5),
(3, 8, 3);

-- 13. Promotions
INSERT INTO promotions (promo_code, discount_amount, min_order_value, start_date, end_date, is_active) VALUES
('GIECMS100K', 100000.00, 2000000.00, '2026-01-01 00:00:00', '2026-12-31 23:59:59', TRUE);

-- 14. Carts
INSERT INTO carts (customer_id) VALUES (3);

-- 15. Cart Items
INSERT INTO cart_items (cart_id, variant_id, quantity) VALUES
(1, 1, 1);

-- 16. Orders
INSERT INTO orders (order_code, customer_id, receiver_name, phone, shipping_address, total_amount, discount_amount, final_amount, promotion_id, order_status, note) VALUES
('ORD-20261107-001', 3, 'Lê Văn Khách Hàng', '0903333333', '123 Đường Nguyễn Huệ, Quận 1, TP.HCM', 2500000.00, 0.00, 2500000.00, NULL, 'Pending', 'Giao giờ hành chính');

-- 17. Order Details
INSERT INTO order_details (order_id, variant_id, quantity, unit_price) VALUES
(1, 1, 1, 2500000.00);

-- 18. Order Status History
INSERT INTO order_status_history (order_id, previous_status, new_status, changed_by_user_id, note) VALUES
(1, NULL, 'Pending', 3, 'Khách hàng tạo đơn hàng thành công');

-- 19. Payments
INSERT INTO payments (order_id, payment_method, amount, payment_status, confirmed_at) VALUES
(1, 'COD', 2500000.00, 'Unpaid', NULL);

-- 20. Product Reviews
INSERT INTO product_reviews (product_id, customer_id, rating, comment) VALUES
(1, 3, 5, 'Giày đẹp, chính hãng, đi rất êm chân!');