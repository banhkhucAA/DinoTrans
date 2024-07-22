Đây là chương trình sàn giao dịch vận tải DinoTrans, nơi kết nối giữa bên cần vận chuyển các máy móc xây dựng với bên cho thuê dịch vụ vận tải sau
- Luồng logic chính: Bên công ty máy xây dựng cần vận chuyển các máy móc xây dựng hạng nặng với khoảng cách xa, họ cần thuê công ty cung cấp dịch vụ vận tải để vận chuyển các loại máy móc này. Công ty sở hữu máy xây dựng sẽ tạo thầu,
sau đó các công ty vận chuyển sẽ thấy được các thầu đó để tiến hành đấu thầu. Cuộc đấu thầu được diễn ra trên nguyên tắc các công ty vận chuyển đặt giá vận chuyển một cách độc lập nhau, công ty sở hữu máy vận chuyển sẽ là người quyết
ịnh cuối cùng xem bên nào sẽ là người đấu thầu.
- Chương trình tách biệt 2 server riêng cho Front-end và Back-end
- Về phần Front-end: Chương trình sử dụng HTML, CSS, Bootstrap, Blazor WebAssembly để nhận dữ liệu từ phía Back-end trả về sau đó hiển thị dữ liệu.
- Về phần Back-end: Chương trình sử dụng ASP.Net Core Web API để tạo các endpoint cho từng chức năng riêng.
** Các chức năng chính
- Đăng nhập, đăng ký, phân quyền người dùng theo công ty (bằng JWT) (công ty máy xây dựng hay công ty cho thuê dịch vụ vận tải) và theo cấp bậc trong công ty (chủ công ty/ nhân viên)
- Quản lý thông tin cá nhân, thông tin nhân viên công ty, thông tin công ty
- Quản lý thầu theo thời gian thực (signalR) (để đảm báo tính chính xác, đúng đắn của thời gian của các buổi đấu thầu)
