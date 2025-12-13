using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem instance;

    [Header("Grid + Tilemap")]
    public GridLayout gridLayout;
    private Grid grid;
    [SerializeField] private Tilemap MainTilemap;   // marks occupied cells
    [SerializeField] private TileBase OccupiedTile; // tile used to mark taken area

    [Header("Prefabs")]
    public GameObject housePrefab;

    private Placeble objectToPlace;
    private int groundLayer;

    private void Awake()
    {
        instance = this;
        grid = gridLayout.gameObject.GetComponent<Grid>();
        groundLayer = LayerMask.NameToLayer("Ground"); // Cache ground layer index
    }

    private void Update()
    {
        // To start placing house
        if (Input.GetKeyDown(KeyCode.H))
            StartPlacement(housePrefab);

        if (objectToPlace == null) return;
        if (Input.GetKeyDown(KeyCode.R) && objectToPlace != null)
        {
            objectToPlace.Rotate();
        }
        Vector3 mousePos = GetMouseWorldPosition();
        Vector3 snappedPos = SnapCoordinateToGrid(mousePos);
        objectToPlace.transform.position = snappedPos;
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (CanBePlaced(objectToPlace))
            {
                Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                TakeArea(start, objectToPlace.Size);
                objectToPlace.Place();
                SavePlacedObject(objectToPlace);
                objectToPlace = null;

            }
            else
            {
                Debug.Log("[BuildingSystem] Blocked! Cannot place here.");
            }
        }

        // Cancel placement
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(objectToPlace.gameObject);
            objectToPlace = null;
        }
    }
    public void StartPlacement(GameObject prefab, string itemId = "")
    {
        Vector3 mousePos = GetMouseWorldPosition();
        Vector3 snappedPos = SnapCoordinateToGrid(mousePos);

        GameObject obj = Instantiate(prefab, snappedPos, Quaternion.identity);
        objectToPlace = obj.GetComponentInChildren<Placeble>();
        if (objectToPlace == null)
        {
            Debug.LogError("Prefab " + prefab.name + " is missing a Placeble component.");
        }
        else
        {
            objectToPlace.prefabId = itemId; // assign ID from ShopItemSO
        }
    }


    #region Utils
    public static Vector3 GetMouseWorldPosition()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        int groundMask = LayerMask.GetMask("Ground");
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
            return hit.point;
        float t = -ray.origin.y / ray.direction.y;
        return ray.origin + ray.direction * t;
    }

    public Vector3 SnapCoordinateToGrid(Vector3 position)
    {
        Vector3Int cellPos = gridLayout.WorldToCell(position);
        Vector3 center = grid.GetCellCenterWorld(cellPos);

        // Ground level
        center.y = 0f;

        if (objectToPlace != null)
        {
            float bottomLocalY = 0f;

            // Use collider so it works with any pivot (center or bottom)
            var col = objectToPlace.GetComponent<BoxCollider>();
            if (col)
            {
                bottomLocalY = col.center.y - col.size.y * 0.5f; // local Y of the bottom
            }
            else
            {
                // Fallback to renderer if there’s no collider
                var r = objectToPlace.GetComponentInChildren<Renderer>();
                if (r) bottomLocalY = -r.bounds.extents.y;
            }

            // Lift so the collider bottom sits exactly on the ground
            center.y -= bottomLocalY;
        }

        return center;
    }

    private static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
    {
        TileBase[] array = new TileBase[area.size.x * area.size.y * area.size.z];
        int counter = 0;
        foreach (var v in area.allPositionsWithin)
        {
            Vector3Int pos = new Vector3Int(v.x, v.y, 0);
            array[counter] = tilemap.GetTile(pos);
            counter++;
        }
        return array;
    }
    #endregion

    #region Tile Handling
    public bool CanBePlaced(Placeble placeble)
    {
        Vector3Int start = gridLayout.WorldToCell(placeble.GetStartPosition());
        BoundsInt area = new BoundsInt(start, placeble.Size);
        // Check if over ground
        if (!IsOverGround(placeble))
        {
            Debug.Log("[BuildingSystem] Cannot place — not over ground!");
            return false;
        }
        // Check if the area is free (not occupied)
        TileBase[] baseArray = GetTilesBlock(area, MainTilemap);
        foreach (var b in baseArray)
        {
            if (b == OccupiedTile)
                return false;
        }

        return true;
    }
    public GameObject InitializeWithObject(GameObject prefab, Vector3 pos)
    {
        // Snap position to grid
        Vector3 snappedPos = SnapCoordinateToGrid(pos);

        // Instantiate
        GameObject obj = Instantiate(prefab, snappedPos, Quaternion.identity);

        // Ensure it has a Placeble component
        Placeble temp = obj.GetComponentInChildren<Placeble>();
        if (temp == null)
        {
            Debug.LogError(prefab.name + "missing a Placeble component.");
        }

        return obj;
    }

    private bool IsOverGround(Placeble placeble)
    {
        // Start ray a little above the pivot to avoid collider self-hit
        Vector3 origin = placeble.transform.position + Vector3.up * 0.2f;

        // Raycast straight down
        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 5f))
        {
            return hit.collider.gameObject.layer == groundLayer;
        }

        return false;
    }
    public void TakeArea(Vector3Int start, Vector3Int size)
    {
        // Mark occupied tiles ONCE when placed
        MainTilemap.BoxFill(
            start,
            OccupiedTile,
            start.x, start.y,
            start.x + size.x - 1,
            start.y + size.y - 1
        );
    }
    #endregion
    private void SavePlacedObject(Placeble placeble)
    {
        string id = placeble.prefabId;
        if (string.IsNullOrEmpty(id)) return;

        PlayerPrefs.SetFloat("MachinePosX_" + id, placeble.transform.position.x);
        PlayerPrefs.SetFloat("MachinePosY_" + id, placeble.transform.position.y);
        PlayerPrefs.SetFloat("MachinePosZ_" + id, placeble.transform.position.z);
        PlayerPrefs.SetFloat("MachineRotY_" + id, placeble.transform.eulerAngles.y);
        PlayerPrefs.SetInt("MachineOwned_" + id, 1);
        PlayerPrefs.Save();

        Debug.Log("[BuildingSystem] Saved placement for " + id);
    }

}
