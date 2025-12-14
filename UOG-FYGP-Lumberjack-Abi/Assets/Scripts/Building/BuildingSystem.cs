using UnityEngine;
using UnityEngine.Tilemaps;

public class BuildingSystem : MonoBehaviour
{
    public static BuildingSystem instance;

    [Header("Grid + Tilemap")]
    public GridLayout gridLayout;
    private Grid grid;
    [SerializeField] private Tilemap MainTilemap;
    [SerializeField] private TileBase OccupiedTile;

    [Header("Prefabs")]
    public GameObject housePrefab;

    private Placeble objectToPlace;
    private int groundLayer;

    void Awake()
    {
        instance = this;

        if (gridLayout == null)
            gridLayout = GetComponentInChildren<GridLayout>();

        if (gridLayout != null)
            grid = gridLayout.gameObject.GetComponent<Grid>();

        groundLayer = LayerMask.NameToLayer("Ground");
    }

    void Update()
    {
        if (objectToPlace == null) return;

        if (Input.GetKeyDown(KeyCode.R))
            objectToPlace.Rotate();

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

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Destroy(objectToPlace.gameObject);
            objectToPlace = null;
        }
    }

    public void StartPlacement(ShopItemSO item)
    {
        if (item == null) return;
        if (item.prefabToPlace == null) return;
        if (string.IsNullOrEmpty(item.id))
        {
            Debug.LogError("[BuildingSystem] ShopItemSO id is empty on " + item.name);
            return;
        }

        Vector3 mousePos = GetMouseWorldPosition();
        Vector3 snappedPos = SnapCoordinateToGrid(mousePos);

        GameObject obj = Instantiate(item.prefabToPlace, snappedPos, Quaternion.identity);
        objectToPlace = obj.GetComponentInChildren<Placeble>();

        if (objectToPlace == null)
        {
            Debug.LogError("[BuildingSystem] No Placeble component found on prefab " + item.prefabToPlace.name);
            Destroy(obj);
            return;
        }

        objectToPlace.prefabId = item.id;
    }

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
        if (gridLayout == null || grid == null)
            return position;

        Vector3Int cellPos = gridLayout.WorldToCell(position);
        Vector3 center = grid.GetCellCenterWorld(cellPos);
        center.y = 0f;

        if (objectToPlace != null)
        {
            float bottomLocalY = 0f;

            var col = objectToPlace.GetComponent<BoxCollider>();
            if (col)
                bottomLocalY = col.center.y - col.size.y * 0.5f;
            else
            {
                var r = objectToPlace.GetComponentInChildren<Renderer>();
                if (r) bottomLocalY = -r.bounds.extents.y;
            }

            center.y -= bottomLocalY;
        }

        return center;
    }

    public bool CanBePlaced(Placeble placeble)
    {
        if (gridLayout == null || MainTilemap == null || OccupiedTile == null) return false;

        Vector3Int start = gridLayout.WorldToCell(placeble.GetStartPosition());
        BoundsInt area = new BoundsInt(start, placeble.Size);

        if (!IsOverGround(placeble))
        {
            Debug.Log("[BuildingSystem] Cannot place — not over ground!");
            return false;
        }

        TileBase[] baseArray = GetTilesBlock(area, MainTilemap);
        foreach (var b in baseArray)
        {
            if (b == OccupiedTile)
                return false;
        }

        return true;
    }

    static TileBase[] GetTilesBlock(BoundsInt area, Tilemap tilemap)
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

    bool IsOverGround(Placeble placeble)
    {
        Vector3 origin = placeble.transform.position + Vector3.up * 0.2f;

        if (Physics.Raycast(origin, Vector3.down, out RaycastHit hit, 5f))
            return hit.collider.gameObject.layer == groundLayer;

        return false;
    }

    public void TakeArea(Vector3Int start, Vector3Int size)
    {
        if (MainTilemap == null || OccupiedTile == null) return;

        MainTilemap.BoxFill(
            start,
            OccupiedTile,
            start.x, start.y,
            start.x + size.x - 1,
            start.y + size.y - 1
        );
    }

    void SavePlacedObject(Placeble placeble)
    {
        string id = placeble.prefabId;
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogError("[BuildingSystem] prefabId is empty, cannot save!");
            return;
        }

        PlayerPrefs.SetFloat("MachinePosX_" + id, placeble.transform.position.x);
        PlayerPrefs.SetFloat("MachinePosY_" + id, placeble.transform.position.y);
        PlayerPrefs.SetFloat("MachinePosZ_" + id, placeble.transform.position.z);
        PlayerPrefs.SetFloat("MachineRotY_" + id, placeble.transform.eulerAngles.y);
        PlayerPrefs.SetInt("MachineOwned_" + id, 1);
        PlayerPrefs.Save();

        Debug.Log("[BuildingSystem] Saved placement for " + id);
    }
}
