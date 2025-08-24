# Shopping Online Application

## Mô tả
Ứng dụng Shopping Online được xây dựng bằng WPF với kiến trúc 3-layer (DAL, BLL, Presentation).

## Cấu trúc dự án
- **DAL (Data Access Layer)**: Chứa entities và repositories để truy cập database
- **BLL (Business Logic Layer)**: Chứa services để xử lý business logic
- **ShoppingOnline (Presentation Layer)**: Giao diện người dùng WPF

## Cài đặt và chạy

### 1. Cấu hình Database
1. Đảm bảo SQL Server đang chạy
2. Tạo database tên "ShoppingOnline"
3. Chạy script `sample_data.sql` để tạo dữ liệu mẫu
4. Cập nhật connection string trong `ShoppingOnline/appsettings.json` nếu cần

### 2. Build và chạy
1. Mở solution trong Visual Studio
2. Build solution (Ctrl+Shift+B)
3. Chạy project ShoppingOnline

## Thay đổi từ Hard Code sang Database

### Trước đây:
- Dữ liệu sản phẩm được hard code trực tiếp trong XAML
- Không thể thay đổi dữ liệu mà không sửa code

### Bây giờ:
- Dữ liệu được lấy từ database thông qua Entity Framework
- Sử dụng MVVM pattern với binding
- Có thể thay đổi dữ liệu trong database mà không cần sửa code

### Các file đã thay đổi:

#### 1. DAL Layer:
- `ProductRepo.cs`: Repository để truy cập dữ liệu sản phẩm
- `ShoppingOnlineContext.cs`: DbContext cho Entity Framework

#### 2. BLL Layer:
- `ProductService.cs`: Service để xử lý business logic

#### 3. Presentation Layer:
- `HomeWindow.xaml/.cs`: Tích hợp trực tiếp ProductService và binding logic

### Cách hoạt động:
1. `HomeWindow` khởi tạo và gọi `ProductService`
2. `ProductService` gọi `ProductRepo` để lấy dữ liệu từ database
3. Dữ liệu được binding đến `ItemsControl` trong `HomeWindow.xaml`
4. Mỗi sản phẩm được hiển thị bằng DataTemplate trực tiếp

## Tính năng
- Hiển thị danh sách sản phẩm từ database
- Responsive design với UniformGrid
- Binding dữ liệu động
- Kiến trúc 3-layer rõ ràng
- Trang chi tiết sản phẩm với layout 2 cột
- Tương tác với sản phẩm (click để xem chi tiết)
- Các tùy chọn mua hàng (phiên bản, màu sắc)
- Nút thêm vào giỏ hàng

## Lưu ý
- Đảm bảo database có dữ liệu trước khi chạy ứng dụng
- Connection string phải chính xác
- Cần có quyền truy cập database