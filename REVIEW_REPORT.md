# Báo cáo rà soát ShoeCommerce

Ngày rà soát: 21/11/2026

## 1. Tổng quan trạng thái

Dự án đã build được bằng .NET 8.0.428 sau khi sửa lỗi seed và cấu hình SDK. Các trang storefront/login render được và route quản trị chưa đăng nhập được redirect về trang đăng nhập nội bộ. Kiểm thử tích hợp MySQL trên cổng 3307 chưa chạy được do MySQL thử nghiệm chưa sẵn sàng.

## 2. Bảng lỗi tìm thấy

| Mức độ | File và dòng | Mô tả lỗi | Nguyên nhân | Trạng thái / cách sửa | Cách test lại |
|---|---|---|---|---|---|
| Critical | `ShoeCommerce/Program.cs:75-82` | Seed dữ liệu yêu cầu `ShoeStoreDbContext`, nhưng DI chỉ đăng ký `IDbContextFactory` và gọi factory từ root provider. | Lifetime và scope DI không phù hợp. | Đã sửa: resolve factory trong scope và seed trong scope. | Chạy app; không còn lỗi resolve service. |
| High | `ShoeCommerce/Services/SessionSecurity.cs:48-59` | Lỗi MySQL trong cookie validation có thể làm request lỗi hoặc crash. | Không bắt exception truy cập database của cookie. | Đã sửa: bắt `DbException`/`InvalidOperationException`, reject và đăng xuất session an toàn. | Tắt MySQL hoặc thay connection rồi reload trang đã đăng nhập. |
| High | `ShoeCommerce/Services/SessionSecurity.cs:65-116` | Thiếu token antiforgery hoặc token hết hạn có thể làm endpoint POST trả lỗi không thân thiện. | Chưa bắt `AntiforgeryValidationException`. | Đã sửa: redirect kèm thông báo tiếng Việt cho login/register/logout. | Reload trang, submit lại; kiểm tra không có HTTP 500. |
| High | `ShoeCommerce/Program.cs` | Token antiforgery có thể mất hiệu lực sau khi app restart do key Data Protection không được lưu ổn định. | Key mặc định không được cấu hình lưu/persist bền vững. | Đã sửa: lưu key Data Protection tại `data-protection-keys/`, áp dụng application name `ShoeCommerce` và mã hóa DPAPI trên Windows. | POST đăng nhập bằng token vừa render; 302 redirect `/admin`. |
| Medium | `ShoeCommerce/Program.cs:24-31` | Authentication cookie thiếu chính sách Secure rõ ràng. | Chỉ cấu hình HttpOnly và SameSite. | Đã sửa: cookie `SecurePolicy=SameAsRequest` trong Development, `Always` ngoài Development. | Kiểm tra `Set-Cookie` trên HTTP/HTTPS. |
| Medium | `ShoeCommerce/Program.cs:63-82` | Seed demo tài khoản quản trị có thể chạy ngoài môi trường phát triển. | Điều kiện seed từng cho phép mọi môi trường nếu cấu hình bật. | Đã sửa: chỉ seed trong Development và khi key bật. | Chạy Production: tài khoản demo không được seed nếu key không đổi. |
| Medium | `ShoeCommerce/Services/AuthService.cs:89-92`, `ShoeCommerce/Services/AuthService.cs:197-198`, `ShoeCommerce/Services/UserService.cs:51,221` | Chính sách mật khẩu tối thiểu 6 ký tự. | Thông số ngắn, chưa đủ mạnh. | Đã sửa: yêu cầu tối thiểu 8 ký tự ở đăng ký, đổi mật khẩu và tạo tài khoản admin. | Thử mật khẩu 7 ký tự, hệ thống phải từ chối. |
| Medium | `ShoeCommerce/Components/Pages/Admin/AdminCustomers.razor:281`, `ShoeCommerce/Components/Pages/Admin/AdminEmployees.razor:152`, `ShoeCommerce/Components/Pages/CustomerProfile.razor:84-94`, `ShoeCommerce/Components/Pages/Register.razor:61-62` | Giao diện nhập mật khẩu còn ghi tối thiểu 6 ký tự. | Placeholder/form không đồng bộ với service. | Đã sửa thành tối thiểu 8 ký tự. | Kiểm tra các form. |
| Medium | `global.json` | SDK pin `8.0.xxx` không hợp lệ. | Phiên bản SDK placeholder. | Đã sửa pin `8.0.428`, cho phép latestPatch. | Chạy `dotnet --version` và build. |
| Medium | `ShoeCommerce/appsettings.Local.example.json:3` | Ví dụ cấu hình chứa mật khẩu mẫu `123456`. | Dữ liệu cấu hình mẫu có secret-like value. | Đã thay bằng placeholder `<your-user>` và `<your-password>`. | Kiểm tra không còn mật khẩu thật trong file mẫu. |
| Low | `.gitignore` | Chưa loại đủ file log/tạm và thư mục IDE. | Thiếu pattern bổ sung. | Đã thêm `*.log`, `*.tmp`, `*.user`, `*.suo`, `.vs/`, `.vscode/`. | Chạy `git status --ignored` và kiểm tra. |

