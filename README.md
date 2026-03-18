# Dự Án FpolyCafe - API Backend

Dự án API Backend cho hệ thống quản lý quán cà phê FpolyCafe, xây dựng bằng C# .NET 8, Clean Architecture.

## Yêu cầu hệ thống (Prerequisites)

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- SQL Server (Developer/Express) hoặc LocalDB.
- Trình biên dịch: Visual Studio 2022 / Visual Studio Code / Rider.

## Hướng dẫn cài đặt và chạy dự án

### Bước 1: Clone dự án
Clone code từ kho lưu trữ về máy phân nhánh của bạn:
```bash
git clone <đường_dẫn_repo_github_của_bạn>
cd FpolyCafe
```

### Bước 2: Cấu hình Database
Mở file `FpolyCafe.Api/appsettings.json` (hoặc `appsettings.Development.json`) và kiểm tra chuỗi kết nối ở phần `"ConnectionStrings": { "DefaultConnection": "..." }`.
Mặc định chuỗi kết nối đang sử dụng là cho server local:
```json
"Server=localhost;Database=FpolyCafe;Trusted_Connection=True;MultipleActiveResultSets=true;Encrypt=False"
```
*(Bạn có thể thay đổi `localhost` thành tên Server SQL của máy cá nhân nếu gặp lỗi kết nối).*

### Bước 3: Chạy ứng dụng (Tự động tạo DB & Dữ liệu mẫu)
Dự án đã được cấu hình tự động chạy Migration và Seed Data (cấp sẵn dữ liệu mẫu) trong lần chạy đầu tiên.

Mở Terminal (hoặc CMD/PowerShell) tại thư mục root (**FpolyCafe**) và chạy lệnh:
```bash
cd FpolyCafe.Api
dotnet run
```
*(Hoặc bạn có thể mở file `FpolyCafe.sln` bằng Visual Studio và ấn `F5` / nút Play xanh).*

Sau khi chạy thành công, trình duyệt sẽ tự động mở trang Swagger: `http://localhost:5189/swagger` (port có thể khác nếu được cấu hình lại trong `launchSettings.json`).

---

## Hướng dẫn Test API dành cho Frontend (FE)

Mọi API quan trọng (ngoại trừ Login) đều yêu cầu phải có Token để truy cập. Dưới đây là các bước quy chuẩn để lấy Token và cấu hình trên giao diện Swagger.

### 1. Lấy Token (Đăng nhập)
Trên giao diện Swagger, tìm đến API:
**`POST /api/Auth/login`**

Bấm **Try it out**, nhập thông tin tài khoản admin đã được tạo sẵn trong Database:
```json
{
  "username": "admin",
  "password": "admin123"
}
```
*(Tài khoản mẫu khác cho nhân viên: `staff1` / Mật khẩu: `staff123`)*

Bấm **Execute**. Kéo xuống phần *Response body*, bạn sẽ nhận được một JSON có chứa trường `"token"`.
**Copy (Sao chép)** đoạn mã token rất dài đó.

### 2. Xác thực (Authorize)
- Cuộn lên đầu trang Swagger, bấm vào nút **Authorize** (hình ổ khóa màu xanh lá ở góc phải trên).
- Trong ô **Value**, nhập **chính xác** theo cú pháp sau (KHÔNG được thiếu từ "Bearer"):
  ```text
  Bearer <dán_mã_token_vào_đây>
  ```
  *(Lưu ý bắt buộc: Chữ `Bearer` viết hoa chữ B đầu tiên, theo sau là **một khoảng trắng/dấu cách** rồi mới đến phần token).*
  *Ví dụ đúng:* `Bearer eyJhbGciOiJIUz...`
- Bấm nút **Authorize**, sau đó bấm **Close** để hoàn tất.

### 3. Bắt đầu gọi API
Bây giờ biểu tượng ổ khóa ở các API khác đã được đóng lại (vào trạng thái an toàn), nghĩa là bạn đã được cấp quyền truy cập. FE có thể bắt đầu test và lấy dữ liệu mẫu bằng cách gọi các API như:
- `GET /api/Categories` (Lấy danh sách các danh mục sản phẩm)
- `GET /api/Products` (Lấy danh sách các mã sản phẩm, xem menu)
- `POST /api/Bills` (Tạo hóa đơn/Gửi Order)
- v.v...

---

## Cấu trúc thư mục dự án (Architecture Layers)
- **FpolyCafe.Domain**: Chứa các đối tượng Entities, Enums (Lõi của hệ thống).
- **FpolyCafe.Application**: Chứa cấu hình Interfaces, DTOs, các Business Services xử lý Logic.
- **FpolyCafe.Infrastructure**: Triển khai các Interfaces (Database configuration, File Storage, Services ngoài,...), kết nối Entity Framework Core.
- **FpolyCafe.Api**: Phần lõi API giao tiếp với bên ngoài (Controllers, Authentication, Middleware, Program.cs).

🎉 **Chúc các thành viên team Frontend ghép API thuận lợi và thành công!** Nếu gặp lỗi `500 Internal Server` do token hoặc `401 Unauthorized` hãy lưu ý lại bước *Mã xác thực Bearer*.