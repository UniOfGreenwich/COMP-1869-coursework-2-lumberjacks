using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Machine : MonoBehaviour, IDropHandler
{
    [Header("Items")]
    public ItemSO logItem;                 // input item is logs
    public ItemSO plankItem;               // output planks utilities category
    [Min(1)] public int planksPerLog = 2;

    [Header("Processing")]
    [Min(0.1f)] public float secondsPerLog = 0.75f;
    public Transform inputPoint;
    public Transform outputPoint;
    public GameObject inputVfxPrefab;
    public GameObject outputVfxPrefab;
    public float vfxLifetime = 1.5f;

    [Header("Drop Zone (World-Space)")]
    [Min(0.2f)] public float dropZoneWorldSize = 1.2f;
    public bool showDropZone = false;

    [Header("Quantity UI (auto-find if left empty)")]
    public GameObject promptPanel;     // autofind quantity ui object
    public TextMeshProUGUI promptTitle;
    public Slider promptSlider;
    public TextMeshProUGUI promptValue;
    public Button okButton;
    public Button cancelButton;

    [Header("Pickup Blimp")]
    public bool enableBlimp = true;
    public float blimpHeight = 1.6f;                      // height above the machine
    public Vector2 blimpSize = new Vector2(140, 90);      // pixels size before scaling
    public Sprite blimpSprite;                                // optional blimp background sprite

    [Header("Debug")]
    public bool debugLogs = true;

    // internal fields and helpers
    private StorageManager storage;
    private Placeble placeble;
    private Canvas dropCanvas;     // worldspace drop event receiver
    private bool busy;
    private int pendingPlanks;     // planks waiting inside blimp
    private MachineBlimp blimp;    // blimp ui behaviour helper

    void Awake()
    {
        storage = FindFirstObjectByType<StorageManager>();
        placeble = GetComponent<Placeble>();
        EnsureDropZone();
        SetupPrompt();
        EnsureBlimp();
        if (debugLogs) Debug.Log("[Machine] Ready.");
    }

    void Update()
    {
        if (!dropCanvas) return;
        dropCanvas.enabled = placeble == null || placeble.placed;
        if (dropCanvas.worldCamera == null) dropCanvas.worldCamera = Camera.main;
        if (blimp) blimp.FaceCamera(Camera.main);
    }
    public void OnDrop(PointerEventData eventData)
    {
        if (busy || (placeble && !placeble.placed)) return;

        var drag = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<DraggableItemUI>() : null;
        if (!drag) return;

        var stack = drag.TakePayload();
        if (stack.IsEmpty || stack.item != logItem)
        {
            drag.ReturnRemainder(stack);
            return;
        }

        ShowPrompt("Logs  Planks", stack.count,
            useCount =>
            {
                stack.count -= useCount;            // consume selected log quantity
                drag.ReturnRemainder(stack);        // leftovers return to source
                StartCoroutine(ProcessLogs(useCount));
            },
            () => drag.ReturnRemainder(stack)       // cancelled return everything back
        );
    }

    private IEnumerator ProcessLogs(int logs)
    {
        if (logs <= 0 || busy) yield break;
        busy = true;

        for (int i = 0; i < logs; i++)
        {
            if (inputVfxPrefab && inputPoint)
            {
                var vIn = Instantiate(inputVfxPrefab, inputPoint.position, Quaternion.identity);
                if (vfxLifetime > 0) Destroy(vIn, vfxLifetime);
            }

            yield return new WaitForSeconds(secondsPerLog);

            // stage output inside blimp
            pendingPlanks += planksPerLog;
            UpdateBlimpUI();

            if (outputVfxPrefab && outputPoint)
            {
                var vOut = Instantiate(outputVfxPrefab, outputPoint.position, Quaternion.identity);
                if (vfxLifetime > 0) Destroy(vOut, vfxLifetime);
            }
        }
        busy = false;
    }

    private void EnsureBlimp()
    {
        if (!enableBlimp) return;

        var cgo = new GameObject("BlimpCanvas", typeof(Canvas), typeof(GraphicRaycaster));
        cgo.transform.SetParent(transform, false);
        cgo.transform.localPosition = new Vector3(0, blimpHeight, 0);

        var canvas = cgo.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.worldCamera = Camera.main;
        canvas.sortingOrder = 2100;

        // size and scale setup
        var crt = canvas.GetComponent<RectTransform>();
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = blimpSize;
        float s = 0.6f / Mathf.Max(1f, blimpSize.x);
        canvas.transform.localScale = new Vector3(s, s, s);

        // background and button setup
        var bg = new GameObject("Blimp", typeof(RectTransform), typeof(Image), typeof(Button));
        bg.transform.SetParent(cgo.transform, false);
        var bgRT = bg.GetComponent<RectTransform>();
        bgRT.anchorMin = bgRT.anchorMax = bgRT.pivot = new Vector2(0.5f, 0.5f);
        bgRT.sizeDelta = blimpSize;

        var img = bg.GetComponent<Image>();
        if (blimpSprite) { img.sprite = blimpSprite; img.type = Image.Type.Sliced; }
        else img.color = new Color(0f, 0f, 0f, 0.55f);

        // count text bottom aligned
        var txtGO = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
        txtGO.transform.SetParent(bg.transform, false);
        var trt = txtGO.GetComponent<RectTransform>();
        trt.anchorMin = trt.anchorMax = new Vector2(0.5f, 0f);
        trt.pivot = new Vector2(0.5f, 0f);
        trt.anchoredPosition = new Vector2(0f, 6f);
        trt.sizeDelta = new Vector2(blimpSize.x, 28f);

        var tmp = txtGO.GetComponent<TextMeshProUGUI>();
        tmp.text = "x0";
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 26; tmp.fontSizeMax = 36;
        tmp.color = Color.black;

        blimp = cgo.AddComponent<MachineBlimp>();
        blimp.Init(this, tmp, bg.GetComponent<Button>());

        cgo.SetActive(false); // hide until planks pending
    }

    private void UpdateBlimpUI()
    {
        if (!enableBlimp || !blimp) return;
        blimp.gameObject.SetActive(pendingPlanks > 0);
        blimp.SetCount(pendingPlanks); // update count text display
    }

    internal void CollectBlimp()
    {
        if (pendingPlanks <= 0) return;
        if (!storage) { if (debugLogs) Debug.LogWarning("[Machine] No StorageManager."); return; }

        storage.Put(plankItem, pendingPlanks);           // send planks to utilities
        if (debugLogs) Debug.Log($"[Machine] Collected {pendingPlanks} planks to Storage.", this);
        pendingPlanks = 0;
        UpdateBlimpUI();
    }
    private void EnsureDropZone()
    {
        var existing = GetComponentInChildren<MachineDropForwarder>(true);
        if (existing)
        {
            existing.target = this;
            dropCanvas = existing.GetComponentInParent<Canvas>();
            return;
        }

        var cgo = new GameObject("DropCanvas", typeof(Canvas), typeof(GraphicRaycaster));
        cgo.transform.SetParent(transform, false);
        dropCanvas = cgo.GetComponent<Canvas>();
        dropCanvas.renderMode = RenderMode.WorldSpace;
        dropCanvas.worldCamera = Camera.main;
        dropCanvas.sortingOrder = 2000;
        dropCanvas.enabled = false;

        var crt = dropCanvas.GetComponent<RectTransform>();
        crt.anchorMin = crt.anchorMax = crt.pivot = new Vector2(0.5f, 0.5f);
        crt.sizeDelta = new Vector2(300f, 300f);
        float s = Mathf.Max(0.001f, dropZoneWorldSize / crt.sizeDelta.x);
        dropCanvas.transform.localScale = new Vector3(s, s, s);

        var igo = new GameObject("DropZone", typeof(RectTransform), typeof(Image), typeof(MachineDropForwarder));
        igo.transform.SetParent(cgo.transform, false);
        var rt = igo.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = crt.sizeDelta;
        rt.localPosition = Vector3.zero;

        var img = igo.GetComponent<Image>();
        img.color = showDropZone ? new Color(0f, 1f, 0f, 0.18f) : new Color(1f, 1f, 1f, 0.001f);
        igo.GetComponent<MachineDropForwarder>().target = this;
    }
    private void SetupPrompt()
    {
        if (!promptPanel)
        {
            Transform qT = null;
            foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
                if (t && t.name == "PlankQuantityUI" && t.hideFlags == HideFlags.None) { qT = t; break; }
            if (!qT)
                foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
                    if (t && t.name == "QuantityUI" && t.hideFlags == HideFlags.None) { qT = t; break; }
            if (!qT)
            {
                var tagged = GameObject.FindGameObjectWithTag("QuantityUI");
                if (tagged) qT = tagged.transform;
            }
            if (qT)
            {
                var q = qT.gameObject;
                promptPanel = q;
                promptTitle = q.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
                promptSlider = q.transform.Find("Slider")?.GetComponent<Slider>();
                promptValue = q.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
                okButton = q.transform.Find("OK")?.GetComponent<Button>();
                cancelButton = q.transform.Find("Cancel")?.GetComponent<Button>();
            }
        }

        if (promptPanel) promptPanel.SetActive(false);

        if (promptSlider)
        {
            promptSlider.wholeNumbers = true;
            promptSlider.onValueChanged.RemoveAllListeners();
            promptSlider.onValueChanged.AddListener(v =>
            {
                if (promptValue) promptValue.text = "x" + ((int)v).ToString();
            });
        }
        if (okButton)
        {
            okButton.onClick.RemoveAllListeners();
            okButton.onClick.AddListener(() => ClosePrompt(true));
        }
        if (cancelButton)
        {
            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(() => ClosePrompt(false));
        }
    }

    private System.Action<int> _onConfirm;
    private System.Action _onCancel;

    private void ShowPrompt(string title, int max, System.Action<int> onConfirm, System.Action onCancel)
    {
        if (!promptPanel || !promptSlider) { onCancel?.Invoke(); return; }
        _onConfirm = onConfirm; _onCancel = onCancel;

        if (promptTitle) promptTitle.text = title;
        promptSlider.minValue = 1;
        promptSlider.maxValue = Mathf.Max(1, max);
        promptSlider.value = promptSlider.maxValue;
        if (promptValue) promptValue.text = "x" + ((int)promptSlider.value).ToString();

        promptPanel.SetActive(true);
    }

    private void ClosePrompt(bool confirmed)
    {
        if (!promptPanel || !promptSlider) return;
        int value = (int)promptSlider.value;
        promptPanel.SetActive(false);

        var c = _onConfirm; var x = _onCancel; _onConfirm = null; _onCancel = null;
        if (confirmed) c?.Invoke(value); else x?.Invoke();
    }
}

// forward ui drop events
public class MachineDropForwarder : MonoBehaviour, IDropHandler
{
    public Machine target;
    public void OnDrop(PointerEventData e) { if (target) target.OnDrop(e); }
}

// helper attached at runtime
public class MachineBlimp : MonoBehaviour, IPointerClickHandler
{
    private Machine machine;
    private TextMeshProUGUI countText;
    private Button bgButton;

    public void Init(Machine m, TextMeshProUGUI txt, Button btn)
    {
        machine = m; countText = txt; bgButton = btn;
        if (bgButton) bgButton.onClick.AddListener(Collect);
    }

    public void SetCount(int n) { if (countText) countText.text = "x" + n; }

    public void FaceCamera(Camera cam)
    {
        if (!cam) return;
        var t = transform;
        var dir = (t.position - cam.transform.position); dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f) t.rotation = Quaternion.LookRotation(dir);
    }

    public void OnPointerClick(PointerEventData e) { Collect(); }

    private void Collect() { if (machine) machine.CollectBlimp(); }
}
