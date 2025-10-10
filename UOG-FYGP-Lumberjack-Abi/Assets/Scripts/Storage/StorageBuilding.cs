using UnityEngine;
using UnityEngine.UI;

public class StorageBuilding : MonoBehaviour
{
    [Header("Assign in Inspector")]
    [SerializeField] private GameObject windowPrefab;   // Your Storage UI window prefab
    [SerializeField] private Canvas canvas;             // Drag your main Canvas
    [SerializeField] private GameObject storageButtonPrefab; // Small "Storage" button prefab

    private GameObject windowInstance;
    private GameObject storageButtonInstance;

    private void Start()
    {
        // Ensure we have a reference to the Canvas
        if (canvas == null)
            canvas = FindObjectOfType<Canvas>();

        // Spawn Storage UI (hidden at start)
        if (windowPrefab != null && canvas != null)
        {
            windowInstance = Instantiate(windowPrefab, canvas.transform);
            windowInstance.SetActive(false);
        }
        else
        {
            Debug.LogWarning("[StorageBuilding] Missing windowPrefab or Canvas.");
        }
    }

    private void Update()
    {
        // Detect click manually (bypasses player movement)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                // If we clicked THIS object (the cylinder)
                if (hit.collider.gameObject == gameObject)
                {
                    ShowStorageButton();
                }
            }
        }
    }

    private void ShowStorageButton()
    {
        if (storageButtonInstance != null) return;

        storageButtonInstance = Instantiate(storageButtonPrefab, canvas.transform);
        RectTransform rect = storageButtonInstance.GetComponent<RectTransform>();
        rect.anchoredPosition = Vector2.zero; // Center button on screen

        // Set the button text to "Storage"
        Text txt = storageButtonInstance.GetComponentInChildren<Text>();
        if (txt != null) txt.text = "Storage";

        // Add the click event
        Button btn = storageButtonInstance.GetComponent<Button>();
        btn.onClick.AddListener(OpenStorageUI);

        Debug.Log("[StorageBuilding] Storage button created!");
    }

    private void OpenStorageUI()
    {
        if (windowInstance != null)
        {
            windowInstance.SetActive(true);
            Debug.Log("[StorageBuilding] Storage UI opened!");
        }

        if (storageButtonInstance != null)
            Destroy(storageButtonInstance);
    }

    public void CloseStorageUI()
    {
        if (windowInstance != null)
            windowInstance.SetActive(false);
    }
}