## 3. Danh sách file đã thay đổi

- `global.json`
- `.gitignore`
- `ShoeCommerce/Program.cs`
- `ShoeCommerce/Services/SessionSecurity.cs`
- `ShoeCommerce/Services/AuthService.cs`
- `ShoeCommerce/Services/UserService.cs`
- `ShoeCommerce/Components/Pages/Register.razor`
- `ShoeCommerce/Components/Pages/CustomerProfile.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminCustomers.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminEmployees.razor`
- `ShoeCommerce/appsettings.Local.example.json`
- `REVIEW_REPORT.md`

## 4. Danh sách chưa sửa và lý do

- Chưa chạy bộ kiểm thử tích hợp MySQL 3307 vì máy hiện tại không có MySQL đang lắng nghe cổng đó.
- Chưa chạy browser smoke đầy đủ vì cần database thử nghiệm riêng và Edge/Playwright đã cấu hình.
- Chưa thay đổi lịch sử Git hay mật khẩu MySQL thực. `appsettings.Local.json` đang được Git bỏ qua và không được commit.
- Chưa tăng mật khẩu demo lên chính sách 12 ký tự vì sẽ làm thay đổi tài khoản demo được ghi trong tài liệu; đã dùng mức tối thiểu 8 ký tự để giảm rủi ro mà giữ tương thích demo hiện tại.
- Chưa thay đổi EF từ fixed `MySqlServerVersion(8.0.16)` sang `ServerVersion.AutoDetect`: version hiện tại khớp MySQL 8 và an toàn hơn cho startup; không kết luận tự động detect phù hợp khi database chưa sẵn sàng.

## 5. Kết quả kiểm chứng đã thực hiện

- `dotnet restore`: thành công.
- `dotnet build --no-restore`: thành công, 0 warning, 0 error sau sửa.
- `dotnet test --no-build`: exit code 0; solution không chạy kịch bản tích hợp console trong bước này.
- Khởi động app bằng HTTP: thành công.
- `/`: HTTP 200.
- `/login`: HTTP 200 và render antiforgery token.
- `/admin/login`: HTTP 200 và render antiforgery token.
- `/admin/orders`: redirect 302 về `/admin/login` khi chưa đăng nhập.
- Seed demo: MySQL thực tế cổng cấu hình đã cho phép app khởi động và seed được kiểm tra qua EF.
- Smoke tích hợp cổng 3307: chưa chạy do không kết nối được MySQL.

## 6. Checklist chức năng

| Nhóm chức năng | Trạng thái | Ghi chú |
|---|---|---|
| Tài khoản Admin/Employee/Customer | Đạt | Đăng nhập/cookie/role đã được rà soát và nâng cấp xử lý lỗi. |
| Phân quyền trang `/admin` | Đạt | Có route `[Authorize(Roles = "Admin,Employee")]` và redirect riêng. |
| Phân quyền chức năng Admin | Đạt | Có kiểm tra `AccessGuard` ở service. |
| Quản lý nhân viên | Đạt | Có trang và nghiệp vụ. Cần chạy MySQL để test thêm/sửa/khóa. |
| Quản lý khách hàng | Đạt | Có trang chi tiết, địa chỉ, lịch sử đơn. |
| Danh mục và thương hiệu | Đạt | Có trang quản lý tìm kiếm/thêm/sửa/xóa. |
| Sản phẩm và biến thể | Đạt | Có phân trang, lọc, SKU, size/màu, trạng thái. |
| Đánh giá | Đạt | Có đánh giá khách hàng và trang quản trị xóa. |
| Khuyến mãi | Đạt | Có CRUD và validation nghiệp vụ. |
| Quản lý đơn hàng | Đạt | Có trạng thái, hủy, lịch sử, cập nhật thông tin người nhận. |
| Thanh toán | Đạt | COD và chuyển khoản thủ công đã có luồng. |
| Nhà cung cấp | Đạt | Có CRUD và ràng buộc dữ liệu liên quan. |
| Kho/chứng từ kho | Đạt | Có draft, xác nhận, tự động trừ/hoàn kho và concurrency check. |
| Giỏ hàng | Đạt | Có cộng dồn, cập nhật, xóa, kiểm tra tồn. |
| Checkout | Đạt | Có địa chỉ, mã giảm, COD/chuyển khoản và tạo đơn giao dịch. |
| Lịch sử đơn khách hàng | Đạt | Có danh sách, chi tiết, trạng thái thanh toán/hủy. |
| Thống kê | Đạt | Có bảng, doanh thu theo khoảng ngày và phân bổ trạng thái. |
| Đồ họa biểu đồ | Đạt | Dùng CSS bar chart/Pure CSS, không phụ thuộc thư viện. |
| Upload ảnh | Chưa có xác nhận đầy đủ | Dự án chủ yếu lưu đường dẫn/URL ảnh, chưa có luồng upload file server cần test giới hạn MIME/kích thước. |
| E2E browser đầy đủ | Cần sửa môi trường | Cần MySQL test 3307 và Edge/Playwright mới xác nhận hết kịch bản. |
