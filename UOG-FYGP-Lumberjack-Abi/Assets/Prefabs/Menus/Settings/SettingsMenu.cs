using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using System.Linq;

public class SettingsMenu : MonoBehaviour
{
    [Header("Prefab (from Project)")]
    [SerializeField] private GameObject settingsPanelPrefab;   // Drag Settings_Panel prefab here
    [SerializeField] private Transform panelParentOverride;    // Optional override

    [Header("Buttons")]
    [SerializeField] private Button openButton;
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape;

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mixer;
    [SerializeField] private string musicParam = "MusicVol";
    [SerializeField] private string sfxParam = "SFXVol";

    [Header("Behaviour")]
    [SerializeField] private bool pauseOnOpen = false;
    [SerializeField] private bool lockPlayerInputOnOpen = true;

    // runtime
    private GameObject panelInstance;
    private Slider musicSlider;
    private Slider sfxSlider;
    private Button closeButton;

    private string MusicKey => $"VOL_{musicParam}";
    private string SfxKey => $"VOL_{sfxParam}";

    private void Awake()
    {
        if (settingsPanelPrefab == null)
        {
            return;
        }

        // Find parent canvas
        Transform parent = panelParentOverride;
        if (parent == null)
        {
            Canvas c = GetComponentInParent<Canvas>();
            parent = c != null ? c.transform : null;
        }

        // Instantiate prefab
        panelInstance = Instantiate(settingsPanelPrefab, parent);
        panelInstance.name = settingsPanelPrefab.name + "_INSTANCE";
        panelInstance.SetActive(false);
        Debug.Log("[SettingsMenu] Instantiated: " + panelInstance.name);

        // Auto-wire open button
        if (openButton != null)
        {
            openButton.onClick.AddListener(Toggle);
        }

        // ------------------------------------
        // AUTO-WIRE SLIDERS
        // ------------------------------------
        Debug.Log("[SettingsMenu] Auto-wiring sliders…");

        // Find all sliders in the panel
        var sliders = panelInstance.GetComponentsInChildren<Slider>(true);

        // Try match by name
        musicSlider = sliders.FirstOrDefault(s => s.name.ToLower().Contains("music"));
        sfxSlider = sliders.FirstOrDefault(s => s.name.ToLower().Contains("sfx"));

        // Try tags (optional)
        if (musicSlider == null)
            musicSlider = sliders.FirstOrDefault(s => s.CompareTag("MusicVolume"));
        if (sfxSlider == null)
            sfxSlider = sliders.FirstOrDefault(s => s.CompareTag("SfxVolume"));

        // Fallback: first 2 sliders
        if (musicSlider == null && sliders.Length > 0) musicSlider = sliders[0];
        if (sfxSlider == null && sliders.Length > 1) sfxSlider = sliders[1];

        Debug.Log("[SettingsMenu] Found Music Slider: " + (musicSlider ? musicSlider.name : "NONE"));
        Debug.Log("[SettingsMenu] Found SFX Slider: " + (sfxSlider ? sfxSlider.name : "NONE"));

        if (musicSlider != null)
        {
            musicSlider.minValue = 0f;
            musicSlider.maxValue = 1f;
            musicSlider.onValueChanged.AddListener(SetMusicVolumeFromSlider);
        }

        if (sfxSlider != null)
        {
            sfxSlider.minValue = 0f;
            sfxSlider.maxValue = 1f;
            sfxSlider.onValueChanged.AddListener(SetSfxVolumeFromSlider);
        }

        // ------------------------------------
        // AUTO-WIRE CLOSE BUTTON
        // ------------------------------------
        closeButton = panelInstance.GetComponentsInChildren<Button>(true)
                                   .FirstOrDefault(b => b.name.ToLower().Contains("close"));

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Close);
            Debug.Log("[SettingsMenu] Found close button: " + closeButton.name);
        }
        else
        {
            Debug.LogWarning("[SettingsMenu] No close button found inside prefab.");
        }

        // Load saved volume
        float savedMusic = PlayerPrefs.GetFloat(MusicKey, 1f);
        float savedSfx = PlayerPrefs.GetFloat(SfxKey, 1f);

        if (musicSlider != null) musicSlider.value = savedMusic;
        if (sfxSlider != null) sfxSlider.value = savedSfx;

        ApplyVolumeToMixer(musicParam, savedMusic);
        ApplyVolumeToMixer(sfxParam, savedSfx);
    }

    private void Update()
    {
        if (toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey))
        {
            Toggle();
        }
    }

    public void Open()
    {
        if (panelInstance == null) return;
        if (panelInstance.activeSelf) return;

        panelInstance.SetActive(true);

        if (pauseOnOpen) Time.timeScale = 0f;
        if (lockPlayerInputOnOpen) TrySetPlayerInputLocked(true);
    }

    public void Close()
    {
        if (panelInstance == null) return;
        if (!panelInstance.activeSelf) return;

        panelInstance.SetActive(false);

        if (pauseOnOpen) Time.timeScale = 1f;
        if (lockPlayerInputOnOpen) TrySetPlayerInputLocked(false);
    }

    public void Toggle()
    {
        if (panelInstance == null) return;

        if (panelInstance.activeSelf) Close();
        else Open();
    }

    private void SetMusicVolumeFromSlider(float v)
    {
        ApplyVolumeToMixer(musicParam, v);
        PlayerPrefs.SetFloat(MusicKey, v);
    }

    private void SetSfxVolumeFromSlider(float v)
    {
        ApplyVolumeToMixer(sfxParam, v);
        PlayerPrefs.SetFloat(SfxKey, v);
    }

    private void ApplyVolumeToMixer(string param, float slider01)
    {
        if (mixer == null) return;

        float clamped = Mathf.Clamp(slider01, 0.0001f, 1f);
        float dB = Mathf.Log10(clamped) * 20f;
        mixer.SetFloat(param, dB);
    }

    private void TrySetPlayerInputLocked(bool locked)
    {
        var type = System.Type.GetType("PlayerController");
        if (type == null) return;

        var field = type.GetField("IsInputLocked",
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.Static);

        if (field != null && field.FieldType == typeof(bool))
        {
            field.SetValue(null, locked);
        }
    }
}
