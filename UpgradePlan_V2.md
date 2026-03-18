# Kế Hoạch Nâng Cấp Tương Lai: Dự Án FpolyCafe V2 (Advanced Features)

Bản thiết kế kỹ thuật (Technical Blueprint) nhằm tích hợp các tính năng nâng cao vào kiến trúc Clean Architecture hiện tại của hệ thống ASP.NET Core 8 Web API.

---

## 1. Phân trang, Sắp xếp và Lọc Nâng cao (Advanced Pagination/Filtering)
*Mục tiêu: Xử lý khối lượng lớn dữ liệu (sản phẩm, hóa đơn) trả về cho Frontend mà không gây quá tải Database.*

### Thư viện đề xuất:
- Tự build bằng `IQueryable` kết hợp Expression Trees (Hiệu năng tốt nhất).
- Quản lý metadata phân trang (Base Pagination Class).

### Roadmap Triển khai (Clean Architecture):
- **Tầng Application**:
  - Tạo class `PagedRequest` (chứa `PageIndex`, `PageSize`, `SortBy`, `SortDirection`, `SearchKeyword`).
  - Tạo class `PagedResponse<T>` (chứa `Data`, `TotalCount`, `TotalPages`, `HasNext`, `HasPrevious`).
  - Cập nhật các DTO (như `ProductFilterDto` kế thừa `PagedRequest`).
- **Tầng Infrastructure**:
  - Viết Extension Method `PaginateAsync<T>` cho `IQueryable<T>` để tự động đếm dòng và lấy dữ liệu bằng `Skip()` & `Take()`.
- **Tầng API**:
  - Sửa API `GET /api/Products` thành nhận `[FromQuery]` các tham số phân trang. Trả về metadata pagination qua Headers (cách chuẩn REST) hoặc gói gọn trong Responose Class.

---

## 2. Upload hình ảnh sản phẩm lên Cloud (AWS S3 / Cloudinary)
*Mục tiêu: Không lưu ảnh trong thư mục Local (WWWROOT) để tránh nặng source code và dễ dàng scale server.*

### Thư viện đề xuất:
- **Cloudinary**: Dùng package `CloudinaryDotNet` (Khuyên dùng vì có sẵn hệ thống CDN thu nhỏ ảnh tự động, dễ cài đặt bản miễn phí).
- **AWS S3**: Dùng package `AWSSDK.S3` (Khuyên dùng với dự án Enterprise lớn).

### Roadmap Triển khai:
- **Tầng Domain/Application**:
  - Sửa `Product` Entity (Thêm cột `ImageUrl` và `ImagePublicId` để map với Cloud).
  - Tạo Interface `ICloudStorageService` (có hàm `UploadImageAsync(IFormFile file)` và `DeleteImageAsync(string publicId)`).
- **Tầng Infrastructure**:
  - Tạo `CloudinaryService` kế thừa Interface trên. Implement gọi API Cloudinary.
  - Inject API Key, API Secret từ `appsettings.json`.
- **Tầng API**:
  - Nâng cấp API `POST /api/Products` thành gửi dạng `multipart/form-data` (để nhận `IFormFile image`).

---

## 3. Input dữ liệu (Import products) và Xuất báo cáo (Export Excel)
*Mục tiêu: Đẩy nhanh quá trình nhập kho hàng trăm sản phẩm và cung cấp dữ liệu Doanh thu cho Quản lý báo cáo.*

### Thư viện đề xuất:
- Khuyên dùng: `ClosedXML` (Open source, miễn phí 100% thương mại, dễ dùng hơn OpenXML thuần).
- Khác: `EPPlus` (Chú ý bản quyền nếu dùng trong dự án công ty).

### Roadmap Triển khai:
- **Tầng Application**:
  - Tạo Interface `IExcelService`.
  - Hàm `GenerateRevenueReportAsync(...)`: Nhận vào Dữ liệu Bill -> Trả về mảng byte `byte[]`.
  - Hàm `ImportProductsAsync(Stream fileStream)`: Đọc từng dòng Excel -> Map ra danh sách `CreateProductCommand` -> Handle tạo Product.
- **Tầng API**:
  - Tạo endpoint `GET /api/Reports/export-excel` (Trả về `FileContentResult` dạng `.xlsx`).
  - Tạo endpoint `POST /api/Products/import-excel` nhận `IFormFile excelFile`.

---

## 4. Tích hợp Thanh toán Online (VNPay / MoMo)
*Mục tiêu: Cho phép khách hàng/nhân viên quét mã QR thanh toán hoặc chuyển hướng sang Cổng thanh toán.*

### Thư viện đề xuất:
- Thuần tuý sử dụng `HttpClient` có sẵn của .NET và thư viện `System.Security.Cryptography` để tạo chữ ký bảo mật (HMAC SHA512).

### Roadmap Triển khai:
- **Tầng Domain**:
  - Bảng `Bill`: Thêm các cột ( `PaymentMethod` = Cash/VNPay, `PaymentTransactionId`, `PaymentStatus` = Pending/Success/Failed ).
- **Tầng Application**:
  - Tạo Interface `IVNPayService` có 2 hàm:
    - `CreatePaymentUrl(Bill thông_tin_đơn_hàng) -> string (URL chuyển hướng)`
    - `ValidateCallback(Dictionary<string, string> callbackData) -> bool`
- **Tầng Infrastructure**:
  - Implement `VNPayService`. Xây dựng logic đọc Config hash secret code (từ VNPAY sandbox). Nối chuỗi để tạo Checksum và gọi URL.
- **Tầng API (Quy trình chuẩn)**:
  1. API `POST /api/Bills/checkout-vnpay`: Tạo Order (Status = Pending) -> Trả về URL cho Mobile/Frontend mở WebView.
  2. API `GET /api/Bills/vnpay-return`: Phía Frontend redirect tới sau khi user thanh toán xong (Kiểm tra checksum giao diện).
  3. **Quét ngầm (IPN/Webhook)** API `GET /api/Bills/vnpay-ipn`: VNPAY Server gọi thẳng vào API Server. Server tiến hành **verify chữ ký -> Update Bill Status -> Ghi nhận doanh thu**. (Bắt buộc phải có hàm này để tránh bị hack).

---

## 🔥 Đề xuất thứ tự Ưu tiên Nâng cấp (Sprint Planning)
1. **Sprint 1 (Dễ, Nền tảng)**: **Phân trang & Lọc nâng cao** -> Cải thiện ngay hiệu năng cho API danh sách sản phẩm.
2. **Sprint 2 (Trung bình, Bắt buộc)**: **Upload Ảnh Cloudinary** -> Giúp ứng dụng Frontend hiển thị Menu món nước sinh động mà không chết Server.
3. **Sprint 3 (Khá khó, Tiện ích)**: **Excel Import/Export** -> Thêm tính năng "Wow" cho người dùng Quản trị (Admin).
4. **Sprint 4 (Khó, Khó test IPN local)**: **Thanh toán VNPay / MoMo** -> Cần sử dụng Ngrok để public Localhost IP tạo Webhook IPN cho VNPay gọi vào chạy test.

*Nếu bạn muốn thực hiện tính năng nào trước, hãy nhắn cho tôi, tôi sẽ cấp ngay Service Code và API endpoint cho tính năng đó!*
