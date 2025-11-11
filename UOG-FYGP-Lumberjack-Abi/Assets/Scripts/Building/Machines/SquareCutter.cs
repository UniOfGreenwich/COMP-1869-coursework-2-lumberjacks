using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SquareCutter : MonoBehaviour, IDropHandler
{
    // item setup fields
    public ItemSO plankItem;
    public ItemSO defaultSquareItem;
    [Min(1)] public int planksPerPiece = 4;

    // recipe data container
    [System.Serializable]
    public struct Recipe
    {
        [Min(1)] public int width;        // size width value
        [Min(1)] public int height;       // size height value
        public ItemSO outputItem;         // optional output asset
        [Min(0)] public int planksCost;   // zero uses strategy
        [Min(0f)] public float seconds;   // zero uses scaling
    }
    public List<Recipe> recipes = new List<Recipe>();

    // processing rule fields
    [Min(1f)] public float secondsForThreeByThree = 4f;
    public Transform inputPoint;
    public Transform outputPoint;
    public GameObject inputEffectPrefab;
    public GameObject outputEffectPrefab;
    public float effectLifetime = 1.5f;

    // drop zone visuals
    [Min(0.2f)] public float dropZoneWorldSize = 1.2f;
    public bool showDropZone = true;
    public Vector3 dropZoneLocalOffset = new Vector3(0f, 1.6f, 0f);
    public Sprite dropZoneSprite;
    public Color dropZoneColor = new Color(0f, 1f, 0f, 0.25f);

    // blimp and timer
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

    // storage fly effect
    public Canvas overlayCanvas;
    public RectTransform storageAnchor;
    public Sprite outputIcon;
    public Vector2 flyIconSize = new Vector2(36, 36);
    public float flyDuration = 0.7f;
    public int flyBurst = 6;
    public AnimationCurve flyCurve;

    // auto wiring controls
    public bool autoFindStorageUI = true;
    public string overlayCanvasTag = "OverlayCanvas";
    public string storageAnchorName = "StorageAnchor";
    public float uiProbeInterval = 0.5f;

    // dimension ui references
    public GameObject dimensionPanel;
    public TMP_InputField widthField;
    public TMP_InputField heightField;
    public TextMeshProUGUI dimSummary;
    public Button dimOkButton;
    public Button dimCancelButton;
    public bool rememberLastDimension = true;

    // dimension limits here
    [Min(1)] public int maxDimension = 8;

    // cost strategy controls
    public enum CostMode { Fixed, ScaleByArea }
    [Header("Cost Strategy")]
    public CostMode costMode = CostMode.ScaleByArea;
    [Range(0.25f, 2f)] public float areaExponent = 1f;

    // auto generation toggles
    [Header("Auto Generation")]
    public bool generateOnAwakeIfEmpty = true;

    // diagnostics toggle here
    public bool debugLogs = false;

    // runtime caches here
    private StorageManager storage;
    private Placeble placeble;
    private Canvas dropCanvas;
    private TextMeshProUGUI dropHint;
    private bool busy;
    private bool waitingForDrop;

    // runtime state values
    private int selW = 2, selH = 2;
    private ItemSO currentOutput;
    private int costPlanks;
    private float secondsPerPiece;
    private int pendingCount;
    private MachineBlimp blimp;
    private Transform blimpRoot;
    private TextMeshProUGUI blimpCountText;
    private Canvas timerCanvas;
    private RectTransform timerRect;
    private float _nextProbeTime;
    private ItemSO lastPendingItem;

    private void Awake()
    {
        // find scene references
        storage = FindFirstObjectByType<StorageManager>();
        placeble = GetComponent<Placeble>();

        // optional auto creation
        if (generateOnAwakeIfEmpty && (recipes == null || recipes.Count == 0))
            RuntimeAutoGenerateRecipes(); // build recipes list

        // build all visuals
        EnsureDropZone();                // create drop zone
        EnsureBlimp();                   // create blimp ui
        EnsureTimer();                   // create timer ui
        SetupDimensionUI();              // wire inputs now

        // default easing curve
        if (flyCurve == null || flyCurve.keys.Length == 0)
            flyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

        // wire overlay anchor
        TryAutoWireStorage();

        // compute selected dimension
        ComputeDimensionParams();

        // hide zone initially
        if (dropCanvas) dropCanvas.gameObject.SetActive(false);
    }

    private void Update()
    {
        // manage drop canvas
        if (dropCanvas)
        {
            bool canShow = waitingForDrop && !busy && showDropZone;
            dropCanvas.gameObject.SetActive(canShow);
            dropCanvas.worldCamera = Camera.main;
            FaceCanvas(dropCanvas.transform, Camera.main);
        }

        // periodic overlay probe
        if (autoFindStorageUI && Time.unscaledTime >= _nextProbeTime)
        {
            _nextProbeTime = Time.unscaledTime + uiProbeInterval;
            TryAutoWireStorage();
        }

        // timer anchor follow
        if (timerCanvas && timerDockToBlimp)
        {
            Vector3 anchor = blimpRoot ? blimpRoot.localPosition : new Vector3(0, blimpHeight, 0);
            timerCanvas.transform.localPosition = anchor + timerLocalOffset;
        }

        // billboard behaviours here
        if (blimp) blimp.FaceCamera(Camera.main);
        if (timerCanvas && timerCanvas.gameObject.activeSelf)
        {
            FaceCanvas(timerCanvas.transform, Camera.main);
            if (timerRect) timerRect.Rotate(0f, 0f, -timerSpinSpeed * Time.deltaTime);
        }

        // show hint gate
        if (dropHint) dropHint.gameObject.SetActive(waitingForDrop && !busy);
    }

    private void OnMouseDown()
    {
        // open if ready
        if (busy) return;
        if (placeble && !placeble.placed) return;
        OpenDimensionUI();
    }

    public void OnDrop(PointerEventData e)
    {
        // validate drop gate
        if (!waitingForDrop || busy) return;
        if (placeble && !placeble.placed) return;

        // fetch draggable payload
        var drag = e.pointerDrag ? e.pointerDrag.GetComponent<DraggableItemUI>() : null;
        if (!drag) return;
        var payload = drag.TakePayload();
        if (payload.IsEmpty || payload.item != plankItem)
        {
            drag.ReturnRemainder(payload);
            return;
        }

        // consume exact cost
        int needed = costPlanks;
        if (payload.count < needed)
        {
            drag.ReturnRemainder(payload);
            return;
        }

        payload.count -= needed;
        drag.ReturnRemainder(payload);

        waitingForDrop = false;
        if (debugLogs) Debug.Log($"[SquareCutter] Consumed={needed} Returned={payload.count}");

        StartCoroutine(ProcessPieces(1));
    }

    private IEnumerator ProcessPieces(int pieces)
    {
        // start processing now
        busy = true;
        SetTimer(true);

        for (int i = 0; i < pieces; i++)
        {
            // input effect spawn
            if (inputEffectPrefab && inputPoint)
            {
                var fxIn = Instantiate(inputEffectPrefab, inputPoint.position, Quaternion.identity);
                if (effectLifetime > 0) Destroy(fxIn, effectLifetime);
            }

            // wait piece time
            yield return new WaitForSeconds(secondsPerPiece);
            pendingCount++;
            UpdateBlimpUI();

            // output effect spawn
            if (outputEffectPrefab && outputPoint)
            {
                var fxOut = Instantiate(outputEffectPrefab, outputPoint.position, Quaternion.identity);
                if (effectLifetime > 0) Destroy(fxOut, effectLifetime);
            }
        }

        // finish processing now
        SetTimer(false);
        busy = false;
        if (dropHint) dropHint.text = $"Drop {costPlanks} planks";
    }

    private void ComputeDimensionParams()
    {
        // find matching recipe
        Recipe? match = null;
        foreach (var r in recipes)
        {
            bool same = (r.width == selW && r.height == selH) || (r.width == selH && r.height == selW);
            if (same) { match = r; break; }
        }

        // resolve outputs here
        currentOutput = match.HasValue && match.Value.outputItem ? match.Value.outputItem : (defaultSquareItem ? defaultSquareItem : plankItem);

        // resolve cost here
        costPlanks = ComputeCostFor(selW, selH, match);

        // resolve seconds here
        if (match.HasValue && match.Value.seconds > 0f) secondsPerPiece = match.Value.seconds;
        else secondsPerPiece = ComputeSecondsFor(selW, selH);

        // update summary text
        if (dimSummary) dimSummary.text = $"{selW}x{selH} -> Need {costPlanks} planks - {secondsPerPiece:0.#}s";
        if (dropHint) dropHint.text = $"Drop {costPlanks} planks";
    }

    private int ComputeCostFor(int w, int h, Recipe? match)
    {
        // recipe override wins
        if (match.HasValue && match.Value.planksCost > 0)
            return match.Value.planksCost;

        // area scaled strategy
        if (costMode == CostMode.ScaleByArea)
        {
            float area = Mathf.Max(1, w * h);
            float scale = Mathf.Pow(area / 9f, areaExponent);
            return Mathf.Max(1, Mathf.RoundToInt(planksPerPiece * scale));
        }

        // fixed strategy fallback
        return Mathf.Max(1, planksPerPiece);
    }

    private float ComputeSecondsFor(int w, int h)
    {
        // scale seconds value
        float area = Mathf.Max(1, w * h);
        return secondsForThreeByThree * (area / 9f);
    }

    private void OpenDimensionUI()
    {
        // handle missing panel
        if (!dimensionPanel)
        {
            Debug.LogWarning("[SquareCutter] Dimension UI missing.");
            waitingForDrop = true;
            return;
        }

        // seed input fields
        if (widthField) widthField.text = selW.ToString();
        if (heightField) heightField.text = selH.ToString();
        ComputeDimensionParams();
        dimensionPanel.SetActive(true);
    }

    private void CloseDimensionUI(bool confirmed)
    {
        // handle close intent
        if (!dimensionPanel) return;

        if (confirmed)
        {
            int w = ParseField(widthField, selW);
            int h = ParseField(heightField, selH);
            selW = Mathf.Clamp(w, 1, maxDimension);
            selH = Mathf.Clamp(h, 1, maxDimension);
            ComputeDimensionParams();
            if (pendingCount > 0 && currentOutput != lastPendingItem) CollectBlimp();
            lastPendingItem = currentOutput;
            waitingForDrop = true;
        }

        dimensionPanel.SetActive(false);
    }

    private static int ParseField(TMP_InputField f, int fallback)
    {
        // parse integer text
        if (!f || string.IsNullOrWhiteSpace(f.text)) return fallback;
        if (int.TryParse(f.text, out int v)) return v;
        return fallback;
    }

    private void UpdateBlimpUI()
    {
        // update blimp visuals
        if (!enableBlimp) return;
        if (blimp) blimp.gameObject.SetActive(pendingCount > 0);
        if (blimpCountText) blimpCountText.text = $"x{pendingCount}";
    }

    internal void CollectBlimp()
    {
        // collect pending items
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
        // create zone canvas
        var canvasObj = new GameObject("SquareDropCanvas", typeof(Canvas), typeof(GraphicRaycaster));
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.localPosition = dropZoneLocalOffset;
        dropCanvas = canvasObj.GetComponent<Canvas>();
        dropCanvas.renderMode = RenderMode.WorldSpace;
        dropCanvas.worldCamera = Camera.main;
        dropCanvas.sortingOrder = 2000;

        // size and scale rect
        var rc = dropCanvas.GetComponent<RectTransform>();
        rc.anchorMin = rc.anchorMax = rc.pivot = new Vector2(0.5f, 0.5f);
        rc.sizeDelta = new Vector2(300f, 300f);
        float scale = Mathf.Max(0.001f, dropZoneWorldSize / rc.sizeDelta.x);
        dropCanvas.transform.localScale = new Vector3(scale, scale, scale);

        // create zone visual
        var zoneObj = new GameObject("DropZone", typeof(RectTransform), typeof(Image));
        zoneObj.transform.SetParent(canvasObj.transform, false);
        var zoneRc = zoneObj.GetComponent<RectTransform>();
        zoneRc.anchorMin = zoneRc.anchorMax = zoneRc.pivot = new Vector2(0.5f, 0.5f);
        zoneRc.sizeDelta = rc.sizeDelta;
        zoneRc.localPosition = Vector3.zero;

        // set zone style
        var img = zoneObj.GetComponent<Image>();
        img.sprite = dropZoneSprite;
        img.type = Image.Type.Sliced;
        img.color = showDropZone ? dropZoneColor : new Color(1f, 1f, 1f, 0.001f);
        img.raycastTarget = true;

        // add drop proxy
        var proxy = zoneObj.AddComponent<_SquareDropProxy>();
        proxy.square = this;

        // add hint label
        var hintObj = new GameObject("DropHint", typeof(RectTransform), typeof(TextMeshProUGUI));
        hintObj.transform.SetParent(zoneObj.transform, false);
        var hintRc = hintObj.GetComponent<RectTransform>();
        hintRc.anchorMin = new Vector2(0.5f, 0f);
        hintRc.anchorMax = new Vector2(0.5f, 0f);
        hintRc.pivot = new Vector2(0.5f, 0f);
        hintRc.anchoredPosition = new Vector2(0, 10f);
        hintRc.sizeDelta = new Vector2(280f, 60f);

        // configure hint text
        dropHint = hintObj.GetComponent<TextMeshProUGUI>();
        dropHint.alignment = TextAlignmentOptions.Center;
        dropHint.enableAutoSizing = true;
        dropHint.fontSizeMin = 20; dropHint.fontSizeMax = 32;
        dropHint.color = Color.black;
        dropHint.text = $"Drop {planksPerPiece} planks";
    }

    private void EnsureBlimp()
    {
        // feature toggle guard
        if (!enableBlimp) return;

        // create blimp canvas
        var canvasObj = new GameObject("BlimpCanvas", typeof(Canvas), typeof(GraphicRaycaster));
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.localPosition = new Vector3(0, blimpHeight, 0);

        // configure canvas now
        var c = canvasObj.GetComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.worldCamera = Camera.main;
        c.sortingOrder = 2100;

        // size and scale rect
        var rc = c.GetComponent<RectTransform>();
        rc.anchorMin = rc.anchorMax = rc.pivot = new Vector2(0.5f, 0.5f);
        rc.sizeDelta = blimpSize;
        float scale = 0.6f / Mathf.Max(1f, blimpSize.x);
        c.transform.localScale = new Vector3(scale, scale, scale);

        // create blimp button
        var btnObj = new GameObject("Blimp", typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(canvasObj.transform, false);
        var btnRc = btnObj.GetComponent<RectTransform>();
        btnRc.anchorMin = btnRc.anchorMax = btnRc.pivot = new Vector2(0.5f, 0.5f);
        btnRc.sizeDelta = blimpSize;

        // set background style
        var bg = btnObj.GetComponent<Image>();
        if (blimpBackgroundSprite) { bg.sprite = blimpBackgroundSprite; bg.type = Image.Type.Sliced; }
        else bg.color = new Color(0f, 0f, 0f, 0.55f);

        // add count text
        var textObj = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);
        var textRc = textObj.GetComponent<RectTransform>();
        textRc.anchorMin = textRc.anchorMax = new Vector2(0.5f, 0f);
        textRc.pivot = new Vector2(0.5f, 0f);
        textRc.anchoredPosition = new Vector2(0f, 6f);
        textRc.sizeDelta = new Vector2(blimpSize.x, 28f);

        // configure text now
        var countText = textObj.GetComponent<TextMeshProUGUI>();
        countText.text = "x0";
        countText.alignment = TextAlignmentOptions.Center;
        countText.enableAutoSizing = true;
        countText.fontSizeMin = 26; countText.fontSizeMax = 36;
        countText.color = Color.black;

        // attach blimp script
        blimp = canvasObj.AddComponent<MachineBlimp>();
        blimp.InitForSquare(this, countText, btnObj.GetComponent<Button>());

        // cache references now
        blimpCountText = countText;
        blimpRoot = canvasObj.transform;
        canvasObj.SetActive(false);
    }

    private void EnsureTimer()
    {
        // feature toggle guard
        if (!showTimer) return;

        // create timer canvas
        var timerCanvasObj = new GameObject("TimerCanvas", typeof(Canvas), typeof(GraphicRaycaster));
        timerCanvasObj.transform.SetParent(transform, false);

        // compute anchor offset
        Vector3 anchor = blimpRoot ? blimpRoot.localPosition : new Vector3(0, blimpHeight, 0);
        timerCanvasObj.transform.localPosition = anchor + timerLocalOffset;

        // configure canvas now
        var c = timerCanvasObj.GetComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.worldCamera = Camera.main;
        c.sortingOrder = 2110;

        // configure size values
        var rc = c.GetComponent<RectTransform>();
        rc.anchorMin = rc.anchorMax = rc.pivot = new Vector2(0.5f, 0.5f);
        rc.sizeDelta = timerSize;
        float scale = 0.4f / Mathf.Max(1f, timerSize.x);
        c.transform.localScale = new Vector3(scale, scale, scale);

        // build hourglass image
        var imgObj = new GameObject("Hourglass", typeof(RectTransform), typeof(Image));
        imgObj.transform.SetParent(timerCanvasObj.transform, false);
        timerRect = imgObj.GetComponent<RectTransform>();
        timerRect.anchorMin = timerRect.anchorMax = timerRect.pivot = new Vector2(0.5f, 0.5f);
        timerRect.sizeDelta = timerSize;

        // set sprite color
        var timerImage = imgObj.GetComponent<Image>();
        timerImage.sprite = hourglassSprite;
        timerImage.color = new Color(1f, 1f, 1f, 0.9f);

        // cache timer canvas
        timerCanvas = c;
        timerCanvasObj.SetActive(false);
    }

    private void SetTimer(bool on)
    {
        // toggle timer ui
        if (!showTimer || !timerCanvas) return;
        timerCanvas.gameObject.SetActive(on);
        if (timerRect) timerRect.localRotation = Quaternion.identity;
    }

    private void SetupDimensionUI()
    {
        // ensure panel hidden
        if (dimensionPanel)
        {
            dimensionPanel.SetActive(false);
            if (dimOkButton) { dimOkButton.onClick.RemoveAllListeners(); dimOkButton.onClick.AddListener(() => CloseDimensionUI(true)); }
            if (dimCancelButton) { dimCancelButton.onClick.RemoveAllListeners(); dimCancelButton.onClick.AddListener(() => CloseDimensionUI(false)); }
        }

        // integer only inputs
        if (widthField)
        {
            widthField.contentType = TMP_InputField.ContentType.IntegerNumber;
            widthField.onValueChanged.AddListener(_ => OnDimChanged());
        }
        if (heightField)
        {
            heightField.contentType = TMP_InputField.ContentType.IntegerNumber;
            heightField.onValueChanged.AddListener(_ => OnDimChanged());
        }
    }

    private void OnDimChanged()
    {
        // live recomputation here
        int w = ParseField(widthField, selW);
        int h = ParseField(heightField, selH);
        selW = Mathf.Clamp(w, 1, maxDimension);
        selH = Mathf.Clamp(h, 1, maxDimension);
        ComputeDimensionParams();
    }

    private IEnumerator FlyToStorageRoutine(Vector3 startWorld, int count)
    {
        // compute flight points
        int icons = Mathf.Clamp(count, 1, flyBurst);
        RectTransform overlayCanvasRect = overlayCanvas.transform as RectTransform;

        Vector2 startLocal;
        Vector2 endLocal;

        Vector2 startScreen = RectTransformUtility.WorldToScreenPoint(Camera.main, startWorld);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayCanvasRect, startScreen, overlayCanvas.worldCamera, out startLocal);

        Vector2 endScreen = RectTransformUtility.WorldToScreenPoint(overlayCanvas ? overlayCanvas.worldCamera : null, storageAnchor.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayCanvasRect, endScreen, overlayCanvas ? overlayCanvas.worldCamera : null, out endLocal);

        Vector2 control = Vector2.Lerp(startLocal, endLocal, 0.5f) + Vector2.up * 80f;

        // spawn flight icons
        for (int i = 0; i < icons; i++)
            StartCoroutine(SingleFlyIcon(startLocal, control, endLocal, i * 0.03f));

        yield return new WaitForSeconds(flyDuration + 0.15f);
        StartCoroutine(PulseStorageAnchor());
    }

    private IEnumerator SingleFlyIcon(Vector2 start, Vector2 control, Vector2 end, float delay)
    {
        // handle initial delay
        if (delay > 0f) yield return new WaitForSeconds(delay);

        // create icon object
        RectTransform overlayCanvasRect = overlayCanvas.transform as RectTransform;
        var iconObj = new GameObject("FlyIcon", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        iconObj.transform.SetParent(overlayCanvasRect, false);

        // configure rect here
        var iconRc = iconObj.GetComponent<RectTransform>();
        iconRc.sizeDelta = flyIconSize;
        iconRc.anchoredPosition = start;

        // configure image now
        var img = iconObj.GetComponent<Image>();
        img.sprite = outputIcon;
        img.raycastTarget = false;

        // configure canvasgroup
        var grp = iconObj.GetComponent<CanvasGroup>();
        grp.alpha = 1f;

        // tween over curve
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

        // destroy icon object
        Destroy(iconObj);
    }

    private static Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        // quadratic bezier calculation
        float u = 1f - t;
        return u * u * a + 2f * u * t * b + t * t * c;
    }

    private IEnumerator PulseStorageAnchor()
    {
        // pulse scale up
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

        // pulse scale down
        t = 0f;
        while (t < down)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / down);
            storageAnchor.localScale = Vector3.Lerp(bigScale, baseScale, k);
            yield return null;
        }

        // restore base scale
        storageAnchor.localScale = baseScale;
    }

    private void TryAutoWireStorage()
    {
        // probe overlay canvas
        if (overlayCanvas == null && autoFindStorageUI)
        {
            foreach (var c in Resources.FindObjectsOfTypeAll<Canvas>())
            {
                if (!c || !c.gameObject.scene.IsValid()) continue;
                if (!string.IsNullOrEmpty(overlayCanvasTag) && c.gameObject.tag == overlayCanvasTag)
                { overlayCanvas = c; break; }
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

        // find anchor child
        if (overlayCanvas && storageAnchor == null)
        {
            foreach (var rt in overlayCanvas.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt.name == storageAnchorName) { storageAnchor = rt; break; }
            }
        }

        // synthesize anchor node
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
        // manual anchor wiring
        overlayCanvas = canvas;
        storageAnchor = anchor;
    }

    private void FaceCanvas(Transform t, Camera cam)
    {
        // billboard toward camera
        if (!cam) return;
        var dir = t.position - cam.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f) t.rotation = Quaternion.LookRotation(dir);
    }

    // runtime auto generation
    private void RuntimeAutoGenerateRecipes()
    {
        // clear then populate
        if (recipes == null) recipes = new List<Recipe>();
        recipes.Clear();

        for (int w = 1; w <= maxDimension; w++)
        {
            for (int h = 1; h <= maxDimension; h++)
            {
                var r = new Recipe
                {
                    width = w,
                    height = h,
                    outputItem = null,
                    planksCost = ComputeCostFor(w, h, null),
                    seconds = ComputeSecondsFor(w, h)
                };
                recipes.Add(r);
            }
        }
    }

    // inspector callable entry
    public void EditorAutoGenerateRecipes()
    {
        // editor generation call
        RuntimeAutoGenerateRecipes();
    }
}

public class _SquareDropProxy : MonoBehaviour, IDropHandler
{
    // forward drop event
    public SquareCutter square;
    public void OnDrop(PointerEventData eventData) { if (square) square.OnDrop(eventData); }
}

public static class MachineBlimpExtensions
{
    // wire blimp events
    public static void InitForSquare(this MachineBlimp blimp, SquareCutter cutter, TextMeshProUGUI text, Button button)
    {
        var f = new _SquareBlimpForwarder { cutter = cutter };
        blimp.gameObject.AddComponent<_SquareBlimpForwarderHolder>().forwarder = f;
        if (button) button.onClick.AddListener(() => f.Collect());
    }

    // forwarder implementation here
    class _SquareBlimpForwarder { public SquareCutter cutter; public void Collect() { if (cutter) cutter.CollectBlimp(); } }
    class _SquareBlimpForwarderHolder : MonoBehaviour { public _SquareBlimpForwarder forwarder; }
}
