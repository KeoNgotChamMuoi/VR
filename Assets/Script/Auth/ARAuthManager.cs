using System;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;

public class ARAuthManager : MonoBehaviour
{
    [Header("UI Input Fields")]
    public TMPro.TMP_InputField inputEmail;
    public TMPro.TMP_InputField inputPassword;
    public TMPro.TextMeshProUGUI txtStatus;

    [Header("AR System Layers")]
    public GameObject Canvas_Auth;       // Layer màn hình đăng nhập phẳng
    public GameObject AR_Gameplay_Layer; // Layer chứa cụm quét sàn và gameplay AR

    private FirebaseAuth auth;
    private string nhatKyStatus = "";
    private bool canChuyenCanhAR = false;
    private bool dangXulyFirebase = false;

    void Start()
    {
        txtStatus.text = "Đang kết nối server...";

#if UNITY_EDITOR
        // BYPASS TRÊN PC EDITOR: Khởi tạo thẳng để tránh bẫy nghẽn vô hạn của Google
        try
        {
            auth = FirebaseAuth.DefaultInstance;
            txtStatus.text = "Kết nối Server thành công! Mời nhập thông tin.";
        }
        catch (Exception ex)
        {
            Debug.LogWarning("Firebase Editor Init Warning: " + ex.Message);
            txtStatus.text = "Editor Mode: Sẵn sàng tương tác.";
        }
#else
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == DependencyStatus.Available)
            {
                auth = FirebaseAuth.DefaultInstance;
                nhatKyStatus = "Kết nối Server thành công!";
            }
            else
            {
                nhatKyStatus = "Lỗi kết nối Firebase: " + dependencyStatus;
                Debug.LogError($"Không thể khởi tạo Firebase: {dependencyStatus}");
            }
        });
#endif
    }

    void Update()
    {
        // Liên tục lắng nghe luồng ngầm để cập nhật chữ lên giao diện (Main Thread)
        if (!string.IsNullOrEmpty(nhatKyStatus))
        {
            txtStatus.text = nhatKyStatus;
            nhatKyStatus = ""; // In xong thì xóa bộ nhớ tạm
        }

        // Nếu nhận được cờ chuyển cảnh thành công từ luồng mạng
        if (canChuyenCanhAR)
        {
            canChuyenCanhAR = false;
            ThucHienChuyenCanh();
        }
    }

    // Hàm xử lý khi bấm nút ĐĂNG KÝ
    public void ButtonRegisterClick()
    {
        string email = inputEmail.text.Trim();
        string password = inputPassword.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            txtStatus.text = "Vui lòng nhập đầy đủ Email và Mật khẩu!";
            return;
        }

        txtStatus.text = "Đang tiến hành đăng ký...";
        dangXulyFirebase = true; // Đánh dấu bắt đầu gửi lệnh lên Server

        if (auth == null) auth = FirebaseAuth.DefaultInstance;

        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            dangXulyFirebase = false; // Luồng ngầm đã phản hồi xong (không bị nghẽn vô hạn)

            if (task.IsCanceled) {
                nhatKyStatus = "Đăng ký bị hủy!";
                return;
            }
            if (task.IsFaulted) {
                nhatKyStatus = "Lỗi: Email sai định dạng hoặc đã tồn tại!";
                return;
            }
            if (task.IsCompleted) {
                nhatKyStatus = "Đăng ký thành công!";
                canChuyenCanhAR = true; // Kích hoạt đổi cảnh ngoài Update
            }
        });

#if UNITY_EDITOR
        // CHỐT CHẶN BẢO VỆ TRÊN EDITOR: 
        // Sau 2.5 giây, CHỈ ÉP VÀO GAME nếu luồng ngầm Firebase im lặng hoàn toàn (bị kẹt thread ngầm)
        Invoke("KiemTraEpBuocChuyenCanh", 2.5f);
#endif
    }

    // Hàm xử lý khi bấm nút ĐĂNG NHẬP
    public void ButtonLoginClick()
    {
        string email = inputEmail.text.Trim();
        string password = inputPassword.text.Trim();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            txtStatus.text = "Vui lòng nhập đầy đủ Email và Mật khẩu!";
            return;
        }

        txtStatus.text = "Đang tiến hành xác minh...";
        dangXulyFirebase = true; // Đánh dấu bắt đầu gửi lệnh

        if (auth == null) auth = FirebaseAuth.DefaultInstance;

        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWith(task => {
            dangXulyFirebase = false; // Luồng ngầm đã phản hồi xong

            if (task.IsCanceled) {
                nhatKyStatus = "Đăng nhập bị hủy!";
                return;
            }
            if (task.IsFaulted) {
                nhatKyStatus = "Lỗi: Sai tài khoản hoặc mật khẩu!";
                return;
            }
            if (task.IsCompleted) {
                nhatKyStatus = "Đăng nhập thành công!";
                canChuyenCanhAR = true; // Kích hoạt đổi cảnh ngoài Update
            }
        });

#if UNITY_EDITOR
        // CHỐT CHẶN BẢO VỆ TRÊN EDITOR CHO ĐĂNG NHẬP
        Invoke("KiemTraEpBuocChuyenCanh", 2.5f);
#endif
    }

    private void KiemTraEpBuocChuyenCanh()
    {
        // CHỈ ÉP VÀO GAME khi màn hình Auth còn bật VÀ Firebase thực sự bị treo (dangXulyFirebase vẫn là true)
        // Nếu Firebase phản hồi nhanh (ví dụ báo trùng Email), biến đã về false -> Hàm này sẽ tự bỏ qua.
        if (Canvas_Auth != null && Canvas_Auth.activeSelf && dangXulyFirebase)
        {
            Debug.LogWarning("==> Kích hoạt cơ chế bọc lót: Tự động vào thẳng màn AR do kẹt luồng ngầm Editor <==");
            dangXulyFirebase = false;
            txtStatus.text = "Đăng nhập thành công (Editor Mode)!";
            ThucHienChuyenCanh();
        }
    }

    private void ThucHienChuyenCanh()
    {
        if (Canvas_Auth != null) Canvas_Auth.SetActive(false);         // Ẩn màn đăng nhập xám
        if (AR_Gameplay_Layer != null) AR_Gameplay_Layer.SetActive(true); // Bật cụm quét sàn AR lên
    }
}