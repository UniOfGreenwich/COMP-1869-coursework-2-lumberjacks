using System;
using TMPro;
using UnityEngine;

public class OfflineHarvesterNPC : MonoBehaviour
{
    [Header("Items")]
    public ItemSO logItem;              // log item reference
    public ItemSO seedItem;             // seed item reference

    [Header("Rates")]
    [Min(0.01f)] public float logsPerMinute = 2f;
    [Min(1)] public int logsPerSeedBatch = 5;
    [Min(1)] public int seedsPerBatch = 3;

    [Header("UI")]
    public TextMeshProUGUI statusText;  // status text label
    public TextMeshProUGUI pendingText; // pending text label

    [Header("Animation")]
    public Animator npcAnimator;        // npc animator reference
    public string sleepBoolName = "IsSleeping";

    [Header("Save Key")]
    public string lastQuitKey = "NPC_LastQuitTime";

    private StorageManager storage;
    private int pendingLogs;
    private int pendingSeeds;
    private bool offlineComputed;

    void Awake()
    {
        storage = UnityEngine.Object.FindFirstObjectByType<StorageManager>();
        ComputeOfflineHarvestOnce();
        UpdateStatusText();
        UpdatePendingText();
        SetSleepState(true);
    }

    void OnApplicationQuit()
    {
        SaveQuitTimeNow();
    }

    void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            SaveQuitTimeNow();
        }
    }

    public void OnCollectButtonPressed()
    {
        if (!storage) return;
        if (!logItem || !seedItem) return;
        if (pendingLogs <= 0 && pendingSeeds <= 0) return;

        storage.Put(logItem, pendingLogs);
        storage.Put(seedItem, pendingSeeds);

        pendingLogs = 0;
        pendingSeeds = 0;

        UpdatePendingText();
    }

    public void OnRefreshButtonPressed()
    {
        UpdatePendingText();
        UpdateStatusText();
    }

    private void ComputeOfflineHarvestOnce()
    {
        if (offlineComputed) return;

        DateTime now = DateTime.UtcNow;
        DateTime lastQuit = LoadLastQuitTime(now);

        TimeSpan offline = now - lastQuit;
        if (offline.TotalMinutes < 0.0)
        {
            offline = TimeSpan.Zero;
        }

        double minutes = offline.TotalMinutes;
        float logsFloat = logsPerMinute * (float)minutes;
        int logs = Mathf.FloorToInt(logsFloat);
        if (logs < 0) logs = 0;

        pendingLogs = logs;

        int batches = pendingLogs / logsPerSeedBatch;
        pendingSeeds = batches * seedsPerBatch;

        offlineComputed = true;
    }

    private DateTime LoadLastQuitTime(DateTime fallback)
    {
        if (!PlayerPrefs.HasKey(lastQuitKey))
        {
            PlayerPrefs.SetString(lastQuitKey, fallback.ToBinary().ToString());
            PlayerPrefs.Save();
            return fallback;
        }

        string raw = PlayerPrefs.GetString(lastQuitKey, fallback.ToBinary().ToString());
        long bin;
        if (!long.TryParse(raw, out bin))
        {
            bin = fallback.ToBinary();
        }

        return DateTime.FromBinary(bin);
    }

    private void SaveQuitTimeNow()
    {
        DateTime now = DateTime.UtcNow;
        PlayerPrefs.SetString(lastQuitKey, now.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    private void UpdateStatusText()
    {
        if (!statusText) return;

        statusText.text =
            "Works when game closed\n" +
            $"Rate {logsPerMinute:0.##} logs/min\n" +
            $"{logsPerSeedBatch} logs = {seedsPerBatch} seeds";
    }

    private void UpdatePendingText()
    {
        if (!pendingText) return;

        pendingText.text =
            $"Offline results\n{pendingLogs} logs ready\n{pendingSeeds} seeds ready";
    }

    private void SetSleepState(bool sleeping)
    {
        if (!npcAnimator) return;
        if (string.IsNullOrEmpty(sleepBoolName)) return;

        npcAnimator.SetBool(sleepBoolName, sleeping);
    }
}
