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
                Debug.Log("Firebase đã kết nối thành công!");
            } else {
                Debug.LogError("Không thể kết nối Firebase: " + task.Result);
            }
        });
    }

    public void LoadDataFromServer(string artifactId, TextMeshProUGUI nameUI, TextMeshProUGUI descUI)
    {
        Debug.Log("--- Đang truy vấn Firebase với ID: " + artifactId + " ---");
        dbReference.Child("Artifacts").Child(artifactId).GetValueAsync().ContinueWithOnMainThread(task => {
            DataSnapshot snapshot = task.Result;
            Debug.Log("Dữ liệu thô từ Firebase: " + snapshot.GetRawJsonValue());
            if (snapshot.Exists) {
                string name = snapshot.Child("name").Value.ToString();
                string desc = snapshot.Child("description").Value.ToString();
                
                nameUI.text = name;
                descUI.text = desc;
                
                Debug.Log("Lấy data thành công: " + name);
            } else {
                Debug.LogWarning("Không tìm thấy ID '" + artifactId + "' trên Firebase. Kiểm tra lại tên nhánh!");
            }
        });
    }
}