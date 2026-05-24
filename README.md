# 🏛️ AR Tour Artifacts - Khám Phá Cổ Vật Việt Nam Qua Công Nghệ AR

Dự án nghiên cứu và phát triển ứng dụng di động sử dụng công nghệ Thực tế ảo tăng cường (AR) kết hợp cơ sở dữ liệu đám mây thời gian thực nhằm mục đích số hóa, bảo tồn và nâng cao trải nghiệm tương tác với các di sản lịch sử văn hóa Việt Nam.

Ứng dụng cho phép người dùng đăng nhập hệ thống, quét môi trường thực tế để tự động rải các mô hình 3D cổ vật quốc gia (như Chùa Một Cột, Trống đồng Đông Sơn...) và tương tác chạm (Touch) để tra cứu dữ liệu lịch sử thời gian thực từ Firebase.

---

## 🚀 Tính Năng Cốt Lõi

* **Xác Thực Người Dùng (Firebase Auth):** Hệ thống đăng ký, đăng nhập an toàn kết nối luồng quản lý trạng thái (`Canvas_Auth`), tự động giải phóng tài nguyên màn hình phẳng khi chuyển cảnh AR.
* **Nhận Diện Không Gian (SLAM - AR Foundation):** Quét mặt sàn phẳng (`ARPlaneManager`) thời gian thực dựa trên các lưới điểm ảo ổn định của thiết bị di động.
* **Tự Động Rải Cổ Vật (Auto Spawn):** Tính toán vòng tròn lượng giác quanh người chơi để tự động sinh đồng loạt mô hình 3D (.glb) sắc nét lên mặt sàn thực tế mà không bị chồng đè vị trí.
* **Tương Tác Vật Lý Đời Mới (Raycasting):** Bắn tia va chạm từ ngón tay trúng `Box Collider` để trích xuất chính xác tên định danh (ID) vật thể, tương thích hoàn hảo với hệ thống cảm ứng mới.
* **Đồng Bộ Đám Mây (Firebase Realtime Database):** Đồng bộ hóa đa luồng dữ liệu (`Multi-threading`) bất đồng bộ, tải trực tiếp dữ liệu văn bản lịch sử (`name` và `description`) đổ thẳng về giao diện Canvas điện thoại chỉ trong <0.5 giây.

---

## 🛠️ Tech Stack & Cấu Hình Hệ Thống

* **Game Engine:** Unity 2022.3 LTS+ (hoặc bản tương đương sử dụng OpenXR).
* **Công nghệ AR:** AR Foundation 5.x kết hợp ARCore (Android) / ARKit (iOS).
* **Hệ thống Input:** Unity Input System Package (New) – Thay thế hoàn toàn hệ thống driver cũ để tương thích cảm ứng đa điểm thế hệ mới và khắc phục triệt để lỗi xung đột `InvalidOperationException`.
* **Cơ sở dữ liệu:** Firebase Unity SDK 11.x (Auth & Realtime Database).
* **Định dạng Asset:** Mô hình 3D tối ưu dạng low-poly `.prefab` / `.glb` kết hợp thư viện gLTFast.

---

## 📂 Cấu Trúc Thư Mục Source Code Chính

```text
Assets/
 ├── Prefabs/
 │    └── Chuamotcot.prefab      # Model 3D Chùa Một Cột (Gán tag "Artifact", Box Collider)
 ├── Script/
 │    ├── ARAuthManager.cs       # Luồng đăng nhập, xử lý luồng an toàn trên Unity Editor & Mobile
 │    ├── ARPlacementManager.cs  # Logic quét mặt phẳng, tự động rải và xử lý Raycast Input mới
 │    └── FirebaseManager.cs     # Kết nối Database, trích xuất chuỗi bất đồng bộ và đồng bộ hóa UI
 └── Scenes/
      └── SampleScene.unity      # Phân cảnh chính (Cấu hình XR Origin, Canvas UI, EventSystem mới)
⚙️ Hướng Dẫn Cấu Hình Hệ Thống
1. Cấu hình cấu trúc dữ liệu trên Firebase Console
Truy cập vào Realtime Database của dự án và khởi tạo cấu trúc JSON mẫu nằm trong node cha Artifacts như sau (Key ID của cổ vật bắt buộc phải trùng khớp 100% với tên Prefab đặt trong Unity):
{
  "Artifacts": {
    "Chuamotcot": {
      "name": "Chùa Một Cột (Diên Hựu Tự)",
      "description": "Ngôi chùa được xây dựng vào năm 1049 dưới thời vua Lý Thái Tông. Kiến trúc độc đáo mô phỏng một đóa hoa sen nở rộ sừng sững giữa hồ, là biểu tượng văn hóa và tâm linh cao quý của thủ đô Hà Nội."
    }
  }
}
2. Liên kết dây mạch (Ghim References) trên Unity Inspector
  - Bấm vào Object AR_Gameplay_Layer trong cửa sổ Hierarchy.
  - Kéo thả các thành phần tương ứng vào component AR Placement Manager (Script) để kết nối mạch dữ liệu:
    + Info Canvas $\rightarrow$ Kéo khung Canvas chứa bảng thông tin hiển thị cổ vật vào.
    + Title Text $\rightarrow$ Kéo ô chữ tiêu đề (TextMeshPro) vào.
    + Body Text $\rightarrow$ Kéo ô chữ nội dung mô tả (TextMeshPro) vào.
    + Plane Manager $\rightarrow$ Nắm đầu Object XR Origin kéo thả vào để kích hoạt cảm biến sàn.
    + Models To Place $\rightarrow$ Thiết lập Size = 1 và kéo Prefab Chuamotcot từ ô Project vào ô Element 0.
  - Kiểm tra Object EventSystem: Bấm nút Replace with InputSystemUIInputModule tại bảng Inspector để UI nhận driver cảm ứng đời mới.
  - Kiểm tra Object UI_Camera trong Canvas_Auth: Tiến hành gỡ bỏ (Remove Component) Audio Listener thừa để tránh xung đột tiếng âm thanh với Main Camera.
📱 Quy Trình Build Ứng Dụng (Android)
1.Vào File -> Build Settings, chuyển nền tảng sang Android.
2.Mở Project Settings -> XR Plug-in Management, tích chọn kích hoạt ARCore.
3.Mở Player Settings -> Other Settings:
  - Graphics APIs: Loại bỏ Vulkan (Chỉ giữ lại OpenGLES3).
  - Minimum API Level: Chọn từ Android 8.0 'Oreo' (API Level 26) trở lên để hỗ trợ thư viện AR.
  - Scripting Backend: Chuyển từ Mono sang IL2CPP và tích chọn kiến trúc ARM64 để tối ưu hóa hiệu năng thiết bị.
4.Cấu hình tùy biến tệp Gradle custom (mainTemplate.gradle) để xử lý các xung đột thư viện Metadata giữa Firebase SDK và hệ thống Unity Build.
5.Bấm Build hoặc Build and Run để xuất trực tiếp file .apk cài đặt lên điện thoại.
⚖️ Giấy Phép & Bản Quyền
Dự án được phát triển phục vụ mục đích nghiên cứu học thuật, phát triển ứng dụng di động bảo tồn văn hóa di sản Việt Nam.
