using UnityEngine;

public class StorageSpawner : MonoBehaviour
{
    [SerializeField] private GameObject cylinderPrefab; // Your Cylinder prefab
    [SerializeField] private Vector3 spawnPosition = new Vector3(0, 0.5f, 0);

    private void Start()
    {
        if (cylinderPrefab != null)
        {
            Instantiate(cylinderPrefab, spawnPosition, Quaternion.identity);
        }
        else
        {
            Debug.LogError("No cylinder prefab assigned in StorageSpawner!");
        }
    }
}
