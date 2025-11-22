using UnityEngine;

[CreateAssetMenu(menuName = "Jobs/Job Reward", fileName = "JobReward_")]
public class JobRewardSO : ScriptableObject
{
    public string id;
    public string displayName;

    [Min(0)] public int moneyReward = 100;
    [Min(0)] public int xpReward = 10;

    [Min(1)] public int estimatedMinutes = 3;
    public bool isBigJob;
}
