using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OfflineHarvesterNPC : MonoBehaviour
{
    [Header("Items")]
    public ItemSO logItem;
    public ItemSO seedItem;

    [Header("Offline Rates")]
    [Min(0f)] public float logsPerMinute = 10f;
    [Min(1)] public int logsPerSeedBatch = 5;
    [Min(1)] public int seedsPerBatch = 3;

    [Header("Blimp UI")]
    public float blimpHeight = 2f;
    public Vector2 blimpSize = new Vector2(140, 90);
    public Sprite blimpBackgroundSprite;
    public Sprite logIcon;
    public Sprite seedIcon;

    [Header("Storage FX")]
    public Canvas overlayCanvas;
    public RectTransform storageAnchor;
    public Vector2 flyIconSize = new Vector2(36, 36);
    public float flyDuration = 0.7f;
    public int flyBurst = 6;
    public AnimationCurve flyCurve;
    public string overlayCanvasTag = "OverlayCanvas";
    public string storageAnchorName = "StorageAnchor";
    public float uiProbeInterval = 0.5f;

    [Header("Blimp Style")]
    public Sprite logsBlimpSprite;
    public Sprite seedsBlimpSprite;
    public TMP_FontAsset blimpFont;
    public Color blimpTextColor = Color.white;
    public bool blimpTextBold = false;
    public bool blimpTextAutoSize = true;
    public int blimpTextFontSizeMin = 22;
    public int blimpTextFontSizeMax = 36;

    private StorageManager storage;

    private int pendingLogs;
    private int pendingSeeds;
    private float logRemainder;
    private int seedProgressLogs;

    private Canvas logsCanvas;
    private TextMeshProUGUI logsText;
    private Button logsButton;

    private Canvas seedsCanvas;
    private TextMeshProUGUI seedsText;
    private Button seedsButton;

    private float nextProbeTime;

    private const string PrefKeyLast = "OfflineHarvester_LastQuit";
    private const string PrefKeyPendingLogs = "OfflineHarvester_PendingLogs";
    private const string PrefKeyPendingSeeds = "OfflineHarvester_PendingSeeds";
    private const string PrefKeyLogRemainder = "OfflineHarvester_LogRemainder";
    private const string PrefKeySeedProgress = "OfflineHarvester_SeedProgressLogs";

    void Awake()
    {
        storage = FindFirstObjectByType<StorageManager>();
        EnsureBlimps();

        if (flyCurve == null || flyCurve.keys.Length == 0)
            flyCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged += OnPlayModeChanged;
#endif
    }

    void OnDestroy()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.playModeStateChanged -= OnPlayModeChanged;
#endif
    }

    void Start()
    {
        LoadState();
        ApplyOfflineProduction();
        UpdateBlimpUI();
        AutoWireStorage();
        SaveState(NowUnix());
    }

    void Update()
    {
        FaceCanvases();

        if ((overlayCanvas == null || storageAnchor == null) && Time.unscaledTime >= nextProbeTime)
        {
            nextProbeTime = Time.unscaledTime + uiProbeInterval;
            AutoWireStorage();
        }
    }

    void OnApplicationQuit()
    {
        SaveState(NowUnix());
    }

    void OnApplicationPause(bool pause)
    {
        if (pause) SaveState(NowUnix());
    }

#if UNITY_EDITOR
    private void OnPlayModeChanged(UnityEditor.PlayModeStateChange state)
    {
        if (state == UnityEditor.PlayModeStateChange.ExitingPlayMode)
            SaveState(NowUnix());
    }
