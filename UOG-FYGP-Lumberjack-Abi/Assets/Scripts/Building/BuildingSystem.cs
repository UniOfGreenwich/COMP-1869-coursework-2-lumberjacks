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
        // Snap ghost to grid
        Vector3 mousePos = GetMouseWorldPosition();
        Vector3 snappedPos = SnapCoordinateToGrid(mousePos);
        objectToPlace.transform.position = snappedPos;

        // Confirm placement    
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (CanBePlaced(objectToPlace))
            {
                Vector3Int start = gridLayout.WorldToCell(objectToPlace.GetStartPosition());
                TakeArea(start, objectToPlace.Size);
                objectToPlace.Place();
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

    private void StartPlacement(GameObject prefab)
    {
        Vector3 mousePos = GetMouseWorldPosition();
        Vector3 snappedPos = SnapCoordinateToGrid(mousePos);

        GameObject obj = Instantiate(prefab, snappedPos, Quaternion.identity);
        objectToPlace = obj.GetComponentInChildren<Placeble>();
        if (objectToPlace == null)
            Debug.LogError("Prefab " + prefab.name + " is missing a Placeble component.");
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
        center.y = 0; // ground level
        if (objectToPlace != null)
        {
            float height = objectToPlace.GetComponent<Renderer>().bounds.size.y;
            center.y += height / 2f;
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
}
