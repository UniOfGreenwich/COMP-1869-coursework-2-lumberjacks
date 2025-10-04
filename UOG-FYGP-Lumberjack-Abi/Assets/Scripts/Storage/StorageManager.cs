using UnityEngine;

public class StorageManager : MonoBehaviour
{
    public static StorageManager current;
    [SerializeField] private GameObject Storage;
    private StorageBuilding StorageBuilding;
    private void Awake()
    {
        current = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        // Spawn storage directly at a given position
        GameObject storageObj = BuildingSystem.instance.InitializeWithObject(Storage, new Vector3(8f, -0.25f, 0f));

        // Get the Placeble script
        Placeble storage = storageObj.GetComponentInChildren<Placeble>();

        if (storage != null)
        {
            storage.Load(); // finalize immediately (no ghost preview)
        }
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
