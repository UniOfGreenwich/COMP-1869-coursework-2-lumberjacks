using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

[RequireComponent(typeof(Collider))] // needs 3D collider
public class StorageBuilding : MonoBehaviour
{
    [Header("Assign In Inspector")]
    [SerializeField] private GameObject windowPrefab;                 // window prefab here
    [SerializeField] private GameObject storageButtonPrefab;          // button prefab here
    [SerializeField] private Vector3 buttonOffset = new Vector3(0, 2f, 0); // world offset here

    private Canvas canvas;                 // target screen canvas
    private GameObject windowInstance;     // window instance cache
    private GameObject storageButtonInstance; // button instance cache
    private RectTransform storageButtonRect;  // button rect cache
    private Camera mainCam;                // main camera cache

    void Awake()
    {
        EnsureEventSystem(); // ensure event system
    }

    void Start()
    {
        mainCam = Camera.main; // cache camera now

#if UNITY_2023_1_OR_NEWER
        var canvases = Object.FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None); // find canvases unsorted
#else
        var canvases = Object.FindObjectsOfType<Canvas>(true); // legacy canvases find
#endif
        foreach (var c in canvases)
        {
            if (c.renderMode == RenderMode.ScreenSpaceOverlay || c.renderMode == RenderMode.ScreenSpaceCamera)
            { canvas = c; break; } // choose screen canvas
        }
        if (!canvas && canvases.Length > 0) canvas = canvases[0]; // fallback first

        if (!canvas)
        {
            Debug.LogError("StorageBuilding: No Canvas found in scene!"); // hard fail here
            return;
        }

        if (windowPrefab != null)
        {
            windowInstance = Instantiate(windowPrefab, canvas.transform); // spawn window now
            windowInstance.SetActive(false); // start hidden state
        }
        else
        {
            Debug.LogWarning("StorageBuilding: windowPrefab not assigned."); // soft warn here
        }
    }

    void Update()
    {
        if (!mainCam) mainCam = Camera.main; // rebind camera now

        // click spawns button
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current && EventSystem.current.IsPointerOverGameObject()) return; // ignore ui hits

            var ray = mainCam ? mainCam.ScreenPointToRay(Input.mousePosition) : new Ray(Vector3.zero, Vector3.forward); // build ray now
            if (Physics.Raycast(ray, out RaycastHit hit, 1000f)) // 3d ray only
            {
                var building = hit.collider.GetComponentInParent<StorageBuilding>(); // climb parents now
                if (building == this) ShowStorageButton(); // spawn button now
            }
        }

        // track screen position
        if (storageButtonRect && mainCam)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(transform.position + buttonOffset); // compute screen pos
            storageButtonRect.position = screenPos; // update button pos
        }
    }

    private void ShowStorageButton()
    {
        if (storageButtonInstance) return; // already exists guard
        if (!canvas)
        {
            Debug.LogWarning("StorageBuilding: Canvas missing, cannot spawn button."); // guard path here
            return;
        }

        storageButtonInstance = storageButtonPrefab
            ? Instantiate(storageButtonPrefab, canvas.transform)                    // use prefab path
            : BuildFallbackButton(canvas.transform);                                // build fallback path

        storageButtonRect = storageButtonInstance.GetComponent<RectTransform>();     // cache rect now

        EnsureReadableLabel(storageButtonInstance, "Open Storage");                   // force visible text
        var btn = storageButtonInstance.GetComponent<Button>();                       // fetch button ref
        if (btn) btn.onClick.AddListener(OpenStorageUI);                              // wire open action
    }

    private void OpenStorageUI()
    {
        if (windowInstance) windowInstance.SetActive(true); // open window now
        if (storageButtonInstance)
        {
            Destroy(storageButtonInstance); // remove button now
            storageButtonInstance = null;   // clear instance ref
            storageButtonRect = null;       // clear rect ref
        }
        PlayerController.IsInputLocked = true; // lock player input
    }

    public void CloseStorageUI()
    {
        if (windowInstance) windowInstance.SetActive(false); // close window now
        PlayerController.IsInputLocked = false; // unlock player input
    }

    void OnDisable()
    {
        if (storageButtonInstance) Destroy(storageButtonInstance); // cleanup spawned button
    }

    // force visible label
    private void EnsureReadableLabel(GameObject root, string text)
    {
        // try tmp first
        var tmp = root.GetComponentInChildren<TextMeshProUGUI>(true); // find tmp label now
        if (tmp)
        {
            tmp.text = string.IsNullOrWhiteSpace(tmp.text) ? text : tmp.text; // set text fallback
            tmp.enableAutoSizing = true; // enable auto size now
            tmp.fontSizeMin = 18;        // minimum size here
            tmp.fontSizeMax = 36;        // maximum size here
            var c = tmp.color; c.a = 1f; tmp.color = c; // ensure alpha one
            return; // done with tmp path
        }

        // fallback to uGUI
        var ugui = root.GetComponentInChildren<Text>(true); // find uGUI label now
        if (ugui)
        {
            ugui.text = string.IsNullOrWhiteSpace(ugui.text) ? text : ugui.text; // set text fallback
            ugui.alignment = TextAnchor.MiddleCenter; // center align text
            var c = ugui.color; c.a = 1f; ugui.color = c; // ensure alpha one
            if (ugui.font == null) ugui.font = Resources.GetBuiltinResource<Font>("Arial.ttf"); // ensure font set
        }
    }

    // fallback button build
    private GameObject BuildFallbackButton(Transform parent)
    {
        // root button object
        var btnGO = new GameObject("StorageButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button)); // create root now
        btnGO.transform.SetParent(parent, false); // parent under canvas
        var rect = btnGO.GetComponent<RectTransform>(); // get rect transform
        rect.sizeDelta = new Vector2(180, 48); // button size here
        var img = btnGO.GetComponent<Image>(); // get image component
        img.color = new Color(0.12f, 0.12f, 0.12f, 0.9f); // dark panel color

        // text child object
        var txtGO = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI)); // text child now
        txtGO.transform.SetParent(btnGO.transform, false); // parent to button
        var txtRect = txtGO.GetComponent<RectTransform>(); // fetch text rect now
        txtRect.anchorMin = Vector2.zero; // stretch anchors here
        txtRect.anchorMax = Vector2.one;  // stretch anchors here
        txtRect.offsetMin = Vector2.zero; // zero offsets here
        txtRect.offsetMax = Vector2.zero; // zero offsets here

        var tmp = txtGO.GetComponent<TextMeshProUGUI>(); // get tmp component
        tmp.text = "Open Storage"; // default label text
        tmp.alignment = TextAlignmentOptions.Center; // centered alignment now
        tmp.enableAutoSizing = true; // enable auto size now
        tmp.fontSizeMin = 18;        // minimum size here
        tmp.fontSizeMax = 36;        // maximum size here
        tmp.color = Color.white;     // white readable color

        return btnGO; // return built button
    }

    // ensure event system
    private void EnsureEventSystem()
    {
        if (EventSystem.current) return; // already exists guard
        var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule)); // create system now
        DontDestroyOnLoad(es); // persist across scenes
    }
}
