namespace ShoppingOnline.Constants
{
    public static class AppConstants
    {
        // Account Types
        public static class AccountTypes
        {
            public const string Admin = "Admin";
            public const string Customer = "Customer";
            public const string Carrier = "Carrier";
        }

        // Order Status
        public static class OrderStatus
        {
            public const string Pending = "Pending";
            public const string Confirmed = "Confirmed";
            public const string Preparing = "Preparing";
            public const string Shipping = "Shipping";
            public const string Delivered = "Delivered";
            public const string Completed = "Completed";
            public const string Cancelled = "Cancelled";
        }

        // Default Values
        public static class DefaultValues
        {
            public const string DefaultAdminEmail = "";
            public const string DefaultAdminPassword = "";
            public const string DefaultPhone = "";
            public const string DefaultOrderStatus = OrderStatus.Pending;
        }

        // Status Display Names
        public static class StatusDisplayNames
        {
            public const string Pending = "Chờ xác nhận";
            public const string Confirmed = "Đã xác nhận";
            public const string Preparing = "Đang chuẩn bị";
            public const string Shipping = "Đang giao hàng";
            public const string Delivered = "Đã giao hàng";
            public const string Completed = "Hoàn thành";
            public const string Cancelled = "Đã hủy";
        }

        // Status Colors (Hex)
        public static class StatusColors
        {
            public const string Pending = "#FFA500";      // Orange
            public const string Confirmed = "#4169E1";    // Blue
            public const string Preparing = "#9370DB";    // Purple
            public const string Shipping = "#32CD32";     // Green
            public const string Delivered = "#20B2AA";    // Light Sea Green
            public const string Completed = "#228B22";    // Forest Green
            public const string Cancelled = "#FF6B6B";    // Red
        }

        // Status Icons
        public static class StatusIcons
        {
            public const string Pending = "⏳ Chờ xác nhận";
            public const string Confirmed = "✅ Đã xác nhận";
            public const string Preparing = "📦 Đang chuẩn bị";
            public const string Shipping = "🚚 Đang giao hàng";
            public const string Delivered = "📬 Đã giao hàng";
            public const string Completed = "✅ Hoàn thành";
            public const string Cancelled = "❌ Đã hủy";
        }

        // Messages
        public static class Messages
        {
            public const string AdminAccountCreated = "Tài khoản admin mặc định đã được tạo!\nEmail: {0}\nPassword: {1}";
            public const string OrderCancelled = "Đã hủy đơn hàng thành công!\n\nĐơn hàng #{0} giá trị {1:N0} VND đã bị loại khỏi tổng giá trị đơn hàng.";
            public const string OrderConfirmed = "Đã xác nhận đơn hàng thành công!";
            public const string OrderCompleted = "Đã hoàn thành đơn hàng thành công!";
            public const string OrderCancelledNote = "[CANCELLED] - Hủy bỏ lúc {0:dd/MM/yyyy HH:mm:ss}";
        }

        // Validation Messages
        public static class ValidationMessages
        {
            public const string RequiredField = "Trường này là bắt buộc";
            public const string InvalidEmail = "Email không hợp lệ";
            public const string InvalidPhone = "Số điện thoại không hợp lệ";
            public const string InvalidPrice = "Giá phải lớn hơn 0";
            public const string InvalidQuantity = "Số lượng phải lớn hơn 0";
            public const string InvalidStock = "Số lượng trong kho không đủ";
            public const string CategoryNameRequired = "Tên danh mục là bắt buộc";
            public const string CategoryNameLength = "Tên danh mục phải từ 2-50 ký tự";
            public const string DescriptionLength = "Mô tả không được quá 500 ký tự";
        }
    }
}
