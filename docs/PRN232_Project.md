# Phân quyền

* `Admin` được truy cập Admin Panel.
* `User` và `Seller` chỉ truy cập Marketplace.
* API Admin yêu cầu JWT có role `Admin`.

# Login , logout

Quy trình:

1. Người dùng nhập email và password.
2. API chuẩn hóa email.
3. Tìm tài khoản trong database.
4. Kiểm tra trạng thái tài khoản.
5. Kiểm tra password bằng BCrypt.
6. Tạo JWT nếu thông tin hợp lệ.
7. MVC lưu JWT vào Session.
8. Điều hướng theo role.

Hệ thống hiện tại chỉ có access token dưới dạng JWT.

* `POST /api/auth/login` trả về `token`.
* Token được gửi qua `Authorization: Bearer <token>`.
* Thời hạn mặc định: 60 phút.
* Chưa có refresh token.
* Chưa có endpoint `/api/auth/refresh`.
* Khi token hết hạn, người dùng phải đăng nhập lại.
* Logout chỉ xóa JWT khỏi MVC Session; JWT đã phát hành không bị revoke ngay lập tức.

# Dashboard

* Hiển thị tổng User, Product, Order, doanh thu.
* Hiển thị User Active/Banned, Product Hidden.
* Hiển thị cảnh báo khi có Dispute cần xử lý.

# Quản lý người dùng

* Hỗ trợ tìm kiếm, lọc theo role, trạng thái, sắp xếp và phân trang.
* Admin có thể xem danh sách và chi tiết tài khoản.
* Admin có thể duyệt tài khoản:
  * `Pending → Active`
* Admin có thể khóa tài khoản đang hoạt động:
  * Bắt buộc nhập lý do khóa.
  * `Active → Banned`
* Admin có thể mở khóa tài khoản:
  * `Banned → Active`
* Sau khi thay đổi thành công:
  * Hiển thị thông báo trên giao diện bằng `TempData`.
  * Gửi thông báo realtime bằng SignalR đến các Admin đang online.
  * Ghi lịch sử thao tác vào Audit Log.
* Không cho phép thực hiện sai trạng thái, ví dụ khóa tài khoản đã bị khóa.

Các chức năng khác cũng có thể viết tương tự:

# Quản lý sản phẩm

* Hỗ trợ tìm kiếm, lọc theo Seller, trạng thái, sắp xếp và phân trang.
* Admin có thể xem danh sách và chi tiết sản phẩm.
* Admin có thể ẩn sản phẩm đang hoạt động:
  * `Active → Hidden`
* Admin có thể hiển thị lại sản phẩm:
  * `Hidden → Active`
* Sau khi thay đổi:
  * Hiển thị thông báo.
  * Gửi thông báo SignalR.
  * Ghi Audit Log.

# Quản lý đơn hàng

* Hỗ trợ lọc theo trạng thái, người mua, khoảng thời gian, sắp xếp và phân trang.
* Admin có thể xem danh sách đơn hàng.
* Admin có thể xem chi tiết sản phẩm, thanh toán và giao hàng.
* Chức năng hiện tại chủ yếu là giám sát, chưa cho Admin thay đổi trạng thái đơn hàng.

# Quản lý khiếu nại

* Hỗ trợ lọc theo trạng thái và phân trang.
* Admin có thể xem danh sách và chi tiết khiếu nại.
* Admin có thể phân công khiếu nại cho một Admin:
  * `Open → Assigned`
* Admin có thể giải quyết khiếu nại:
  * Bắt buộc nhập nội dung xử lý.
  * `Assigned → Resolved`
* Admin có thể từ chối khiếu nại:
  * Bắt buộc nhập lý do.
  * `Open → Rejected`
* Sau khi xử lý:
  * Hiển thị thông báo.
  * Gửi SignalR.
  * Ghi Audit Log.

# Quản lý Feedback

* Hỗ trợ lọc theo Seller, rating tối thiểu, rating tối đa và phân trang.
* Admin có thể xem điểm đánh giá trung bình.
* Admin có thể xem tổng số review.
* Admin có thể xem tỷ lệ đánh giá tích cực.
* Chức năng hiện tại chỉ đọc, chưa hỗ trợ chỉnh sửa hoặc xóa.

# Audit Logs

* Hỗ trợ tìm kiếm và lọc theo action, resource, Admin và thời gian.
* Admin có thể xem lịch sử các thao tác quản trị.
* Ghi nhận người thực hiện, đối tượng bị tác động, thời gian và metadata.
* Hỗ trợ xuất danh sách Audit Log ra file Excel.

# Reports

* Hỗ trợ lọc báo cáo theo khoảng thời gian.
* Hiển thị tổng số User, Product và Order.
* Hiển thị doanh thu từ các Payment có trạng thái `Paid`.
* Thống kê User, Product, Order và Dispute theo trạng thái.
* Thống kê các thao tác Audit phổ biến.

# SignalIR



| Chức năng | SignalR |
| ----- | ----- |
| Duyệt/khóa/mở khóa User | Có |
| Ẩn/hiện Product | Có |
| Cache refresh thất bại | Có |
| Xử lý Dispute | Chưa |
| Đơn hàng | Chưa |
| Feedback | Chưa |
| Audit Logs | Chưa |
| Reports | Chưa |

# Offline Mode ở MVC:

* Khi API mất kết nối hoặc timeout, hệ thống đặt `OfflineMode = true`.
* Các dữ liệu GET đã cache vẫn được hiển thị.
* Hiển thị banner “Offline Mode”.
* Các thao tác thay đổi dữ liệu bị vô hiệu hóa.
* Khi API hoạt động lại, hệ thống tự chuyển về Online.
* Không hỗ trợ lưu thao tác offline để đồng bộ sau.
