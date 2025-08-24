# ShoppingOnline - Hệ thống bán hàng điện tử

## Mô tả dự án
ShoppingOnline là một ứng dụng desktop WPF được phát triển bằng C# và .NET Framework, mô phỏng một hệ thống bán hàng điện tử với đầy đủ các chức năng cơ bản.

## Cấu trúc dự án

### 1. ShoppingOnline (WPF Application)
- **LoginWindow**: Cửa sổ đăng nhập
- **HomeWindow**: Trang chủ hiển thị sản phẩm
- **CartWindow**: Giỏ hàng
- **DetailWindow**: Chi tiết sản phẩm
- **OrderHistoryWindow**: Lịch sử đơn hàng
- **UserProfileWindow**: Thông tin cá nhân (dữ liệu động từ DB)
- **AccountSettingsWindow**: Cài đặt tài khoản
- **AddressManagementWindow**: Quản lý địa chỉ (dữ liệu động từ DB)

### 2. BLL (Business Logic Layer)
- **AccountService**: Xử lý logic đăng nhập, quản lý tài khoản
- **ProductService**: Xử lý logic sản phẩm
- **CartService**: Xử lý logic giỏ hàng
- **OrderService**: Xử lý logic đơn hàng

### 3. DAL (Data Access Layer)
- **Entities**: Các entity classes
- **Repositories**: Các repository classes để truy cập database
  - **AccountRepo**: Truy cập dữ liệu tài khoản
  - **ProductRepo**: Truy cập dữ liệu sản phẩm
  - **CartRepo**: Truy cập dữ liệu giỏ hàng
  - **OrderRepo**: Truy cập dữ liệu đơn hàng

## Cơ sở dữ liệu

### Các bảng chính:
- **Account**: Thông tin tài khoản
- **Customers**: Thông tin khách hàng
- **Admin**: Thông tin admin
- **Carrier**: Thông tin shipper
- **Products**: Sản phẩm
- **Categories**: Danh mục sản phẩm
- **Orders**: Đơn hàng
- **OrderDetails**: Chi tiết đơn hàng
- **ShoppingCart**: Giỏ hàng

## Tính năng chính

### 1. Quản lý người dùng
- Đăng nhập/Đăng xuất
- Xem thông tin cá nhân
- Cập nhật thông tin tài khoản

### 2. Mua sắm
- Xem danh sách sản phẩm
- Tìm kiếm và lọc sản phẩm
- Xem chi tiết sản phẩm
- Thêm vào giỏ hàng
- Đặt hàng

### 3. Quản lý đơn hàng
- Xem lịch sử đơn hàng
- Theo dõi trạng thái đơn hàng
- Xem chi tiết đơn hàng

## Hướng dẫn sử dụng

### 1. Cài đặt và chạy
1. Clone repository về máy
2. Mở file `3W_PRN212_02_Group1_GroupAssignment.sln` trong Visual Studio
3. Restore NuGet packages
4. Chạy script SQL trong thư mục `Database` để tạo database:
   - Chạy `Base.sql` để tạo database và các bảng
5. Cập nhật connection string trong `appsettings.json`
6. Build và chạy project

### 2. Đăng nhập
- Sử dụng email: `nguyenvana@gmail.com` và password: `123456` để đăng nhập với quyền Customer
- Hoặc sử dụng email: `nguyenduyhieu@gmail.com` và password: `123456` để đăng nhập với quyền Admin

### 3. Sử dụng hệ thống
1. **Mua sắm**: Chọn sản phẩm từ trang chủ, thêm vào giỏ hàng và đặt hàng
2. **Xem thông tin cá nhân**: Click vào menu user và chọn "Xem thông tin cá nhân"
3. **Cài đặt tài khoản**: Click vào menu user và chọn "Cài đặt tài khoản"
4. **Quản lý địa chỉ**: Click vào menu user và chọn "Quản lý địa chỉ"



## Công nghệ sử dụng
- **.NET Framework 4.8**
- **WPF (Windows Presentation Foundation)**
- **Entity Framework Core**
- **SQL Server**
- **MVVM Pattern**

## Nhóm phát triển
- Nguyễn Duy Hiếu
- Trần Thái Thịnh
- Và các thành viên khác

## Phiên bản
- Version: 1.0
- Ngày cập nhật: 2024