INSERT INTO Categories (CategoryID, CategoryName, Description, IsActive) VALUES
('CAT001', 'Điện thoại', 'Các loại điện thoại di động', 1),
('CAT002', 'Laptop', 'Máy tính xách tay', 1),
('CAT003', 'Tablet', 'Máy tính bảng', 1),
('CAT004', 'Phụ kiện', 'Phụ kiện điện tử', 1);

-- Insert Products
INSERT INTO Products (ProductID, CategoryID, ProductName, Description, Price, StockQuantity, IsActive, CreatedDate) VALUES
('P001', 'CAT001', 'iPhone 16 Pro Max 256GB', 'iPhone 16 Pro Max chính hãng VN/A', 29990000, 50, 1, GETDATE()),
('P002', 'CAT001', 'Xiaomi 14T Pro 12GB 512GB', 'Xiaomi 14T Pro phiên bản cao cấp', 14870000, 30, 1, GETDATE()),
('P003', 'CAT001', 'Samsung Galaxy S25 Ultra', 'Samsung Galaxy S25 Ultra 12GB 256GB', 28380000, 25, 1, GETDATE()),
('P004', 'CAT001', 'iPhone 15 128GB', 'iPhone 15 chính hãng VN/A', 15190000, 40, 1, GETDATE()),
('P005', 'CAT001', 'Samsung Galaxy Z Fold7', 'Samsung Galaxy Z Fold7 12GB 256GB', 44990000, 15, 1, GETDATE()),
('P006', 'CAT001', 'Samsung Galaxy S24 FE 5G', 'Samsung Galaxy S24 FE 8GB 128GB', 12590000, 35, 1, GETDATE()),
('P007', 'CAT001', 'Xiaomi Redmi Note 14', 'Xiaomi Redmi Note 14 6GB 128GB', 4700000, 60, 1, GETDATE()),
('P008', 'CAT001', 'Samsung Galaxy S25 256GB', 'Samsung Galaxy S25 256GB', 18690000, 20, 1, GETDATE()),
('P009', 'CAT001', 'iPhone 13 128GB', 'iPhone 13 chính hãng VN/A', 11490000, 45, 1, GETDATE()),
('P010', 'CAT001', 'Samsung Galaxy 128GB', 'Samsung Galaxy 128GB', 9510000, 30, 1, GETDATE());

-- Insert Product Images (optional - for future use)
INSERT INTO ProductImages (ProductID, ImageURL, IsPrimary, DisplayOrder) VALUES
('P001', 'https://example.com/iphone16.jpg', 1, 1),
('P002', 'https://example.com/xiaomi14t.jpg', 1, 1),
('P003', 'https://example.com/samsung-s25.jpg', 1, 1),
('P004', 'https://example.com/iphone15.jpg', 1, 1),
('P005', 'https://example.com/samsung-fold7.jpg', 1, 1);
