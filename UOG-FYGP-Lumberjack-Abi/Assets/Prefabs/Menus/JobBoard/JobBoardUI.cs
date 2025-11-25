using System.Collections.Generic;
using UnityEngine;

public class JobBoardUI : MonoBehaviour
{
    [Header("Job Templates")]
    public List<JobRewardSO> jobDefinitions = new List<JobRewardSO>();
    public int bigJobsPerDay = 3;
    public int smallJobsPerDay = 3;

    [Header("Customers Pool")]
    public CustomerTypeSO[] customerTypes;

    [Header("Customer Spawn")]
    public Customer customerPrefab;
    public Transform customerSpawnPoint;
    public float spawnSpacing = 1.5f;

    [Header("UI")]
    public Transform jobsContainer;
    public JobCardUI jobCardPrefab;

    class ActiveJob
    {
        public JobRewardSO job;
        public CustomerTypeSO customer;
    }

    readonly List<ActiveJob> todaysJobs = new List<ActiveJob>();
    Inventory inventory;

    void Start()
    {
        inventory = FindFirstObjectByType<Inventory>();
        GenerateJobsForToday();
        BuildJobListUI();
    }

    void GenerateJobsForToday()
    {
        todaysJobs.Clear();

        var big = new List<JobRewardSO>();
        var small = new List<JobRewardSO>();

        for (int i = 0; i < jobDefinitions.Count; i++)
        {
            JobRewardSO job = jobDefinitions[i];
            if (!job) continue;

            if (job.isBigJob) big.Add(job);
            else small.Add(job);
        }

        AddRandomJobs(big, bigJobsPerDay);
        AddRandomJobs(small, smallJobsPerDay);
    }

    void AddRandomJobs(List<JobRewardSO> source, int count)
    {
        if (source.Count == 0 || customerTypes == null || customerTypes.Length == 0)
            return;

        int toAdd = Mathf.Min(count, source.Count);

        for (int i = 0; i < toAdd; i++)
        {
            int jobIndex = Random.Range(0, source.Count);
            JobRewardSO job = source[jobIndex];

            int custIndex = Random.Range(0, customerTypes.Length);
            CustomerTypeSO custType = customerTypes[custIndex];

            todaysJobs.Add(new ActiveJob
            {
                job = job,
                customer = custType
            });
        }
    }

    void BuildJobListUI()
    {
        if (!jobsContainer || !jobCardPrefab) return;

        for (int i = jobsContainer.childCount - 1; i >= 0; i--)
            Destroy(jobsContainer.GetChild(i).gameObject);

        for (int i = 0; i < todaysJobs.Count; i++)
        {
            ActiveJob aj = todaysJobs[i];
            if (aj.job == null || aj.customer == null) continue;

            JobCardUI card = Instantiate(jobCardPrefab, jobsContainer);
            card.Bind(this, i, aj.job, aj.customer);
        }
    }

    public void AcceptJob(int index)
    {
        if (index < 0 || index >= todaysJobs.Count) return;
        if (!customerPrefab || !customerSpawnPoint || !inventory) return;

        ActiveJob aj = todaysJobs[index];

        int existingCustomers = FindObjectsOfType<Customer>().Length;
        Vector3 spawnPos = customerSpawnPoint.position + new Vector3(spawnSpacing * existingCustomers, 0f, 0f);

        Customer spawned = Instantiate(customerPrefab, spawnPos, customerSpawnPoint.rotation);
        spawned.Init(aj.job, aj.customer, inventory);
    }
}
