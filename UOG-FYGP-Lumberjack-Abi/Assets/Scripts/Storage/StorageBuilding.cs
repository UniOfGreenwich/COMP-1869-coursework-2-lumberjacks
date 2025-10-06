using UnityEngine;
public class StorageBuilding : Placeble
{
    private StorageUI storageUI;    
    public string Name { get; private set; }
    [SerializeField] private GameObject windowPrefab;
    public void initialize(string name)
    {
        {
            Name = name;

            if (GameManager.current == null || GameManager.current.canvas == null)
            {
                Debug.LogError("[StorageBuilding] GameManager or its Canvas is missing!");
                return;
            }

            GameObject window = Instantiate(windowPrefab, GameManager.current.canvas.transform);
            window.SetActive(false);

            storageUI = window.GetComponent<StorageUI>();
            if (storageUI != null)
            {
                storageUI.SetNameText(name);
            }
            else
            {
                Debug.LogError("[StorageBuilding] Window prefab has no StorageUI component!");
            }
        }

        /* Name = name;
         GameObject window = Instantiate(windowPrefab, GameManager.current.canvas.transform);
         window.SetActive(false);
         storageUI=window.GetComponent<StorageUI>();
         storageUI.SetNameText(name);*/
    }
    public virtual void OnClick()
    {
        storageUI.gameObject.SetActive(true);
    }
    private void OnMouseUpAsButton()
    {
        OnClick();
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
