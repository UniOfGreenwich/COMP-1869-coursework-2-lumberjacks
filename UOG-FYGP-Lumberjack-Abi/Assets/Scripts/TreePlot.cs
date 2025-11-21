using System;
using TMPro;
using UnityEngine;

public class TreePlot : MonoBehaviour
{
    [Header("Identity")]
    public string plotId = "Plot_01";        // unique plot id

    [Header("Items")]
    public ItemSO seedItem;                  // seed item reference
    public ItemSO logItem;                   // log item reference

    [Header("Growth")]
    [Min(0.01f)] public float growHours = 3f; // growth time hours
    public int logsPerHarvest = 10;          // logs per harvest

    [Header("Visuals")]
    public GameObject emptyVisual;           // empty visual object
    public GameObject growingVisual;         // growing visual object
    public GameObject grownVisual;           // grown visual object

    [Header("UI")]
    public TextMeshProUGUI statusText;       // status label reference

    private StorageManager storage;
    private bool isGrowing;
    private bool isGrown;
    private DateTime plantedTime;

    void Awake()
    {
        storage = FindObjectOfType<StorageManager>();
        LoadState();
        UpdateVisuals();
        UpdateStatusText();
    }

    void Update()
    {
        if (isGrowing)
        {
            DateTime now = DateTime.UtcNow;
            TimeSpan delta = now - plantedTime;
            double growSeconds = growHours * 3600.0;

            if (delta.TotalSeconds >= growSeconds)
            {
                isGrowing = false;
                isGrown = true;
                SaveState();
                UpdateVisuals();
                UpdateStatusText();
            }
            else
            {
                UpdateStatusText();
            }
        }
    }

    void OnMouseDown()
    {
        if (!storage) return;

        if (!isGrowing && !isGrown)
        {
            TryPlant();
        }
        else if (isGrown)
        {
            HarvestTree();
        }
    }

    private void TryPlant()
    {
        if (!seedItem) return;

        int taken = storage.Take(seedItem, 1);
        if (taken <= 0) return;

        plantedTime = DateTime.UtcNow;
        isGrowing = true;
        isGrown = false;
        SaveState();
        UpdateVisuals();
        UpdateStatusText();
    }

    private void HarvestTree()
    {
        if (logItem && logsPerHarvest > 0 && storage != null)
        {
            storage.Put(logItem, logsPerHarvest);
        }

        isGrowing = false;
        isGrown = false;
        SaveState();
        UpdateVisuals();
        UpdateStatusText();
    }

    private void UpdateVisuals()
    {
        if (emptyVisual) emptyVisual.SetActive(!isGrowing && !isGrown);
        if (growingVisual) growingVisual.SetActive(isGrowing);
        if (grownVisual) grownVisual.SetActive(isGrown);
    }

    private void UpdateStatusText()
    {
        if (!statusText) return;

        if (!isGrowing && !isGrown)
        {
            statusText.text = "Tap to plant seed";
        }
        else if (isGrowing)
        {
            TimeSpan elapsed = DateTime.UtcNow - plantedTime;
            double growSeconds = growHours * 3600.0;
            float t = Mathf.Clamp01((float)(elapsed.TotalSeconds / growSeconds));
            int percent = Mathf.RoundToInt(t * 100f);
            statusText.text = $"Growing {percent}%";
        }
        else if (isGrown)
        {
            statusText.text = "Tree grown tap harvest";
        }
    }

    private void SaveState()
    {
        string baseKey = "TreePlot_" + plotId;
        int state = 0;
        if (isGrowing) state = 1;
        if (isGrown) state = 2;

        PlayerPrefs.SetInt(baseKey + "_State", state);
        PlayerPrefs.SetString(baseKey + "_Time", plantedTime.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    private void LoadState()
    {
        string baseKey = "TreePlot_" + plotId;
        int state = PlayerPrefs.GetInt(baseKey + "_State", 0);
        string timeRaw = PlayerPrefs.GetString(baseKey + "_Time", DateTime.UtcNow.ToBinary().ToString());

        long bin;
        if (!long.TryParse(timeRaw, out bin))
        {
            bin = DateTime.UtcNow.ToBinary();
        }

        plantedTime = DateTime.FromBinary(bin);

        isGrowing = state == 1;
        isGrown = state == 2;
    }
}
