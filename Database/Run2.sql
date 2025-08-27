-- Script tổng hợp cập nhật dữ liệu theo Categories mới
-- Chạy script này để cập nhật toàn bộ dữ liệu

USE ShoppingOnline;
GO

PRINT '=== BẮT ĐẦU CẬP NHẬT DỮ LIỆU ===';

-- 0. Xóa dữ liệu theo thứ tự đúng (từ child đến parent)
PRINT '0. Xóa dữ liệu cũ...';

-- Xóa ShoppingCart trước (vì reference Products)
DELETE FROM ShoppingCart;
PRINT '   - Đã xóa ShoppingCart';

-- Xóa OrderDetails (vì reference Products)
DELETE FROM OrderDetails;
PRINT '   - Đã xóa OrderDetails';

-- Xóa ProductImages (vì reference Products)
DELETE FROM ProductImages;
PRINT '   - Đã xóa ProductImages';

-- Xóa Products (vì reference Categories)
DELETE FROM Products;
PRINT '   - Đã xóa Products';

-- Xóa Categories
DELETE FROM Categories;
PRINT '   - Đã xóa Categories';

-- 1. Cập nhật Categories
PRINT '1. Cập nhật Categories...';
INSERT INTO Categories (CategoryID, CategoryName, Description, IsActive) VALUES
('CAT001', N'Điện thoại, Tablet', N'Các loại điện thoại thông minh và máy tính bảng', 1),
('CAT002', N'Laptop', N'Máy tính xách tay cho học tập và làm việc', 1),
('CAT003', N'Âm thanh, Mic thu âm', N'Thiết bị âm thanh và microphone thu âm', 1),
('CAT004', N'Đồng hồ, Camera', N'Đồng hồ thông minh và camera', 1),
('CAT005', N'Đồ gia dụng', N'Các thiết bị gia dụng cho gia đình', 1),
('CAT006', N'Phụ kiện', N'Phụ kiện điện tử và công nghệ', 1),
('CAT007', N'PC, Màn hình, Máy in', N'Máy tính để bàn, màn hình và máy in', 1),
('CAT008', N'Tivi', N'Tivi và thiết bị giải trí', 1);

PRINT '   - Đã thêm 8 Categories';

-- 2. Cập nhật Products
PRINT '2. Cập nhật Products...';
INSERT INTO Products (ProductID, CategoryID, ProductName, Description, Price, StockQuantity, IsActive, CreatedDate) VALUES
-- CAT001: Điện thoại, Tablet
('P001', 'CAT001', N'iPhone 16 Pro Max 256GB', N'iPhone 16 Pro Max chính hãng VN/A', 29990000, 50, 1, GETDATE()),
('P002', 'CAT001', N'Samsung Galaxy S25 Ultra', N'Samsung Galaxy S25 Ultra 12GB 256GB', 28380000, 25, 1, GETDATE()),
('P003', 'CAT001', N'iPad Pro 12.9 inch M4', N'iPad Pro 12.9 inch chip M4 256GB', 25990000, 30, 1, GETDATE()),
('P004', 'CAT001', N'Samsung Galaxy Tab S10', N'Samsung Galaxy Tab S10 11 inch 128GB', 15990000, 40, 1, GETDATE()),

-- CAT002: Laptop
('P005', 'CAT002', N'MacBook Air M3 13 inch', N'MacBook Air M3 13 inch 256GB', 28990000, 20, 1, GETDATE()),
('P006', 'CAT002', N'Dell XPS 13 Plus', N'Dell XPS 13 Plus Intel Core i7', 32990000, 15, 1, GETDATE()),
('P007', 'CAT002', N'Lenovo ThinkPad X1 Carbon', N'Lenovo ThinkPad X1 Carbon Gen 12', 25990000, 25, 1, GETDATE()),
('P008', 'CAT002', N'ASUS ROG Strix G16', N'ASUS ROG Strix G16 Gaming Laptop', 35990000, 10, 1, GETDATE()),

-- CAT003: Âm thanh, Mic thu âm
('P009', 'CAT003', N'AirPods Pro 3', N'AirPods Pro 3 chống ồn chủ động', 6990000, 100, 1, GETDATE()),
('P010', 'CAT003', N'Sony WH-1000XM5', N'Sony WH-1000XM5 Headphones', 8990000, 50, 1, GETDATE()),
('P011', 'CAT003', N'Blue Yeti X', N'Blue Yeti X USB Microphone', 3990000, 30, 1, GETDATE()),
('P012', 'CAT003', N'JBL Flip 6', N'JBL Flip 6 Portable Speaker', 2990000, 80, 1, GETDATE()),

