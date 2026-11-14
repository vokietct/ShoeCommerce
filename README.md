ae dùng vscode để mở project thì mở vào trong thư mục ShoeCommerce luôn nha, còn mấy thư mục .sln hay .vscode chỉ là cấu hình của project

## Chạy project

- Cần .NET 8 SDK và MySQL 8.
- Database mới thì chạy file `ShoeCommerce/data/DB.txt` trong MySQL Workbench để tạo `shoestore_db`.
- Copy `ShoeCommerce/appsettings.Local.example.json` thành `ShoeCommerce/appsettings.Local.json` rồi điền user/pass MySQL của máy mình (file này không đẩy lên git).
- Chạy:

```
dotnet run --project ShoeCommerce --launch-profile http
```

Cửa hàng: http://localhost:5220 - Trang quản trị: http://localhost:5220/admin/login

Tài khoản demo (khi `Database:SeedDemoData` = true):

| Vai trò | Email | Mật khẩu |
| --- | --- | --- |
| Quản lý | admin@shoestore.vn | admin123 |
| Nhân viên | linh@shoestore.com | employee123 |
| Khách hàng | minhanh@example.com | customer123 |
