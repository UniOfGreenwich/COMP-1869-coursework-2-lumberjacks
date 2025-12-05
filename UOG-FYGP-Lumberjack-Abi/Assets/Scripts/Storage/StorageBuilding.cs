using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Collider))]
public class StorageBuilding : MonoBehaviour
{
    [Header("Assign In Inspector")]
    [SerializeField] private GameObject windowPrefab;
    [SerializeField] private GameObject storageButtonPrefab;
    [SerializeField] private Vector3 buttonOffset = new Vector3(0, 2f, 0);

    private Canvas canvas;
    private Camera cam;

    private GameObject window;
    private GameObject button;
    private RectTransform buttonRect;

    void Awake()
    {
        EnsureEventSystem();
    }

    void Start()
    {
        cam = Camera.main;

        // Find ANY valid screen-space canvas
        Canvas[] canvases = FindObjectsOfType<Canvas>(true);
        foreach (var c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay || c.renderMode == RenderMode.ScreenSpaceCamera)
            {
                canvas = c;
                break;
            }
        }

        if (canvas == null && canvases.Length > 0)
            canvas = canvases[0];

        if (canvas == null)
        {
            Debug.LogError("StorageBuilding: No Canvas found in scene.");
            return;
        }

        // Create storage window
        if (windowPrefab)
        {
            window = Instantiate(windowPrefab, canvas.transform);
            window.SetActive(false);
        }
    }

    void Update()
    {
        if (!cam) cam = Camera.main;

        if (Input.GetMouseButtonDown(0))
        {
            if (PointerOverUI()) return;

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f))
            {
                if (hit.collider.GetComponentInParent<StorageBuilding>() == this)
                {
                    ShowButton();
                }
            }
        }

        UpdateButtonPosition();
    }

    // ---------------------------
    // BUTTON HANDLING
    // ---------------------------

    private void ShowButton()
    {
        // Always destroy any old button (prevents invisible leftovers)
        if (button != null)
        {
            Destroy(button);
            button = null;
            buttonRect = null;
        }

        // Spawn new button
        button = Instantiate(storageButtonPrefab, canvas.transform);
        buttonRect = button.GetComponent<RectTransform>();

        // Make sure it's visible and readable
        ForceText(button, "Open Storage");

        // Bring to top
        button.transform.SetAsLastSibling();

        // Hook events
        button.GetComponent<Button>().onClick.AddListener(OpenWindow);
    }

    private void UpdateButtonPosition()
    {
        if (buttonRect == null || cam == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(transform.position + buttonOffset);

        // Hide when behind camera
        if (screenPos.z < 0)
        {
            buttonRect.gameObject.SetActive(false);
            return;
        }

        // Ensure visible
        if (!buttonRect.gameObject.activeSelf)
            buttonRect.gameObject.SetActive(true);

        buttonRect.position = screenPos;
    }

    // ---------------------------
    // WINDOW HANDLING
    // ---------------------------

    private void OpenWindow()
    {
        if (window != null)
            window.SetActive(true);

        // Button no longer needed
        if (button != null)
            Destroy(button);

        button = null;
        buttonRect = null;

        PlayerController.IsInputLocked = true;
    }

    public void CloseWindow()
    {
        if (window != null)
            window.SetActive(false);

        PlayerController.IsInputLocked = false;
    }

    void OnDisable()
    {
        if (button != null)
            Destroy(button);
    }

    // ---------------------------
    // UTILS
    // ---------------------------

    private bool PointerOverUI()
    {
        return EventSystem.current && EventSystem.current.IsPointerOverGameObject();
    }

    private void ForceText(GameObject root, string fallback)
    {
        var tmp = root.GetComponentInChildren<TextMeshProUGUI>(true);
        if (tmp)
        {
            if (string.IsNullOrWhiteSpace(tmp.text)) tmp.text = fallback;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMin = 18;
            tmp.fontSizeMax = 36;
            tmp.color = Color.black;
        }
    }

    private void EnsureEventSystem()
    {
        if (EventSystem.current != null) return;

        var es = new GameObject("EventSystem",
            typeof(EventSystem),
            typeof(StandaloneInputModule));

        DontDestroyOnLoad(es);
    }
}