-- CAT004: Đồng hồ, Camera
('P013', 'CAT004', N'Apple Watch Series 10', N'Apple Watch Series 10 45mm GPS', 12990000, 60, 1, GETDATE()),
('P014', 'CAT004', N'Samsung Galaxy Watch 7', N'Samsung Galaxy Watch 7 44mm', 8990000, 45, 1, GETDATE()),
('P015', 'CAT004', N'Canon EOS R6 Mark II', N'Canon EOS R6 Mark II Mirrorless Camera', 45990000, 15, 1, GETDATE()),
('P016', 'CAT004', N'Sony A7 IV', N'Sony A7 IV Full-Frame Camera', 52990000, 12, 1, GETDATE()),

-- CAT005: Đồ gia dụng
('P017', 'CAT005', N'Samsung Smart Refrigerator', N'Samsung Smart Refrigerator 4 cửa', 25990000, 8, 1, GETDATE()),
('P018', 'CAT005', N'LG Smart Washing Machine', N'LG Smart Washing Machine 10kg', 15990000, 20, 1, GETDATE()),
('P019', 'CAT005', N'Sharp Microwave Oven', N'Sharp Microwave Oven 23L', 3990000, 35, 1, GETDATE()),
('P020', 'CAT005', N'Philips Air Fryer', N'Philips Air Fryer HD9650/90', 5990000, 25, 1, GETDATE()),

-- CAT006: Phụ kiện
('P021', 'CAT006', N'Apple Magic Keyboard', N'Apple Magic Keyboard với Touch ID', 3990000, 40, 1, GETDATE()),
('P022', 'CAT006', N'Logitech MX Master 3S', N'Logitech MX Master 3S Wireless Mouse', 2990000, 60, 1, GETDATE()),
('P023', 'CAT006', N'Samsung T7 Portable SSD', N'Samsung T7 Portable SSD 1TB', 2990000, 50, 1, GETDATE()),
('P024', 'CAT006', N'Anker PowerCore 20000', N'Anker PowerCore 20000mAh Power Bank', 1990000, 100, 1, GETDATE()),

-- CAT007: PC, Màn hình, Máy in
('P025', 'CAT007', N'Alienware Aurora R16', N'Alienware Aurora R16 Gaming Desktop', 45990000, 5, 1, GETDATE()),
('P026', 'CAT007', N'Samsung Odyssey G9', N'Samsung Odyssey G9 49" Gaming Monitor', 25990000, 8, 1, GETDATE()),
('P027', 'CAT007', N'HP LaserJet Pro M404n', N'HP LaserJet Pro M404n Printer', 5990000, 15, 1, GETDATE()),
('P028', 'CAT007', N'LG 27" 4K Monitor', N'LG 27" 4K Ultra HD Monitor', 8990000, 12, 1, GETDATE()),

-- CAT008: Tivi
('P029', 'CAT008', N'Samsung QLED 4K 65"', N'Samsung QLED 4K Smart TV 65"', 35990000, 10, 1, GETDATE()),
('P030', 'CAT008', N'LG OLED 4K 55"', N'LG OLED 4K Smart TV 55"', 29990000, 8, 1, GETDATE()),
('P031', 'CAT008', N'Sony Bravia 4K 75"', N'Sony Bravia 4K Smart TV 75"', 45990000, 5, 1, GETDATE()),
('P032', 'CAT008', N'TCL 4K Smart TV 50"', N'TCL 4K Smart TV 50" Android TV', 12990000, 20, 1, GETDATE());

PRINT '   - Đã thêm 32 Products';

-- 3. Cập nhật ProductImages
PRINT '3. Cập nhật ProductImages...';
INSERT INTO ProductImages (ProductID, ImageURL, IsPrimary, DisplayOrder) VALUES
-- iPhone 16 Pro Max
('P001', 'img/iphone16-pro-max-1.jpg', 1, 1),
('P001', 'img/iphone16-pro-max-2.jpg', 0, 2),
('P001', 'img/iphone16-pro-max-3.jpg', 0, 3),

