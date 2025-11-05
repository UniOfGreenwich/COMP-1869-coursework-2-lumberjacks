using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class SquareCutter : MonoBehaviour, IDropHandler
{
    [Header("Item Setup")]
    [Tooltip("What item counts as input? (Planks)")]
    public ItemSO plankItem;
    [Tooltip("Fallback output if no recipe matches a dimension choice.")]
    public ItemSO defaultSquareItem;
    [Min(1)] public int planksPerPiece = 4;

    [Header("Dimension Output Recipes (optional)")]
    public List<Recipe> recipes = new List<Recipe>();
    [System.Serializable]
    public struct Recipe
    {
        [Min(1)] public int width;
        [Min(1)] public int height;
        public ItemSO outputItem;
        [Min(1)] public int planksCost;
        [Min(0.1f)] public float seconds;
    }

    [Header("Processing (time rule)")]
    [Tooltip("3x3 should take about this long. Other sizes scale by (W*H)/9 unless overridden by a Recipe.")]
    [Min(1f)] public float secondsForThreeByThree = 30f;
    public Transform inputPoint;
    public Transform outputPoint;
    public GameObject inputEffectPrefab;
    public GameObject outputEffectPrefab;
    public float effectLifetime = 1.5f;

    [Header("Drop Zone (World-Space)")]
    [Min(0.2f)] public float dropZoneWorldSize = 1.2f;
    public bool showDropZone = true;

    [Header("Blimp + Timer (same behaviour as Plank Machine)")]
    public bool enableBlimp = true;
    public float blimpHeight = 1.6f;
    public Vector2 blimpSize = new Vector2(140, 90);
    public Sprite blimpBackgroundSprite;

    public bool showTimer = true;
    public Sprite hourglassSprite;
    public float timerHeight = 1.4f;
    public Vector2 timerSize = new Vector2(64, 64);
    public float timerSpinSpeed = 180f;
    public bool timerDockToBlimp = true;
    public Vector3 timerLocalOffset = new Vector3(0.28f, 0f, 0f);

    [Header("Storage Fly FX")]
    public Canvas overlayCanvas;
    public RectTransform storageAnchor;
    public Sprite outputIcon;
    public Vector2 flyIconSize = new Vector2(36, 36);
    public float flyDuration = 0.7f;
    public int flyBurst = 6;
    public AnimationCurve flyCurve;

    [Header("Auto Wiring")]
    public bool autoFindStorageUI = true;
    public string overlayCanvasTag = "OverlayCanvas";
    public string storageAnchorName = "StorageAnchor";
    public float uiProbeInterval = 0.5f;

    [Header("Dimension UI (auto-find by name or tag 'DimensionUI')")]
    public GameObject dimensionPanel;
    public TMP_InputField widthField;
    public TMP_InputField heightField;
    public TextMeshProUGUI dimSummary;
    public Button dimOkButton;
    public Button dimCancelButton;
    [Tooltip("Keep the last chosen dimension for the next run.")]
    public bool rememberLastDimension = true;

    private StorageManager storage;
    private Placeble placeble;
    private Canvas dropCanvas;
    private TextMeshProUGUI dropHint;
    private bool busy;
    private bool waitingForDrop;

    private int selW = 2, selH = 2;
    private ItemSO currentOutput;
    private int costPlanks;
    private float secondsPerPiece;
    private int pendingCount;
    private MachineBlimp blimp;
    private Transform blimpRoot;

    private Canvas timerCanvas;
    private RectTransform timerRect;

    private float _nextProbeTime;

    private void Awake()
    {
        storage = FindFirstObjectByType<StorageManager>();
        placeble = GetComponent<Placeble>();

        EnsureDropZone();
        EnsureBlimp();
        EnsureTimer();
        SetupDimensionUI();

        if (flyCurve == null || flyCurve.keys.Length == 0)
            flyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        TryAutoWireStorage();
        ComputeDimensionParams();
    }

    private void Update()
    {
        if (dropCanvas)
        {
            dropCanvas.enabled = placeble == null || placeble.placed;
            if (dropCanvas.worldCamera == null) dropCanvas.worldCamera = Camera.main;
        }

        if (autoFindStorageUI && Time.unscaledTime >= _nextProbeTime)
        {
            _nextProbeTime = Time.unscaledTime + uiProbeInterval;
            TryAutoWireStorage();
        }

        if (timerCanvas && timerDockToBlimp)
        {
            Vector3 anchor = blimpRoot ? blimpRoot.localPosition : new Vector3(0, blimpHeight, 0);
            timerCanvas.transform.localPosition = anchor + timerLocalOffset;
        }

        if (blimp) blimp.FaceCamera(Camera.main);
        if (timerCanvas && timerCanvas.gameObject.activeSelf)
        {
            FaceCanvas(timerCanvas.transform, Camera.main);
            if (timerRect) timerRect.Rotate(0f, 0f, -timerSpinSpeed * Time.deltaTime);
        }
        if (dropHint) dropHint.gameObject.SetActive(waitingForDrop && !busy);
    }

    private void OnMouseDown()
    {
        if (busy) return;
        if (placeble && !placeble.placed) return;
        OpenDimensionUI();
    }

    public void OnDrop(PointerEventData e)
    {
        if (!waitingForDrop || busy) return;
        if (placeble && !placeble.placed) return;

        var drag = e.pointerDrag ? e.pointerDrag.GetComponent<DraggableItemUI>() : null;
        if (!drag) return;

        var payload = drag.TakePayload();
        if (payload.IsEmpty || payload.item != plankItem)
        {
            drag.ReturnRemainder(payload);
            return;
        }
        int pieces = costPlanks > 0 ? payload.count / costPlanks : 0;
        if (pieces <= 0)
        {
            drag.ReturnRemainder(payload);
            return;
        }
        int used = pieces * costPlanks;
        payload.count -= used;
        drag.ReturnRemainder(payload);
        waitingForDrop = false;
        StartCoroutine(ProcessPieces(pieces));
    }

    private IEnumerator ProcessPieces(int pieces)
    {
        busy = true;
        SetTimer(true);

        for (int i = 0; i < pieces; i++)
        {
            if (inputEffectPrefab && inputPoint)
            {
                var fxIn = Instantiate(inputEffectPrefab, inputPoint.position, Quaternion.identity);
                if (effectLifetime > 0) Destroy(fxIn, effectLifetime);
            }

            yield return new WaitForSeconds(secondsPerPiece);
            pendingCount++;
            UpdateBlimpUI();

            if (outputEffectPrefab && outputPoint)
            {
                var fxOut = Instantiate(outputEffectPrefab, outputPoint.position, Quaternion.identity);
                if (effectLifetime > 0) Destroy(fxOut, effectLifetime);
            }
        }

        SetTimer(false);
        busy = false;
        if (dropHint) dropHint.text = $"Drop {costPlanks} planks";
    }

    private void ComputeDimensionParams()
    {
        Recipe? match = null;
        foreach (var r in recipes)
        {
            bool same = (r.width == selW && r.height == selH) || (r.width == selH && r.height == selW);
            if (same) { match = r; break; }
        }

        currentOutput = match.HasValue && match.Value.outputItem ? match.Value.outputItem : (defaultSquareItem ? defaultSquareItem : plankItem);
        costPlanks = match.HasValue && match.Value.planksCost > 0 ? match.Value.planksCost : Mathf.Max(1, planksPerPiece);

        if (match.HasValue && match.Value.seconds > 0f)
            secondsPerPiece = match.Value.seconds;
        else
        {
            float area = Mathf.Max(1, selW * selH);
            secondsPerPiece = secondsForThreeByThree * (area / 9f);
        }

        if (dimSummary)
            dimSummary.text = $"{selW}x{selH} -> Need {costPlanks} planks - {secondsPerPiece:0.#}s";

        if (dropHint)
            dropHint.text = $"Drop {costPlanks} planks";
    }

    private void OpenDimensionUI()
    {
        if (!dimensionPanel)
        {
            Debug.LogWarning("[SquareCutter] Dimension UI panel not found. (Name it 'DimensionUI' or tag it 'DimensionUI')");
            waitingForDrop = true;
            return;
        }

        if (widthField) widthField.text = selW.ToString();
        if (heightField) heightField.text = selH.ToString();
        ComputeDimensionParams();

        dimensionPanel.SetActive(true);
    }

    private void CloseDimensionUI(bool confirmed)
    {
        if (!dimensionPanel) return;

        if (confirmed)
        {
            int w = ParseField(widthField, selW);
            int h = ParseField(heightField, selH);
            selW = Mathf.Max(1, w);
            selH = Mathf.Max(1, h);
            ComputeDimensionParams();
            if (pendingCount > 0 && currentOutput != lastPendingItem)
                CollectBlimp();

            lastPendingItem = currentOutput;

            waitingForDrop = true;
        }

        dimensionPanel.SetActive(false);
    }

    private static int ParseField(TMP_InputField f, int fallback)
    {
        if (!f || string.IsNullOrWhiteSpace(f.text)) return fallback;
        if (int.TryParse(f.text, out int v)) return v;
        return fallback;
    }

    private ItemSO lastPendingItem;

    private void UpdateBlimpUI()
    {
        if (!enableBlimp || !blimp) return;
        blimp.gameObject.SetActive(pendingCount > 0);
        blimp.SetCount(pendingCount);
    }

    internal void CollectBlimp()
    {
        if (pendingCount <= 0 || !storage) { pendingCount = 0; UpdateBlimpUI(); return; }

        Vector3 startWorld = transform.position + new Vector3(0f, blimpHeight, 0f);
        if (blimp) startWorld = blimp.transform.position;

        int collected = pendingCount;
        storage.Put(currentOutput, collected);
        pendingCount = 0;
        UpdateBlimpUI();

        if (!overlayCanvas || !storageAnchor) TryAutoWireStorage();
        if (overlayCanvas && storageAnchor && (outputIcon != null))
            StartCoroutine(FlyToStorageRoutine(startWorld, collected));
    }

    private void EnsureDropZone()
    {
        var forwarderExisting = GetComponentInChildren<MachineDropForwarder>(true);
        if (forwarderExisting)
        {
            forwarderExisting.target = null;
        }

        var canvasObj = new GameObject("SquareDropCanvas", typeof(Canvas), typeof(GraphicRaycaster));
        canvasObj.transform.SetParent(transform, false);
        dropCanvas = canvasObj.GetComponent<Canvas>();
        dropCanvas.renderMode = RenderMode.WorldSpace;
        dropCanvas.worldCamera = Camera.main;
        dropCanvas.sortingOrder = 2000;
        dropCanvas.enabled = true;

        var rc = dropCanvas.GetComponent<RectTransform>();
        rc.anchorMin = rc.anchorMax = rc.pivot = new Vector2(0.5f, 0.5f);
        rc.sizeDelta = new Vector2(300f, 300f);
        float scale = Mathf.Max(0.001f, dropZoneWorldSize / rc.sizeDelta.x);
        dropCanvas.transform.localScale = new Vector3(scale, scale, scale);

        var zoneObj = new GameObject("DropZone", typeof(RectTransform), typeof(Image), typeof(MachineDropForwarder));
        zoneObj.transform.SetParent(canvasObj.transform, false);
        var zoneRc = zoneObj.GetComponent<RectTransform>();
        zoneRc.anchorMin = zoneRc.anchorMax = zoneRc.pivot = new Vector2(0.5f, 0.5f);
        zoneRc.sizeDelta = rc.sizeDelta;
        zoneRc.localPosition = Vector3.zero;

        var img = zoneObj.GetComponent<Image>();
        img.color = showDropZone ? new Color(0f, 1f, 0f, 0.18f) : new Color(1f, 1f, 1f, 0.001f);

        var fwd = zoneObj.GetComponent<MachineDropForwarder>();
        fwd.target = null;
        var proxy = zoneObj.AddComponent<_SquareDropProxy>();
        proxy.square = this;

        var hintObj = new GameObject("DropHint", typeof(RectTransform), typeof(TextMeshProUGUI));
        hintObj.transform.SetParent(zoneObj.transform, false);
        var hintRc = hintObj.GetComponent<RectTransform>();
        hintRc.anchorMin = new Vector2(0.5f, 0f);
        hintRc.anchorMax = new Vector2(0.5f, 0f);
        hintRc.pivot = new Vector2(0.5f, 0f);
        hintRc.anchoredPosition = new Vector2(0, 10f);
        hintRc.sizeDelta = new Vector2(280f, 60f);

        dropHint = hintObj.GetComponent<TextMeshProUGUI>();
        dropHint.alignment = TextAlignmentOptions.Center;
        dropHint.enableAutoSizing = true;
        dropHint.fontSizeMin = 20; dropHint.fontSizeMax = 32;
        dropHint.color = Color.black;
        dropHint.text = $"Drop {planksPerPiece} planks";
        dropHint.gameObject.SetActive(false);
    }

    private void EnsureBlimp()
    {
        if (!enableBlimp) return;

        var canvasObj = new GameObject("BlimpCanvas", typeof(Canvas), typeof(GraphicRaycaster));
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.localPosition = new Vector3(0, blimpHeight, 0);

        var c = canvasObj.GetComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.worldCamera = Camera.main;
        c.sortingOrder = 2100;

        var rc = c.GetComponent<RectTransform>();
        rc.anchorMin = rc.anchorMax = rc.pivot = new Vector2(0.5f, 0.5f);
        rc.sizeDelta = blimpSize;
        float scale = 0.6f / Mathf.Max(1f, blimpSize.x);
        c.transform.localScale = new Vector3(scale, scale, scale);

        var btnObj = new GameObject("Blimp", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(canvasObj.transform, false);
        var btnRc = btnObj.GetComponent<RectTransform>();
        btnRc.anchorMin = btnRc.anchorMax = btnRc.pivot = new Vector2(0.5f, 0.5f);
        btnRc.sizeDelta = blimpSize;

        var bg = btnObj.GetComponent<Image>();
        if (blimpBackgroundSprite) { bg.sprite = blimpBackgroundSprite; bg.type = Image.Type.Sliced; }
        else bg.color = new Color(0f, 0f, 0f, 0.55f);

        var textObj = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);
        var textRc = textObj.GetComponent<RectTransform>();
        textRc.anchorMin = textRc.anchorMax = new Vector2(0.5f, 0f);
        textRc.pivot = new Vector2(0.5f, 0f);
        textRc.anchoredPosition = new Vector2(0f, 6f);
        textRc.sizeDelta = new Vector2(blimpSize.x, 28f);

        var countText = textObj.GetComponent<TextMeshProUGUI>();
        countText.text = "x0";
        countText.alignment = TextAlignmentOptions.Center;
        countText.enableAutoSizing = true;
        countText.fontSizeMin = 26; countText.fontSizeMax = 36;
        countText.color = Color.black;

        blimp = canvasObj.AddComponent<MachineBlimp>();
        blimp.InitForSquare(this, countText, btnObj.GetComponent<Button>());

        blimpRoot = canvasObj.transform;
        canvasObj.SetActive(false);
    }

    private void EnsureTimer()
    {
        if (!showTimer) return;

        var timerCanvasObj = new GameObject("TimerCanvas", typeof(Canvas), typeof(GraphicRaycaster));
        timerCanvasObj.transform.SetParent(transform, false);

        Vector3 anchor = blimpRoot ? blimpRoot.localPosition : new Vector3(0, blimpHeight, 0);
        timerCanvasObj.transform.localPosition = anchor + timerLocalOffset;

        var c = timerCanvasObj.GetComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.worldCamera = Camera.main;
        c.sortingOrder = 2110;

        var rc = c.GetComponent<RectTransform>();
        rc.anchorMin = rc.anchorMax = rc.pivot = new Vector2(0.5f, 0.5f);
        rc.sizeDelta = timerSize;
        float scale = 0.4f / Mathf.Max(1f, timerSize.x);
        c.transform.localScale = new Vector3(scale, scale, scale);

        var imgObj = new GameObject("Hourglass", typeof(RectTransform), typeof(Image));
        imgObj.transform.SetParent(timerCanvasObj.transform, false);
        timerRect = imgObj.GetComponent<RectTransform>();
        timerRect.anchorMin = timerRect.anchorMax = timerRect.pivot = new Vector2(0.5f, 0.5f);
        timerRect.sizeDelta = timerSize;

        var timerImage = imgObj.GetComponent<Image>();
        timerImage.sprite = hourglassSprite;
        timerImage.color = new Color(1f, 1f, 1f, 0.9f);

        timerCanvas = c;
        timerCanvasObj.SetActive(false);
    }

    private void SetTimer(bool on)
    {
        if (!showTimer || !timerCanvas) return;
        timerCanvas.gameObject.SetActive(on);
        if (timerRect) timerRect.localRotation = Quaternion.identity;
    }

    private void SetupDimensionUI()
    {
        if (!dimensionPanel)
        {
            foreach (var t in Resources.FindObjectsOfTypeAll<Transform>())
                if (t && t.name == "DimensionUI" && t.hideFlags == HideFlags.None) { dimensionPanel = t.gameObject; break; }

            if (!dimensionPanel)
            {
                var tagged = GameObject.FindGameObjectWithTag("DimensionUI");
                if (tagged) dimensionPanel = tagged;
            }

            if (dimensionPanel)
            {
                widthField = dimensionPanel.transform.Find("W")?.GetComponent<TMP_InputField>();
                heightField = dimensionPanel.transform.Find("H")?.GetComponent<TMP_InputField>();
                dimSummary = dimensionPanel.transform.Find("Summary")?.GetComponent<TextMeshProUGUI>();
                dimOkButton = dimensionPanel.transform.Find("OK")?.GetComponent<Button>();
                dimCancelButton = dimensionPanel.transform.Find("Cancel")?.GetComponent<Button>();
            }
        }

        if (dimensionPanel)
        {
            dimensionPanel.SetActive(false);
            if (dimOkButton)
            {
                dimOkButton.onClick.RemoveAllListeners();
                dimOkButton.onClick.AddListener(() => CloseDimensionUI(true));
            }
            if (dimCancelButton)
            {
                dimCancelButton.onClick.RemoveAllListeners();
                dimCancelButton.onClick.AddListener(() => CloseDimensionUI(false));
            }
        }
    }

    private IEnumerator FlyToStorageRoutine(Vector3 startWorld, int count)
    {
        int icons = Mathf.Clamp(count, 1, flyBurst);
        RectTransform overlayCanvasRect = overlayCanvas.transform as RectTransform;

        Vector2 startLocal;
        Vector2 endLocal;

        Vector2 startScreen = RectTransformUtility.WorldToScreenPoint(Camera.main, startWorld);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayCanvasRect, startScreen, overlayCanvas.worldCamera, out startLocal);

        Vector2 endScreen = RectTransformUtility.WorldToScreenPoint(overlayCanvas ? overlayCanvas.worldCamera : null, storageAnchor.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayCanvasRect, endScreen, overlayCanvas ? overlayCanvas.worldCamera : null, out endLocal);

        Vector2 control = Vector2.Lerp(startLocal, endLocal, 0.5f) + Vector2.up * 80f;

        for (int i = 0; i < icons; i++)
            StartCoroutine(SingleFlyIcon(startLocal, control, endLocal, i * 0.03f));

        yield return new WaitForSeconds(flyDuration + 0.15f);
        StartCoroutine(PulseStorageAnchor());
    }

    private IEnumerator SingleFlyIcon(Vector2 start, Vector2 control, Vector2 end, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        RectTransform overlayCanvasRect = overlayCanvas.transform as RectTransform;
        var iconObj = new GameObject("FlyIcon", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        iconObj.transform.SetParent(overlayCanvasRect, false);

        var iconRc = iconObj.GetComponent<RectTransform>();
        iconRc.sizeDelta = flyIconSize;
        iconRc.anchoredPosition = start;

        var img = iconObj.GetComponent<Image>();
        img.sprite = outputIcon;
        img.raycastTarget = false;

        var grp = iconObj.GetComponent<CanvasGroup>();
        grp.alpha = 1f;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.0001f, flyDuration);
            float k = Mathf.Clamp01(t);
            float e = flyCurve != null ? flyCurve.Evaluate(k) : k;

            Vector2 p = QuadraticBezier(start, control, end, e);
            iconRc.anchoredPosition = p;

            float s = Mathf.Lerp(1.0f, 0.65f, e);
            iconRc.localScale = new Vector3(s, s, 1f);
            grp.alpha = 1f - e * 0.2f;

            yield return null;
        }

        Destroy(iconObj);
    }

    private static Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        float u = 1f - t;
        return u * u * a + 2f * u * t * b + t * t * c;
    }

    private IEnumerator PulseStorageAnchor()
    {
        float up = 0.1f;
        float down = 0.1f;
        Vector3 baseScale = storageAnchor.localScale;
        Vector3 bigScale = baseScale * 1.12f;

        float t = 0f;
        while (t < up)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / up);
            storageAnchor.localScale = Vector3.Lerp(baseScale, bigScale, k);
            yield return null;
        }

        t = 0f;
        while (t < down)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / down);
            storageAnchor.localScale = Vector3.Lerp(bigScale, baseScale, k);
            yield return null;
        }

        storageAnchor.localScale = baseScale;
    }

    private void TryAutoWireStorage()
    {
        if (overlayCanvas == null && autoFindStorageUI)
        {
            foreach (var c in Resources.FindObjectsOfTypeAll<Canvas>())
            {
                if (!string.IsNullOrEmpty(overlayCanvasTag) && c.CompareTag(overlayCanvasTag)) { overlayCanvas = c; break; }
            }
            if (overlayCanvas == null)
            {
                foreach (var c in Resources.FindObjectsOfTypeAll<Canvas>())
                {
                    if (!c || !c.gameObject.scene.IsValid()) continue;
                    if (c.renderMode == RenderMode.ScreenSpaceOverlay || c.renderMode == RenderMode.ScreenSpaceCamera)
                    { overlayCanvas = c; break; }
                }
            }
        }

        if (overlayCanvas && storageAnchor == null)
        {
            foreach (var rt in overlayCanvas.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt.name == storageAnchorName) { storageAnchor = rt; break; }
            }
        }

        if (overlayCanvas && storageAnchor == null)
        {
            var go = new GameObject(storageAnchorName, typeof(RectTransform));
            go.transform.SetParent(overlayCanvas.transform, false);
            var rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(1f, 1f);
            rt.anchorMax = new Vector2(1f, 1f);
            rt.pivot = new Vector2(1f, 1f);
            rt.anchoredPosition = new Vector2(-32f, -32f);
            rt.sizeDelta = new Vector2(8f, 8f);
            storageAnchor = rt;
        }
    }

    public void WireStorageUI(Canvas canvas, RectTransform anchor)
    {
        overlayCanvas = canvas;
        storageAnchor = anchor;
    }

    private void FaceCanvas(Transform t, Camera cam)
    {
        if (!cam) return;
        var dir = t.position - cam.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f) t.rotation = Quaternion.LookRotation(dir);
    }
}

public class _SquareDropProxy : MonoBehaviour, IDropHandler
{
    public SquareCutter square;
    public void OnDrop(PointerEventData eventData) { if (square) square.OnDrop(eventData); }
}

public static class MachineBlimpExtensions
{
    public static void InitForSquare(this MachineBlimp blimp, SquareCutter cutter, TextMeshProUGUI text, Button button)
    {
        var f = new _SquareBlimpForwarder { cutter = cutter };
        blimp.gameObject.AddComponent<_SquareBlimpForwarderHolder>().forwarder = f;

        var btn = button;
        if (btn) btn.onClick.AddListener(() => f.Collect());
    }

    class _SquareBlimpForwarder
    {
        public SquareCutter cutter;
        public void Collect() { if (cutter) cutter.CollectBlimp(); }
    }

    class _SquareBlimpForwarderHolder : MonoBehaviour
    {
        public _SquareBlimpForwarder forwarder;
    }
}
