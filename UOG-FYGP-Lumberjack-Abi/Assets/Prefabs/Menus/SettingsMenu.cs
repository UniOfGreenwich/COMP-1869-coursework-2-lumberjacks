using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    [Header("Panel & Buttons")]
    [SerializeField] private GameObject panel;        
    [SerializeField] private Button openButton;       
    [SerializeField] private Button closeButton;     
    [SerializeField] private KeyCode toggleKey = KeyCode.Escape; 

    [Header("Audio Mixer")]
    [SerializeField] private AudioMixer mixer;        
    [SerializeField] private string musicParam = "MusicVol";
    [SerializeField] private string sfxParam = "SFXVol";

    [Header("Sliders")]
    [SerializeField] private Slider musicSlider;      
    [SerializeField] private Slider sfxSlider;        

    [Header("Behaviour")]
    [SerializeField] private bool pauseOnOpen = false;
    [SerializeField] private bool lockPlayerInputOnOpen = true;

    // PlayerPrefs keys
    private string MusicKey => $"VOL_{musicParam}";
    private string SfxKey => $"VOL_{sfxParam}";

    private bool _isOpen;

    private void Awake()
    {
        // Safe defaults for sliders
        if (musicSlider != null) { musicSlider.minValue = 0f; musicSlider.maxValue = 1f; }
        if (sfxSlider != null) { sfxSlider.minValue = 0f; sfxSlider.maxValue = 1f; }

        // Wire UI
        if (openButton != null) openButton.onClick.AddListener(Open);
        if (closeButton != null) closeButton.onClick.AddListener(Close);

        if (musicSlider != null) musicSlider.onValueChanged.AddListener(SetMusicVolumeFromSlider);
        if (sfxSlider != null) sfxSlider.onValueChanged.AddListener(SetSfxVolumeFromSlider);

        // Ensure panel starts hidden unless you want it visible in editor
        if (panel != null) panel.SetActive(false);
        _isOpen = false;

        // Load saved values (default full volume)
        float savedMusic = PlayerPrefs.GetFloat(MusicKey, 1f);
        float savedSfx = PlayerPrefs.GetFloat(SfxKey, 1f);

        if (musicSlider != null) musicSlider.value = savedMusic;
        if (sfxSlider != null) sfxSlider.value = savedSfx;

        // Apply to mixer immediately
        ApplyVolumeToMixer(musicParam, savedMusic);
        ApplyVolumeToMixer(sfxParam, savedSfx);
    }

    private void Update()
    {
        if (toggleKey != KeyCode.None && Input.GetKeyDown(toggleKey))
        {
            if (_isOpen) Close(); else Open();
        }
    }

    // --- Open / Close ---

    public void Open()
    {
        if (panel == null || _isOpen) return;

        panel.SetActive(true);
        _isOpen = true;

        if (pauseOnOpen) Time.timeScale = 0f;

        if (lockPlayerInputOnOpen)
        {
            // Requires your PlayerController to expose a public static flag
            // Safe-guard if class is missing in this scene
            TrySetPlayerInputLocked(true);
        }
    }

    public void Close()
    {
        if (panel == null || !_isOpen) return;

        panel.SetActive(false);
        _isOpen = false;

        if (pauseOnOpen) Time.timeScale = 1f;

        if (lockPlayerInputOnOpen)
        {
            TrySetPlayerInputLocked(false);
        }
    }

    public void Toggle()
    {
        if (_isOpen) Close(); else Open();
    }

    // --- Volume hooks ---

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
        if (mixer == null || string.IsNullOrEmpty(param)) return;

        // Convert linear [0..1] slider to decibels; clamp away from log(0)
        float clamped = Mathf.Clamp(slider01, 0.0001f, 1f);
        float dB = Mathf.Log10(clamped) * 20f;  // 1.0 -> 0 dB, 0.5 -> ~-6 dB, ~0 -> ~-80 dB
        mixer.SetFloat(param, dB);
    }



    private void TrySetPlayerInputLocked(bool locked)
    {
       
        var type = System.Type.GetType("PlayerController");
        if (type == null) return;

        var field = type.GetField("IsInputLocked", System.Reflection.BindingFlags.Public |
                                               System.Reflection.BindingFlags.Static);
        if (field != null && field.FieldType == typeof(bool))
        {
            field.SetValue(null, locked);
        }
    }
}