-- Samsung Galaxy S25 Ultra
('P002', 'img/samsung-s25-ultra-1.jpg', 1, 1),
('P002', 'img/samsung-s25-ultra-2.jpg', 0, 2),

-- iPad Pro 12.9 inch M4
('P003', 'img/ipad-pro-12-9-m4-1.jpg', 1, 1),
('P003', 'img/ipad-pro-12-9-m4-2.jpg', 0, 2),

-- Samsung Galaxy Tab S10
('P004', 'img/samsung-tab-s10-1.jpg', 1, 1),

-- MacBook Air M3
('P005', 'img/macbook-air-m3-1.jpg', 1, 1),
('P005', 'img/macbook-air-m3-2.jpg', 0, 2),

-- Dell XPS 13 Plus
('P006', 'img/dell-xps-13-plus-1.jpg', 1, 1),

-- Lenovo ThinkPad X1 Carbon
('P007', 'img/lenovo-thinkpad-x1-1.jpg', 1, 1),

-- ASUS ROG Strix G16
('P008', 'img/asus-rog-strix-g16-1.jpg', 1, 1),

-- AirPods Pro 3
('P009', 'img/airpods-pro-3-1.jpg', 1, 1),

-- Sony WH-1000XM5
('P010', 'img/sony-wh-1000xm5-1.jpg', 1, 1),

-- Blue Yeti X
('P011', 'img/blue-yeti-x-1.jpg', 1, 1),

-- JBL Flip 6
('P012', 'img/jbl-flip-6-1.jpg', 1, 1),

-- Apple Watch Series 10
('P013', 'img/apple-watch-series-10-1.jpg', 1, 1),

-- Samsung Galaxy Watch 7
('P014', 'img/samsung-galaxy-watch-7-1.jpg', 1, 1),

-- Canon EOS R6 Mark II
('P015', 'img/canon-eos-r6-mark-ii-1.jpg', 1, 1),

-- Sony A7 IV
('P016', 'img/sony-a7-iv-1.jpg', 1, 1),

-- Samsung Smart Refrigerator
('P017', 'img/samsung-smart-refrigerator-1.jpg', 1, 1),

-- LG Smart Washing Machine
('P018', 'img/lg-smart-washing-machine-1.jpg', 1, 1),

-- Sharp Microwave Oven
('P019', 'img/sharp-microwave-oven-1.jpg', 1, 1),

-- Philips Air Fryer
('P020', 'img/philips-air-fryer-1.jpg', 1, 1),

-- Apple Magic Keyboard
('P021', 'img/apple-magic-keyboard-1.jpg', 1, 1),

-- Logitech MX Master 3S
('P022', 'img/logitech-mx-master-3s-1.jpg', 1, 1),

-- Samsung T7 Portable SSD
('P023', 'img/samsung-t7-portable-ssd-1.jpg', 1, 1),

-- Anker PowerCore 20000
('P024', 'img/anker-powercore-20000-1.jpg', 1, 1),

-- Alienware Aurora R16
('P025', 'img/alienware-aurora-r16-1.jpg', 1, 1),

-- Samsung Odyssey G9
('P026', 'img/samsung-odyssey-g9-1.jpg', 1, 1),

-- HP LaserJet Pro M404n
('P027', 'img/hp-laserjet-pro-m404n-1.jpg', 1, 1),

-- LG 27" 4K Monitor
('P028', 'img/lg-27-4k-monitor-1.jpg', 1, 1),

-- Samsung QLED 4K 65"
('P029', 'img/samsung-qled-4k-65-1.jpg', 1, 1),

-- LG OLED 4K 55"
('P030', 'img/lg-oled-4k-55-1.jpg', 1, 1),

-- Sony Bravia 4K 75"
('P031', 'img/sony-bravia-4k-75-1.jpg', 1, 1),

-- TCL 4K Smart TV 50"
('P032', 'img/tcl-4k-smart-tv-50-1.jpg', 1, 1);

PRINT '   - Đã thêm ProductImages';

PRINT '=== HOÀN THÀNH CẬP NHẬT DỮ LIỆU ===';
PRINT 'Tổng cộng:';
PRINT '   - 8 Categories';
PRINT '   - 32 Products';
PRINT '   - ProductImages cho tất cả sản phẩm';
PRINT '';
PRINT 'Bạn có thể chạy ứng dụng để kiểm tra dữ liệu!';
