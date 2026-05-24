using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;

public class FirebaseManager : MonoBehaviour
{
    DatabaseReference dbReference;

    void Start()
    {
        // Khởi tạo Firebase và kiểm tra các gói phụ thuộc (Dependencies)
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available) {
                dbReference = FirebaseDatabase.GetInstance("https://artourproject-f293c-default-rtdb.firebaseio.com/").RootReference;
                Debug.Log("==> Firebase đã kết nối thành công!");
            } else {
                Debug.LogError("Không thể kết nối Firebase: " + task.Result);
            }
        });
    }

    // Đồng bộ tên biến ở hàm nhận trùng với mạch gọi từ ARPlacementManager sang
    public void LoadDataFromServer(string artifactId, TextMeshProUGUI titleText, TextMeshProUGUI bodyText, GameObject infoCanvas)
    {
        Debug.Log("--- Đang truy vấn Firebase với ID: " + artifactId + " ---");
        
        if (dbReference == null)
        {
            Debug.LogError("Lỗi: dbReference chưa khởi tạo thành công!");
            return;
        }

        dbReference.Child("Artifacts").Child(artifactId).GetValueAsync().ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Lỗi trong quá trình lấy dữ liệu bất đồng bộ từ Firebase!");
                return;
            }

            DataSnapshot snapshot = task.Result;
            Debug.Log("Dữ liệu thô từ Firebase: " + snapshot.GetRawJsonValue());
            
            if (snapshot.Exists) {
                // Trích xuất dữ liệu an toàn tránh lỗi crash nếu node trống
                string name = snapshot.Child("name").Value != null ? snapshot.Child("name").Value.ToString() : "Không có tên";
                string desc = snapshot.Child("description").Value != null ? snapshot.Child("description").Value.ToString() : "Không có mô tả";
                
                // Gán tên cổ vật vào ô chữ tiêu đề UI
                if (titleText != null) 
                {
                    titleText.text = name;
                }
                else 
                {
                    Debug.LogError("=> [LỖI THAM CHIẾU]: Ô ghim 'Title Text' truyền từ ARPlacementManager sang đang bị trống (Null)!");
                }

                // Gán mô tả cổ vật vào ô chữ nội dung UI
                if (bodyText != null) 
                {
                    bodyText.text = desc;
                }
                else 
                {
                    Debug.LogError("=> [LỖI THAM CHIẾU]: Ô ghim 'Body Text' truyền từ ARPlacementManager sang đang bị trống (Null)!");
                }
                
                // Kích hoạt bảng UI hiển thị lên màn hình điện thoại
                if (infoCanvas != null) 
                {
                    infoCanvas.SetActive(true);
                    Debug.Log("==> [UI] Đã kích hoạt hiển thị bảng infoCanvas lên màn hình thành công!");
                }
                else
                {
                    Debug.LogError("=> [LỖI THAM CHIẾU]: Ô ghim 'Info Canvas' truyền sang đang bị trống (Null)!");
                }

                Debug.Log("Lấy data thành công từ Server: " + name);
            } else {
                Debug.LogWarning("Không tìm thấy ID '" + artifactId + "' trên Firebase. Kiểm tra lại tên nhánh!");
            }
        });
    }
}