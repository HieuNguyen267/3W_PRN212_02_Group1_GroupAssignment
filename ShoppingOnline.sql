CREATE DATABASE ShoppingOnline;
GO

USE ShoppingOnline;
GO

CREATE TABLE Account (
    AccountID INT IDENTITY(1,1) PRIMARY KEY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    Password NVARCHAR(255) NOT NULL,
    Email NVARCHAR(100) UNIQUE NOT NULL,
    AccountType NVARCHAR(20) NOT NULL CHECK (AccountType IN ('Admin', 'Customer', 'Carrier')),
    CreatedDate DATETIME DEFAULT GETDATE(),
    IsActive BIT DEFAULT 1
);

CREATE TABLE Admin (
    AdminID INT IDENTITY(1,1) PRIMARY KEY,
    AccountID INT FOREIGN KEY REFERENCES Account(AccountID),
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    CreatedDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE Customers (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    AccountID INT FOREIGN KEY REFERENCES Account(AccountID),
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    Address NVARCHAR(500),
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE Carrier (
    CarrierID INT IDENTITY(1,1) PRIMARY KEY,
    AccountID INT FOREIGN KEY REFERENCES Account(AccountID),
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20),
    VehicleNumber NVARCHAR(20),
    IsAvailable BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE Categories (
    CategoryID NVARCHAR(20) PRIMARY KEY, -- string ID
    CategoryName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(500),
    IsActive BIT DEFAULT 1
);

CREATE TABLE Products (
    ProductID NVARCHAR(20) PRIMARY KEY, -- string ID
    CategoryID NVARCHAR(20) FOREIGN KEY REFERENCES Categories(CategoryID),
    ProductName NVARCHAR(200) NOT NULL,
    Description NVARCHAR(1000),
    Price DECIMAL(18,2) NOT NULL CHECK (Price >= 0),
    StockQuantity INT NOT NULL DEFAULT 0,
    IsActive BIT DEFAULT 1,
    CreatedDate DATETIME DEFAULT GETDATE(),
    UpdatedDate DATETIME DEFAULT GETDATE()
);

CREATE TABLE ProductImages (
    ImageID INT IDENTITY(1,1) PRIMARY KEY,
    ProductID NVARCHAR(20) FOREIGN KEY REFERENCES Products(ProductID),
    ImageURL NVARCHAR(500) NOT NULL,
    IsPrimary BIT DEFAULT 0,
    DisplayOrder INT DEFAULT 0
);

CREATE TABLE Orders (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT FOREIGN KEY REFERENCES Customers(CustomerID),
    CarrierID INT FOREIGN KEY REFERENCES Carrier(CarrierID),
    OrderDate DATETIME DEFAULT GETDATE(),
    TotalAmount DECIMAL(18,2) NOT NULL CHECK (TotalAmount >= 0),
    Status NVARCHAR(50) DEFAULT 'Pending' CHECK (Status IN ('Pending', 'Confirmed', 'Preparing', 'Shipping', 'Delivered', 'Completed')),
    ShippingAddress NVARCHAR(500),
    Phone NVARCHAR(20),
    Notes NVARCHAR(1000)
);

CREATE TABLE OrderDetails (
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT FOREIGN KEY REFERENCES Orders(OrderID),
    ProductID NVARCHAR(20) FOREIGN KEY REFERENCES Products(ProductID),
    Quantity INT NOT NULL CHECK (Quantity > 0),
    UnitPrice DECIMAL(18,2) NOT NULL CHECK (UnitPrice >= 0),
    SubTotal DECIMAL(18,2) NOT NULL CHECK (SubTotal >= 0)
);

CREATE TABLE ShoppingCart (
    CartID INT IDENTITY(1,1) PRIMARY KEY,
    CustomerID INT FOREIGN KEY REFERENCES Customers(CustomerID),
    ProductID NVARCHAR(20) FOREIGN KEY REFERENCES Products(ProductID),
    Quantity INT NOT NULL DEFAULT 1 CHECK (Quantity > 0),
    AddedDate DATETIME DEFAULT GETDATE()
);

-- ACCOUNT
INSERT INTO Account (Username, Password, Email, AccountType)
VALUES 
('nguyenduyhieu', '123456', 'nguyenduyhieu@gmail.com', 'Admin'),
('tranthaithinh', '123456', 'tranthaithinh@gmail.com', 'Admin'),
('nguyenvana', '123456', 'nguyenvana@gmail.com', 'Customer'),
('tranthibich', '123456', 'tranthibich@gmail.com', 'Customer'),
('lethithu', '123456', 'lethithu@gmail.com', 'Customer'),
('phamvantuan', '123456', 'phamvantuan@gmail.com', 'Customer'),
('dangquanghuy', '123456', 'dangquanghuy@gmail.com', 'Carrier'),
('ngothimai', '123456', 'ngothimai@gmail.com', 'Carrier');

-- ADMIN
INSERT INTO Admin (AccountID, FullName, Phone)
VALUES 
(1, N'Nguyễn Duy Hiếu', '0909000001'),
(2, N'Trần Thái Thịnh', '0909000002');

-- CUSTOMERS
INSERT INTO Customers (AccountID, FullName, Phone, Address)
VALUES
(3, N'Nguyễn Văn An', '0909000003', N'123 Nguyễn Trãi, Hà Nội'),
(4, N'Trần Thị Bích', '0909000004', N'45 Lê Lợi, TP.HCM'),
(5, N'Lê Thị Thu', '0909000005', N'78 Hai Bà Trưng, Đà Nẵng'),
(6, N'Phạm Văn Tuấn', '0909000006', N'99 Trần Hưng Đạo, Cần Thơ');

-- CARRIER
INSERT INTO Carrier (AccountID, FullName, Phone, VehicleNumber)
VALUES
(7, N'Đặng Quang Huy', '0909000007', '29A-12345'),
(8, N'Ngô Thị Mai', '0909000008', '30B-67890');

-- CATEGORIES
INSERT INTO Categories (CategoryID, CategoryName, Description)
VALUES
('C001', N'Điện thoại', N'Các loại điện thoại thông minh'),
('C002', N'Laptop', N'Laptop cho học tập và làm việc'),
('C003', N'Thời trang', N'Quần áo và phụ kiện'),
('C004', N'Gia dụng', N'Đồ gia dụng cho gia đình');

-- PRODUCTS
INSERT INTO Products (ProductID, CategoryID, ProductName, Description, Price, StockQuantity)
VALUES
('P001', 'C001', N'iPhone 14 Pro', N'Apple iPhone 14 Pro 128GB', 28990000, 50),
('P002', 'C001', N'Samsung Galaxy S23', N'Flagship của Samsung 2023', 24990000, 40),
('P003', 'C002', N'Dell XPS 13', N'Ultrabook cao cấp 13 inch', 32990000, 30),
('P004', 'C002', N'MacBook Air M2', N'Apple MacBook Air 2023', 28990000, 20),
('P005', 'C003', N'Áo Thun Nam', N'Áo thun cotton 100%', 199000, 200),
('P006', 'C004', N'Nồi Cơm Điện', N'Nồi cơm điện 1.8L Sharp', 1290000, 100);

-- PRODUCT IMAGES
INSERT INTO ProductImages (ProductID, ImageURL, IsPrimary, DisplayOrder)
VALUES
('P001', 'https://example.com/images/iphone14pro.jpg', 1, 1),
('P002', 'https://example.com/images/galaxyS23.jpg', 1, 1),
('P003', 'https://example.com/images/dellxps13.jpg', 1, 1),
('P004', 'https://example.com/images/macbookairm2.jpg', 1, 1),
('P005', 'https://example.com/images/aothunnam.jpg', 1, 1),
('P006', 'https://example.com/images/noicomdien.jpg', 1, 1);

-- ORDERS
INSERT INTO Orders (CustomerID, CarrierID, TotalAmount, Status, ShippingAddress, Phone, Notes)
VALUES
(1, 1, 29189000, 'Confirmed', N'123 Nguyễn Trãi, Hà Nội', '0909000003', N'Giao trong giờ hành chính'),
(2, 2, 24990000, 'Preparing', N'45 Lê Lợi, TP.HCM', '0909000004', N'Giao nhanh'),
(3, 1, 2019000, 'Pending', N'78 Hai Bà Trưng, Đà Nẵng', '0909000005', N'Liên hệ trước khi giao'),
(4, 2, 28990000, 'Shipping', N'99 Trần Hưng Đạo, Cần Thơ', '0909000006', N'Giao ngoài giờ hành chính');

-- ORDER DETAILS
INSERT INTO OrderDetails (OrderID, ProductID, Quantity, UnitPrice, SubTotal)
VALUES
(1, 'P001', 1, 28990000, 28990000),
(1, 'P005', 10, 199000, 1990000),
(2, 'P002', 1, 24990000, 24990000),
(3, 'P006', 1, 1290000, 1290000),
(3, 'P005', 3, 199000, 597000),
(4, 'P004', 1, 28990000, 28990000);

-- SHOPPING CART
INSERT INTO ShoppingCart (CustomerID, ProductID, Quantity)
VALUES
(1, 'P003', 1),
(1, 'P004', 1),
(2, 'P005', 2),
(3, 'P002', 1),
(4, 'P006', 2);
