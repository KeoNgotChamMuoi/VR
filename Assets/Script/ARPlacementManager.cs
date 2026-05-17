using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.EventSystems;

public class ARPlacementManager : MonoBehaviour
{
    public GameObject infoCanvas;
    public GameObject objectToPlace;
    public FirebaseManager firebaseManager;
    public TMPro.TextMeshProUGUI titleText;
    public TMPro.TextMeshProUGUI bodyText;
    public ARRaycastManager raycastManager;
    
    private GameObject spawnedObject;
    static List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Update()
    {
        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
        {
            return; // Thoát hàm Update luôn, nhường hoàn toàn quyền xử lý chuột cho ô Đăng Nhập
        }
        //  Kiểm tra đầu vào (Touch cho Mobile hoặc Mouse cho PC)
        bool isPressed = false;
        Vector2 inputPosition = Vector2.zero;

        // Ưu tiên kiểm tra Touch trước (Mobile)
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                isPressed = true;
                inputPosition = touch.position;
            }
        }
        // Nếu không có touch, kiểm tra Mouse (PC Simulation)
        else if (Input.GetMouseButtonDown(0))
        {
            isPressed = true;
            inputPosition = Input.mousePosition;
        }

        //  Nếu có hành động nhấn/chạm
        if (isPressed)
        {
            Ray ray = Camera.main.ScreenPointToRay(inputPosition);
            RaycastHit hitObject;
            // KIỂM TRA: Nếu chạm trúng một Object đã có trên sàn
            if (Physics.Raycast(ray, out hitObject))
            {
                if (hitObject.transform.CompareTag("Artifact")) // Đặt tag cho Cube là Artifact
                {
                    // Lấy tên của Object làm ID để tra cứu trên Firebase
                    string id = hitObject.transform.name.Replace("(Clone)", "").Trim();
                    Debug.Log("Đang tìm ID trên Firebase: " + id);
                    infoCanvas.SetActive(true); // Hiện bảng thông tin
                    firebaseManager.LoadDataFromServer(id, titleText, bodyText); 
                }
            }
            
            // Bắn một tia từ điểm nhấn xuống mặt phẳng
            if (raycastManager.Raycast(inputPosition, hits, TrackableType.PlaneWithinPolygon))
            {
                // Lấy vị trí và góc quay của điểm va chạm
                Pose hitPose = hits[0].pose;

                if (spawnedObject == null)
                {
                    // Nếu chưa có khối nào thì tạo mới (Instantiate)
                    spawnedObject = Instantiate(objectToPlace, hitPose.position, hitPose.rotation);
                }
                else
                {
                    // Nếu đã có rồi thì chỉ việc di chuyển nó tới chỗ mới
                    spawnedObject.transform.position = hitPose.position;
                    spawnedObject.transform.rotation = hitPose.rotation;
                }
            }
        }
    }
}