using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacementManager : MonoBehaviour
{
    [Header("Firebase & UI")]
    public GameObject infoCanvas;
    public FirebaseManager firebaseManager;
    public TMPro.TextMeshProUGUI titleText;
    public TMPro.TextMeshProUGUI bodyText;

    [Header("AR Foundation Components")]
    public ARPlaneManager planeManager; // Quản lý vòng đời của các mặt phẳng quét được

    [Header("3D Models Setup")]
    public List<GameObject> modelsToPlace; // Danh sách các Model 3D cậu muốn rải (Kéo thả nhiều Prefab vào đây)
    public float spawnRadius = 1.5f;       // Bán kính rải model quanh người chơi (mét)

    private List<GameObject> spawnedObjects = new List<GameObject>();
    private bool hasSpawnedModels = false; // Cờ đánh dấu để chỉ tự động sinh 1 lần duy nhất khi vào game

    void OnEnable()
    {
        // Đăng ký sự kiện: Khi AR Plane Manager quét được mặt phẳng mới, nó sẽ gọi hàm OnPlanesChanged
        if (planeManager != null)
        {
            planeManager.planesChanged += OnPlanesChanged;
        }
    }

    void OnDisable()
    {
        // Hủy đăng ký sự kiện khi script bị tắt để tránh rò rỉ bộ nhớ
        if (planeManager != null)
        {
            planeManager.planesChanged -= OnPlanesChanged;
        }
    }

    // Hàm tự động kích hoạt khi môi trường AR có sự thay đổi (tìm thấy sàn)
    private void OnPlanesChanged(ARPlanesChangedEventArgs args)
    {
        // Nếu đã sinh model rồi, hoặc chưa quét được mặt phẳng nào mới -> Bỏ qua
        if (hasSpawnedModels || args.added == null || args.added.Count == 0) return;

        // Lấy mặt phẳng đầu tiên quét được làm nền móng
        ARPlane firstPlane = args.added[0];
        float groundY = firstPlane.transform.position.y; // Độ cao chuẩn của mặt sàn thực tế

        // Tiến hành rải tự động danh sách Model
        AutoSpawnModelsAroundPlayer(groundY);
    }

    private void AutoSpawnModelsAroundPlayer(float groundHeight)
    {
        hasSpawnedModels = true; // Đánh dấu đã sinh xong, không chạy lại nữa
        Vector3 playerPos = Camera.main.transform.position; // Vị trí người cầm điện thoại

        Debug.Log($"==> Đã tìm thấy sàn ở độ cao {groundHeight}m. Bắt đầu tự động sinh {modelsToPlace.Count} Model 3D... <==");

        // Vòng lặp duyệt qua từng Model trong danh sách cậu chuẩn bị
        for (int i = 0; i < modelsToPlace.Count; i++)
        {
            // Tính toán góc rải đều (Ví dụ có 3 model thì chia góc 120 độ, 4 model chia 90 độ để không bị đè nhau)
            float angle = i * (360f / modelsToPlace.Count) * Mathf.Deg2Rad;

            // Tính tọa độ X và Z dựa trên vòng tròn lượng giác quanh Player
            float spawnX = playerPos.x + Mathf.Cos(angle) * spawnRadius;
            float spawnZ = playerPos.z + Mathf.Sin(angle) * spawnRadius;

            // Vị trí xuất hiện cuối cùng: Lấy X, Z ngẫu nhiên quanh người, nhưng Y phải ép bằng độ cao mặt sàn quét được!
            Vector3 spawnPosition = new Vector3(spawnX, groundHeight, spawnZ);

            // Tạo hướng quay của Model quay mặt về phía người chơi cho dễ nhìn
            Vector3 lookDirection = playerPos - spawnPosition;
            lookDirection.y = 0; // Không cho Model bị ngửa cổ lên trời
            Quaternion spawnRotation = Quaternion.LookRotation(lookDirection);

            // Tiến hành sinh Model ra ngoài thế giới thực
            GameObject obj = Instantiate(modelsToPlace[i], spawnPosition, spawnRotation);
            
            // Đảm bảo Object được bật lên
            obj.SetActive(true);

            // Lưu vào danh sách quản lý để sau này tương tác
            spawnedObjects.Add(obj);
        }
    }

    void Update()
    {
        // 1. Kiểm tra và xử lý cảm ứng chạm trên ĐIỆN THOẠI THẬT
        if (UnityEngine.InputSystem.Touchscreen.current != null)
        {
            var touch = UnityEngine.InputSystem.Touchscreen.current.primaryTouch;
            if (touch.press.wasPressedThisFrame)
            {
                Vector2 touchPosition = touch.position.ReadValue();
                XulyClickKhamPha(touchPosition);
            }
        }

        // 2. Kiểm tra và xử lý click CHUỘT khi test trên PC EDITOR
        if (UnityEngine.InputSystem.Mouse.current != null)
        {
            if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
            {
                Vector2 mousePosition = UnityEngine.InputSystem.Mouse.current.position.ReadValue();
                XulyClickKhamPha(mousePosition);
            }
        }
    }

    private void XulyClickKhamPha(Vector2 inputPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(inputPos);
        RaycastHit hitObject;

        // Bắn tia từ mắt người xem xem có chạm trúng cái Model 3D nào không
        if (Physics.Raycast(ray, out hitObject))
        {
            // Kiểm tra xem Object có tag là Artifact không
            if (hitObject.transform.CompareTag("Artifact"))
            {
                // Lấy tên của Object (loại bỏ chữ (Clone) nếu có) làm ID tra cứu Firebase
                string id = hitObject.transform.name.Replace("(Clone)", "").Trim();
                Debug.Log("Người dùng click khám phá ID: " + id);

                if (infoCanvas != null) infoCanvas.SetActive(true); // Hiện bảng chữ thông tin UI lên
                
                // Kêu gọi FirebaseManager lấy data nạp vào UI
                if (firebaseManager != null)
                {
                    firebaseManager.LoadDataFromServer(id, titleText, bodyText, infoCanvas);
                }
            }
        }
    }
}