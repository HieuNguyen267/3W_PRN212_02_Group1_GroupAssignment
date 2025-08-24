INSERT INTO Account (Username, Password, Email, AccountType, CreatedDate, IsActive)
VALUES 
('admin', '123456', 'admin@cellphones.com', 'Admin', GETDATE(), 1),
('customer1', '123456', 'customer1@cellphones.com', 'Customer', GETDATE(), 1),
('customer2', '123456', 'customer2@cellphones.com', 'Customer', GETDATE(), 1);

-- Insert demo admin
INSERT INTO Admin (AccountID, FullName, Phone, CreatedDate)
VALUES 
(1, 'Administrator', '0123456789', GETDATE());

-- Insert demo customers
INSERT INTO Customers (AccountID, FullName, Phone, Address, CreatedDate, UpdatedDate)
VALUES 
(2, 'Nguyễn Văn A', '0987654321', '123 Đường ABC, Quận 1, TP.HCM', GETDATE(), GETDATE()),
(3, 'Trần Thị B', '0111222333', '456 Đường XYZ, Quận 2, TP.HCM', GETDATE(), GETDATE());