#endif

    void EnsureBlimps()
    {
        logsCanvas = BuildBlimpCanvas("LogsBlimp", new Vector3(-0.6f, blimpHeight, 0f), out logsText, out logsButton);
        seedsCanvas = BuildBlimpCanvas("SeedsBlimp", new Vector3(0.6f, blimpHeight, 0f), out seedsText, out seedsButton);

        if (logsButton && logsButton.image) logsButton.image.sprite = logsBlimpSprite;
        if (seedsButton && seedsButton.image) seedsButton.image.sprite = seedsBlimpSprite;

        ApplyTextStyle(logsText);
        ApplyTextStyle(seedsText);

        if (logsButton) logsButton.onClick.AddListener(CollectLogs);
        if (seedsButton) seedsButton.onClick.AddListener(CollectSeeds);

        if (logsCanvas) logsCanvas.gameObject.SetActive(false);
        if (seedsCanvas) seedsCanvas.gameObject.SetActive(false);
    }

    void ApplyTextStyle(TextMeshProUGUI t)
    {
        if (!t) return;

        t.font = blimpFont;
        t.color = blimpTextColor;
        t.enableAutoSizing = blimpTextAutoSize;

        if (blimpTextAutoSize)
        {
            t.fontSizeMin = blimpTextFontSizeMin;
            t.fontSizeMax = blimpTextFontSizeMax;
        }
        else
        {
            t.fontSize = blimpTextFontSizeMax;
        }

        t.fontStyle = blimpTextBold ? FontStyles.Bold : FontStyles.Normal;
    }

    Canvas BuildBlimpCanvas(string name, Vector3 localOffset, out TextMeshProUGUI textOut, out Button buttonOut)
    {
        var canvasObj = new GameObject(name + "_Canvas", typeof(Canvas), typeof(GraphicRaycaster));
        canvasObj.transform.SetParent(transform, false);
        canvasObj.transform.localPosition = localOffset;

        var c = canvasObj.GetComponent<Canvas>();
        c.renderMode = RenderMode.WorldSpace;
        c.worldCamera = Camera.main;
        c.sortingOrder = 2200;

        var rc = c.GetComponent<RectTransform>();
        rc.anchorMin = rc.anchorMax = rc.pivot = new Vector2(0.5f, 0.5f);
        rc.sizeDelta = blimpSize;

        float scale = 0.6f / Mathf.Max(1f, blimpSize.x);
        c.transform.localScale = new Vector3(scale, scale, scale);

        var btnObj = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        btnObj.transform.SetParent(canvasObj.transform, false);

        var btnRc = btnObj.GetComponent<RectTransform>();
        btnRc.anchorMin = btnRc.anchorMax = btnRc.pivot = new Vector2(0.5f, 0.5f);
        btnRc.sizeDelta = blimpSize;

        var bg = btnObj.GetComponent<Image>();
        if (blimpBackgroundSprite)
        {
            bg.sprite = blimpBackgroundSprite;
            bg.type = Image.Type.Sliced;
        }
        else
        {
            bg.color = new Color(0f, 0f, 0f, 0.55f);
        }

        var textObj = new GameObject("Count", typeof(RectTransform), typeof(TextMeshProUGUI));
        textObj.transform.SetParent(btnObj.transform, false);

        var textRc = textObj.GetComponent<RectTransform>();
        textRc.anchorMin = textRc.anchorMax = new Vector2(0.5f, 0.5f);
        textRc.pivot = new Vector2(0.5f, 0.5f);
        textRc.sizeDelta = new Vector2(blimpSize.x, blimpSize.y);

        var tmp = textObj.GetComponent<TextMeshProUGUI>();
        tmp.text = "x0";
        tmp.alignment = TextAlignmentOptions.Center;

        textOut = tmp;
        buttonOut = btnObj.GetComponent<Button>();
        return c;
    }

    void LoadState()
    {
        pendingLogs = PlayerPrefs.GetInt(PrefKeyPendingLogs, 0);
        pendingSeeds = PlayerPrefs.GetInt(PrefKeyPendingSeeds, 0);
        seedProgressLogs = PlayerPrefs.GetInt(PrefKeySeedProgress, 0);

        string remStr = PlayerPrefs.GetString(PrefKeyLogRemainder, "0");
        if (!float.TryParse(remStr, NumberStyles.Float, CultureInfo.InvariantCulture, out logRemainder))
            logRemainder = 0f;
    }

    void ApplyOfflineProduction()
    {
        if (!PlayerPrefs.HasKey(PrefKeyLast)) return;

        if (!double.TryParse(PlayerPrefs.GetString(PrefKeyLast, "0"), NumberStyles.Float, CultureInfo.InvariantCulture, out double last))
            return;

        double now = NowUnix();
        double deltaSeconds = Mathf.Max(0f, (float)(now - last));
        float minutes = (float)(deltaSeconds / 60.0);

        float totalLogsFloat = minutes * Mathf.Max(0f, logsPerMinute) + Mathf.Max(0f, logRemainder);
        int producedLogs = Mathf.FloorToInt(totalLogsFloat);
        logRemainder = Mathf.Max(0f, totalLogsFloat - producedLogs);

        if (producedLogs <= 0) return;

        pendingLogs += producedLogs;

        seedProgressLogs += producedLogs;
        int batchSize = Mathf.Max(1, logsPerSeedBatch);
        int seedBatches = seedProgressLogs / batchSize;

        if (seedBatches > 0)
        {
            pendingSeeds += seedBatches * Mathf.Max(1, seedsPerBatch);
            seedProgressLogs -= seedBatches * batchSize;
        }
    }

    void SaveState(double now)
    {
        PlayerPrefs.SetString(PrefKeyLast, now.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.SetInt(PrefKeyPendingLogs, pendingLogs);
        PlayerPrefs.SetInt(PrefKeyPendingSeeds, pendingSeeds);
        PlayerPrefs.SetInt(PrefKeySeedProgress, seedProgressLogs);
        PlayerPrefs.SetString(PrefKeyLogRemainder, logRemainder.ToString(CultureInfo.InvariantCulture));
        PlayerPrefs.Save();
    }

    double NowUnix()
    {
        return System.DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    void UpdateBlimpUI()
    {
        bool showLogs = pendingLogs > 0;
        bool showSeeds = pendingSeeds > 0;

        if (logsCanvas) logsCanvas.gameObject.SetActive(showLogs);
        if (seedsCanvas) seedsCanvas.gameObject.SetActive(showSeeds);

        if (logsText) logsText.text = "logs:" + pendingLogs;
        if (seedsText) seedsText.text = "seeds:" + pendingSeeds;
    }

    void CollectLogs()
    {
        if (pendingLogs <= 0 || storage == null || logItem == null) return;

        int amount = pendingLogs;
        pendingLogs = 0;
        storage.Put(logItem, amount);

        Vector3 startWorld = transform.position + Vector3.up * blimpHeight;
        if (overlayCanvas && storageAnchor && logIcon)
            StartCoroutine(FlyToStorageRoutine(startWorld, amount, logIcon));

        SaveState(NowUnix());
        UpdateBlimpUI();
    }

    void CollectSeeds()
    {
        if (pendingSeeds <= 0 || storage == null || seedItem == null) return;

        int amount = pendingSeeds;
        pendingSeeds = 0;
        storage.Put(seedItem, amount);

        Vector3 startWorld = transform.position + Vector3.up * blimpHeight;
        if (overlayCanvas && storageAnchor && seedIcon)
            StartCoroutine(FlyToStorageRoutine(startWorld, amount, seedIcon));

        SaveState(NowUnix());
        UpdateBlimpUI();
    }

    void FaceCanvases()
    {
        var cam = Camera.main;
        if (!cam) return;

        if (logsCanvas)
        {
            if (logsCanvas.worldCamera == null) logsCanvas.worldCamera = cam;
            FaceCanvas(logsCanvas.transform, cam);
        }

        if (seedsCanvas)
        {
            if (seedsCanvas.worldCamera == null) seedsCanvas.worldCamera = cam;
            FaceCanvas(seedsCanvas.transform, cam);
        }
    }

    void FaceCanvas(Transform t, Camera cam)
    {
        var dir = t.position - cam.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f)
            t.rotation = Quaternion.LookRotation(dir);
    }

    void AutoWireStorage()
    {
        if (overlayCanvas == null)
        {
            foreach (var c in Resources.FindObjectsOfTypeAll<Canvas>())
            {
                if (!c || !c.gameObject.scene.IsValid()) continue;

                if (!string.IsNullOrEmpty(overlayCanvasTag) && c.CompareTag(overlayCanvasTag))
                {
                    overlayCanvas = c;
                    break;
                }

                if (c.renderMode == RenderMode.ScreenSpaceOverlay || c.renderMode == RenderMode.ScreenSpaceCamera)
                {
                    overlayCanvas = c;
                    break;
                }
            }
        }

        if (overlayCanvas && storageAnchor == null)
        {
            foreach (var rt in overlayCanvas.GetComponentsInChildren<RectTransform>(true))
            {
                if (rt.name == storageAnchorName)
                {
                    storageAnchor = rt;
                    break;
                }
            }
        }
    }

    IEnumerator FlyToStorageRoutine(Vector3 startWorld, int count, Sprite icon)
    {
        if (!overlayCanvas || !storageAnchor || icon == null) yield break;

        int icons = Mathf.Clamp(count, 1, flyBurst);
        RectTransform overlayRect = overlayCanvas.transform as RectTransform;

        Vector2 startScreen = RectTransformUtility.WorldToScreenPoint(Camera.main, startWorld);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRect, startScreen, overlayCanvas.worldCamera, out Vector2 startLocal);

        Vector2 endScreen = RectTransformUtility.WorldToScreenPoint(overlayCanvas.worldCamera, storageAnchor.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(overlayRect, endScreen, overlayCanvas.worldCamera, out Vector2 endLocal);

        Vector2 control = Vector2.Lerp(startLocal, endLocal, 0.5f) + Vector2.up * 80f;

        for (int i = 0; i < icons; i++)
            StartCoroutine(SingleFlyIcon(startLocal, control, endLocal, icon, i * 0.03f));

        yield return new WaitForSeconds(flyDuration + 0.15f);
    }

    IEnumerator SingleFlyIcon(Vector2 start, Vector2 control, Vector2 end, Sprite icon, float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);

        RectTransform overlayRect = overlayCanvas.transform as RectTransform;

        var iconObj = new GameObject("FlyIcon", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        iconObj.transform.SetParent(overlayRect, false);

        var iconRc = iconObj.GetComponent<RectTransform>();
        iconRc.sizeDelta = flyIconSize;
        iconRc.anchoredPosition = start;

        var img = iconObj.GetComponent<Image>();
        img.sprite = icon;
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

    static Vector2 QuadraticBezier(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        float u = 1f - t;
        return u * u * a + 2f * u * t * b + t * t * c;
    }
}
