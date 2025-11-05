using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class StorageBuilding : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private GameObject windowPrefab;        // Storage UI prefab (panel)
    [SerializeField] private GameObject storageButtonPrefab; // "Storage" button prefab (UGUI Button)
    [SerializeField] private Vector3 buttonOffset = new Vector3(0, 2f, 0);

    private Canvas canvas;
    private GameObject windowInstance;
    private GameObject storageButtonInstance;
    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        var canvases = FindObjectsOfType<Canvas>(true);
        foreach (var c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay ||
                c.renderMode == RenderMode.ScreenSpaceCamera)
            { canvas = c; break; }
        }
        if (!canvas && canvases.Length > 0) canvas = canvases[0];

        if (!canvas)
        {
            Debug.LogError("StorageBuilding: No Canvas found in scene!");
            return;
        }

        if (windowPrefab != null)
        {
            windowInstance = Instantiate(windowPrefab, canvas.transform);
            windowInstance.SetActive(false);
        }
        else
        {
            Debug.LogWarning("StorageBuilding: windowPrefab not assigned.");
        }
    }

    void Update()
    {
        if (!mainCam) mainCam = Camera.main;

        // Don’t place button when clicking UI
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return;

            var ray = mainCam ? mainCam.ScreenPointToRay(Input.mousePosition) : new Ray();
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                // Accept any child collider of this building
                var building = hit.collider.GetComponentInParent<StorageBuilding>();
                if (building == this) ShowStorageButton();
            }
        }
        if (storageButtonInstance != null && mainCam)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position + buttonOffset);
            storageButtonInstance.GetComponent<RectTransform>().position = screenPos;
        }
    }

    private void ShowStorageButton()
    {
        if (storageButtonInstance != null) return;

        if (storageButtonPrefab == null || canvas == null)
        {
            Debug.LogWarning("StorageBuilding: storageButtonPrefab or canvas not set.");
            return;
        }

        storageButtonInstance = Instantiate(storageButtonPrefab, canvas.transform);
        var btn = storageButtonInstance.GetComponent<Button>();
        if (btn) btn.onClick.AddListener(OpenStorageUI);
    }

    private void OpenStorageUI()
    {
        if (windowInstance != null) windowInstance.SetActive(true);
        if (storageButtonInstance != null) Destroy(storageButtonInstance);
         PlayerController.IsInputLocked = true;
    }

    public void CloseStorageUI()
    {
        if (windowInstance != null) windowInstance.SetActive(false);
        PlayerController.IsInputLocked = false;
    }

    void OnDisable()
    {
        if (storageButtonInstance) Destroy(storageButtonInstance);
    }
}
