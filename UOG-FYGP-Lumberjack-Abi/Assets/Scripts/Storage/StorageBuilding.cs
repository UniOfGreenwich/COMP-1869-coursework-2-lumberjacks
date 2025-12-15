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

    private void ShowButton()
    {
        if (button != null)
        {
            Destroy(button);
            button = null;
            buttonRect = null;
        }

        button = Instantiate(storageButtonPrefab, canvas.transform);
        buttonRect = button.GetComponent<RectTransform>();

        ForceText(button, "Open Storage");

        button.transform.SetAsLastSibling();

        button.GetComponent<Button>().onClick.AddListener(OpenWindow);
    }

    private void UpdateButtonPosition()
    {
        if (buttonRect == null || cam == null) return;

        Vector3 screenPos = cam.WorldToScreenPoint(transform.position + buttonOffset);

        if (screenPos.z < 0)
        {
            buttonRect.gameObject.SetActive(false);
            return;
        }

        if (!buttonRect.gameObject.activeSelf)
            buttonRect.gameObject.SetActive(true);

        buttonRect.position = screenPos;
    }


    private void OpenWindow()
    {
        if (window != null)
            window.SetActive(true);

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
