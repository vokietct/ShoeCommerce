# Kết quả rà soát — 14/11/2026

Đối chiếu code với `doc_content.txt`, 42 màn hình trong `extracted_screens_summary.txt`, bản `.fig`/thumbnail tại `designUI` và file DB đính kèm. Chưa đọc được Figma trực tuyến nên chưa xác nhận độ khớp từng pixel với bản trên Figma.

| Nhóm | Phần bổ sung/sửa |
| --- | --- |
| Đăng nhập | Cookie, antiforgery, giới hạn tần suất, chặn redirect ngoài, giữ phiên sau reload |
| Phân quyền | Kiểm tra route, service, tài khoản đang hoạt động, người thao tác và quyền sở hữu |
| Tài khoản/địa chỉ | Transaction tạo user/customer/cart; đổi mặc định không vi phạm unique index; Admin quản lý sổ địa chỉ khách |
| Sản phẩm | Khoảng giá khớp cùng một biến thể; phân trang/sắp xếp SQL; chặn chi tiết sản phẩm ngừng bán |
| Giỏ | Đồng bộ với checkout; chặn sản phẩm ngừng bán; cộng dồn có giới hạn; cập nhật badge ngay |
| Đặt hàng | Từ chối mã giảm giá sai; rollback toàn bộ khi lỗi; tránh bán vượt tồn khi tranh mua |
| Đơn | Khóa đơn khi thay đổi; hoàn kho một lần; trang thành công chỉ cho chủ đơn và hiển thị trạng thái hiện tại |
| Thanh toán | Chặn xác nhận chuyển khoản cho COD; đồng bộ với hủy/chuyển trạng thái; xác nhận kiểm tra chưa nhận tiền trước khi hủy |
| Kho | Giao diện sửa nháp; kiểm tra dòng phiếu/NCC; khóa phiếu, tồn kho nguyên tử; rollback nhiều dòng |
| Giao diện | Tìm danh mục/thương hiệu; menu mobile; tải dữ liệu khi tham số route đổi |
| Cấu hình | MySQL riêng được Git bỏ qua; thông tin ngân hàng cấu hình dùng chung; hướng dẫn chạy/test |

Kiểm chứng: build .NET 10; 57 kiểm tra tích hợp trên MySQL 8.0.41, gồm tranh mua đơn vị cuối, xác nhận phiếu đồng thời và hủy đơn đồng thời. Kịch bản trình duyệt lưu tại `tests/browser/smoke.cjs`.

Database cổng 3306 từ chối thông tin kết nối trong project. Kiểm thử dùng MySQL tạm riêng cổng 3307, không sửa dữ liệu database hiện có. Cần điền tài khoản MySQL và thông tin ngân hàng thực tế khi triển khai.

Phạm vi theo tài liệu: COD/chuyển khoản xác nhận thủ công; không tích hợp cổng thanh toán, đổi trả hoặc hoàn tiền. Lỗi `CASCADE` với cột sinh tự động trong DB đính kèm được giải thích tại README.
