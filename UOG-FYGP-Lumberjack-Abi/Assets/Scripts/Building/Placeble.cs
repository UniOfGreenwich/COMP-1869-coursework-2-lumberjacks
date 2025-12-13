using UnityEngine;

public class Placeble : MonoBehaviour
{
    public bool placed { get; private set; }
    public Vector3Int Size { get; private set; }
    public Vector3[] Vertices { get; private set; }

    Vector3[] localBottomCorners;
    MeshRenderer objectrenderer;
    Material originalMaterial;
    Material ghostMaterial;
    public string prefabId;

    void Awake()
    {
        objectrenderer = GetComponent<MeshRenderer>();
        if (objectrenderer != null)
        {
            originalMaterial = objectrenderer.material;
            ghostMaterial = new Material(originalMaterial);
            objectrenderer.material = ghostMaterial;
        }
        else
        {
            Debug.LogError("[Placeble] No MeshRenderer found on " + gameObject.name);
        }

        CacheBottomCorners();
    }

    public void Load()
    {
        placed = true;
        RestoreOriginalMaterial();
        var drag = GetComponent<ObjectDrag>();
        if (drag) Destroy(drag);
    }

    void Start()
    {
        CalculateSize();
    }

    public void Rotate()
    {
        Debug.Log("Rotating: " + gameObject.name);
        transform.Rotate(Vector3.up, 90f, Space.World);
        CalculateSize();
    }

    void Update()
    {
        if (!placed)
        {
            bool canPlace = BuildingSystem.instance != null && BuildingSystem.instance.CanBePlaced(this);
            SetGhostColor(canPlace ? Color.green : Color.red);
        }
    }

    void CacheBottomCorners()
    {
        BoxCollider b = GetComponent<BoxCollider>();
        if (b == null)
            b = GetComponentInChildren<BoxCollider>();

        if (b == null)
        {
            Debug.LogError("[Placeble] No BoxCollider found on " + gameObject.name);
            localBottomCorners = new Vector3[0];
            Size = new Vector3Int(1, 1, 1);
            return;
        }

        var bounds = b.bounds;
        Vector3 c = bounds.center;
        Vector3 e = bounds.extents;

        Vector3[] world = new Vector3[4];
        world[0] = new Vector3(c.x - e.x, c.y - e.y, c.z - e.z);
        world[1] = new Vector3(c.x + e.x, c.y - e.y, c.z - e.z);
        world[2] = new Vector3(c.x + e.x, c.y - e.y, c.z + e.z);
        world[3] = new Vector3(c.x - e.x, c.y - e.y, c.z + e.z);

        localBottomCorners = new Vector3[4];
        for (int i = 0; i < 4; i++)
            localBottomCorners[i] = transform.InverseTransformPoint(world[i]);
    }

    void CalculateSize()
    {
        if (localBottomCorners == null || localBottomCorners.Length == 0)
            CacheBottomCorners();

        if (localBottomCorners == null || localBottomCorners.Length == 0)
        {
            Size = new Vector3Int(1, 1, 1);
            return;
        }

        var grid = BuildingSystem.instance.gridLayout;
        Vector3Int[] cells = new Vector3Int[localBottomCorners.Length];

        for (int i = 0; i < localBottomCorners.Length; i++)
        {
            Vector3 worldPos = transform.TransformPoint(localBottomCorners[i]);
            cells[i] = grid.WorldToCell(worldPos);
        }

        int minX = cells[0].x;
        int maxX = cells[0].x;
        int minY = cells[0].y;
        int maxY = cells[0].y;

        for (int i = 1; i < cells.Length; i++)
        {
            if (cells[i].x < minX) minX = cells[i].x;
            if (cells[i].x > maxX) maxX = cells[i].x;
            if (cells[i].y < minY) minY = cells[i].y;
            if (cells[i].y > maxY) maxY = cells[i].y;
        }

        Size = new Vector3Int(maxX - minX + 1, maxY - minY + 1, 1);
    }

    public Vector3 GetStartPosition()
    {
        var grid = BuildingSystem.instance.gridLayout;

        if (localBottomCorners == null || localBottomCorners.Length == 0)
            CacheBottomCorners();

        if (localBottomCorners == null || localBottomCorners.Length == 0)
        {
            Vector3Int cell = grid.WorldToCell(transform.position);
            return grid.CellToWorld(new Vector3Int(cell.x, cell.y, 0));
        }

        Vector3Int minCell = grid.WorldToCell(transform.TransformPoint(localBottomCorners[0]));

        for (int i = 1; i < localBottomCorners.Length; i++)
        {
            var c = grid.WorldToCell(transform.TransformPoint(localBottomCorners[i]));
            if (c.x < minCell.x) minCell.x = c.x;
            if (c.y < minCell.y) minCell.y = c.y;
        }

        return grid.CellToWorld(new Vector3Int(minCell.x, minCell.y, 0));
    }

    public void Place()
    {
        placed = true;
        RestoreOriginalMaterial();
        PlayerPrefs.SetFloat("MachinePosX_" + prefabId, transform.position.x);
        PlayerPrefs.SetFloat("MachinePosY_" + prefabId, transform.position.y);
        PlayerPrefs.SetFloat("MachinePosZ_" + prefabId, transform.position.z);
        PlayerPrefs.SetFloat("MachineRotY_" + prefabId, transform.eulerAngles.y);
        PlayerPrefs.SetInt("MachineOwned_" + prefabId, 1);
        PlayerPrefs.Save();
    }


    void SetGhostColor(Color color)
    {
        if (ghostMaterial != null && !placed)
        {
            color.a = 0.6f;
            ghostMaterial.color = color;
        }
    }

    void RestoreOriginalMaterial()
    {
        if (objectrenderer != null && originalMaterial != null)
        {
            objectrenderer.material = originalMaterial;
        }
    }

}
