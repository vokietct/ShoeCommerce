# Báo cáo rà soát frontend

Ngày rà soát: 21/11/2026

## Danh sách lỗi đã sửa

| Trang/component | Mô tả lỗi | Mức độ | File đã sửa | Kiểm chứng |
|---|---|---|---|---|
| Footer storefront | Liên kết mạng xã hội dùng `href="#"` nhưng không có trang đích. | High | `ShoeCommerce/Components/Layout/MainLayout.razor` | Không còn link chết; icon hiển thị dưới dạng nội dung thông tin. |
| Đánh giá/thống kê admin | Đường dẫn fallback `/images/placeholder-shoe.png` không tồn tại. | High | `AdminReviews.razor`, `AdminStatistics.razor` | Đổi sang ảnh sản phẩm mặc định có thật. |
| Toàn bộ ảnh | Có nguy cơ hiện icon broken image khi URL ảnh bị sai hoặc thiếu. | High | `ShoeCommerce/wwwroot/app.js`, `Components/App.razor` | Thêm fallback toàn cục sang ảnh giày mặc định. |
| Sidebar admin | Sidebar mobile có thể đóng ngoài ý muốn do `@onclick` trên toàn bộ aside. | Medium | `AdminLayout.razor` | Chỉ đóng bằng backdrop/nút điều khiển. |
| Counter/Weather | Hai route template còn nội dung demo không thuộc đồ án. | High | `Counter.razor`, `Weather.razor` | `/counter` và `/weather` redirect 302 về trang chủ. |
| Xóa giỏ hàng/sản phẩm | Thao tác xóa làm trống giỏ không có xác nhận. | High | `CartPage.razor` | Thêm `confirm()` cho xóa món và làm trống giỏ. |
| Xóa dữ liệu admin | Xóa sản phẩm, khách hàng, nhân viên, danh mục, thương hiệu, khuyến mãi, nhà cung cấp, biến thể không có xác nhận. | High | Các trang `AdminProducts`, `AdminCustomers`, `AdminEmployees`, `AdminCategoriesBrands`, `AdminPromotions`, `AdminSuppliers`, `AdminProductVariants` | Thêm thông báo xác nhận tiếng Việt trước khi gọi service. |
| Phiếu kho | Xác nhận/xóa phiếu chưa có xác nhận người dùng. | High | `AdminStockDocuments.razor` | Thêm xác nhận; ghi rõ sau xác nhận không thể sửa/xóa. |
| Địa chỉ khách hàng | Xóa địa chỉ chưa có xác nhận. | Medium | `Components/Shared/AddressBook.razor` | Thêm xác nhận trước khi xóa. |
| Checkout | Chỉ có một lỗi chung, không chỉ rõ trường nào thiếu. | Medium | `CheckoutPage.razor` | Tách thông báo cho họ tên, điện thoại và địa chỉ. |
| Trang chủ | Không có loading/empty state rõ ràng cho danh mục và sản phẩm nổi bật. | Medium | `Home.razor` | Thêm spinner và empty state. |
| Bộ lọc kho/đơn/thanh toán | Có nhiều nhãn tiếng Anh như Pending, Confirmed, Shipping, Draft. | Medium | `AdminOrders.razor`, `AdminPayments.razor`, `AdminStockDocuments.razor`, `AdminStockDocumentNew.razor` | Bỏ nhãn trạng thái tiếng Anh khỏi select. |
| Mobile admin/storefront | CSS responsive cho bảng, topbar, search chưa đủ cho màn hình nhỏ. | Medium | `wwwroot/app.css` | Thêm overflow mượt, giới hạn search/topbar trên mobile. |

## Danh sách file đã thay đổi

- `ShoeCommerce/Components/App.razor`
- `ShoeCommerce/Components/Layout/MainLayout.razor`
- `ShoeCommerce/Components/Layout/AdminLayout.razor`
- `ShoeCommerce/Components/Shared/AddressBook.razor`
- `ShoeCommerce/Components/Pages/Counter.razor`
- `ShoeCommerce/Components/Pages/Weather.razor`
- `ShoeCommerce/Components/Pages/CartPage.razor`
- `ShoeCommerce/Components/Pages/CheckoutPage.razor`
- `ShoeCommerce/Components/Pages/Home.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminProducts.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminCustomers.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminEmployees.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminCategoriesBrands.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminPromotions.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminSuppliers.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminProductVariants.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminStockDocuments.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminOrders.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminPayments.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminStockDocumentNew.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminReviews.razor`
- `ShoeCommerce/Components/Pages/Admin/AdminStatistics.razor`
- `ShoeCommerce/wwwroot/app.css`
- `ShoeCommerce/wwwroot/app.js`

## Kết quả build và smoke

- `dotnet build ... --no-restore`: thành công, 0 warning, 0 error.
- `/`: HTTP 200.
- `/products`: HTTP 200.
- `/login`: HTTP 200.
- `/admin/login`: HTTP 200.
- `/cart`: HTTP 200.
- `/counter`: HTTP 302 redirect về `/`.
- `/weather`: HTTP 302 redirect về `/`.
- `/checkout`: HTTP 302 redirect tới login do route yêu cầu khách hàng.
- `/orders`: HTTP 302 redirect tới login do route yêu cầu khách hàng.

## Danh sách chưa kiểm tra trực quan đầy đủ

- Chưa chụp toàn bộ responsive tại 1440px, 1280px, 1024px, 768px, 414px, 375px, 320px trong phiên làm việc này.
- Chưa chạy Playwright trên Edge với tài khoản Admin/Employee/Customer.
- Chưa kiểm tra từng modal form bằng tương tác chuột thực tế.
- Chưa xử lý 100% mọi bảng admin thành card riêng trên mobile; phạm vi đã giữ `.table-responsive` và có thể cuộn ngang.

## Checklist test UI đề xuất

| Trang | Kiểm tra | Trạng thái |
|---|---|---|
| Trang chủ | Hero, danh mục, sản phẩm nổi bật, loading/empty | Chưa có bằng chứng Playwright |
| Danh sách sản phẩm | Card, lọc, phân trang, mobile | Chưa có bằng chứng Playwright |
| Chi tiết sản phẩm | Size/màu, số lượng, add to cart | Chưa có bằng chứng Playwright |
| Giỏ hàng | Cập nhật số lượng, xóa, làm trống, tổng tiền | Chưa có bằng chứng Playwright |
| Checkout | Validation từng trường, COD/chuyển khoản, submit | Chưa có bằng chứng Playwright |
| Đăng nhập/đăng ký | Antiforgery, thông báo lỗi, loading | Đã smoke GET; chưa chạy submit E2E |
| Admin dashboard | Thống kê, biểu đồ, mobile menu | Chưa có bằng chứng Playwright |
| Bảng admin | Scroll ngang, badge, date/price, confirm xóa | Build xanh; chưa thao tác trực tiếp |
| Ảnh | Không còn broken image; fallback dùng ảnh mặc định | Đã thêm global fallback |
| Route demo | `/counter`, `/weather` không còn hiển thị template | Đã redirect |

## Đề xuất cải tiến tiếp theo

- Thay native `confirm()` bằng modal Bootstrap dùng chung để giao diện đồng nhất hơn.
- Thêm component `ConfirmDialog` và `StatusBadge` dùng lại trên admin.
- Thêm skeleton cho danh sách sản phẩm và dữ liệu admin.
- Thêm validation HTML/inline cho toàn bộ modal thêm/sửa admin.
- Chạy Playwright ở các breakpoint và lưu ảnh kiểm chứng.
