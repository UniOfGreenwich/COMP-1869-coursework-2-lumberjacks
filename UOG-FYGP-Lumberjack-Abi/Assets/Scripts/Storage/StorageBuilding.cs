using UnityEngine;
using UnityEngine.UI;

public class StorageBuilding : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private GameObject windowPrefab;        // Storage UI prefab
    [SerializeField] private GameObject storageButtonPrefab; // "Storage" button prefab
    [SerializeField] private Vector3 buttonOffset = new Vector3(0, 2f, 0); // how high above the building button appears

    private Canvas canvas;
    private GameObject windowInstance;
    private GameObject storageButtonInstance;

    private Camera mainCam;

    private void Start()
    {
        canvas = FindFirstObjectByType<Canvas>();
        mainCam = Camera.main;

        if (canvas == null)
        {
            Debug.LogError("StorageBuilding: No Canvas found in scene!");
            return;
        }

        // Create the UI window (hidden by default)
        if (windowPrefab != null)
        {
            windowInstance = Instantiate(windowPrefab, canvas.transform);
            windowInstance.SetActive(false);
        }
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = mainCam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject == gameObject)
                    ShowStorageButton();
            }
        }

        // Keep button positioned above storage in screen space
        if (storageButtonInstance != null)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position + buttonOffset);
            RectTransform rect = storageButtonInstance.GetComponent<RectTransform>();
            rect.position = screenPos;
        }
    }

    private void ShowStorageButton()
    {
        if (storageButtonInstance != null || storageButtonPrefab == null) return;

        storageButtonInstance = Instantiate(storageButtonPrefab, canvas.transform);
        Button btn = storageButtonInstance.GetComponent<Button>();
        btn.onClick.AddListener(OpenStorageUI);
    }

    private void OpenStorageUI()
    {
        if (windowInstance != null)
            windowInstance.SetActive(true);

        if (storageButtonInstance != null)
            Destroy(storageButtonInstance);

        // Stop player movement
        PlayerController.IsInputLocked = true;
    }

    public void CloseStorageUI()
    {
        if (windowInstance != null)
            windowInstance.SetActive(false);

        // Resume player movement
        PlayerController.IsInputLocked = false;
    }
}
